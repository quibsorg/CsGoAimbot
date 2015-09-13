using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using CsGoApplicationAimbot.CSGOClasses;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.SharpDXRenderer;

namespace CsGoApplicationAimbot
{
    public class Program
    {
        #region CONSTANTS
        private const string GameProcess = "csgo";
        private const string GameTitle = "Counter-Strike: Global Offensive";
        #endregion

        #region VARIABLES
        public static KeyUtils KeyUtils;
        private static IntPtr _hWnd;
        public static Framework Framework;
        public static ProcUtils ProcUtils;
        public static MemUtils MemUtils;
        public static CsgoConfigUtils ConfigUtils;
        #endregion

        #region CONTROLS
        public static SharpDXOverlay ShdxOverlay;
        #endregion

        #region METHODS
        public static void Main(string[] args)
        {
            PrintSuccess("Smurf bot");
            KeyUtils = new KeyUtils();
            ConfigUtils = new CsgoConfigUtils();
            
            //Creates the setting file
            AddAndApplySettings();

            PrintInfo("> Waiting for CSGO to start up...");
            while (!ProcUtils.ProcessIsRunning(GameProcess))
                Thread.Sleep(250);

            ProcUtils = new ProcUtils(GameProcess, WinAPI.ProcessAccessFlags.VirtualMemoryRead | WinAPI.ProcessAccessFlags.VirtualMemoryWrite | WinAPI.ProcessAccessFlags.VirtualMemoryOperation);
            MemUtils = new MemUtils
            {
                Handle = ProcUtils.Handle
            };

            PrintInfo("> Waiting for CSGOs window to show up...");
            while ((_hWnd = WinAPI.FindWindowByCaption(_hWnd, GameTitle)) == IntPtr.Zero)
                Thread.Sleep(250);

            ProcessModule clientDll, engineDll;
            PrintInfo("> Waiting for CSGO to load client.dll...");
            while ((clientDll = ProcUtils.GetModuleByName(@"bin\client.dll")) == null)
                Thread.Sleep(250);
            PrintInfo("> Waiting for CSGO to load engine.dll...");
            while ((engineDll = ProcUtils.GetModuleByName(@"engine.dll")) == null)
                Thread.Sleep(250);

            Framework = new Framework(clientDll, engineDll);

            PrintInfo("> Initializing overlay");
            using (ShdxOverlay = new SharpDXOverlay())
            {
                ShdxOverlay.Attach(_hWnd);
                ShdxOverlay.TickEvent += overlay_TickEvent;
            }
            Application.Run();
            ConfigUtils.SaveSettingsToFile("Config.cfg");
        }

        private static void AddAndApplySettings()
        {
            ConfigUtils.BooleanSettings.AddRange(new[]
            {
                "Aim Enabled",
                "Aim Hold",
                "Aim Smooth Enabled",
                "Aim Spotted",
                "Aim Spotted By",
                "Aim Enemies",
                "Aim Allies",
            });
            ConfigUtils.KeySettings.Add("Aim Key");
            ConfigUtils.FloatSettings.AddRange(new[]
            {
                "Aim Fov",
                "Aim Smooth Value"
            });
            ConfigUtils.IntegerSettings.Add("Aim Bone");
            //RCS
            ConfigUtils.BooleanSettings.Add("Rcs Enabled");
            ConfigUtils.FloatSettings.AddRange(new[]
            {
                "Rcs Force Max",
                "Rcs Force Min"
            });
            //Trigger
            ConfigUtils.BooleanSettings.AddRange(new[]
            {
                "Trigger Enabled",
                "Trigger Toggle",
                "Trigger Hold",
                "Trigger Enemies",
                "Trigger Allies",
                "Trigger Burst Enabled",
                "Trigger Burst Randomize"
            });
            ConfigUtils.KeySettings.Add("Trigger Key");
            ConfigUtils.FloatSettings.AddRange(new[]
            {
                "Trigger Delay FirstShot",
                "Trigger Delay Shots",
                "Trigger Burst Shots"
            });
            ConfigUtils.FillDefaultValues();

            if (!File.Exists("Config.cfg"))
            {
                ConfigUtils.SaveSettingsToFile("Config.cfg");
            }
            ConfigUtils.ReadSettingsFromFile("Config.cfg");
        }

        private static void overlay_TickEvent(object sender, SharpDXOverlay.DeltaEventArgs e)
        {
            KeyUtils.Update();
            Framework.Update();
            ShdxOverlay.UpdateControls(e.SecondsElapsed, KeyUtils);

            //TODO PRINT OUT TO CONSOLE
            //if (Framework.IsPlaying())
            //{
            //    #region Spectators
            //    if (Framework.LocalPlayer != null)
            //    {
            //        var spectators = Framework.Players.Where(x => x.Item2.MHObserverTarget == Framework.LocalPlayer.M_IId && x.Item2.MIHealth == 0 && x.Item2.MIDormant != 1);
            //        StringBuilder builder = new StringBuilder();
            //        foreach (Tuple<int, CsPlayer> spec in spectators)
            //        {
            //            CsPlayer player = spec.Item2;
            //            builder.AppendFormat("{0} [{1}]{2}", Framework.Names[player.M_IId], (SpectatorView)player.MIObserverMode, builder.Length > 0 ? "\n" : "");
            //        }
            //        if (builder.Length > 0)
            //            _labelSpectators.Text = builder.ToString();
            //        else
            //            _labelSpectators.Text = "<none>";
            //    }
            //    else
            //    {
            //        _labelSpectators.Text = "<none>";
            //    }
            //    #endregion
            //}
            //else
            //{
            //    _labelSpectators.Text = "<none>";
            //}
        }
        #endregion

        #region HELPERS
        public static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }
        public static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }
        public static void PrintError(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Red, arguments);
        }
        public static void PrintException(Exception ex)
        {
            PrintError("An Exception occured: {0}\n\"{1}\"\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace);
        }
        public static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            ConsoleColor clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }
        #endregion
    }
}
