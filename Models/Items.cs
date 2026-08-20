using Finder.Bot.Enums;

namespace Finder.Bot.Models;

public class Item {
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool Tradeable { get; set; }
    public bool Buyable { get; set; }
    public bool Sellable { get; set; }
    public ItemRarity Rarity { get; set; }
    public int BuyPrice { get; set; }
    public int SellPrice { get; set; }
}