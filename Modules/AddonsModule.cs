using Discord;
using Discord.Interactions;
using Finder.Bot.Db.Models;
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
        foreach (var addon in Enum.GetValues<Enums.Addons>()) {
            embed.AddField(addon.ToString(), addons.Contains((Enums.Addons)addon) ? "Installed" : "Not installed");
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
        if (value.Contains(addonEnum)) {
            await RespondAsync("Error: Addon already installed");
            return;
        }
        await unitOfWork.Addons.UpsertItemAsync((m) => m.GuildId == Context.Guild.Id && m.Addon == addonEnum, new AddonsModel {
            GuildId = Context.Guild.Id,
            Addon = addonEnum,
            Enabled = true
        });
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
        if (!value.Contains(addonEnum)) {
            await RespondAsync("Error: Addon not installed");
            return;
        }
        await unitOfWork.Addons.UpsertItemAsync((m) => m.GuildId == Context.Guild.Id && m.Addon == addonEnum, new AddonsModel {
            GuildId = Context.Guild.Id,
            Addon = addonEnum,
            Enabled = false
        });
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
        results.RemoveAll(r => value.Contains(Enum.Parse<Enums.Addons>(r.Name)));
        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}
    
public class AddonsUninstallAutocompleteHandler(IUnitOfWork unitOfWork) : AutocompleteHandler {
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
        var value = await unitOfWork.Addons.GetAddonsForGuildAsync(context.Guild.Id);
        List<AutocompleteResult> results = [
            .. value.Select(addon => new AutocompleteResult(addon.ToString(), addon.ToString()))
        ];
        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}