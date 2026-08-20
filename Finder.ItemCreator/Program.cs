using Finder.Shared.Enum;
using Finder.Shared.Models;
using Newtonsoft.Json;
using Spectre.Console;

namespace Finder.ItemCreator;

public static class Program {
    public static void Main(string[] args) {
        AnsiConsole.MarkupLine("[blue]Welcome to Finder ItemCreator[/]");
        var option = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[aqua]Select an option[/]")
                .PageSize(3)
                .AddChoices("Create new Item", "Exit")
        );
        switch (option) {
            case "Create new Item":
                CreateItem();
                break;
            case "Exit":
                AnsiConsole.MarkupLine("[blue]Exiting[/]");
                break;
        }
    }
    private static void CreateItem() {
        var name = AnsiConsole.Prompt(new TextPrompt<string>("[aqua]Enter the name of the item[/] [purple](string)[/]"));
        var description = AnsiConsole.Prompt(new TextPrompt<string>("[aqua]Enter the description of the item[/] [purple](string)[/]"));
        var buyPrice = AnsiConsole.Prompt(new TextPrompt<int>("[aqua]Enter the buy price of the item[/] [purple](integer)[/]"));
        var sellPrice = AnsiConsole.Prompt(new TextPrompt<int>("[aqua]Enter the sell price of the item[/] [purple](integer)[/]"));
        var buyableStr = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[aqua]Is the item buyable[/] [purple](boolean)[/]")
                .PageSize(3)
                .AddChoices("Yes", "No")
        );
        var buyable = buyableStr == "Yes";
        var sellableStr = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[aqua]Is the item sellable[/] [purple](boolean)[/]")
                .PageSize(3)
                .AddChoices("Yes", "No")
        );
        var sellable = sellableStr == "Yes";
        var tradableStr = AnsiConsole.Prompt(new SelectionPrompt<string>()
            .Title("[aqua]Is the item tradable[/] [purple](boolean)[/]")
            .PageSize(3)
            .MoreChoicesText("[green]More choices[/]")
            .AddChoices("Yes", "No"));
        var tradable = tradableStr == "Yes";
        var rarity = AnsiConsole.Prompt(new SelectionPrompt<ItemRarity>()
            .Title("[aqua]Select the rarity of the item[/] [purple](enum)[/]")
            .AddChoices(ItemRarity.Common, ItemRarity.Uncommon, ItemRarity.Rare, ItemRarity.Epic, ItemRarity.Legendary, ItemRarity.Mythic));

        Item item = new() {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description,
            BuyPrice = buyPrice,
            SellPrice = sellPrice,
            Buyable = buyable,
            Sellable = sellable,
            Tradeable = tradable,
            Rarity = rarity
        };
        AnsiConsole.MarkupLine("[blue]Item created![/]");
        var json = JsonConvert.SerializeObject(item);
        AnsiConsole.MarkupLine("[blue]Item JSON:[/]");
        AnsiConsole.MarkupLine(json);
    }
}