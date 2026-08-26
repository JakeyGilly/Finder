using Discord;
using Finder.Web.Models.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Finder.Db.UnitOfWork;
using Finder.Web.Services;

namespace Finder.Web.Controllers;

[Authorize]
[Route("dashboard")]
public class DashboardController(
    IWebUnitOfWork unitOfWork,
    IDiscordApiService discordApiService
) : Controller {
    [Route("")]
    public async Task<IActionResult> Index() {
        return View("Index", new DashboardSelectorDTO {
            BotGuilds = await discordApiService.ExecuteAsBotAsync(async client => {
                var summaries = await client.GetGuildSummariesAsync().FlattenAsync();
                return summaries.ToList();
            }),
            UserGuilds = await discordApiService.ExecuteAsUserAsync(async client => {
                var summaries = await client.GetGuildSummariesAsync().FlattenAsync();
                return summaries.ToList();
            }),
            UserProfile = await discordApiService.ExecuteAsUserAsync(client => client.GetCurrentUserAsync())
        });
    }


    [Route("{id}")]
    public async Task<IActionResult> Guild(string id) {
        if (!ulong.TryParse(id, out var guildId)) {
            return BadRequest("Invalid Guild ID format.");
        }
        var dashboardData = await discordApiService.ExecuteAsBotAsync(async client => {
            var guild = await client.GetGuildAsync(guildId, withCounts: true);
            if (guild == null) return null;

            var members = await guild.GetUsersAsync().FlattenAsync();
            var channels = await guild.GetChannelsAsync();
            return new GuildDashboardDTO {
                Guild = guild,
                GuildMembers = [.. members],
                GuildChannels = [.. channels]
            };
        });
        if (dashboardData == null) {
            return NotFound("Guild not found.");
        }
        return View("Dashboard", dashboardData);
    }
    
    [HttpPost("{id}/addons")]
    public async Task<IActionResult> Addons(string id, [FromForm] bool ticTacToeAddon, [FromForm] bool economyAddon, [FromForm] bool levellingAddon, [FromForm] bool ticketingAddon) {
        ulong.TryParse(id, out var guildId);
        var currentTicTacToeAddon = await unitOfWork.Addons.GetItemAsync(m => m.GuildId == guildId && m.Addon == Shared.Enum.Addons.TicTacToe);
        var currentEconomyAddon = await unitOfWork.Addons.GetItemAsync(m => m.GuildId == guildId && m.Addon == Shared.Enum.Addons.Economy);
        var currentLevelingAddon = await unitOfWork.Addons.GetItemAsync(m => m.GuildId == guildId && m.Addon == Shared.Enum.Addons.Levelling);
        var currentTicketingAddon = await unitOfWork.Addons.GetItemAsync(m => m.GuildId == guildId && m.Addon == Shared.Enum.Addons.Ticketing);
        
        if (currentTicTacToeAddon == null) {
            unitOfWork.Addons.AddItem(currentTicTacToeAddon = new() {
                GuildId = guildId,
                Addon = Shared.Enum.Addons.TicTacToe,
            });
        }
        if (currentEconomyAddon == null) {
            unitOfWork.Addons.AddItem(currentEconomyAddon = new() {
                GuildId = guildId,
                Addon = Shared.Enum.Addons.Economy,
            });
        }
        if (currentLevelingAddon == null) {
            unitOfWork.Addons.AddItem(currentLevelingAddon = new() {
                GuildId = guildId,
                Addon = Shared.Enum.Addons.Levelling,
            });
        }
        if (currentTicketingAddon == null) {
            unitOfWork.Addons.AddItem(currentTicketingAddon = new() {
                GuildId = guildId,
                Addon = Shared.Enum.Addons.Ticketing,
            });
        }
        currentTicTacToeAddon.Enabled = ticTacToeAddon;
        currentEconomyAddon.Enabled = economyAddon;
        currentLevelingAddon.Enabled = levellingAddon;
        currentTicketingAddon.Enabled = ticketingAddon;
        await unitOfWork.SaveChangesAsync();
        return RedirectToAction("Guild", new { id });
    }
    
    [HttpPost("{id}/message")]
    public async Task<IActionResult> Message(string id, [FromForm] ulong channelId, [FromForm] string message) {
        await discordApiService.ExecuteAsBotAsync(async client => {
            var channel = await client.GetChannelAsync(channelId);
            if (channel is not ITextChannel textChannel) {
                throw new Exception("Channel is not a text channel.");
            }
            return await textChannel.SendMessageAsync(message);
        });
        return RedirectToAction("Guild", new { id });
    }
}