using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CsGoApplicationAimbot.CSGOClasses;
using CsGoApplicationAimbot.CSGOClasses.Enums;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.SharpDXRenderer;
using ExternalUtilsCSharp.UI;
using SharpDX.DirectWrite;
using SharpDX;

namespace CsGoApplicationAimbot
{
    public static class Program
    {
        #region CONTROLS
        private static SharpDXOverlay _shdxOverlay;
        #endregion

        #region CONSTANTS

        private const string GameProcess = "csgo";
        private const string GameTitle = "Counter-Strike: Global Offensive";

        #endregion

        #region VARIABLES

        public static KeyUtils KeyUtils;
        private static IntPtr _hWnd;
        public static Framework Framework;
        private static ProcUtils _procUtils;
        public static MemUtils MemUtils;
        public static CsgoConfigUtils ConfigUtils;
        public static SettingsConfig Settings;

        #endregion

        #region Method
        public static void Main(string[] args)
        {
            PrintSuccess("Smurf bot");
            //We make the config if it dosen't exist.
            Settings = new SettingsConfig();
            KeyUtils = new KeyUtils();
            ConfigUtils = new CsgoConfigUtils();

            PrintInfo("> Waiting for CSGO to start up...");
            while (!ProcUtils.ProcessIsRunning(GameProcess))
                Thread.Sleep(250);

            _procUtils = new ProcUtils(GameProcess,
                WinAPI.ProcessAccessFlags.VirtualMemoryRead | WinAPI.ProcessAccessFlags.VirtualMemoryWrite |
                WinAPI.ProcessAccessFlags.VirtualMemoryOperation);
            MemUtils = new MemUtils
            {
                Handle = _procUtils.Handle
            };

            PrintInfo("> Waiting for CSGOs window to show up...");
            while ((_hWnd = WinAPI.FindWindowByCaption(_hWnd, GameTitle)) == IntPtr.Zero)
                Thread.Sleep(250);

            ProcessModule clientDll, engineDll;
            PrintInfo("> Waiting for CSGO to load client.dll...");
            while ((clientDll = _procUtils.GetModuleByName(@"bin\client.dll")) == null)
                Thread.Sleep(250);
            PrintInfo("> Waiting for CSGO to load engine.dll...");
            while ((engineDll = _procUtils.GetModuleByName(@"engine.dll")) == null)
                Thread.Sleep(250);

            Framework = new Framework(engineDll, clientDll);

            _shdxOverlay = new SharpDXOverlay();
            _shdxOverlay.Attach(_hWnd);
            _shdxOverlay.TickEvent += ProOverlayTickEvent;

            Application.Run();
            ConfigUtils.SaveSettingsToFile("Config.cfg");
        }

        private static void ProOverlayTickEvent(object sender, Overlay<SharpDXRenderer, Color, Vector2, TextFormat>.DeltaEventArgs e)
        {
            KeyUtils.Update();
            Framework.Update();
            _shdxOverlay.UpdateControls(e.SecondsElapsed, KeyUtils);
        }
        #endregion

        #region HELPERS

        public static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        private static void PrintSuccess(string text, params object[] arguments)
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

        private static void PrintEncolored(string text, ConsoleColor color, params object[] arguments)
        {
            var clr = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text, arguments);
            Console.ForegroundColor = clr;
        }

        #endregion
    }
}