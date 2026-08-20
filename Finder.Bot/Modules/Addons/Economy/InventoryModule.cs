using Discord;
using Discord.Interactions;
using Finder.Bot.Attributes;
using Finder.Db.UnitOfWork;

namespace Finder.Bot.Modules.Addons.Economy;

[Group("inventory", "The inventory commands to view your items.")]
[RequireAddon(Shared.Enum.Addons.Economy)]
public class InventoryModule(IBotUnitOfWork botUnitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("inventory", "View your inventory.")]
    public async Task InventoryCommand() {
        var items = await botUnitOfWork.Inventory.GetItemsAsync(m => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id);
        if (items.Count == 0) {
            await RespondAsync("You do not have any items.");
            return;
        }
        var embed = new EmbedBuilder {
            Title = "Your inventory"
        };
        foreach (var item in items) {
            var itemToBuy = ShopModule.Items.Find(x => x.Id == item.ItemId);
            if (itemToBuy == null) {
                Console.WriteLine($"Item with ID {item.ItemId} not found in items.json");
                continue;
            }
            var amount = items.Count(x => x == item);
            embed.AddField($"{itemToBuy.Name} x{amount}", itemToBuy.Description);
        }
        await RespondAsync(embed: embed.Build());
    }
}