using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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

        private static string Encrypt(MD5 md5Hash, string password)
        {
            var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            var sBuilder = new StringBuilder();
            foreach (var t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }
            return sBuilder.ToString();
        }
        #region Fields

        private static readonly Timer Timer1 = new Timer(1);
        private static readonly Timer Timer2 = new Timer(0.5);

        #endregion

        #region Constants

        public const string GameProcess = "csgo";
        public const string GameTitle = "Counter-Strike: Global Offensive";
        private const string Chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!\"§$%&/()=?`+#-.,<>|²³{[]}\\~´";

        #endregion

        #region Variables

        private static IntPtr _hWnd;
        private static SettingsConfig _settings;
        private static ProcUtils _procUtils;

        //Updaters
        private static Aimbot _aimbot;
        private static TriggerBot _triggerBot;
        private static RCS _rcs;
        private static BunnyJump _bunnyJump;
        private static Sonar _sonar;
        public static Memory Memory;

        public static MemUtils MemUtils;
        public static KeyUtils KeyUtils;

        private static readonly string _connectionString = "Server=MYSQL5011.myWindowsHosting.com;Database=db_9b8e03_smurf;Uid=9b8e03_smurf;Pwd=Phanta123!;";

        private static bool _authorized;
        private static bool _hwidMatch;
        private static int _userGroup;
        public static string Username = string.Empty;
        public static string Password = string.Empty;
        private static string _hwid = string.Empty;
        private static bool _loggedIn;
        #endregion

        #region Method

        public static void Main(string[] args)
        {
            if (!File.Exists("Config.ini"))
            {
                Console.Write("Enter your username: ");
                Username = Console.ReadLine();
                Console.Clear();

                Console.Write("Enter your password: ");
                using (var md5Hash = MD5.Create())
                {
                    Password = Encrypt(md5Hash, Password = Console.ReadLine());
                }
                Console.Clear();
            }
            else
            {
                _settings = new SettingsConfig();
                Username = _settings.GetString("User", "Username");
                Password = _settings.GetString("User", "Password");
            }

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
            _settings = new SettingsConfig();
            KeyUtils = new KeyUtils();

            PrintInfo("> Waiting for CSGO to start up...");
            while (!ProcUtils.ProcessIsRunning(GameProcess))
                Thread.Sleep(250);

            _procUtils = new ProcUtils(GameProcess,
                WinAPI.ProcessAccessFlags.VirtualMemoryRead | WinAPI.ProcessAccessFlags.VirtualMemoryWrite |
                WinAPI.ProcessAccessFlags.VirtualMemoryOperation);
            MemUtils = new MemUtils {Handle = _procUtils.Handle};

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
            _rcs = new RCS();
            _bunnyJump = new BunnyJump();
            _sonar = new Sonar();

            Timer1.Elapsed += Timer1Elapsed;
            Timer1.Start();

            Timer2.Elapsed += Timer2_Elapsed;
            Timer2.Start();

            PrintSuccess("Cheat is now running.");
            Application.Run();
        }

        private static void Timer1Elapsed(object sender, ElapsedEventArgs e)
        {
            KeyUtils.Update();
            _rcs.Update();
        }

        private static void Timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            Memory.Update();
            _bunnyJump.Update();
            _sonar.Update();
            _triggerBot.Update();
            _aimbot.Update();
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