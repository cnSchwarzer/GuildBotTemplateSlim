using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Playwright;

namespace GuildBotTemplate;

public class Browser {
   public static Browser Instance { get; private set; }

   public async Task<IBrowserContext> NewChromiumContext() {
      try {
         var chrome = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions() {
            ChromiumSandbox = true
         });
         return await chrome.NewContextAsync(new BrowserNewContextOptions() {
            ViewportSize = new ViewportSize {
               Width = 500,
               Height = 2000
            },
            DeviceScaleFactor = 4
         });
      } catch (Exception ex) {
         App.Logger.LogError(ex, "启动浏览器失败");
         Process.GetCurrentProcess().Kill();
         return null;
      }
   }

   private IPlaywright _playwright = null!;

   private Browser() { }

   public static async Task Create() {
      if (Instance != null) {
         throw new Exception("Browser should be singleton.");
      }
      var ret = new Browser {
         _playwright = await Playwright.CreateAsync().ConfigureAwait(false)
      };
      Instance = ret;
   }

   public static void Install(string[] args) {
      Microsoft.Playwright.Program.Main(args);
   }
}