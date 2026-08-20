using Discord;
using Discord.Interactions;
using Finder.Bot.Attributes;
using Finder.Db.UnitOfWork;

namespace Finder.Bot.Modules.Addons.Economy; 

[Group("economy", "Command For Managing Economy")]
[RequireAddon(Shared.Enum.Addons.Economy)]
public class EconomyModule(IBotUnitOfWork botUnitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    [SlashCommand("balance", "Checks user's balance.", runMode: RunMode.Async)]
    public async Task Balance(IUser? user = null) {
        user ??= Context.User;
        var economy = await botUnitOfWork.Economy.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.UserId == user.Id) ?? new() {
            GuildId = Context.Guild.Id,
            UserId = user.Id,
        };
        await RespondAsync(embed: new EmbedBuilder {
            Title = $"{user.Username}\'s balance",
            Fields = [
                new() {
                    Name = "Money",
                    Value = economy.Money.ToString()
                },
                new() {
                    Name = "Bank",
                    Value = economy.Bank.ToString()
                }
            ]
        }.Build());
    }

    [SlashCommand("deposit", "Deposits money into your bank.", runMode: RunMode.Async)]
    public async Task Deposit(int amount) {
        if (amount < 0) {
            await RespondAsync("You cannot deposit a negative amount.");
            return;
        }
        var economy = await botUnitOfWork.Economy.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id);
        if (economy == null || economy.Money < amount) {
            await RespondAsync("You don\'t have enough money.");
            return;
        }
        economy.Money -= amount;
        economy.Bank += amount;
        await botUnitOfWork.SaveChangesAsync();
        await RespondAsync(embed: new EmbedBuilder {
            Title = "Deposit",
            Fields = [
                new() {
                    Name = "You deposited",
                    Value = amount.ToString()
                }
            ]
        }.Build());
    }

    [SlashCommand("withdraw", "Withdraws money from your bank.", runMode: RunMode.Async)]
    public async Task Withdraw(int amount) {
        if (amount < 0) {
            await RespondAsync("You cannot withdraw a negative amount.");
            return;
        }
        var economy = await botUnitOfWork.Economy.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id);
        if (economy == null || economy.Bank < amount) {
            await RespondAsync("You don\'t have enough money in your bank.");
            return;
        }
        economy.Bank -= amount;
        economy.Money += amount;
        await botUnitOfWork.SaveChangesAsync();
        await RespondAsync(embed: new EmbedBuilder {
            Title = "Withdraw",
            Fields = [
                new() {
                    Name = "You withdrew",
                    Value = amount.ToString()
                }
            ]
        }.Build());
    }

    [SlashCommand("pay", "Pays money to another user.", runMode: RunMode.Async)]
    public async Task Pay(IUser user, int amount) {
        if (amount <= 0) {
            await RespondAsync("Amount must be greater than zero.");
            return;
        }
        if (user.Id == Context.User.Id) {
            await RespondAsync("You cannot pay yourself!");
            return;
        }
        if (user.IsBot) {
            await RespondAsync("You cannot pay a bot.");
            return;
        }
        var economy = await botUnitOfWork.Economy.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id);
        if (economy == null || economy.Money < amount) {
            await RespondAsync("You don\'t have enough money.");
            return;
        }
        economy.Money -= amount;
        var payeeEconomy = await botUnitOfWork.Economy.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.UserId == user.Id);
        if (payeeEconomy == null) {
            botUnitOfWork.Economy.AddItem(payeeEconomy = new() {
                GuildId = Context.Guild.Id,
                UserId = user.Id,
            });
        }
        payeeEconomy.Money += amount;
        await botUnitOfWork.SaveChangesAsync();
        await RespondAsync(embed: new EmbedBuilder {
            Title = "Pay",
            Fields = [
                new() {
                    Name = "Payee",
                    Value = user.Username
                },
                new() {
                    Name = "Amount",
                    Value = amount.ToString()
                }
            ]
        }.Build());
    }

    [SlashCommand("transfer", "Transfers money to another user from your bank.", runMode: RunMode.Async)]
    public async Task Transfer(IUser user, int amount) {
        if (amount <= 0) {
            await RespondAsync("Amount must be greater than zero.");
            return;
        }
        if (user.Id == Context.User.Id) {
            await RespondAsync("You cannot transfer to yourself!");
            return;
        }
        if (user.IsBot) {
            await RespondAsync("You cannot transfer to a bot.");
            return;
        }
        var economy = await botUnitOfWork.Economy.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.UserId == Context.User.Id);
        if (economy == null || economy.Bank < amount) {
            await RespondAsync("You don\'t have enough money in your bank.");
            return;
        }
        economy.Bank -= amount;
        var payeeEconomy = await botUnitOfWork.Economy.GetItemAsync((m) => m.GuildId == Context.Guild.Id && m.UserId == user.Id);
        if (payeeEconomy == null) {
            botUnitOfWork.Economy.AddItem(payeeEconomy = new() {
                GuildId = Context.Guild.Id,
                UserId = user.Id,
            });
        }
        payeeEconomy.Bank += amount;
        await botUnitOfWork.SaveChangesAsync();
        await RespondAsync(embed: new EmbedBuilder {
            Title = "Transfer",
            Fields = [
                new() {
                    Name = "Payee",
                    Value = user.Username
                },
                new() {
                    Name = "Amount",
                    Value = amount.ToString()
                }
            ]
        }.Build());
    }

    [SlashCommand("addbalance", "Adds to the balance of a user.", runMode: RunMode.Async)]
    public async Task AddBalance(IUser user, int amount) {
        if (amount <= 0) {
            await RespondAsync("Amount must be greater than zero.");
            return;
        }
        if (user.IsBot) {
            await RespondAsync("You cannot add balance to a bot.");
            return;
        }
        var economy = await botUnitOfWork.Economy.GetItemAsync(m => m.GuildId == Context.Guild.Id && m.UserId == user.Id);
        if (economy == null) {
            botUnitOfWork.Economy.AddItem(economy = new() {
                GuildId = Context.Guild.Id,
                UserId = user.Id,
            });
        }
        economy.Money += amount;
        await botUnitOfWork.SaveChangesAsync();
        await RespondAsync(embed: new EmbedBuilder {
            Title = "Set Balance",
            Fields = [
                new() {
                    Name = "User",
                    Value = user.Username
                },
                new() {
                    Name = "Amount",
                    Value = amount.ToString()
                }
            ]
        }.Build());
    }
}