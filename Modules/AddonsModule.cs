using Discord;
using Discord.Interactions;
using Finder.Bot.Db;
using Finder.Bot.Models.Data.Bot;

namespace Finder.Bot.Modules; 

[Group("addons", "Command For Managing Addons")]
public class AddonsModule : InteractionModuleBase<ShardedInteractionContext> {
    private readonly ICosmosDbService _cosmosDbService;
    public AddonsModule(ICosmosDbService cosmosDbService) {
        _cosmosDbService = cosmosDbService;
    }
    
    [SlashCommand("list", "Lists the installed addons", runMode: RunMode.Async)]
    public async Task GetAddons() {
        AddonsModel? value = await _cosmosDbService.GetItemAsync(Context.Guild.Id.ToString());
        EmbedBuilder embed = new EmbedBuilder {
            Title = "Addon list",
            Footer = new EmbedFooterBuilder {
                Text = "FinderBot"
            }
        };
        if (value == null || value.Addons.Count == 0) {
            foreach (object addon in Enum.GetValues(typeof(Enums.Addons))) {
                embed.AddField(addon.ToString(), "Not installed");
            }
        } else {
            foreach (object addon in Enum.GetValues(typeof(Enums.Addons))) {
                embed.AddField(addon.ToString(), value.Addons.Keys.Contains(addon.ToString()) && value.Addons.First(x => x.Key == addon.ToString()).Value ? "Installed" : "Not Installed");
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
        AddonsModel? value = await _cosmosDbService.GetItemAsync(Context.Guild.Id.ToString());
        if (value == null) {
            await _cosmosDbService.AddItemAsync(new AddonsModel {
                Id = Context.Guild.Id.ToString(),
                Addons = new Dictionary<string, bool> {
                    {addon, true}
                }
            });
        } else if (value.Addons.ContainsKey(addon)) {
            await RespondAsync("Error: Addon already installed");
            return;
        } else {
            value.Addons.Add(addon, true);
            await _cosmosDbService.UpdateItemAsync(value.Id, value);
        }
        await RespondAsync(embed: new EmbedBuilder {
            Title = "Addon Installed",
            Fields = new List<EmbedFieldBuilder> {
                new() {
                    Name = "Addon",
                    Value = addon
                }
            },
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
        AddonsModel? value = await _cosmosDbService.GetItemAsync(Context.Guild.Id.ToString());
        if (value == null || !value.Addons.ContainsKey(addon)) {
            await RespondAsync("Error: Addon not installed");
            return;
        }
        value.Addons.Remove(addon);
        await _cosmosDbService.UpdateItemAsync(value.Id, value);
        await RespondAsync(embed: new EmbedBuilder {
            Title = "Addon Uninstalled",
            Fields = new List<EmbedFieldBuilder> {
                new() {
                    Name = "Addon",
                    Value = addon
                }
            },
            Footer = new EmbedFooterBuilder {
                Text = "FinderBot"
            }
        }.Build());
    }
}
    
public class AddonsInstallAutocompleteHandler : AutocompleteHandler {
    private readonly ICosmosDbService _cosmosDbService;
    public AddonsInstallAutocompleteHandler(ICosmosDbService cosmosDbService) {
        _cosmosDbService = cosmosDbService;
    }
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
        List<AutocompleteResult> results = new List<AutocompleteResult>();
        foreach (object addon in Enum.GetValues(typeof(Enums.Addons))) {
            AddonsModel? value = await _cosmosDbService.GetItemAsync(context.Guild.Id.ToString());
            if (value == null || !value.Addons.Keys.Contains(addon.ToString()) || !value.Addons.First(x => x.Key == addon.ToString()).Value) {
                results.Add(new AutocompleteResult(addon.ToString(), addon.ToString()));
            }
        }
        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}
    
public class AddonsUninstallAutocompleteHandler : AutocompleteHandler {
    private readonly ICosmosDbService _cosmosDbService;
    public AddonsUninstallAutocompleteHandler(ICosmosDbService cosmosDbService) {
        _cosmosDbService = cosmosDbService;
    }
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
        List<AutocompleteResult> results = new List<AutocompleteResult>();
        foreach (object addon in Enum.GetValues(typeof(Enums.Addons))) {
            AddonsModel? value = await _cosmosDbService.GetItemAsync(context.Guild.Id.ToString());
            if (value != null && value.Addons.Keys.Contains(addon.ToString()) && value.Addons.First(x => x.Key == addon.ToString()).Value) {
                results.Add(new AutocompleteResult(addon.ToString(), addon.ToString()));
            }
        }
        // max - 25 suggestions at a time (API limit)
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}