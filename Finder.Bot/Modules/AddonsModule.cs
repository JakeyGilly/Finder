using Discord;
using Discord.Interactions;
using Finder.Bot.Db.Repositories;

namespace Finder.Bot.Modules; 

[Group("addons", "Command For Managing Addons")]
public class AddonsModule(IUnitOfWork unitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("list", "Lists the installed addons", runMode: RunMode.Async)]
    public async Task GetAddons() {
        var addons = await unitOfWork.Addons.GetItemsAsync((m) => m.GuildId == Context.Guild.Id && m.Enabled);
        var embed = new EmbedBuilder {
            Title = "Addon list",
            Footer = new EmbedFooterBuilder {
                Text = "FinderBot"
            }
        };
        foreach (var addon in Enum.GetValues<Enums.Addons>()) {
            embed.AddField(addon.ToString(), addons.Any(a => a.Addon == addon) ? "Installed" : "Not Installed");
        }
        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("install", "Installs an addon", runMode: RunMode.Async)]
    public async Task InstallAddon([Autocomplete(typeof(AddonsInstallAutocompleteHandler))] string addon) {
        if (!Enum.TryParse(addon, out Enums.Addons addonEnum)) {
            await RespondAsync("Error: Addon not found");
            return;
        }
        var addonData = await unitOfWork.Addons.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.Addon == addonEnum);
        if (addonData?.Enabled == true) {
            await RespondAsync("Error: Addon already installed");
            return;
        }
        if (addonData == null) {
            unitOfWork.Addons.AddItem(new() {
                GuildId = Context.Guild.Id,
                Addon = addonEnum,
                Enabled = true
            });
        } else {
            addonData.Enabled = true;
        }
        await unitOfWork.SaveChangesAsync();
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
        var addonData = await unitOfWork.Addons.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.Addon == addonEnum && m.Enabled);
        if (addonData == null) {
            await RespondAsync("Error: Addon not installed");
            return;
        }
        addonData.Enabled = false;
        await unitOfWork.SaveChangesAsync();
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
        var installedAddons = await unitOfWork.Addons.GetItemsAsync((m) => m.GuildId == context.Guild.Id && m.Enabled);
        List<AutocompleteResult> results = [
            .. Enum.GetValues<Enums.Addons>()
                .Where(a => installedAddons.All(x => x.Addon != a))
                .Select(a => new AutocompleteResult(a.ToString(), a.ToString()))
                .Take(25)
        ];
        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(results);
    }
}
    
public class AddonsUninstallAutocompleteHandler(IUnitOfWork unitOfWork) : AutocompleteHandler {
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
        var installed = await unitOfWork.Addons.GetItemsAsync((m) => m.GuildId == context.Guild.Id && m.Enabled);
        List<AutocompleteResult> results = [
            .. installed.Select(a => new AutocompleteResult(a.ToString(), a.ToString()))
        ];
        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}