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
    public async Task<IActionResult> Addons(string id, [FromForm] string ticTacToeAddon, [FromForm] string economyAddon, [FromForm] string levelingAddon, [FromForm] string ticketingAddon) {
        var guildId = ulong.Parse(id);
        unitOfWork.Addons.AddItem(new() {
            GuildId = guildId,
            Addon = Shared.Enum.Addons.TicTacToe,
            Enabled = ticTacToeAddon == "on"
        });
        unitOfWork.Addons.AddItem(new() {
            GuildId = guildId,
            Addon = Shared.Enum.Addons.Economy,
            Enabled = economyAddon == "on"
        });
        unitOfWork.Addons.AddItem(new() {
            GuildId = guildId,
            Addon = Shared.Enum.Addons.Levelling,
            Enabled = levelingAddon == "on"
        });
        unitOfWork.Addons.AddItem(new() {
            GuildId = guildId,
            Addon = Shared.Enum.Addons.Ticketing,
            Enabled = ticketingAddon == "on"
        });
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