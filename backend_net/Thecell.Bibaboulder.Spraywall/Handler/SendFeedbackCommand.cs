using System.Text.Json.Serialization;

namespace Thecell.Bibaboulder.Indoor.Handler;

public class SendFeedbackCommand
{
    public required string Feedback { get; set; }

    [JsonIgnore]
    public string? UserEmail { get; set; }
}
