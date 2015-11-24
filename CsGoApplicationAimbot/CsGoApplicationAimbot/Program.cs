using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using CsGoApplicationAimbot.CSGOClasses.Updaters;
using CsGoApplicationAimbot.Properties;
using ExternalUtilsCSharp;
using Timer = System.Timers.Timer;

namespace CsGoApplicationAimbot
{
    public static class Program
    {
        #region Properties

        public static SoundManager SoundManager { get; private set; }

        #endregion
        #region Fields

        private static readonly Timer Timer1 = new Timer(0.5);

        #endregion

        #region Constants

        public const string GameProcess = "csgo";
        public const string GameTitle = "Counter-Strike: Global Offensive";
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!\"§$%&/()=?`+#-.,<>|²³{[]}\\~´";

        #endregion

        #region Variables

        private static IntPtr _hWnd;
        private static ProcUtils _procUtils;

        //Updaters
        private static Aimbot _aimbot;
        private static TriggerBot _triggerBot;
        private static Rcs _rcs;
        private static BunnyJump _bunnyJump;
        private static Sonar _sonar;
        public static Memory Memory;

        public static MemUtils MemUtils;
        public static KeyUtils KeyUtils;
        #endregion

        #region Method

        public static void Main(string[] args)
        {
            PrintSuccess("Smurf bot");
            //Sets a random title to our Console Window.. Almost useless.
            Console.Title = RandomTitle();

            //Set's up our SoundManager
            ManageAudio();

            //Starts our cheat.
            StartCheat();
        }

        private static void ManageAudio()
        {
            SoundManager = new SoundManager(2);
            SoundManager.Add(0, Resources.heartbeatloop);
            SoundManager.Add(1, Resources.beep);
        }

        private static string RandomTitle()
        {
            var random = new Random();
            return new string(Enumerable.Repeat(Chars, 20)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        private static void StartCheat()
        {
            //We make the config if it dosen't exist.
            KeyUtils = new KeyUtils();

            PrintInfo("> Waiting for CSGO to start up...");
            while (!ProcUtils.ProcessIsRunning(GameProcess))
                Thread.Sleep(250);

            _procUtils = new ProcUtils(GameProcess,
                WinAPI.ProcessAccessFlags.VirtualMemoryRead | WinAPI.ProcessAccessFlags.VirtualMemoryWrite |
                WinAPI.ProcessAccessFlags.VirtualMemoryOperation);
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

            //will update everything we need.
            Memory = new Memory(engineDll, clientDll);

            _aimbot = new Aimbot();
            _triggerBot = new TriggerBot();
            _rcs = new Rcs();
            _bunnyJump = new BunnyJump();
            _sonar = new Sonar();

            Timer1.Elapsed += Timer1Elapsed;
            Timer1.Start();

            PrintSuccess("Cheat is now running.");
            Application.Run();
        }

        private static void Timer1Elapsed(object sender, ElapsedEventArgs e)
        {
            Memory.Update();
            _bunnyJump.Update();
            _sonar.Update();
            _triggerBot.Update();
            _rcs.Update();
            _aimbot.Update();
            KeyUtils.Update();
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