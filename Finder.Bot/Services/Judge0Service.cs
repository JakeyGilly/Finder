using System.Formats.Tar;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace Finder.Bot.Services;

public class Judge0Service : IAsyncDisposable {
    public ContainerInspectResponse? Judge0ApiContainer { get; private set; }
    public ContainerInspectResponse? Judge0WorkerContainer { get; private set; }
    public ContainerInspectResponse? Judge0RedisContainer { get; private set; }
    public ContainerInspectResponse? Judge0DatabaseContainer { get; private set; }

    private bool _isRunning;
    private DockerClient _dockerClient = new DockerClientBuilder().Build();
    

    public async Task StartJudge0Async() {
        if (_isRunning) return;
        
        var configPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "judge0.conf"));
        Console.WriteLine($"Using judge0.conf configuration file at {configPath}");
        if (!File.Exists(configPath)) {
            throw new FileNotFoundException($"Could not find configuration file at {configPath}");
        }
        
        await _dockerClient.Volumes.CreateAsync(new VolumesCreateParameters {
            Name = "judge0-postgres-data"
        });
        
        var existingNetworks = await _dockerClient.Networks.ListNetworksAsync();
        if (existingNetworks.All(n => n.Name != "finder-net")) {
            await _dockerClient.Networks.CreateNetworkAsync(new NetworksCreateParameters {
                Name = "finder-net",
                Driver = "bridge",
                Labels = new Dictionary<string, string> {
                    { "com.docker.compose.project", "finder" }
                }
            });
        }

        var sharedLoggingConfig = new LogConfig {
            Type = "json-file",
            Config = new Dictionary<string, string> {
                { "max-size", "100m" }
            }
        };

        var configFile = await File.ReadAllTextAsync(configPath);
        var envVars = configFile.Split("\n")
            .Where(line => !string.IsNullOrWhiteSpace(line) && !line.StartsWith("#"))
            .Select(line => line.Split('=', 2))
            .Where(parts => parts.Length == 2)
            .ToDictionary(parts => parts[0].Trim(), parts => parts[1].Trim());
        
        envVars.TryGetValue("POSTGRES_USER", out var postgresUser);
        envVars.TryGetValue("POSTGRES_PASSWORD", out var postgresPassword);
        envVars.TryGetValue("POSTGRES_DB", out var postgresDb);
        envVars.TryGetValue("REDIS_PASSWORD", out var redisPassword);
        
        var envList = envVars.Select(kv => $"{kv.Key}={kv.Value}").ToList();

        if (string.IsNullOrEmpty(postgresUser) || string.IsNullOrEmpty(postgresPassword) || string.IsNullOrEmpty(postgresDb)) {
            throw new InvalidOperationException("POSTGRES_USER, POSTGRES_PASSWORD, and POSTGRES_DB must be set in judge0.conf");
        }
        if (string.IsNullOrEmpty(redisPassword)) {
            throw new InvalidOperationException("REDIS_PASSWORD must be set in judge0.conf");
        }
        
        var existingContainers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters {
            All = true,
            Filters = new Dictionary<string, IDictionary<string, bool>> {
                { "name", new Dictionary<string, bool> {
                    { "judge0-db", true },
                    { "judge0-redis", true },
                    { "judge0", true },
                    { "judge0-worker", true }
                } }
            }
        });

        CreateContainerResponse? judge0DatabaseContainer = null;
        var existingDb = existingContainers.FirstOrDefault(c => c.Names.Any(n => n.Contains("judge0-db")));
        if (existingDb == null) {
            judge0DatabaseContainer = await _dockerClient.Containers.CreateContainerAsync(
                new CreateContainerParameters {
                    Name = "judge0-db",
                    Image = "postgres:latest",
                    Env = envList,
                    HostConfig = new HostConfig {
                        NetworkMode = "finder-net",
                        RestartPolicy = new RestartPolicy {
                            Name = RestartPolicyKind.Always
                        },
                        Mounts = new List<Mount> {
                            new() {
                                Type = "volume",
                                Source = "judge0-postgres-data",
                                Target = "/var/lib/postgresql"
                            }
                        },
                        LogConfig = sharedLoggingConfig
                    },
                    Healthcheck = new HealthcheckConfig() {
                        Test = ["CMD-SHELL", $"sh -c 'pg_isready -U \"{postgresUser}\" -d \"{postgresDb}\"'"],
                        Interval = TimeSpan.FromSeconds(5),
                        Timeout = TimeSpan.FromSeconds(5),
                        Retries = 5
                    },
                    Labels = new Dictionary<string, string> {
                        { "com.docker.compose.project", "finder" },
                    },
                });
        }

        var dbContainerId = judge0DatabaseContainer?.ID ?? existingDb!.ID;
        await _dockerClient.Containers.StartContainerAsync(dbContainerId, new ContainerStartParameters());
        Judge0DatabaseContainer = await _dockerClient.Containers.InspectContainerAsync(dbContainerId);
        
        CreateContainerResponse? judge0RedisContainer = null;
        var existingRedis = existingContainers.FirstOrDefault(c => c.Names.Any(n => n.Contains("judge0-redis")));
        if (existingRedis == null) {
            judge0RedisContainer = await _dockerClient.Containers.CreateContainerAsync(
                new CreateContainerParameters {
                    Name = "judge0-redis",
                    Image = "redis:latest",
                    Env = envVars.Select(kv => $"{kv.Key}={kv.Value}").ToList(),
                    Cmd = [
                        "sh", "-c", $"docker-entrypoint.sh --appendonly yes --requirepass \"{redisPassword}\""
                    ],
                    HostConfig = new HostConfig {
                        NetworkMode = "finder-net",
                        RestartPolicy = new RestartPolicy {
                            Name = RestartPolicyKind.Always
                        },
                        LogConfig = sharedLoggingConfig
                    },
                    Healthcheck = new HealthcheckConfig {
                        Test = ["CMD-SHELL", $"sh -c 'redis-cli -a \"{redisPassword}\" ping'"],
                        Interval = TimeSpan.FromSeconds(5),
                        Timeout = TimeSpan.FromSeconds(5),
                        Retries = 5
                    },
                    Labels = new Dictionary<string, string> {
                        { "com.docker.compose.project", "finder" },
                    },
                });
        }

        var redisContainerId = judge0RedisContainer?.ID ?? existingRedis!.ID;
        await _dockerClient.Containers.StartContainerAsync(redisContainerId, new ContainerStartParameters());
        Judge0RedisContainer = await _dockerClient.Containers.InspectContainerAsync(redisContainerId);

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        while (!cts.IsCancellationRequested) {
            var redisInspect = await _dockerClient.Containers.InspectContainerAsync(redisContainerId, cts.Token);
            var dbInspect = await _dockerClient.Containers.InspectContainerAsync(dbContainerId, cts.Token);
            if (redisInspect.State?.Health?.Status == "healthy" && dbInspect.State?.Health?.Status == "healthy") break;
            await Task.Delay(1000, cts.Token);
        }

        CreateContainerResponse? judge0ApiContainer = null;
        var existingApi = existingContainers.FirstOrDefault(c => c.Names.Any(n => n.Contains("judge0-api")));
        if (existingApi == null) {
            judge0ApiContainer = await _dockerClient.Containers.CreateContainerAsync(
                new CreateContainerParameters {
                    Name = "judge0-api",
                    Image = "judge0/judge0:latest",
                    Platform = "linux/amd64",
                    ExposedPorts = new Dictionary<string, EmptyStruct> {
                        { "2358/tcp", default }
                    },
                    Labels = new Dictionary<string, string> {
                        { "com.docker.compose.project", "finder" },
                    },
                    HostConfig = new HostConfig {
                        Privileged = true,
                        NetworkMode = "finder-net",
                        RestartPolicy = new RestartPolicy {
                            Name = RestartPolicyKind.Always
                        },
                        PortBindings = new Dictionary<string, IList<PortBinding>> {
                            { "2358/tcp", new List<PortBinding> { new() { HostPort = "2358" } } }
                        },
                        LogConfig = sharedLoggingConfig
                    },
                });
        }
        
        var apiContainerId = judge0ApiContainer?.ID ?? existingApi!.ID;

        var fileBytes = await File.ReadAllBytesAsync(configPath);
        byte[] tarballBytes;
        using (var archiveStream = new MemoryStream()) {
            await using (var tarArchive = new TarWriter(archiveStream, true)) {
                var entry = new PaxTarEntry(TarEntryType.RegularFile, "judge0.conf") {
                    DataStream = new MemoryStream(fileBytes)
                };
                await tarArchive.WriteEntryAsync(entry);
            }
            tarballBytes = archiveStream.ToArray();
        }

        using (var apiStream = new MemoryStream(tarballBytes)) {
            await _dockerClient.Containers.ExtractArchiveToContainerAsync(apiContainerId, new CopyToContainerParameters { Path = "/" }, apiStream);
        }
        await _dockerClient.Containers.StartContainerAsync(apiContainerId, new ContainerStartParameters());
        Judge0ApiContainer = await _dockerClient.Containers.InspectContainerAsync(apiContainerId);

        CreateContainerResponse? judge0WorkerContainer = null;
        var existingWorker = existingContainers.FirstOrDefault(c => c.Names.Any(n => n.Contains("judge0-worker")));
        if (existingWorker == null) {
            judge0WorkerContainer = await _dockerClient.Containers.CreateContainerAsync(
                new CreateContainerParameters {
                    Name = "judge0-worker",
                    Image = "judge0/judge0:latest",
                    Platform = "linux/amd64",
                    Cmd = ["./scripts/workers"],
                    Labels = new Dictionary<string, string> {
                        { "com.docker.compose.project", "finder" },
                    },
                    HostConfig = new HostConfig {
                        Privileged = true,
                        NetworkMode = "finder-net",
                        RestartPolicy = new RestartPolicy {
                            Name = RestartPolicyKind.Always
                        },
                        LogConfig = sharedLoggingConfig
                    }
                });
        }
        
        var workerContainerId = judge0WorkerContainer?.ID ?? existingWorker!.ID;

        using (var workerStream = new MemoryStream(tarballBytes)) {
            await _dockerClient.Containers.ExtractArchiveToContainerAsync(workerContainerId, new CopyToContainerParameters { Path = "/" }, workerStream);
        }
        await _dockerClient.Containers.StartContainerAsync(workerContainerId, new ContainerStartParameters());
        Judge0WorkerContainer = await _dockerClient.Containers.InspectContainerAsync(workerContainerId);

        _isRunning = true;
    }

    public async Task StopJudge0Async() {
        if (!_isRunning) return;
        var targetServices = new[] { "judge0-api", "judge0-worker", "judge0-redis", "judge0-db" };
        try {
            var containers = await _dockerClient.Containers.ListContainersAsync(new ContainersListParameters {
                Filters = new Dictionary<string, IDictionary<string, bool>> { { "name", targetServices.ToDictionary(s => s, s => true) } }
            });
            foreach (var container in containers) {
                await _dockerClient.Containers.StopContainerAsync(container.ID, new ContainerStopParameters());
            }
        } catch (Exception ex) {
            // ignored
        } finally {
            _isRunning = false;
        }
    }
    
    public async Task<string> GetApiUrlAsync() {
        if (Judge0ApiContainer == null) throw new InvalidOperationException("Judge0 API container is not running.");
        if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != "true") return "http://localhost:2358";
        var apiIp = Judge0ApiContainer.NetworkSettings?.Networks.FirstOrDefault().Value.IPAddress;
        return string.IsNullOrEmpty(apiIp) ? throw new InvalidOperationException("Could not retrieve Judge0 API container IP address.") : $"http://{apiIp}:2358";
    }

    public async ValueTask DisposeAsync() {
        if (!_isRunning) {
            _dockerClient.Dispose();
            return;
        }
        await StopJudge0Async();
        _dockerClient.Dispose();
    }
}