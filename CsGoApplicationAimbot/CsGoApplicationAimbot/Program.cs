using System;
using System.Timers;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using CsGoApplicationAimbot.CSGOClasses;
using ExternalUtilsCSharp;
using Timer = System.Timers.Timer;

namespace CsGoApplicationAimbot
{
    public static class Program
    {
        #region CONTROLS
        static Timer _timer = new Timer(10);
        #endregion

        #region CONSTANTS

        private const string GameProcess = "csgo";
        private const string GameTitle = "Counter-Strike: Global Offensive";

        #endregion

        #region VARIABLES
        private static IntPtr _hWnd;
        private static SettingsConfig _settings;
        private static ProcUtils _procUtils;
        public static Framework Framework;
        public static MemUtils MemUtils;
        public static KeyUtils KeyUtils;
        #endregion

        #region Method
        public static void Main(string[] args)
        {
            PrintSuccess("Smurf bot");
            //We make the config if it dosen't exist.
            _settings = new SettingsConfig();
            KeyUtils = new KeyUtils();

            PrintInfo("> Waiting for CSGO to start up...");
            while (!ProcUtils.ProcessIsRunning(GameProcess))
                Thread.Sleep(250);

            _procUtils = new ProcUtils(GameProcess, WinAPI.ProcessAccessFlags.VirtualMemoryRead | WinAPI.ProcessAccessFlags.VirtualMemoryWrite | WinAPI.ProcessAccessFlags.VirtualMemoryOperation);
            MemUtils = new MemUtils { Handle = _procUtils.Handle };

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

            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
            
            PrintSuccess("Cheat is now running.");
            Application.Run();
        }

        private static void _timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            KeyUtils.Update();
            Framework.Update();
        }
        #endregion

        #region HELPERS

        private static void PrintInfo(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.White, arguments);
        }

        private static void PrintSuccess(string text, params object[] arguments)
        {
            PrintEncolored(text, ConsoleColor.Green, arguments);
        }

        private static void PrintError(string text, params object[] arguments)
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