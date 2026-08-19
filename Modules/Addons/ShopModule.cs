// using Discord;
// using Discord.Interactions;
// using Finder.Bot.Models;
// using Finder.Bot.Models.Data.Bot;
// using Finder.Bot.Repositories;
// using Newtonsoft.Json;
//
// namespace Finder.Bot.Modules.Addons; 
//
// [Group("shop", "The shop commands to buy items.")]
// public class ShopModule : InteractionModuleBase<ShardedInteractionContext> {
//     private readonly IUnitOfWork _unitOfWork;
//     public static ItemsRoot? itemsroot = JsonConvert.DeserializeObject<ItemsRoot>(File.ReadAllText(@"items.json"));
//     public ShopModule(IUnitOfWork unitOfWork) {
//         _unitOfWork = unitOfWork;
//     }
//     [SlashCommand("buy", "Buy an item from the shop.")]
//     public async Task BuyCommand([Autocomplete(typeof(ShopAutocompleteHandler))] string item, int amount = 1) {
//         if (!await _unitOfWork.Addons.AddonEnabled(Context.Guild.Id, "Economy")) {
//             await RespondAsync(embed: new EmbedBuilder {
//                 Title = "Economy",
//                 Description = "This addon is disabled on this server.",
//                 Fields = new List<EmbedFieldBuilder> {
//                     new()
//                     {
//                         Name = "Enable",
//                         Value = "Use `/addons install Economy` to enable this addon."
//                     }
//                 }
//             }.Build());
//             return;
//         }
//         Guid itemId = Guid.Parse(item);
//         if (itemsroot == null) {
//             await ReplyAsync("Could not load items.");
//             return;
//         }
//         var itemToBuy = itemsroot.Items.Find(x => x.Id == itemId);
//         if (!itemToBuy.Buyable) {
//             await RespondAsync("This item is not buyable.");
//             return;
//         }
//         var economy = await _unitOfWork.Economy.FindAsync(Context.Guild.Id, Context.User.Id) ?? new EconomyModel {
//             GuildId = (long)Context.Guild.Id,
//             UserId = (long)Context.User.Id,
//             Money = 0,
//             Bank = 0
//         };
//         if (economy.Money < itemToBuy.BuyPrice * amount) {
//             await RespondAsync("You do not have enough money to buy this item.");
//             return;
//         }
//         await _unitOfWork.Economy.SubtractEconomyAsync(Context.Guild.Id, Context.User.Id, itemToBuy.BuyPrice * amount, 0);
//         await _unitOfWork.ItemInv.AddItemAsync(Context.Guild.Id, Context.User.Id, itemId, amount);
//         await _unitOfWork.SaveChangesAsync();
//         string amountstr = amount == 0 ? amount.ToString() : "an";
//         await RespondAsync(embed: new EmbedBuilder {
//             Title = $"You have purchased {amountstr} item!",
//             Fields = new List<EmbedFieldBuilder> {
//                 new()
//                 {
//                     Name = itemToBuy.Name,
//                     Value = $"For {itemToBuy.BuyPrice * amount}",
//                 }
//             }
//         }.Build());
//     }
//         
//     [SlashCommand("sell", "Sell an item to the shop.")]
//     public async Task SellCommand([Autocomplete(typeof(InvAutocompleteHandler))] string item, int amount = 1) {
//         if (!await _unitOfWork.Addons.AddonEnabled(Context.Guild.Id, "Economy")) {
//             await RespondAsync(embed: new EmbedBuilder {
//                 Title = "Economy",
//                 Description = "This addon is disabled on this server.",
//                 Fields = new List<EmbedFieldBuilder> {
//                     new()
//                     {
//                         Name = "Enable",
//                         Value = "Use `/addons install Economy` to enable this addon."
//                     }
//                 }
//             }.Build());
//             return;
//         }
//         Guid itemId = Guid.Parse(item);
//         if (itemsroot == null) {
//             await ReplyAsync("Could not load items.");
//             return;
//         }
//         var itemToSell = itemsroot.Items.Find(x => x.Id == itemId);
//         if (!itemToSell.Sellable) {
//             await RespondAsync("This item is not sellable.");
//             return;
//         }
//         if (!await _unitOfWork.ItemInv.ItemExistsAsync(Context.Guild.Id, Context.User.Id, itemId)) {
//             await RespondAsync("You do not have this item.");
//             return;
//         }
//         await _unitOfWork.Economy.AddEconomyAsync(Context.Guild.Id, Context.User.Id, itemToSell.SellPrice * amount, 0);
//         await _unitOfWork.ItemInv.RemoveItemAsync(Context.Guild.Id, Context.User.Id, itemId, amount);
//         await _unitOfWork.SaveChangesAsync();
//         string amountstr = amount == 0 ? amount.ToString() : "an";
//         await RespondAsync(embed: new EmbedBuilder {
//             Title = $"You have sold {amountstr} item!",
//             Fields = new List<EmbedFieldBuilder> {
//                 new()
//                 {
//                     Name = itemToSell.Name,
//                     Value = $"For {itemToSell.SellPrice * amount}",
//                 }
//             }
//         }.Build());
//     }
//
//     [SlashCommand("info", "Displays item info in the shop")]
//     public async Task InfoCommand([Autocomplete(typeof(ShopAutocompleteHandler))] string itemStr) {
//         if (!await _unitOfWork.Addons.AddonEnabled(Context.Guild.Id, "Economy")) {
//             await RespondAsync(embed: new EmbedBuilder {
//                 Title = "Economy",
//                 Description = "This addon is disabled on this server.",
//                 Fields = new List<EmbedFieldBuilder> {
//                     new()
//                     {
//                         Name = "Enable",
//                         Value = "Use `/addons install Economy` to enable this addon."
//                     }
//                 }
//             }.Build());
//             return;
//         }
//         Guid itemId = Guid.Parse(itemStr);
//         if (itemsroot == null) {
//             await ReplyAsync("Could not load items.");
//             return;
//         }
//         var item = itemsroot.Items.Find(x => x.Id == itemId);
//         await RespondAsync(embed: new EmbedBuilder {
//             Title = $"{item.Name} infomation",
//             Fields = new List<EmbedFieldBuilder> {
//                 new()
//                 {
//                     Name = "Description",
//                     Value = item.Description,
//                     IsInline = false
//                 },
//                 new()
//                 {
//                     Name = "Rarity",
//                     Value = item.Rarity.ToString(),
//                     IsInline = false
//                 },
//                 new()
//                 {
//                     Name = "Buy Price",
//                     Value = item.Buyable ? item.BuyPrice : "Unbuyable",
//                     IsInline = true
//                 },
//                 new()
//                 {
//                     Name = "Sell Price",
//                     Value = item.Sellable ? item.SellPrice : "Unsellable",
//                     IsInline = true
//                 },
//                 new()
//                 {
//                     Name = "Tradeable",
//                     Value = item.Tradeable ? "Yes" : "No",
//                     IsInline = true
//                 },
//             }
//         }.Build());
//     }
// }
//
// public class InventoryModule : InteractionModuleBase<ShardedInteractionContext> {
//     private readonly IUnitOfWork _unitOfWork;
//     public InventoryModule(IUnitOfWork unitOfWork) {
//         _unitOfWork = unitOfWork;
//     }
//     [SlashCommand("inventory", "View your inventory.")]
//     public async Task InventoryCommand() {
//         if (!await _unitOfWork.Addons.AddonEnabled(Context.Guild.Id, "Economy")) {
//             await RespondAsync(embed: new EmbedBuilder {
//                 Title = "Economy",
//                 Description = "This addon is disabled on this server.",
//                 Fields = new List<EmbedFieldBuilder> {
//                     new()
//                     {
//                         Name = "Enable",
//                         Value = "Use `/addons install Economy` to enable this addon."
//                     }
//                 }
//             }.Build());
//             return;
//         }
//         var items = await _unitOfWork.ItemInv.FindAsync((long)Context.Guild.Id, (long)Context.User.Id);
//         if (items == null || items.ItemIds.Count == 0) {
//             await RespondAsync("You do not have any items.");
//             return;
//         }
//         var embed = new EmbedBuilder {
//             Title = "Your inventory"
//         };
//         foreach (var item in items.ItemIds) {
//             var itemToBuy = ShopModule.itemsroot.Items.Find(x => x.Id == item);
//             var amount = items.ItemIds.Count(x => x == item);
//             if (embed.Fields.Find(x => x.Name.Substring(0, itemToBuy.Name.Length) == itemToBuy.Name) == null) {
//                 embed.AddField($"{itemToBuy.Name} x{amount}", itemToBuy.Description);
//             }
//         }
//         await RespondAsync(embed: embed.Build());
//     }
// }
// public class ShopAutocompleteHandler : AutocompleteHandler {
//     private readonly IUnitOfWork _unitOfWork;
//     public ShopAutocompleteHandler(IUnitOfWork unitOfWork) {
//         _unitOfWork = unitOfWork;
//     }
//     public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
//         if (!await _unitOfWork.Addons.AddonEnabled(context.Guild.Id, "Economy")) {
//             return AutocompletionResult.FromError(InteractionCommandError.Exception, "Economy is disabled on this server.");
//         }
//         IEnumerable<AutocompleteResult> results = new List<AutocompleteResult>();
//         if (ShopModule.itemsroot == null) {
//             return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "Could not load items.");
//         }
//         results = ShopModule.itemsroot.Items.Aggregate(results, (current, i) => current.Append(new AutocompleteResult(i.Name, i.Id.ToString())));
//         return AutocompletionResult.FromSuccess(results.Take(25));
//     }
// }
//     
// public class InvAutocompleteHandler : AutocompleteHandler {
//     private readonly IUnitOfWork _unitOfWork;
//     public InvAutocompleteHandler(IUnitOfWork unitOfWork) {
//         _unitOfWork = unitOfWork;
//     }
//     public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services) {
//         if (!await _unitOfWork.Addons.AddonEnabled(context.Guild.Id, "Economy")) {
//             return AutocompletionResult.FromError(InteractionCommandError.Exception, "Economy is disabled on this server.");
//         }
//         IEnumerable<AutocompleteResult> results = new List<AutocompleteResult>();
//         var items = await _unitOfWork.ItemInv.FindAsync((long)context.Guild.Id, (long)context.User.Id);
//         if (items == null || items.ItemIds.Count == 0) {
//             return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "You do not have any items.");
//         }
//         if (ShopModule.itemsroot == null) {
//             return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "Could not load items.");
//         }
//         foreach (var itemId in items.ItemIds) {
//             var item = ShopModule.itemsroot.Items.Find(x => x.Id == itemId);
//             if (item == null) continue;
//             // if item is already in the list, skip it
//             if (results.Any(x => x.Value.Equals(item.Id.ToString()))) continue;
//             results = results.Append(new AutocompleteResult(item.Name, item.Id.ToString()));
//         }
//         return AutocompletionResult.FromSuccess(results.Take(25));
//     }
// }