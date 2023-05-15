using Microsoft.Extensions.Logging;
using QQChannelFramework.Api;
using QQChannelFramework.Models;
using QQChannelFramework.Models.MessageModels;
using QQChannelFramework.Models.WsModels;

namespace GuildBotTemplate.Core; 

public abstract class Module {
   private static readonly Dictionary<string, Module> Instances = new();

   public T GetModule<T>() where T : Module {
      return Instances[typeof(T).Name] as T ?? throw new InvalidOperationException();
   }

   /// <summary>
   /// Get current bot user id
   /// </summary>
   protected string botId => bot.BotId;

   /// <summary>
   /// Get a valid passive msg id
   /// </summary>
   protected string pMsgId => bot.PassiveMessageId;

   /// <summary>
   /// Get a valid passive event id
   /// </summary>
   protected string pEventId => bot.PassiveEventId;

   /// <summary>
   /// Get logger for this module
   /// </summary>
   protected readonly ILogger log;

   /// <summary>
   /// Get guild bot API instance
   /// </summary>
   protected QQChannelApi api => bot.Api;

   internal GuildBot bot { get; set; }

   protected Module() {
      log = App.LogFactory.CreateLogger(GetType().Name!);
      Instances[GetType().Name] = this;
   }

   public virtual Task OnModuleLoad() {
      return Task.CompletedTask;
   }

   public virtual Task OnModuleUnload() {
      return Task.CompletedTask;
   }

   public virtual Task OnUserMessage(Message msg) {
      return Task.CompletedTask;
   }

   public virtual Task OnDirectMessage(Message msg) {
      return Task.CompletedTask;
   }

   public virtual Task OnAtMessage(Message msg) {
      return Task.CompletedTask;
   }

   public virtual Task OnNewMemberJoin(MemberWithGuildID member) {
      return Task.CompletedTask;
   }

   public virtual Task OnAddedToGuild(WsGuild guild) {
      return Task.CompletedTask;
   }

   public virtual Task OnRemovedFromGuild(WsGuild guild) {
      return Task.CompletedTask;
   }

   public virtual Task OnReactionAdded(MessageReaction reaction) {
      return Task.CompletedTask;
   }

   public virtual Task OnReactionRemoved(MessageReaction reaction) {
      return Task.CompletedTask;
   }
}