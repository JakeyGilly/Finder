// using Discord;
// using Discord.Interactions;
// using Discord.WebSocket;
// using Finder.Bot.Db.Repositories;
//
// namespace Finder.Bot.Modules; 
//
// public class PollModule(IUnitOfWork unitOfWork) : InteractionModuleBase<ShardedInteractionContext> {
//     [SlashCommand("poll", "Create a poll for users to vote on.", runMode: RunMode.Async)]
//     public async Task PollCommand(string question, string? answer1 = null, string? answer2 = null, string? answer3 = null, string? answer4 = null, string? answer5 = null, string? answer6 = null, string? answer7 = null, string? answer8 = null, string? answer9 = null, string? answer10 = null,
//     string? answer11 = null, string? answer12 = null, string? answer13 = null, string? answer14 = null, string? answer15 = null, string? answer16 = null, string? answer17 = null, string? answer18 = null, string? answer19 = null, string? answer20 = null, string? answer21 = null, string? answer22 = null, 
//     string? answer23 = null, string? answer24 = null) {
//         ComponentBuilder builder = new ComponentBuilder();
//         var embed = new EmbedBuilder {
//             Title = question,
//             Description = $"Poll created by {Context.User.Username}",
//             Footer = new EmbedFooterBuilder {
//                 Text = "FinderBot"
//             }
//         };
//         List<string> answers = new List<string>();
//         if (answer1 == null && answer2 == null && answer3 == null && answer4 == null && answer5 == null && answer6 == null && answer7 == null && answer8 == null && answer9 == null && answer10 == null && answer11 == null && answer12 == null && answer13 == null && answer14 == null && answer15 == null && answer16 == null && answer17 == null && answer18 == null && answer19 == null && answer20 == null && answer21 == null && answer22 == null && answer23 == null && answer24 == null) {
//             embed.AddField("Yes", 0, true);
//             builder.WithButton("Yes", "0");
//             answers.Add("Yes");
//             embed.AddField("No", 0, true);
//             builder.WithButton("No", "1");
//             answers.Add("No");
//         }
//         if (answer1 != null) {
//             embed.AddField(answer1, 0, true);
//             builder.WithButton(answer1, "0");
//             answers.Add(answer1);
//         }
//         if (answer2 != null) {
//             embed.AddField(answer2, 0, true);
//             builder.WithButton(answer2, "1");
//             answers.Add(answer2);
//         }
//         if (answer3 != null) {
//             embed.AddField(answer3, 0, true);
//             builder.WithButton(answer3, "2");
//             answers.Add(answer3);
//         }
//         if (answer4 != null) {
//             embed.AddField(answer4, 0, true);
//             builder.WithButton(answer4, "3");
//             answers.Add(answer4);
//         }
//         if (answer5 != null) {
//             embed.AddField(answer5, 0, true);
//             builder.WithButton(answer5, "4");
//             answers.Add(answer5);
//         }
//         if (answer6 != null) {
//             embed.AddField(answer6, 0, true);
//             builder.WithButton(answer6, "5");
//             answers.Add(answer6);
//         }
//         if (answer7 != null) {
//             embed.AddField(answer7, 0, true);
//             builder.WithButton(answer7, "6");
//             answers.Add(answer7);
//         }
//         if (answer8 != null) {
//             embed.AddField(answer8, 0, true);
//             builder.WithButton(answer8, "7");
//             answers.Add(answer8);
//         }
//         if (answer9 != null) {
//             embed.AddField(answer9, 0, true);
//             builder.WithButton(answer9, "8");
//             answers.Add(answer9);
//         }
//         if (answer10 != null) {
//             embed.AddField(answer10, 0, true);
//             builder.WithButton(answer10, "9");
//             answers.Add(answer10);
//         }
//         if (answer11 != null) {
//             embed.AddField(answer11, 0, true);
//             builder.WithButton(answer11, "10");
//             answers.Add(answer11);
//         }
//         if (answer12 != null) {
//             embed.AddField(answer12, 0, true);
//             builder.WithButton(answer12, "11");
//             answers.Add(answer12);
//         }
//         if (answer13 != null) {
//             embed.AddField(answer13, 0, true);
//             builder.WithButton(answer13, "12");
//             answers.Add(answer13);
//         }
//         if (answer14 != null) {
//             embed.AddField(answer14, 0, true);
//             builder.WithButton(answer14, "13");
//             answers.Add(answer14);
//         }
//         if (answer15 != null) {
//             embed.AddField(answer15, 0, true);
//             builder.WithButton(answer15, "14");
//             answers.Add(answer15);
//         }
//         if (answer16 != null) {
//             embed.AddField(answer16, 0, true);
//             builder.WithButton(answer16, "15");
//             answers.Add(answer16);
//         }
//         if (answer17 != null) {
//             embed.AddField(answer17, 0, true);
//             builder.WithButton(answer17, "16");
//             answers.Add(answer17);
//         }
//         if (answer18 != null) {
//             embed.AddField(answer18, 0, true);
//             builder.WithButton(answer18, "17");
//             answers.Add(answer18);
//         }
//         if (answer19 != null) {
//             embed.AddField(answer19, 0, true);
//             builder.WithButton(answer19, "18");
//             answers.Add(answer19);
//         }
//         if (answer20 != null) {
//             embed.AddField(answer20, 0, true);
//             builder.WithButton(answer20, "19");
//             answers.Add(answer20);
//         }
//         if (answer21 != null) {
//             embed.AddField(answer21, 0, true);
//             builder.WithButton(answer21, "20");
//             answers.Add(answer21);
//         }
//         if (answer22 != null) {
//             embed.AddField(answer22, 0, true);
//             builder.WithButton(answer22, "21");
//             answers.Add(answer22);
//         }
//         if (answer23 != null) {
//             embed.AddField(answer23, 0, true);
//             builder.WithButton(answer23, "22");
//             answers.Add(answer23);
//         }
//         if (answer24 != null) {
//             embed.AddField(answer24, 0, true);
//             builder.WithButton(answer24, "23");
//             answers.Add(answer24);
//         }
//         await RespondAsync("", embed: embed.Build(), components: builder.Build());
//         await _unitOfWork.Polls.AddPollsAsync((await GetOriginalResponseAsync()).Id, answers, new List<long>());
//     }
//         
//     public async Task OnButtonExecutedEvent(SocketMessageComponent messageComponent) {
//         if (!await _unitOfWork.Polls.PollExistsAsync(messageComponent.Message.Id)) return;
//         var poll = await _unitOfWork.Polls.GetAsync(messageComponent.Message.Id);
//         for (int i = 0; i < poll.Answers.Count; i++) {
//             if (messageComponent.Data.CustomId != i.ToString()) continue;
//             if (!poll.VotersId.Contains((Int64)messageComponent.User.Id)) {
//                 var message = messageComponent.Message;
//                 var embed = message.Embeds.First();
//                 var fields = embed.Fields;
//                 var newFields = new List<EmbedFieldBuilder>();
//                 for (int j = 0; j < fields.Count(); j++) {
//                     if (j == i) {
//                         newFields.Add(new EmbedFieldBuilder {
//                             Name = fields[j].Name,
//                             Value = int.Parse(fields[j].Value) + 1,
//                             IsInline = fields[j].Inline,
//                         });
//                     } else {
//                         newFields.Add(new EmbedFieldBuilder {
//                             Name = fields[j].Name,
//                             Value = fields[j].Value,
//                             IsInline = fields[j].Inline,
//                         });
//                     }
//                 }
//                 await messageComponent.Message.ModifyAsync(x => {
//                     x.Embed = new EmbedBuilder {
//                         Title = embed.Title,
//                         Description = embed.Description,
//                         Footer = new EmbedFooterBuilder().WithText("FinderBot"),
//                         Fields = newFields
//                     }.Build();
//                 });
//                 await messageComponent.RespondAsync($"You voted for {poll.Answers[i]}", ephemeral: true);
//                 poll.VotersId.Add((Int64)messageComponent.User.Id);
//             } else {
//                 await messageComponent.RespondAsync("You already voted on this poll", ephemeral: true);
//             }   
//         }
//     }
// }