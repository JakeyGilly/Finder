using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Finder.Bot.Attributes;
using Finder.Bot.Db.Models;
using Finder.Bot.Db.Repositories;

namespace Finder.Bot.Modules.Addons; 

[RequireAddon(Enums.Addons.Ticketing)]
[Group("tickets", "Command For Managing Tickets")]
public class TicketingModule(IUnitOfWork unitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
    private ulong _closeConfirmId;

    [SlashCommand("create", "Creates a ticket", runMode: RunMode.Async)]
    public async Task CreateTicket(string name) {
        if (name.Length > 32) {
            await RespondAsync("The name of the ticket is too long.");
            return;
        }
        var supportChannel = await Context.Guild.CreateTextChannelAsync($"ticket-{name}", x => {
            x.PermissionOverwrites = new List<Overwrite> {
                new(Context.Guild.EveryoneRole.Id, PermissionTarget.Role, new OverwritePermissions(readMessageHistory: PermValue.Deny, sendMessages: PermValue.Deny, viewChannel: PermValue.Deny)),
                new(Context.User.Id, PermissionTarget.User, new OverwritePermissions(addReactions: PermValue.Allow, attachFiles: PermValue.Allow, embedLinks: PermValue.Allow, readMessageHistory: PermValue.Allow, sendMessages: PermValue.Allow, viewChannel: PermValue.Allow, useApplicationCommands: PermValue.Allow)),
                new(Context.Guild.CurrentUser.Id, PermissionTarget.User, new OverwritePermissions(viewChannel: PermValue.Allow))
            };
        });
        var message = await supportChannel.SendMessageAsync(embed: new EmbedBuilder {
            Title = "Ticket",
            Fields = [
                new() {
                    Name = name,
                    Value = $"Channel made by {Context.User.Username}"
                }
            ]
        }.Build(), components: new ComponentBuilderV2() { 
            Components = [
                new ButtonBuilder {
                    CustomId = "close",
                    Label = "Close Ticket"
                },
                new ButtonBuilder {
                    CustomId = "claim",
                    Label = "Claim Ticket"
                }
            ]
        }.Build());
        await unitOfWork.Ticketing.AddItemAsync(new TicketsModel() {
            ChannelId = supportChannel.Id,
            GuildId = Context.Guild.Id,
            IntroMessageId = message.Id,
            UserIds = [Context.User.Id],
            Name = name,
            ClaimedUserId = []
        });
        await RespondAsync(embed: new EmbedBuilder {
            Title = "Ticket Created",
            Fields = [
                new() {
                    Name = "Opened a new ticket:",
                    Value = supportChannel.Mention
                }
            ]
        }.Build());
    }

    [SlashCommand("close", "Closes a ticket", runMode: RunMode.Async)]
    public async Task CloseTicket() {
        TicketsModel? ticket = await unitOfWork.Ticketing.GetItemAsync((m) => m.ChannelId == Context.Channel.Id);
        if (ticket == null) {
            await RespondAsync("Ticket not found.", ephemeral: true);
            return;
        }
        if (await Context.Channel.GetMessageAsync(ticket.IntroMessageId) == null) {
            await RespondAsync("You are not in a ticket channel.", ephemeral: true);
            return;
        }
        if (!ticket.UserIds.Contains(Context.User.Id) || !ticket.ClaimedUserId.Contains(Context.User.Id)) {
            await RespondAsync("You are not the owner of this ticket.", ephemeral: true);
            return;
        }
        await RespondAsync("Ticket Closed");
        await ((SocketGuildChannel)Context.Channel).DeleteAsync();
        await unitOfWork.Ticketing.DeleteItemAsync((m) => m.ChannelId == ticket.ChannelId);
    }

    [SlashCommand("claim", "Claims a ticket", runMode: RunMode.Async)]
    public async Task ClaimTicket() {
        if (!((SocketGuildUser)Context.User).GuildPermissions.Administrator) {
            await RespondAsync("You do not have permission to claim a ticket.", ephemeral: true);
            return;
        }
        TicketsModel? ticket = await unitOfWork.Ticketing.GetItemAsync((m) => m.ChannelId == Context.Channel.Id);
        if (ticket == null) {
            await RespondAsync("Ticket not found.", ephemeral: true);
            return;
        }
        if (await Context.Channel.GetMessageAsync(ticket.IntroMessageId) == null) {
            await RespondAsync("You are not in a ticket channel.", ephemeral: true);
            return;
        }
        if (ticket.ClaimedUserId.Contains(Context.User.Id)) {
            await RespondAsync("You have already claimed this ticket.", ephemeral: true);
            return;
        }
        await ((SocketGuildChannel)Context.Channel).AddPermissionOverwriteAsync(Context.User, new OverwritePermissions(
            addReactions: PermValue.Allow,
            attachFiles: PermValue.Allow,
            embedLinks: PermValue.Allow,
            readMessageHistory: PermValue.Allow,
            sendMessages: PermValue.Allow,
            viewChannel: PermValue.Allow,
            useApplicationCommands: PermValue.Allow
        ));
        ticket.ClaimedUserId.Add(Context.User.Id);
        await unitOfWork.Ticketing.UpdateItemAsync((m) => m.ChannelId == ticket.ChannelId, ticket);
        await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
            Title = "Ticket Claimed",
            Fields = [
                new() {
                    Name = "Claimed By",
                    Value = Context.User.Username
                }
            ]
        }.Build());
        await RespondAsync("You have claimed this ticket.", ephemeral: true);
    }

    [SlashCommand("unclaim", "Unclaims a ticket", runMode: RunMode.Async)]
    public async Task UnclaimTicket() {
        TicketsModel? ticket = await unitOfWork.Ticketing.GetItemAsync((m) => m.ChannelId == Context.Channel.Id);
        if (ticket == null) {
            await RespondAsync("Ticket not found.", ephemeral: true);
            return;
        }
        if (await Context.Channel.GetMessageAsync(ticket.IntroMessageId) == null) {
            await RespondAsync("You are not in a ticket channel.", ephemeral: true);
            return;
        }
        if (!ticket.ClaimedUserId.Contains(Context.User.Id)) {
            await RespondAsync("You have not claimed this ticket.", ephemeral: true);
            return;
        }
        await ((SocketGuildChannel)Context.Channel).RemovePermissionOverwriteAsync(Context.User);
        ticket.ClaimedUserId.Remove(Context.User.Id);
        await unitOfWork.Ticketing.UpdateItemAsync((m) => m.ChannelId == ticket.ChannelId, ticket);
        await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
            Title = "Ticket Unclaimed",
            Fields = [
                new() {
                    Name = "User",
                    Value = Context.User.Username
                }
            ]
        }.Build());
        await RespondAsync("You have unclaimed this ticket.", ephemeral: true);
    }

    [SlashCommand("adduser", "Adds a user to a ticket", runMode: RunMode.Async)]
    public async Task AddUserToTicket(IUser user) {
        TicketsModel? ticket = await unitOfWork.Ticketing.GetItemAsync((m) => m.ChannelId == Context.Channel.Id);
        if (ticket == null) {
            await RespondAsync("Ticket not found.", ephemeral: true);
            return;
        }
        if (await Context.Channel.GetMessageAsync(ticket.IntroMessageId) == null) {
            await RespondAsync("You are not in a ticket channel.", ephemeral: true);
            return;
        }
        if (!(ticket.UserIds.Contains(Context.User.Id) || ticket.ClaimedUserId.Contains(Context.User.Id))) {
            await RespondAsync("You are not a member of this ticket.", ephemeral: true);
            return;
        }
        if (ticket.UserIds.Contains(user.Id) || ticket.ClaimedUserId.Contains(user.Id)) {
            await RespondAsync("This user is already a member of this ticket.", ephemeral: true);
            return;
        }
        ticket.UserIds.Add(Context.User.Id);
        await unitOfWork.Ticketing.UpdateItemAsync((m) => m.ChannelId == ticket.ChannelId, ticket);
        await ((SocketGuildChannel)Context.Channel).AddPermissionOverwriteAsync(user, new OverwritePermissions(
            addReactions: PermValue.Allow,
            attachFiles: PermValue.Allow,
            embedLinks: PermValue.Allow,
            readMessageHistory: PermValue.Allow,
            sendMessages: PermValue.Allow,
            viewChannel: PermValue.Allow,
            useApplicationCommands: PermValue.Allow
        ));
        await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
            Title = "User Added",
            Fields = [
                new() {
                    Name = "User",
                    Value = user.Username
                }
            ]
        }.Build());
        await RespondAsync("User added.", ephemeral: true);
    }

    [SlashCommand("removeuser", "Removes a user from a ticket", runMode: RunMode.Async)]
    public async Task RemoveUserFromTicket(IUser user) {
        TicketsModel? ticket = await unitOfWork.Ticketing.GetItemAsync((m) => m.ChannelId == Context.Channel.Id);
        if (ticket == null) {
            await RespondAsync("Ticket not found.", ephemeral: true);
            return;
        }
        if (await Context.Channel.GetMessageAsync(ticket.IntroMessageId) == null) {
            await RespondAsync("You are not in a ticket channel.", ephemeral: true);
            return;
        }
        if (!(ticket.UserIds.Contains(Context.User.Id) || ticket.ClaimedUserId.Contains(Context.User.Id))) {
            await RespondAsync("You are not a member of this ticket.", ephemeral: true);
            return;
        }
        if (!(ticket.UserIds.Contains(user.Id) || ticket.ClaimedUserId.Contains(user.Id))) {
            await RespondAsync("This user is not a member of this ticket.", ephemeral: true);
            return;
        }
        ticket.UserIds.Remove(user.Id);
        ticket.ClaimedUserId.Remove(user.Id);
        await unitOfWork.Ticketing.UpdateItemAsync((m) => m.ChannelId == ticket.ChannelId, ticket);
        await ((SocketGuildChannel)Context.Channel).RemovePermissionOverwriteAsync(user);
        await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
            Title = "User Removed",
            Fields = [
                new() {
                    Name = "User",
                    Value = user.Username
                }
            ]
        }.Build());
        await RespondAsync("User removed.", ephemeral: true);
    }

    [SlashCommand("leave", "Leaves a ticket", runMode: RunMode.Async)]
    public async Task LeaveTicket() {
        TicketsModel? ticket = await unitOfWork.Ticketing.GetItemAsync((m) => m.ChannelId == Context.Channel.Id);
        if (ticket == null) {
            await RespondAsync("Ticket not found.", ephemeral: true);
            return;
        }
        if (await Context.Channel.GetMessageAsync(ticket.IntroMessageId) == null) {
            await RespondAsync("You are not in a ticket channel.", ephemeral: true);
            return;
        }
        if (!(ticket.UserIds.Contains(Context.User.Id) || ticket.ClaimedUserId.Contains(Context.User.Id))) {
            await RespondAsync("You are not a member of this ticket.", ephemeral: true);
            return;
        }
        ticket.UserIds.Remove(Context.User.Id);
        ticket.ClaimedUserId.Remove(Context.User.Id);
        await unitOfWork.Ticketing.UpdateItemAsync((m) => m.ChannelId == ticket.ChannelId, ticket);
        await Context.Channel.SendMessageAsync(embed: new EmbedBuilder {
            Title = "User Removed",
            Fields = [
                new() {
                    Name = "User",
                    Value = Context.User.Username
                }
            ],
            Color = Color.Green
        }.Build());
        await RespondAsync("User removed.", ephemeral: true);
    }

    public async Task OnButtonExecutedEvent(SocketMessageComponent messageComponent) {
        if (!await unitOfWork.Addons.AddonEnabledInGuildAsync(Context.Guild.Id, Enums.Addons.Ticketing)) {
            return;
        }
        TicketsModel? ticket = await unitOfWork.Ticketing.GetItemAsync((m) => m.ChannelId == ((SocketGuildChannel)messageComponent.Message.Channel).Id);
        if (ticket == null) {
            await messageComponent.RespondAsync("Ticket not found.", ephemeral: true);
            return;
        }
        if (messageComponent.Message.Id == _closeConfirmId) {
            switch(messageComponent.Data.CustomId) {
                case "close-yes":
                    await messageComponent.RespondAsync("Ticket Closed");
                    await ((SocketGuildChannel)messageComponent.Message.Channel).DeleteAsync();
                    await unitOfWork.Ticketing.DeleteItemAsync((m) => m.ChannelId == ticket.ChannelId);
                    break;
                case "close-no":
                    await messageComponent.RespondAsync("You have cancelled closing this ticket.");
                    break;
            }
        } else if (messageComponent.Message.Id == ticket.IntroMessageId) {
            switch(messageComponent.Data.CustomId) {
                case "close":
                    await messageComponent.RespondAsync(embed: new EmbedBuilder {
                        Title = "Are you sure?",
                        Fields = [
                            new() {
                                Name = "Close Ticket",
                                Value = "This will close the ticket and delete the channel."
                            }
                        ],
                        Color = Color.Red
                    }.Build(), components: new ComponentBuilder()
                        .WithButton("Yes", "close-yes")
                        .WithButton("No", "close-no")
                        .Build());
                    _closeConfirmId = (await messageComponent.GetOriginalResponseAsync()).Id;
                    return;
                case "claim" when !((SocketGuildUser)messageComponent.User).GuildPermissions.Administrator:
                    await messageComponent.RespondAsync("You do not have permission to claim a ticket.", ephemeral: true);
                    return;
                case "claim":
                    if (ticket.ClaimedUserId.Contains(((SocketGuildUser)messageComponent.User).Id)) {
                        await messageComponent.RespondAsync("You have already claimed this ticket.", ephemeral: true);
                        return;
                    }
                    await ((SocketGuildChannel)messageComponent.Message.Channel).AddPermissionOverwriteAsync((SocketGuildUser)messageComponent.User, new OverwritePermissions(
                        addReactions: PermValue.Allow,
                        attachFiles: PermValue.Allow,
                        embedLinks: PermValue.Allow,
                        readMessageHistory: PermValue.Allow,
                        sendMessages: PermValue.Allow,
                        viewChannel: PermValue.Allow,
                        useApplicationCommands: PermValue.Allow
                    ));
                    ticket.ClaimedUserId.Add(((SocketGuildUser)messageComponent.User).Id);
                    await unitOfWork.Ticketing.UpdateItemAsync((m) => m.ChannelId == ticket.ChannelId, ticket);
                    await messageComponent.Message.Channel.SendMessageAsync(embed: new EmbedBuilder {
                        Title = "Ticket Claimed",
                        Fields = [
                            new() {
                                Name = "Claimed By",
                                Value = ((SocketGuildUser)messageComponent.User).Username
                            }
                        ],
                        Color = Color.Green
                    }.Build());
                    await messageComponent.RespondAsync("You have claimed this ticket.", ephemeral: true);
                    break;
                default:
                    await messageComponent.RespondAsync("You are not in a ticket channel.", ephemeral: true);
                    return;
            }
        }
    }
}