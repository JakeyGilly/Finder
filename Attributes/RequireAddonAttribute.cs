using Discord;
using Discord.Interactions;
using Microsoft.Extensions.DependencyInjection;
using Finder.Bot.Db.Repositories;

namespace Finder.Bot.Attributes;

public class RequireAddonAttribute(Enums.Addons addon) : PreconditionAttribute {
    public override async Task<PreconditionResult> CheckRequirementsAsync(IInteractionContext context, ICommandInfo commandInfo, IServiceProvider services) {
        if (context.Guild == null)
            return PreconditionResult.FromError("This command can only be used in a server.");
        var unitOfWork = services.GetRequiredService<IUnitOfWork>();
        bool isEnabled = await unitOfWork.Addons.AddonEnabledInGuildAsync(context.Guild.Id, addon);

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