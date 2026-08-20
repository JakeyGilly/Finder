using Discord;
using Discord.Interactions;
using Finder.Db.Repositories;
using Finder.Db.UnitOfWork;
using Microsoft.Extensions.DependencyInjection;
using Finder.Shared.Enum;

namespace Finder.Bot.Attributes;

public class RequireAddonAttribute(Addons addon) : PreconditionAttribute {
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services) {
        if (context.Guild == null)
            return PreconditionResult.FromError("This command can only be used in a server.");
        var unitOfWork = services.GetRequiredService<IBotUnitOfWork>();
        var isEnabled = await unitOfWork.Addons.GetItemAsync(m => m.GuildId == context.Guild.Id && m.Addon == addon && m.Enabled) != null;

        if (isEnabled) return PreconditionResult.FromSuccess();
        await context.Interaction.RespondAsync(embed: new EmbedBuilder {
            Title = addon.ToString(),
            Description = "This addon is disabled on this server.",
            Color = Color.Red,
            Fields = [
                new() {
                    Name = "Enable",
                    Value = $"Use `/addons install {addon}` to enable this addon."
                }
            ]
        }.Build(), ephemeral: true);
        return PreconditionResult.FromError($"Addon '{addon}' is disabled.");

    }
}