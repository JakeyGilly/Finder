// using Discord;
// using Discord.Interactions;
// using Finder.Bot.Models.Data.Bot;
// using Finder.Bot.Repositories;
// namespace Finder.Bot.Modules.Addons; 
//
// public class EconomyModule : InteractionModuleBase<ShardedInteractionContext> {
//     private readonly IUnitOfWork _unitOfWork;
//     public EconomyModule(IUnitOfWork unitOfWork) {
//         _unitOfWork = unitOfWork;
//     }
//
//     [SlashCommand("balance", "Checks user's balance.", runMode: RunMode.Async)]
//     public async Task Balance(IUser? user = null) {
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
//         user ??= Context.User;
//         var economy = await _unitOfWork.Economy.FindAsync(Context.Guild.Id, user.Id) ?? new EconomyModel {
//             GuildId = (long)Context.Guild.Id,
//             UserId = (long)user.Id,
//             Money = 0,
//             Bank = 0
//         };
//         await RespondAsync(embed: new EmbedBuilder {
//             Title = $"{user.Username}\'s balance",
//             Fields = new List<EmbedFieldBuilder> {
//                 new()
//                 {
//                     Name = "Money",
//                     Value = economy.Money.ToString()
//                 },
//                 new()
//                 {
//                     Name = "Bank",
//                     Value = economy.Bank.ToString()
//                 }
//             }
//         }.Build());
//     }
//
//     [SlashCommand("deposit", "Deposits money into your bank.", runMode: RunMode.Async)]
//     public async Task Deposit(int amount) {
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
//         var economy = await _unitOfWork.Economy.FindAsync(Context.Guild.Id, Context.User.Id);
//         if (economy == null || economy.Money < amount) {
//             await RespondAsync("You don\'t have enough money.");
//             return;
//         }
//         await _unitOfWork.Economy.AddEconomyAsync(Context.Guild.Id, Context.User.Id, -amount, amount);
//         await _unitOfWork.SaveChangesAsync();
//         await RespondAsync(embed: new EmbedBuilder {
//             Title = "Deposit",
//             Fields = new List<EmbedFieldBuilder> {
//                 new()
//                 {
//                     Name = "You deposited",
//                     Value = amount.ToString()
//                 }
//             }
//         }.Build());
//     }
//
//     [SlashCommand("withdraw", "Withdraws money from your bank.", runMode: RunMode.Async)]
//     public async Task Withdraw(int amount) {
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
//         var economy = await _unitOfWork.Economy.FindAsync(Context.Guild.Id, Context.User.Id);
//         if (economy == null || economy.Bank < amount) {
//             await RespondAsync("You don\'t have enough money in your bank.");
//             return;
//         }
//         await _unitOfWork.Economy.SubtractEconomyAsync(Context.Guild.Id, Context.User.Id, amount, amount);
//         await _unitOfWork.SaveChangesAsync();
//         await RespondAsync(embed: new EmbedBuilder {
//             Title = "Withdraw",
//             Fields = new List<EmbedFieldBuilder> {
//                 new()
//                 {
//                     Name = "You withdrew",
//                     Value = amount.ToString()
//                 }
//             }
//         }.Build());
//     }
//
//     [SlashCommand("pay", "Pays money to another user.", runMode: RunMode.Async)]
//     public async Task Pay(IUser user, int amount) {
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
//         var economy = await _unitOfWork.Economy.FindAsync(Context.Guild.Id, Context.User.Id);
//         if (economy == null || economy.Money < amount) {
//             await RespondAsync("You don\'t have enough money.");
//             return;
//         }
//         await _unitOfWork.Economy.SubtractEconomyAsync(Context.Guild.Id, Context.User.Id, amount, 0);
//         await _unitOfWork.Economy.AddEconomyAsync(Context.Guild.Id, user.Id, amount, 0);
//         await _unitOfWork.SaveChangesAsync();
//         await RespondAsync(embed: new EmbedBuilder {
//             Title = "Pay",
//             Fields = new List<EmbedFieldBuilder> {
//                 new()
//                 {
//                     Name = "Payee",
//                     Value = user.Username
//                 },
//                 new()
//                 {
//                     Name = "Amount",
//                     Value = amount.ToString()
//                 }
//             }
//         }.Build());
//     }
//
//     [SlashCommand("transfer", "Transfers money to another user from your bank.", runMode: RunMode.Async)]
//     public async Task Transfer(IUser user, int amount) {
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
//         var economy = await _unitOfWork.Economy.FindAsync(Context.Guild.Id, Context.User.Id);
//         if (economy == null || economy.Bank < amount) {
//             await RespondAsync("You don\'t have enough money in your bank.");
//             return;
//         }
//         await _unitOfWork.Economy.SubtractEconomyAsync(Context.Guild.Id, Context.User.Id, 0, amount);
//         await _unitOfWork.Economy.AddEconomyAsync(Context.Guild.Id, user.Id, 0, amount);
//         await _unitOfWork.SaveChangesAsync();
//         await RespondAsync(embed: new EmbedBuilder {
//             Title = "Transfer",
//             Fields = new List<EmbedFieldBuilder> {
//                 new()
//                 {
//                     Name = "Payee",
//                     Value = user.Username
//                 },
//                 new()
//                 {
//                     Name = "Amount",
//                     Value = amount.ToString()
//                 }
//             }
//         }.Build());
//     }
//
//     [SlashCommand("addbalance", "Adds to the balance of a user.", runMode: RunMode.Async)]
//     public async Task AddBalance(IUser user, int amount) {
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
//         await _unitOfWork.Economy.AddEconomyAsync(Context.Guild.Id, user.Id, amount, 0);
//         await _unitOfWork.SaveChangesAsync();
//         await RespondAsync(embed: new EmbedBuilder {
//             Title = "Set Balance",
//             Fields = new List<EmbedFieldBuilder> {
//                 new()
//                 {
//                     Name = "User",
//                     Value = user.Username
//                 },
//                 new()
//                 {
//                     Name = "Amount",
//                     Value = amount.ToString()
//                 }
//             }
//         }.Build());
//     }
// }