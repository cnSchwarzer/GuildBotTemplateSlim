using GuildBotTemplate.Core;
using QQChannelFramework.Models.MessageModels;

namespace GuildBotTemplate.Utils;

public static class BotExtensions {
   public static bool IsMe(this Message msg) {
      return msg.Author.Id is App.MeId;
   }

   public static async Task ReplyDirectMessage(this Message msg, string text, byte[] image = null) {
      var b = GuildBot.Instance;
      await b.Api.GetDirectMessageApi()
         .SendMessageAsync(msg.GuildId, text, imageData: image, passiveMsgId: b.PassiveMessageId);
   }

   public static async Task ReplyGuildMessage(this Message msg, string text, byte[] image = null) {
      var b = GuildBot.Instance;
      await b.Api.GetMessageApi()
         .SendMessageAsync(msg.ChannelId, text, imageData: image, passiveEventId: b.PassiveEventId,
            passiveMsgId: b.PassiveMessageId);
   }

   public static async Task CreateReplyDirectMessage(this Message msg, string text, byte[] image = null) {
      var b = GuildBot.Instance;
      var dms = await b.Api.GetDirectMessageApi().CreateDirectMessageSessionAsync(msg.Author.Id, App.GuildId);
      await b.Api.GetDirectMessageApi()
         .SendMessageAsync(dms.GuildId, text, imageData: image, passiveMsgId: b.PassiveMessageId);
   }

   public static async Task CreateReplyDirectMessage(this string userId, string text, byte[] image = null) {
      var b = GuildBot.Instance;
      var dms = await b.Api.GetDirectMessageApi().CreateDirectMessageSessionAsync(userId, App.GuildId);
      await b.Api.GetDirectMessageApi()
         .SendMessageAsync(dms.GuildId, text, imageData: image, passiveMsgId: b.PassiveMessageId);
   }

   public static async Task<T> RetryAsync<T>(int retry, Func<Task<T>> func) {
      var exceptions = new List<Exception>();
      while (retry-- > 0) {
         try {
            return await func();
         } catch (Exception ex) {
            exceptions.Add(ex);
         }
      }
      throw new AggregateException(exceptions);
   }

   public static async Task<T> RetryAsync<T>(this Module module, int retry, Func<Task<T>> func) {
      return await RetryAsync(retry, func);
   }

   public static async Task RetryAsync(int retry, Func<Task> func) {
      var exceptions = new List<Exception>();
      while (retry-- > 0) {
         try {
            await func();
            return;
         } catch (Exception ex) {
            exceptions.Add(ex);
         }
      }
      throw new AggregateException(exceptions);
   }

   public static async Task RetryAsync(this Module module, int retry, Func<Task> func) {
      await RetryAsync(retry, func);
   }
}