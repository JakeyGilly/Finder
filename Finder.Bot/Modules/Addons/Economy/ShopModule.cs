using Discord;
using Discord.Interactions;
using Finder.Bot.Attributes;
using Finder.Db.UnitOfWork;
using Finder.Shared.Models;
using Newtonsoft.Json;

namespace Finder.Bot.Modules.Addons.Economy; 

[Group("shop", "The shop commands to buy items.")]
[RequireAddon(Shared.Enum.Addons.Economy)]
public class ShopModule(IBotUnitOfWork botUnitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    public static List<Item> Items =
        JsonConvert.DeserializeObject<List<Item>>(File.ReadAllText("Modules/Addons/Economy/Items/items.json")) ?? new();

    [SlashCommand("buy", "Buy an item from the shop.")]
    public async Task BuyCommand([Autocomplete(typeof(ShopAutocompleteHandler))] string item, int amount = 1) {
        if (amount < 1) {
            await RespondAsync("You must buy at least 1 item.");
            return;
        }
        Guid itemId = Guid.TryParse(item, out Guid parsedId) ? parsedId : Guid.Empty;
        if (Items.Count == 0) {
            await ReplyAsync("Could not load items.");
            return;
        }
        var itemToBuy = Items.Find(x => x.Id == itemId);
        if (itemToBuy == null) {
            await ReplyAsync("Item not found.");
            return;
        }
        if (!itemToBuy.Buyable) {
            await RespondAsync("This item is not buyable.");
            return;
        }
        var economy = await botUnitOfWork.Economy.GetItemAsync(m => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id);
        if (economy == null) {
            botUnitOfWork.Economy.AddItem(economy = new() {
                GuildId = Context.Guild.Id,
                UserId = Context.User.Id,
            });
        }
        if (economy.Money < itemToBuy.BuyPrice * amount) {
            await RespondAsync("You do not have enough money to buy this item.");
            return;
        }
        economy.Money -= itemToBuy.BuyPrice * amount;
        var inventoryItem = await botUnitOfWork.Inventory.GetItemAsync(m => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id && m.ItemId == itemId);
        if (inventoryItem == null) {
            botUnitOfWork.Inventory.AddItem(inventoryItem = new() {
                GuildId = Context.Guild.Id,
                UserId = Context.User.Id,
                ItemId = itemId
            });
        }
        inventoryItem.Quantity += amount;
        await botUnitOfWork.SaveChangesAsync();
        await RespondAsync(embed: new EmbedBuilder {
            Title = $"You have purchased {(amount == 1 ? "an" : amount.ToString())} item{(amount == 1 ? "" : "s")}",
            Fields = [
                new() {
                    Name = itemToBuy.Name,
                    Value = $"For {itemToBuy.BuyPrice * amount}",
                }
            ]
        }.Build());
    }
        
    [SlashCommand("sell", "Sell an item to the shop.")]
    public async Task SellCommand([Autocomplete(typeof(InvAutocompleteHandler))] string item, int amount = 1) {
        if (amount < 1) {
            await RespondAsync("You must sell at least 1 item.");
            return;
        }
        Guid itemId = Guid.TryParse(item, out Guid parsedId) ? parsedId : Guid.Empty;
        if (Items.Count == 0) {
            await ReplyAsync("Could not load items.");
            return;
        }
        var itemToSell = Items.Find(x => x.Id == itemId);
        if (itemToSell == null) {
            await ReplyAsync("Item not found.");
            return;
        }
        if (!itemToSell.Sellable) {
            await RespondAsync("This item is not sellable.");
            return;
        }
        var inventoryItem = await botUnitOfWork.Inventory.GetItemAsync(m => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id && m.ItemId == itemId);
        if (inventoryItem == null || inventoryItem.Quantity < amount) {
            await RespondAsync("You do not have enough of this item to sell.");
            return;
        }
        var economy = await botUnitOfWork.Economy.GetItemAsync(m => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id);
        if (economy == null) {
            botUnitOfWork.Economy.AddItem(economy = new() {
                GuildId = Context.Guild.Id,
                UserId = Context.User.Id,
            });
        }
        economy.Money += itemToSell.SellPrice * amount;
        inventoryItem.Quantity -= amount;
        if (inventoryItem.Quantity == 0) {
            botUnitOfWork.Inventory.DeleteItem(inventoryItem);
        }
        await botUnitOfWork.SaveChangesAsync();
        await RespondAsync(embed: new EmbedBuilder {
            Title = $"You have sold {(amount == 1 ? amount.ToString() : "an")} item{ (amount == 1 ? "" : "s")}!",
            Fields = [
                new() {
                    Name = itemToSell.Name,
                    Value = $"For {itemToSell.SellPrice * amount}",
                }
            ]
        }.Build());
    }

    [SlashCommand("info", "Displays item info in the shop")]
    public async Task InfoCommand([Autocomplete(typeof(ShopAutocompleteHandler))] string itemStr) {
        Guid itemId = Guid.TryParse(itemStr, out Guid parsedId) ? parsedId : Guid.Empty;
        if (Items.Count == 0) {
            await ReplyAsync("Could not load items.");
            return;
        }
        var item = Items.Find(x => x.Id == itemId);
        if (item == null) {
            await ReplyAsync("Item not found.");
            return;
        }
        await RespondAsync(embed: new EmbedBuilder {
            Title = $"{item.Name} information",
            Fields = [
                new() {
                    Name = "Description",
                    Value = item.Description,
                    IsInline = false
                },
                new() {
                    Name = "Rarity",
                    Value = item.Rarity.ToString(),
                    IsInline = false
                },
                new() {
                    Name = "Buy Price",
                    Value = item.Buyable ? item.BuyPrice : "Unbuyable",
                    IsInline = true
                },
                new() {
                    Name = "Sell Price",
                    Value = item.Sellable ? item.SellPrice : "Unsellable",
                    IsInline = true
                },
                new() {
                    Name = "Tradeable",
                    Value = item.Tradeable ? "Yes" : "No",
                    IsInline = true
                }
            ]
        }.Build());
    }
}

public class ShopAutocompleteHandler(IBotUnitOfWork botUnitOfWork) : AutocompleteHandler {
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
        if (await botUnitOfWork.Addons.GetItemAsync(m => m.GuildId == context.Guild.Id && m.Addon == Shared.Enum.Addons.Economy && m.Enabled) == null) {
            return AutocompletionResult.FromError(InteractionCommandError.Exception, "Economy is disabled on this server.");
        }
        if (ShopModule.Items.Count == 0) {
            return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "Could not load items.");
        }
        var results = ShopModule.Items.Select(item => new AutocompleteResult(item.Name, item.Id.ToString())).Take(25);
        return AutocompletionResult.FromSuccess(results);
    }
}
    
public class InvAutocompleteHandler(IBotUnitOfWork botUnitOfWork) : AutocompleteHandler {
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
        if (await botUnitOfWork.Addons.GetItemAsync(m => m.GuildId == context.Guild.Id && m.Addon == Shared.Enum.Addons.Economy && m.Enabled) == null) {
            return AutocompletionResult.FromError(InteractionCommandError.Exception, "Economy is disabled on this server.");
        }
        var items = await botUnitOfWork.Inventory.GetItemsAsync(m => m.GuildId == context.Guild.Id && m.UserId == context.User.Id);
        if (items.Count == 0) {
            return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "You do not have any items.");
        }
        if (ShopModule.Items.Count == 0) {
            return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "Could not load items.");
        }
        var results = items
            .Select(inv => ShopModule.Items.Find(x => x.Id == inv.ItemId))
            .Where(item => item != null)
            .Select(item => new AutocompleteResult(item!.Name, item.Id.ToString()))
            .Take(25);
        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}