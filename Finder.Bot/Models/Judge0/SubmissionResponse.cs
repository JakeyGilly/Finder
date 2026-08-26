using System.Text.Json.Serialization;

namespace Finder.Bot.Models.Judge0;

public class SubmissionResponse {
    [JsonPropertyName("stdout")]
    public string Stdout { get; set; }

    [JsonPropertyName("stderr")]
    public string Stderr { get; set; }

    [JsonPropertyName("compile_output")]
    public string CompileOutput { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; }

    [JsonPropertyName("exit_code")]
    public int ExitCode { get; set; }

    [JsonPropertyName("exit_signal")]
    public int ExitSignal { get; set; }

    [JsonPropertyName("status")]
    public SubmissionStatus Status { get; set; }

    [JsonPropertyName("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonPropertyName("finished_at")]
    public DateTime? FinishedAt { get; set; }

    [JsonPropertyName("token")]
    public string Token { get; set; }

    [JsonPropertyName("time")]
    public float? Time { get; set; }

    [JsonPropertyName("wall_time")]
    public float WallTime { get; set; }

    [JsonPropertyName("memory")]
    public float? Memory { get; set; }
}