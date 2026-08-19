using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using Finder.Bot.Db;
using Finder.Bot.Db.Repositories;

namespace Finder.Bot.Modules.Addons; 

public class TicTacToeModule : InteractionModuleBase<ShardedInteractionContext> {
    private readonly IUnitOfWork _unitOfWork;
    
    public TicTacToeModule(IUnitOfWork unitOfWork) {
        _unitOfWork = unitOfWork;
    }
    private static readonly List<TicTacToe> Games = new();
    private static readonly IEnumerable<string> ValidEmotes = new List<string> {
        "1️⃣",
        "2️⃣",
        "3️⃣",
        "4️⃣",
        "5️⃣",
        "6️⃣",
        "7️⃣",
        "8️⃣",
        "9️⃣",
        "✅",
        "❌"
    };

    [SlashCommand("tictactoe", "Play TicTacToe", runMode: RunMode.Async)]
    public async Task TicTacToeCommand(SocketGuildUser user) {
        if (!await _unitOfWork.Addons.AddonEnabled(Context.Guild.Id, "TicTacToe")) {
            await RespondAsync(embed: new EmbedBuilder {
                Title = "TicTacToe",
                Description = "This addon is disabled on this server.",
                Fields =
                [
                    new()
                    {
                        Name = "Enable",
                        Value = "Use `/addons install TicTacToe` to enable this addon."
                    }
                ]
            }.Build());
            return;
        }
        if (user.IsBot) {
            await RespondAsync("The user is a bot.");
            return;
        }
        if (user.Id == Context.User.Id) {
            await RespondAsync("You can\'t play TicTacToe with yourself.");
            return;
        }
        await RespondAsync($"{user.Mention} has been invited to play TicTacToe by {Context.User.Mention}!");
        IUser p1 = new Random().Next(0, 2) == 0 ? Context.User : user;
        IUser p2 = Context.User == p1 ? user : Context.User;
        var p1Symbol = new Random().Next(0, 2) == 0 ? "❌" : "⭕";
        var p2Symbol = p1Symbol == "❌" ? "⭕" : "❌";
        var game = new TicTacToe(Context.Guild, p1.Id, p2.Id, p1Symbol, p2Symbol);
        Games.Add(game);
        await game.StartAsync();
    }
    
    public static async Task OnReactionAddedEvent(Cacheable<IUserMessage, ulong> arg1, Cacheable<IMessageChannel, ulong> arg2, SocketReaction reaction) {
        if (reaction.User.Value.IsBot) return;
        if (ValidEmotes.All(symbol => reaction.Emote.Name != symbol)) return;
        for (var i = 0; i < Games.Count; i++) {
            var game = Games[i];
            if (game.playChannel == null || game.lobbyMessage == null) continue;
            if (game.playChannel.Id != reaction.Channel.Id) continue;
            if (game.lobby && reaction.MessageId == game.lobbyMessage.Id) {
                switch(reaction.Emote.Name) {
                    case "✅":
                        game.p1Ready = game.p1Id == reaction.UserId || game.p1Ready;
                        game.p2Ready = game.p2Id == reaction.UserId || game.p2Ready;
                        await game.playChannel.SendMessageAsync($"{reaction.User.Value.Mention} is ready!");
                        if (!game.p1Ready || !game.p2Ready) return;
                        game.lobby = false;
                        await game.playChannel.SendMessageAsync($"{game.guild.GetUser(game.p1Id).Mention} and {game.guild.GetUser(game.p2Id).Mention} are ready!\nStarting game...");
                        game.playMessage = await game.playChannel.SendMessageAsync(embed: new EmbedBuilder {
                            Title = "Tic Tac Toe",
                            Fields =
                            [
                                new()
                                {
                                    Name = "The Playing Board",
                                    Value = "Please Wait..."
                                }
                            ],
                            Footer = new EmbedFooterBuilder {
                                Text = "FinderBot"
                            }
                        }.Build());
                        var emojis = game.board.Select(item => new Emoji(item)).ToList();
                        await game.playMessage.AddReactionsAsync(emojis);
                        await game.playMessage.ModifyAsync(x => {
                            x.Embed = new EmbedBuilder {
                                Title = "Tic Tac Toe",
                                Fields = new List<EmbedFieldBuilder> {
                                    new()
                                    {
                                        Name = "The Playing Board",
                                        Value = game.GenerateGrid()
                                    }
                                },
                                Footer = new EmbedFooterBuilder {
                                    Text = "FinderBot"
                                }
                            }.Build();
                            x.Content = $"{game.guild.GetUser(game.p1Id).Mention} has been assigned the {game.p1Symbol} symbol.\n{game.guild.GetUser(game.p2Id).Mention} has been assigned the {game.p2Symbol} symbol.\n\n{game.guild.GetUser(game.p1Id).Mention}\'s Turn!";
                        });
                        return;
                    case "❌":
                        await game.playChannel.SendMessageAsync($"{reaction.User.Value.Mention} has cancelled game.");
                        Games.Remove(game);
                        return;
                }
            }
            if (game.win || game.playMessage == null || game.playMessage.Id != reaction.MessageId || (!game.p1Go || game.p1Id != reaction.UserId) && (game.p1Go || game.p2Id != reaction.UserId)) return;
            foreach (var slot in game.board.Where(slot => slot == reaction.Emote.Name)) {
                game.board[game.board.IndexOf(slot)] = game.p1Go ? game.p1Symbol : game.p2Symbol;
                break;
            }
            game.p1Go = !game.p1Go;
            await game.playMessage.ModifyAsync(x => {
                x.Embed = new EmbedBuilder {
                    Title = "Tic Tac Toe",
                    Fields =
                    [
                        new()
                        {
                            Name = "The Playing Board",
                            Value = game.GenerateGrid()
                        }
                    ],
                    Footer = new EmbedFooterBuilder {
                        Text = "FinderBot"
                    }
                }.Build();
                x.Content = $"{game.guild.GetUser(game.p1Id).Mention} has been assigned the {game.p1Symbol} symbol.\n{game.guild.GetUser(game.p2Id).Mention} has been assigned the {game.p2Symbol} symbol.\n\n{(game.p1Go ? game.guild.GetUser(game.p1Id).Mention : game.guild.GetUser(game.p2Id).Mention)}\'s Turn!";
            });
            await game.playMessage.RemoveAllReactionsForEmoteAsync(reaction.Emote);
            var winner = game.CheckWin();
            if (winner != null && winner == game.p1Id || winner != null && winner == game.p2Id) {
                await game.playMessage.ModifyAsync(x => {
                    x.Embed = new EmbedBuilder {
                        Title = "Tic Tac Toe",
                        Fields = [
                            new()
                            {
                                Name = "The Playing Board",
                                Value = game.GenerateGrid()
                            }
                        ],
                        Footer = new EmbedFooterBuilder {
                            Text = "FinderBot"
                        }
                    }.Build();
                });
                await game.playChannel.SendMessageAsync($"{game.guild.GetUser(winner.Value).Mention} has won the game!");
                EndGameCleanup(game);
            } else if (game.board.All(x => x is "⭕" or "❌")) {
                await game.playChannel.SendMessageAsync("The game has ended in a draw!");
                EndGameCleanup(game);
            }
            return;
        }
    }

    private class TicTacToe(SocketGuild guild, ulong p1Id, ulong p2Id, string p1Symbol, string p2Symbol) {
        public RestTextChannel? playChannel;
        public RestUserMessage? playMessage;
        public RestUserMessage? lobbyMessage;
        public readonly SocketGuild guild = guild;
        public readonly ulong p1Id = p1Id;
        public readonly ulong p2Id = p2Id;
        public readonly string p1Symbol = p1Symbol;
        public readonly string p2Symbol = p2Symbol;
        public bool p1Ready;
        public bool p2Ready;
        public bool win;
        public bool p1Go = true;
        public bool lobby = true;
        public readonly List<string> board = [
            "1️⃣", "2️⃣", "3️⃣",
            "4️⃣", "5️⃣", "6️⃣",
            "7️⃣", "8️⃣", "9️⃣"
        ];

        public async Task StartAsync() {
            playChannel = await guild.CreateTextChannelAsync("tictactoe", x => {
                x.Topic = "TicTacToe";
                x.PermissionOverwrites = new List<Overwrite> {
                    new(guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(viewChannel: PermValue.Deny)),
                    new(p1Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)),
                    new(p2Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow)),
                    new(guild.CurrentUser.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow))
                };
            });
            lobbyMessage =
                await playChannel.SendMessageAsync("Both players need to react with ✅ to start the game or ❌ to cancel.");
            await lobbyMessage.AddReactionsAsync([new Emoji("✅"), new Emoji("❌")]);
        }
            
        public string GenerateGrid() {
            string grid = "⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛\n⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛\n";
            for (int i = 0; i < 3; i++) {
                for (int j = 0; j < 3; j++) grid += $"⬛⬛⬛{board[i * 3 + j]}";
                grid += "⬛⬛⬛\n⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛\n⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛⬛\n";
            }
            return grid;
        }
            
        public ulong? CheckWin() {
            // Check Player 1
            if (board[0] == board[1] && board[1] == board[2] && board[2] == p1Symbol // 0, 1, 2
                || board[3] == board[4] && board[4] == board[5] && board[5] == p1Symbol // 3, 4, 5
                || board[6] == board[7] && board[7] == board[8] && board[8] == p1Symbol // 6, 7, 8
                || board[0] == board[3] && board[3] == board[6] && board[6] == p1Symbol // 0, 3, 6
                || board[1] == board[4] && board[4] == board[7] && board[7] == p1Symbol // 1, 4, 7
                || board[2] == board[5] && board[5] == board[8] && board[8] == p1Symbol // 2, 5, 8
                || board[0] == board[4] && board[4] == board[8] && board[8] == p1Symbol // 0, 4, 8
                || board[2] == board[4] && board[4] == board[6] && board[6] == p1Symbol // 2, 4, 6
            ) {return p1Id;}
            // Check Player 2
            if (board[0] == board[1] && board[1] == board[2] && board[2] == p2Symbol 
                || board[3] == board[4] && board[4] == board[5] && board[5] == p2Symbol 
                || board[6] == board[7] && board[7] == board[8] && board[8] == p2Symbol 
                || board[0] == board[3] && board[3] == board[6] && board[6] == p2Symbol 
                || board[1] == board[4] && board[4] == board[7] && board[7] == p2Symbol 
                || board[2] == board[5] && board[5] == board[8] && board[8] == p2Symbol 
                || board[0] == board[4] && board[4] == board[8] && board[8] == p2Symbol 
                || board[2] == board[4] && board[4] == board[6] && board[6] == p2Symbol
            ) {return p2Id;}
            return null;
        }
    }
    private static async void EndGameCleanup(TicTacToe game) {
        Games.Remove(game);
        await Task.Delay(15000); 
        if (game.playChannel != null) {
            await game.playChannel.DeleteAsync();
        }
    }
}