using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Db.Repositories;

namespace Finder.Bot.Modules; 

public class PollModule(IUnitOfWork unitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("poll", "Create a poll for users to vote on.", runMode: RunMode.Async)]
    public async Task PollCommand(string question, string? answer1 = null, string? answer2 = null, string? answer3 = null, string? answer4 = null, string? answer5 = null, string? answer6 = null, string? answer7 = null, string? answer8 = null, string? answer9 = null, string? answer10 = null,
    string? answer11 = null, string? answer12 = null, string? answer13 = null, string? answer14 = null, string? answer15 = null, string? answer16 = null, string? answer17 = null, string? answer18 = null, string? answer19 = null, string? answer20 = null, string? answer21 = null, string? answer22 = null, 
    string? answer23 = null, string? answer24 = null) {
        var providedAnswers = new[] { 
            answer1, answer2, answer3, answer4, answer5, answer6, answer7, answer8, 
            answer9, answer10, answer11, answer12, answer13, answer14, answer15, answer16, 
            answer17, answer18, answer19, answer20, answer21, answer22, answer23, answer24 
        }.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a!).ToList();
        if (providedAnswers.Count == 0) {
            providedAnswers.AddRange(["Yes", "No"]);
        }
        var embed = new EmbedBuilder {
            Title = question,
            Description = $"Poll created by {Context.User.Username}",
            Footer = new EmbedFooterBuilder {
                Text = "FinderBot"
            }
        };
        ComponentBuilder builder = new ComponentBuilder();
        for (int i = 0; i < providedAnswers.Count; i++) {
            embed.AddField(providedAnswers[i], "0", true);
            builder.WithButton(providedAnswers[i], $"poll_{i}");
        }
        await RespondAsync("", embed: embed.Build(), components: builder.Build());
        var message = await GetOriginalResponseAsync();
        unitOfWork.Polls.AddItem(new() {
            MessageId = message.Id,
        });
        await unitOfWork.SaveChangesAsync();
    }
        
    public async Task OnButtonExecutedEvent(SocketMessageComponent messageComponent) {
        if (!messageComponent.Data.CustomId.StartsWith("poll_") || 
            !int.TryParse(messageComponent.Data.CustomId.Split('_')[1], out int voteIndex)) {
            return; 
        }
        var poll = await unitOfWork.Polls.GetItemAsync(m => m.MessageId == messageComponent.Message.Id);
        if (poll == null) return;
        if (poll.Voters.Any(v => v.UserId == messageComponent.User.Id)) {
            await messageComponent.RespondAsync("You already voted on this poll", ephemeral: true);
            return;
        }
        var updatedEmbed = messageComponent.Message.Embeds.First().ToEmbedBuilder();
        var targetField = updatedEmbed.Fields[voteIndex];
        string label = targetField.Name;
        targetField.Value = (int.Parse(targetField.Value.ToString()!) + 1).ToString();
        await messageComponent.Message.ModifyAsync(x => x.Embed = updatedEmbed.Build());
        await messageComponent.RespondAsync($"You voted for **{label}**", ephemeral: true);
        poll.Voters.Add(new() { UserId = messageComponent.User.Id, PollMessageId = messageComponent.Message.Id });
        await unitOfWork.SaveChangesAsync();
    }
}