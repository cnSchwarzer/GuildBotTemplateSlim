using System.Text;
using Microsoft.Extensions.Logging;
using QQChannelFramework.Models.MessageModels;

namespace GuildBotTemplate.Utils;

public class Command {
   public string Name { get; init; }
   public string Param { get; init; }
   public string Hint { get; init; }
   public Func<string[], Task> Handler { get; init; }
   public Func<bool> PermissionPredicate { get; init; }

   public Command(string name, Func<string[], Task> handler) : this(name, null, null, handler) { }

   public Command(string name, string param, Func<string[], Task> handler) : this(name, param, null, handler) { }

   public Command(string name, string param, string hint, Func<string[], Task> handler,
      Func<bool> permissionPredicate = null) {
      Name = name;
      Hint = hint;
      Param = param;
      Handler = handler;
      PermissionPredicate = permissionPredicate;
   }
}

public static class Commands {
   private static bool Is(string command, string content, out string[] param) {
      var trimmed = content.Trim();
      if (trimmed.StartsWith($"{command} ") || trimmed.StartsWith($"/{command} ") || trimmed == command ||
          trimmed == $"/{command}") {
         var slash = trimmed.StartsWith("/");
         var @params = trimmed[(command.Length + (slash ? 1 : 0))..];
         param = @params.Trim().Split(' ');
         return true;
      }
      param = null;
      return false;
   }

   public static async Task<bool> Handle(List<Command> commands, Message msg) {
      var content = msg.Content;
      foreach (var cmd in commands) {
         if (Is(cmd.Name, content, out var param)) {
            try {
               if (cmd.PermissionPredicate?.Invoke() ?? true) {
                  await cmd.Handler(param);
               }
            } catch (Exception ex) {
               await msg.ReplyGuildMessage("处理出现异常，请重试");
               App.Logger.LogError(ex, $"Handle {cmd.Name}");
            }
            return true;
         }
      }
      if (Is("帮助", content, out _)
          //|| (msg.Mentions is {Count: > 0} && msg.Mentions.Exists(a => a.Id == GuildBot.Instance.BotId))
         ) {
         var sb = new StringBuilder();
         var cmdList = commands.Where(a => a.PermissionPredicate?.Invoke() ?? true).ToList();
         if (cmdList.Count == 0) return false;
         foreach (var cmd in cmdList) {
            sb.Append($"- {cmd.Name}");
            if (!string.IsNullOrWhiteSpace(cmd.Param)) {
               sb.Append($" `{cmd.Param}`");
            }
            if (!string.IsNullOrWhiteSpace(cmd.Hint)) {
               sb.AppendLine();
               sb.AppendLine(cmd.Hint);
            }
            sb.AppendLine();
         }
         var md = sb.ToString().Replace("\r\n", "\n").Trim();
         var img = await MarkdownRenderer.Instance.MarkdownSnapshot(md);
         await msg.ReplyGuildMessage(null, img);
      }
      return false;
   }
}