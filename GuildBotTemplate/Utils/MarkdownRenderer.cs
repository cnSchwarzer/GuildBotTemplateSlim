using System.IO.Compression;
using System.Net.NetworkInformation;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;
using Newtonsoft.Json;

namespace GuildBotTemplate.Utils;

public class MarkdownRenderer {
   private IBrowserContext _ctx;

   public static MarkdownRenderer Instance { get; private set; }

   static MarkdownRenderer() {
      Instance = new MarkdownRenderer();
   }

   private MarkdownRenderer() {
      ReloadContext().Wait();
   }

   public static string EncodeToEscapedData(object obj) {
      var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(obj));
      using var ms = new MemoryStream();
      using var compressor = new GZipStream(ms, CompressionMode.Compress);
      compressor.Write(data);
      compressor.Close();
      return Uri.EscapeDataString(Convert.ToBase64String(ms.ToArray()));
   }

   private async Task ReloadContext() {
      if (_ctx != null) {
         try {
            var browser = _ctx.Browser!;
            await _ctx.CloseAsync();
            await _ctx.DisposeAsync();
            await browser.CloseAsync();
            await browser.DisposeAsync();
         } catch (Exception) {
            // ignored
         }
      }
      _ctx = await Browser.Instance.NewChromiumContext();
   }

   public static int GetVacantPort(int startingPort = 0) {
      if (startingPort == 0)
         startingPort = Random.Shared.Next(20000, 30000);

      var portArray = new List<int>();

      IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

      //getting active connections
      var connections = properties.GetActiveTcpConnections();
      portArray.AddRange(from n in connections where n.LocalEndPoint.Port >= startingPort select n.LocalEndPoint.Port);

      //getting active tcp listners - WCF service listening in tcp
      var endPoints = properties.GetActiveTcpListeners();
      portArray.AddRange(from n in endPoints where n.Port >= startingPort select n.Port);

      //getting active udp listeners
      endPoints = properties.GetActiveUdpListeners();
      portArray.AddRange(from n in endPoints where n.Port >= startingPort select n.Port);

      portArray.Sort();

      for (var i = startingPort; i < ushort.MaxValue; i++)
         if (!portArray.Contains(i))
            return i;

      return 0;
   }

   public static string GetTimedTheme() {
      return DateTimeOffset.Now.LocalDateTime.Hour is >= 23 or <= 7 ? "dark" : "light";
   }

   public async Task<byte[]> MarkdownSnapshot(string md, string padding = null, string margin = null) {
      App.Logger.LogTrace($"Render Markdown\n{md}");
      IPage page = null;
      while (page == null) {
         try {
            page = await _ctx.NewPageAsync();
         } catch (Exception ex) {
            App.Logger.LogError(ex, "NewPage");
            await ReloadContext();
            await Task.Delay(TimeSpan.FromSeconds(1));
         }
      }
      try {
         var retryCount = 0;
         retry:
         try {
            await page.GotoAsync($"https://md.reito.fun/", new PageGotoOptions() {
                  Timeout = 5000
               })
               .ConfigureAwait(false);
            await page.WaitForLoadStateAsync(LoadState.Load, new PageWaitForLoadStateOptions() {
               Timeout = 5000
            });
            await page.EvaluateAsync($$"""
(content) => {
   window.renderMarkdown({ 
      markdown: content,
      theme: '{{GetTimedTheme()}}',
      padding: {{(string.IsNullOrWhiteSpace(padding) ? "undefined" : $"'{padding}'")}},
      margin: {{(string.IsNullOrWhiteSpace(margin) ? "undefined" : $"'{margin}'")}},
   })
}
""", md);
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions() {
               Timeout = 5000
            });
         } catch (Exception ex) {
            App.Logger.LogWarning(ex, "Wait For Render Complete");
            retryCount++;
            if (retryCount < 3) {
               goto retry;
            }
         }
         var app = await page.QuerySelectorAsync("#app").ConfigureAwait(false);
         var clip = await app!.BoundingBoxAsync().ConfigureAwait(false);
         var ret = await page.ScreenshotAsync(new PageScreenshotOptions {
               Clip = new Clip {
                  Width = clip!.Width,
                  Height = clip.Height,
                  X = clip.X,
                  Y = clip.Y
               },
               FullPage = true, Type = ScreenshotType.Jpeg
            })
            .ConfigureAwait(false);
         return ret;
      } catch (Exception ex) {
         App.Logger.LogError(ex, "Snapshot");
         return null;
      } finally {
         await page.CloseAsync();
      }
   }
}