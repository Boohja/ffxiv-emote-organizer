using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using System.IO;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using EmoteOrganizer.Services;
using EmoteOrganizer.Windows;
using XivCommon;
using Emote = Lumina.Excel.GeneratedSheets.Emote;

namespace EmoteOrganizer
{
    public sealed class Plugin : IDalamudPlugin
    {
        public string Name => "Emote Organizer";
        private const string CommandName = "/emotes";

        internal XivCommonBase XivCommon { get; }
        internal DalamudPluginInterface PluginInterface { get; init; }
        internal ICommandManager CommandManager { get; init; }
//        public static IDataManager Data { get; private set; } = null!;
        public Configuration Configuration { get; init; }
        public WindowSystem WindowSystem = new("EmoteOrganizer");
        public IconService IconService { get; init; }
        public EmoteService EmoteService { get; init; }

        [PluginService]
        [RequiredVersion("1.0")]
        public static ITextureProvider TextureProvider { get; private set; } = null!;

        [PluginService]
        internal IDataManager Data { get; init; } = null!;
        [PluginService]
        internal static IPluginLog Log { get; private set; } = null!;

        private ConfigWindow ConfigWindow { get; init; }
        private MainWindow MainWindow { get; init; }

        public Plugin(
            [RequiredVersion("1.0")] DalamudPluginInterface pluginInterface,
            [RequiredVersion("1.0")] ICommandManager commandManager,
            [RequiredVersion("1.0")] IDataManager dataManager)
        {
            this.PluginInterface = pluginInterface;
            this.CommandManager = commandManager;
            Data = dataManager;
            this.XivCommon = new XivCommonBase(PluginInterface);

            this.Configuration = this.PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
            this.Configuration.Initialize(this.PluginInterface);
            IconService = new IconService(TextureProvider);
            EmoteService = new EmoteService(this);
            
            // you might normally want to embed resources and load them from the manifest stream
            var imagePath = Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName!, "logo.png");
            var goatImage = this.PluginInterface.UiBuilder.LoadImage(imagePath);

            ConfigWindow = new ConfigWindow(this);
            Log.Debug("Hey there :D");
            var emotes = Data.GetExcelSheet<Emote>()!.Where(
                emote => emote.Icon > 0 && emote.TextCommand.Value != null && emote.Icon > 0
            );
            MainWindow = new MainWindow(this);
            
            WindowSystem.AddWindow(ConfigWindow);
            WindowSystem.AddWindow(MainWindow);

            this.CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
            {
                HelpMessage = "A useful message to display in /xlhelp"
            });

            this.PluginInterface.UiBuilder.Draw += DrawUI;
            this.PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;
        }

        public void Dispose()
        {
            this.WindowSystem.RemoveAllWindows();
            
            ConfigWindow.Dispose();
            MainWindow.Dispose();
            
            this.CommandManager.RemoveHandler(CommandName);
        }

        private void OnCommand(string command, string args)
        {
            // in response to the slash command, just display our main ui
            MainWindow.IsOpen = true;
        }

        private void DrawUI()
        {
            this.WindowSystem.Draw();
        }

        public void DrawConfigUI()
        {
            ConfigWindow.IsOpen = true;
        }
    }
}
