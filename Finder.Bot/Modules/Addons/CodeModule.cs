using System.Net.Http.Json;
using System.Text;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Attributes;
using Finder.Bot.Models.Judge0;
using Finder.Bot.Services;

namespace Finder.Bot.Modules.Addons;

[RequireAddon(Shared.Enum.Addons.Code)]
[Group("code", "Command For Executing Code")]
public class CodeModule(Judge0Service judge0Service) : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("startup", "Startup the code execution engine", runMode: RunMode.Async)]
    public async Task Startup() {
        await DeferAsync();
        await judge0Service.StartJudge0Async();
        var api = judge0Service.Judge0ApiContainer.NetworkSettings.Networks.FirstOrDefault().Value.IPAddress;
        await FollowupAsync(embed: new EmbedBuilder {
            Title = "Code",
            Fields = [
                new() {
                    Name = "Code Execution Engine",
                    Value = $"The code execution engine has been started successfully on {api}:2358"
                }
            ],
            Footer = new EmbedFooterBuilder {
                Text = "FinderBot"
            }
        }.Build());
    }

    [SlashCommand("code", "Modal for code execution", runMode: RunMode.Async)]
    public async Task Modal([Autocomplete(typeof(LanguageAutocompleteHandler))] string language) {
        if (judge0Service.Judge0ApiContainer == null) {
            await RespondAsync("The code execution engine is not currently running. Run `/code startup` first.", ephemeral: true);
            return;
        }
        
        string languageName = LanguageAutocompleteHandler.CachedLanguages?
            .FirstOrDefault(l => l.Id.ToString() == language)?.Name ?? "Unknown Language";
        
        await RespondWithModalAsync(new ModalBuilder {
                Title = "Code Editor",
                CustomId = $"code_editor_{language}"
            }
            .AddTextDisplay($"You are coding in {languageName}")
            .AddTextInput("Code", "code_input", TextInputStyle.Paragraph, "Enter your code here", required: true)
            .AddTextInput("Stdin", "stdin_input", TextInputStyle.Short, "Enter your stdin input here (optional)", required: false)
            .Build()
        );
    }
    
    public async Task OnModalInteractionAsync(SocketInteraction socketInteraction) {
        if (socketInteraction is not SocketModal modalInteraction) return;
        if (!modalInteraction.Data.CustomId.StartsWith("code_editor")) return;
        await modalInteraction.DeferAsync();
        List<SocketMessageComponentData> components = [.. modalInteraction.Data.Components];
        string codeInput = components
            .First(x => x.CustomId == "code_input").Value;
        string stdinInput = components
            .FirstOrDefault(x => x.CustomId == "stdin_input")?.Value ?? string.Empty;
        string language = modalInteraction.Data.CustomId.Split('_').Last();
        if (string.IsNullOrWhiteSpace(codeInput)) {
            await modalInteraction.RespondAsync("No code was provided.", ephemeral: true);
            return;
        }
        if (LanguageAutocompleteHandler.CachedLanguages?.All(l => l.Id.ToString() != language) == true) {
            await modalInteraction.RespondAsync("The selected programming language is not supported.", ephemeral: true);
            return;
        }
        
        using var httpClient = new HttpClient();
        httpClient.BaseAddress = new Uri(await judge0Service.GetApiUrlAsync());
        var payload = new Dictionary<string, object> {
            { "source_code", Base64Encode(codeInput) },
            { "language_id", int.Parse(language) },
        };
        if (!string.IsNullOrWhiteSpace(stdinInput)) {
            payload.Add("stdin", Base64Encode(stdinInput));
        }
        
        string jsonString = System.Text.Json.JsonSerializer.Serialize(payload);
        var response = await httpClient.PostAsync("/submissions?base64_encoded=true", new StringContent(jsonString, Encoding.UTF8, "application/json"));
        if (!response.IsSuccessStatusCode) {
            string errorText = await response.Content.ReadAsStringAsync();
            await modalInteraction.FollowupAsync(embed: new EmbedBuilder {
                Title = "Code Execution Error",
                Description = $"Failed to submit code for execution with status: {response.StatusCode}. Please try again later.",
                Fields = [
                    new EmbedFieldBuilder {
                        Name = "Error Details",
                        Value = $"```{errorText}```"
                    }
                ],
                Footer = new EmbedFooterBuilder {
                    Text = "FinderBot"
                }
            }.Build());
            return;
        }
        
        var result = await response.Content.ReadFromJsonAsync<SubmissionResponse>();
        if (result == null) {
            await modalInteraction.FollowupAsync(embed: new EmbedBuilder {
                Title = "Code Execution Error",
                Description = "Failed to retrieve submission response. Please try again later.",
                Fields = [
                    new EmbedFieldBuilder {
                        Name = "Error Details",
                        Value = "The response from the code execution engine was `null`."
                    }
                ],
                Footer = new EmbedFooterBuilder {
                    Text = "FinderBot"
                }
            }.Build());
            return;
        }
        var submissionId = result.Token;
        do {
            response = await httpClient.GetAsync($"/submissions/{submissionId}?base64_encoded=true");
            response.EnsureSuccessStatusCode();
            result = await response.Content.ReadFromJsonAsync<SubmissionResponse>();
            if (result == null) {
                await modalInteraction.FollowupAsync(embed: new EmbedBuilder {
                    Title = "Code Execution Error",
                    Description = "Failed to retrieve submission response during polling. Please try again later.",
                    Fields = [
                        new EmbedFieldBuilder {
                            Name = "Error Details",
                            Value = "The response from the code execution engine was `null`."
                        }
                    ],
                    Footer = new EmbedFooterBuilder {
                        Text = "FinderBot"
                    }
                }.Build());
                return;
            }
            await Task.Delay(1000);
        } while (result.Status.Id < 3);
        
        if (result.Status.Id == 3) {
            string output = Base64Decode(result.Stdout);
            string formattedOutput = string.IsNullOrWhiteSpace(output) ? "No output." : output;
            string memoryUsage = result.Memory.HasValue ? $"{result.Memory.Value} KB" : "N/A";
            string executionTime = result.Time.HasValue ? $"{result.Time.Value} seconds" : "N/A";
            string footerText = $"Memory: {memoryUsage} | Time: {executionTime} | FinderBot";
            await modalInteraction.FollowupAsync(embed: new EmbedBuilder {
                Title = "Code Execution Result",
                Description = $"Execution completed successfully with status: {result.Status.Description}",
                Fields = [
                    new EmbedFieldBuilder {
                        Name = "Output",
                        Value = $"```{formattedOutput}```"
                    },
                ],
                Footer = new EmbedFooterBuilder {
                    Text = footerText
                }
            }.Build());
        } else {
            string errorOutput = string.IsNullOrWhiteSpace(result.Stderr) ? "No error output." : Base64Decode(result.Stderr);
            string memoryUsage = result.Memory.HasValue ? $"{result.Memory.Value} KB" : "N/A";
            string executionTime = result.Time.HasValue ? $"{result.Time.Value} seconds" : "N/A";
            string footerText = $"Memory: {memoryUsage} | Time: {executionTime} | FinderBot";
            await modalInteraction.FollowupAsync(embed: new EmbedBuilder {
                Title = "Code Execution Error",
                Description = $"Execution failed with status: {result?.Status.Description}",
                Fields = [
                    new EmbedFieldBuilder {
                        Name = "Error Details",
                        Value = $"```{errorOutput}```"
                    }
                ],
                Footer = new EmbedFooterBuilder {
                    Text = footerText
                }
            }.Build());
        }
    }
    
    private string Base64Encode(string plainText) {
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
    }
    

    private string Base64Decode(string base64EncodedData) {
        return Encoding.UTF8.GetString(Convert.FromBase64String(base64EncodedData));
    }


}

public class LanguageAutocompleteHandler(Judge0Service judge0Service) : AutocompleteHandler {
    public static List<LanguageReponse>? CachedLanguages;
    private static DateTime _cacheTime = DateTime.MinValue;

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,  IAutocompleteInteraction autocompleteInteraction,  IParameterInfo parameter,  IServiceProvider services) {
        if (judge0Service.Judge0ApiContainer == null) {
            return AutocompletionResult.FromError(new Exception("The code execution engine is not currently running. Run `/code startup` first."));
        }
        
        if (CachedLanguages == null || (DateTime.UtcNow - _cacheTime).TotalHours > 1) {
            using var httpClient = new HttpClient();
            httpClient.BaseAddress = new Uri(await judge0Service.GetApiUrlAsync());
            CachedLanguages = await httpClient.GetFromJsonAsync<List<LanguageReponse>>("/languages");
            _cacheTime = DateTime.UtcNow;
        }

        if (CachedLanguages == null || CachedLanguages.Count == 0) {
            return AutocompletionResult.FromSuccess([new AutocompleteResult("No languages found.", "0")]);
        }

        string userInput = autocompleteInteraction.Data.Current.Value?.ToString() ?? string.Empty;
        var results = CachedLanguages
            .Where(lang => lang.Name.Contains(userInput, StringComparison.OrdinalIgnoreCase))
            .Select(lang => new AutocompleteResult(
                name: lang.Name, 
                value: lang.Id.ToString()
            )).Take(25);

        return AutocompletionResult.FromSuccess(results);
    }
}