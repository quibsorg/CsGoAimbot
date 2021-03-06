﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IniParser;
using IniParser.Model;

namespace CsGoApplicationAimbot
{
    public class Settings
    {
        private static readonly FileIniDataParser Parser = new FileIniDataParser();
        private readonly IniData _data;

        public Settings()
        {
            if (!File.Exists("Config.ini"))
            {
                CreateAndSaveSettings();
            }

            _data = Parser.ReadFile("Config.ini");
        }

        public int GetInt(string section, string key)
        {
            var keyValue = _data[section][key];
            var setting = int.Parse(keyValue);
            return setting;
        }

        public string GetString(string section, string key)
        {
            var keyValue = _data[section][key];
            var setting = keyValue;
            return setting;
        }

        public uint GetUInt(string section, string key)
        {
            var keyValue = _data[section][key];
            var setting = uint.Parse(keyValue);
            return setting;
        }

        public float GetFloat(string section, string key)
        {
            var keyValue = _data[section][key];
            var setting = float.Parse(keyValue);
            return setting;
        }

        public bool GetBool(string section, string key)
        {
            var keyValue = _data[section][key];
            var setting = bool.Parse(keyValue);
            return setting;
        }

        public WinAPI.VirtualKeyShort GetKey(string section, string key)
        {
            var keyValue = _data[section][key];
            var button = (WinAPI.VirtualKeyShort) int.Parse(keyValue);
            return button;
        }

        private static void CreateAndSaveSettings()
        {
            var weaponList = new List<string>
            {
                //Pistols
                "DEagle",
                "Elite",
                "FiveSeven",
                "Glock",
                "P228",
                "P250",
                "HKP2000",
                "Tec9",

                //Heavy
                "NOVA",
                "XM1014",
                "Sawedoff",
                "Mag7",

                //SMG
                "MAC10",
                "MP9",
                "MP7",
                "UMP45",
                "Bizon",
                "P90",

                //Rifles
                "GalilAR",
                "AK47",
                "SG556",
                "Famas",
                "M4A1",
                "Aug",

                //Snipers
                "AWP",
                "SSG08",
                "SCAR20",
                "G3SG1",

                //Machine Guins
                "M249",
                "Negev",

                //Default for unkown weapons Should not be any but just incase.
                "Default"
            };

            var builder = new StringBuilder();
            //Misc
            builder.AppendLine("[Bunny Jump]");
            builder.AppendLine("Bunny Jump Enabled = True");
            builder.AppendLine("Bunny Jump Key = 32").AppendLine();

            //Sound ESP
            builder.AppendLine("[Sonar]");
            builder.AppendLine("Sonar Enabled = False");
            builder.AppendLine("Sonar Range = 0");
            builder.AppendLine("Sonar Interval = 0");
            builder.AppendLine("Sonar Sound = 1");
            builder.AppendLine("Sonar Volume = 0").AppendLine();

            //Misc
            builder.AppendLine("[Misc]");
            builder.AppendLine("Auto Knife = True");
            builder.AppendLine("Trigger Taser = False").AppendLine();

            foreach (var weapon in weaponList)
            {
                builder.AppendLine("[" + weapon + "]");
                //RCS
                builder.AppendLine("Rcs Enabled = True");
                builder.AppendLine("Rcs Start = 1");
                builder.AppendLine("Rcs Force Max = 102");
                builder.AppendLine("Rcs Force Min = 101").AppendLine();
                //Trigger
                builder.AppendLine("Trigger Enabled = True");
                builder.AppendLine("Trigger Key = 18");
                builder.AppendLine("Trigger Toggle = False");
                builder.AppendLine("Trigger Hold = True");
                builder.AppendLine("Trigger Only When Standing Still = True");
                builder.AppendLine("Trigger When Scoped = False");
                builder.AppendLine("Trigger Enemies = True");
                builder.AppendLine("Trigger Allies = False");
                builder.AppendLine("Trigger Burst Enabled = False");
                builder.AppendLine("Trigger Burst Randomize = False");
                builder.AppendLine("Trigger Burst Shots Min = 0");
                builder.AppendLine("Trigger Burst Shots Max = 0");
                builder.AppendLine("Trigger Delay FirstShot = 21");
                builder.AppendLine("Trigger Delay Shots = 21").AppendLine();
            }
            if (!File.Exists("Config.ini"))
            {
                Console.WriteLine(@"> Config does not exist. Creating..");
                var sr = new StreamWriter(@"Config.ini");
                sr.WriteLine(builder);
                sr.Close();
            }
        }
    }
}