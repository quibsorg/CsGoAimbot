﻿using System;
using System.Data;
using System.Timers;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ExternalUtilsCSharp;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using CsGoApplicationAimbot.CSGOClasses.Updaters;
using MySql.Data.MySqlClient;
using Timer = System.Timers.Timer;

namespace CsGoApplicationAimbot
{
    public static class Program
    {
        #region Fields
        private static readonly Timer Timer1 = new Timer(1);
        private static readonly Timer Timer2 = new Timer(0.5);
        private static SoundManager _soundManager;
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
        private static MySqlConnection _connection;
        private static MySqlCommand _cmd;
        private static MySqlDataReader _reader;
        #endregion

        #region Properties
        public static SoundManager SoundManager => _soundManager;

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
                using (MD5 md5Hash = MD5.Create())
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

            _loggedIn = Login();

            if (_loggedIn)
            {
                PrintSuccess("Smurf bot");
                //Sets a random title to our Console Window.. Almost useless.
                Console.Title = RandomTitle();

                //Set's up our SoundManager
                ManageAudio();

                //Starts our cheat.
                StartCheat();
            }

        }

        private static void ManageAudio()
        {
            _soundManager = new SoundManager(2);
            _soundManager.Add(0, Properties.Resources.heartbeatloop);
            _soundManager.Add(1, Properties.Resources.beep);
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
            Memory.Update();
            _bunnyJump.Update();
            _sonar.Update();
            _triggerBot.Update();
            _aimbot.Update();
            _rcs.Update();
        }
        private static void Timer2_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Memory.Update();
            //_bunnyJump.Update();
            //_sonar.Update();
            //_triggerBot.Update();
            //_aimbot.Update();
        }
        #endregion
        static string Encrypt(MD5 md5Hash, string password)
        {
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(password));
            StringBuilder sBuilder = new StringBuilder();
            foreach (byte t in data)
            {
                sBuilder.Append(t.ToString("x2"));
            }
            return sBuilder.ToString();
        }
        private static bool Login()
        {
            using (_connection = new MySqlConnection(_connectionString))
            {
                using (_cmd = _connection.CreateCommand())
                {
                    //Opens our connection to the db
                    _connection.Open();
                    _cmd.CommandText = "SELECT * FROM `users` WHERE `username`= @username";
                    _cmd.Parameters.AddWithValue("@username", Username);
                    using (_reader = _cmd.ExecuteReader())
                    {
                        while (_reader.Read())
                        {
                            if (_reader.HasRows)
                            {
                                //If we get this far we have a user with a matching password
                                if (Password == _reader.GetString("password"))
                                {
                                    //We check the user permssion.
                                    _authorized = CheckPermission();
                                    //We check their hwid, if they have none we insert it.
                                    if (_authorized)
                                    {
                                        _hwidMatch = CheckHwid();
                                        if (_hwidMatch)
                                            return true;
                                    }
                                    return false;
                                }
                            }
                        }
                    }
                    Console.WriteLine("Username or password is incorrect.");
                    Console.ReadLine();
                    return false;
                }
            }
        }

        private static bool CheckHwid()
        {
            _hwid = Hwid.GetHwid();
            using (_connection = new MySqlConnection(_connectionString))
            {
                using (_cmd = _connection.CreateCommand())
                {
                    _connection.Open();
                    _cmd.CommandText = "SELECT * FROM `users` WHERE `username`= @username";
                    _cmd.Parameters.AddWithValue("@username", Username);

                    using (_reader = _cmd.ExecuteReader())
                    {
                        while (_reader.Read())
                        {
                            var hwid = _reader.GetOrdinal("hwid");

                            if (_reader.IsDBNull(hwid))
                            {
                                InsertHwid();
                                return true;
                            }
                            if (_hwid == _reader.GetString("hwid"))
                            {
                                return true;
                            }
                            Console.WriteLine("Hwid dosen't match, ask for a reset.");
                            Console.ReadLine();
                            return false;
                        }
                    }
                }
                return false;
            }
        }

        private static void InsertHwid()
        {
            using (_connection = new MySqlConnection(_connectionString))
            {
                using (_cmd = _connection.CreateCommand())
                {
                    _connection.Open();
                    try
                    {
                        _cmd.CommandText = "UPDATE users SET hwid= @hwid WHERE username = @user;";
                        _cmd.Parameters.AddWithValue("@hwid", _hwid);
                        _cmd.Parameters.AddWithValue("@user", Username);
                        _cmd.ExecuteNonQuery();
                        _loggedIn = true;
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                }
            }
        }

        private static bool CheckPermission()
        {
            using (_connection = new MySqlConnection(_connectionString))
            {
                using (_cmd = _connection.CreateCommand())
                {
                    try
                    {
                        _connection.Open();
                        _cmd.CommandText = "SELECT * FROM `users` WHERE `username`= @username";
                        //get's the row that matches our username.
                        _cmd.Parameters.AddWithValue("@username", Username);

                        using (_reader = _cmd.ExecuteReader())
                        {
                            while (_reader.Read())
                            {
                                _userGroup = _reader.GetInt16("usergroup");
                            }

                            //3 = Super Moderator, 4 == Admin, 6 == Moderator, 8 == CsgoVIP
                            if (_userGroup == 3 || _userGroup == 4 || _userGroup == 6 || _userGroup == 8)
                            {
                                return true;
                            }
                            Console.WriteLine("You do not have permssion to use this product");
                            Console.ReadLine();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                    }
                    finally
                    {
                        if (_connection.State == ConnectionState.Open)
                        {
                            _connection.Close();
                            _reader.Close();
                        }
                    }
                }
            }
            return false;
        }

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