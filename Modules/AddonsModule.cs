using Discord;
using Discord.Interactions;
using Finder.Bot.Db.Repositories;

namespace Finder.Bot.Modules; 

[Group("addons", "Command For Managing Addons")]
public class AddonsModule(IUnitOfWork unitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("list", "Lists the installed addons", runMode: RunMode.Async)]
    public async Task GetAddons() {
        var addons = await unitOfWork.Addons.GetAddonsForGuildAsync(Context.Guild.Id);
        var embed = new EmbedBuilder {
            Title = "Addon list",
            Footer = new EmbedFooterBuilder {
                Text = "FinderBot"
            }
        };
        foreach (var addon in Enum.GetValues(typeof(Enums.Addons))) {
            if (addons.TryGetValue((Enums.Addons)addon, out bool installed) && installed) {
                embed.AddField(addon.ToString(), "Installed");
            } else {
                embed.AddField(addon.ToString(), "Not installed");
            }
        }
        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("install", "Installs an addon", runMode: RunMode.Async)]
    public async Task InstallAddon([Autocomplete(typeof(AddonsInstallAutocompleteHandler))] string addon) {
        if (!Enum.TryParse(addon, out Enums.Addons addonEnum)) {
            await RespondAsync("Error: Addon not found");
            return;
        }
        var value = await unitOfWork.Addons.GetAddonsForGuildAsync(Context.Guild.Id);
        if (value.TryGetValue(addonEnum, out bool installed) && installed) {
            await RespondAsync("Error: Addon already installed");
            return;
        }
        await unitOfWork.Addons.UpdateAddonForGuildAsync(Context.Guild.Id, addonEnum, true);
        await RespondAsync(embed: new EmbedBuilder {
            Title = "Addon Installed",
            Fields = [
                new() {
                    Name = "Addon",
                    Value = addon
                }
            ],
            Footer = new EmbedFooterBuilder {
                Text = "FinderBot"
            }
        }.Build());
    }

    [SlashCommand("uninstall", "Uninstalls an addon", runMode: RunMode.Async)]
    public async Task UninstallAddon([Autocomplete(typeof(AddonsUninstallAutocompleteHandler))] string addon) {
        if (!Enum.TryParse(addon, out Enums.Addons addonEnum)) {
            await RespondAsync("Error: Addon not found");
            return;
        }
        var value = await unitOfWork.Addons.GetAddonsForGuildAsync(Context.Guild.Id);
        if (!value.TryGetValue(addonEnum, out bool installed) || !installed) {
            await RespondAsync("Error: Addon not installed");
            return;
        }
        await unitOfWork.Addons.UpdateAddonForGuildAsync(Context.Guild.Id, addonEnum, false);
        await RespondAsync(embed: new EmbedBuilder {
            Title = "Addon Uninstalled",
            Fields = [
                new() {
                    Name = "Addon",
                    Value = addon
                }
            ],
            Footer = new EmbedFooterBuilder {
                Text = "FinderBot"
            }
        }.Build());
    }
}
    
public class AddonsInstallAutocompleteHandler(IUnitOfWork unitOfWork) : AutocompleteHandler {
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
        var value = await unitOfWork.Addons.GetAddonsForGuildAsync(context.Guild.Id);
        List<AutocompleteResult> results = [
            .. Enum.GetValues<Enums.Addons>()
                .Select(addon => new AutocompleteResult(addon.ToString(), addon.ToString()))
        ];
        foreach (var addon in value) {
            if (value.TryGetValue(addon.Key, out bool installed) && installed) {
                results.RemoveAll(r => r.Name == addon.Key.ToString());
            }
        }
        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}
    
public class AddonsUninstallAutocompleteHandler(IUnitOfWork unitOfWork) : AutocompleteHandler {
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
        var value = await unitOfWork.Addons.GetAddonsForGuildAsync(context.Guild.Id);
        List<AutocompleteResult> results = [
            .. from addon in value
            where value.TryGetValue(addon.Key, out bool installed) && installed
            select new AutocompleteResult(addon.Key.ToString(), addon.Key.ToString())
        ];
        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}