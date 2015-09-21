﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using IniParser;
using IniParser.Model;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class Settings
    {
        static readonly FileIniDataParser Parser = new FileIniDataParser();
        readonly IniData _data;
        public Settings()
        {
            if (!File.Exists("Config.ini"))
            {
                CreateAndSaveConfig();
            }

            _data = Parser.ReadFile("Config.ini");
        }
        public int GetInt(string section, string key)
        {
            string keyValue = _data[section][key];
            int setting = int.Parse(keyValue);
            return setting;
        }
        public uint GetUInt(string section, string key)
        {
            string keyValue = _data[section][key];
            uint setting = uint.Parse(keyValue);
            return setting;
        }
        public float GetFloat(string section, string key)
        {
            string keyValue = _data[section][key];
            float setting = float.Parse(keyValue);
            return setting;
        }
        public bool GetBool(string section, string key)
        {
            string keyValue = _data[section][key];
            bool setting = bool.Parse(keyValue);
            return setting;
        }

        public static void CreateAndSaveConfig()
        {
            var weaponList = new List<string>
            {
                //Pistols
                //"CZ75",
                "DEagle",
                "Elite",
                "FiveSeveN",
                "Glock",
                "P228",
                "P250",
                "HKP2000",
                "Tec9",
                //"USP-S",

                //Heavy
                "Nova",
                "XM1014",
                "Sawed-Off",
                "Mag-7",

                //SMG
                "Mac-10",
                "Mp9",
                "Mp7",
                "Ump-45",
                "PP-Bizon",
                "P90",

                //Rifles
                "Galil AR",
                "AK47",
                "Sg 553",
                "Famas",
                "M4A1",
                "AUG",

                //Snipers
                "AWP",
                "SSG 08",
                "Scar-20",
                "G3SG1",

                //Machine Guins
                "M249",
                "Negev",
            };

            StringBuilder builder = new StringBuilder();
            //Misc
            builder.AppendLine("[Bunny Jump]");
            builder.AppendLine("Bunny Jump Enabled = True");
            builder.AppendLine("Bunny Jump Key = SPACE").AppendLine();

            //Sound ESP
            builder.AppendLine("[Sound Esp]");
            builder.AppendLine("Sound Range = 0");
            builder.AppendLine("Sound Intverval = 0");
            builder.AppendLine("Sound Voulme = 0").AppendLine();
            foreach (var weapon in weaponList)
            {
                builder.AppendLine("[" + weapon + "]");
                builder.AppendLine("Aim Enabled = True");
                builder.AppendLine("Aim Key = LBUTTON");
                builder.AppendLine("Aim Fov = 2");
                builder.AppendLine("Aim Bone = 5");
                builder.AppendLine("Aim Smooth Enabled = True");
                builder.AppendLine("Aim Smooth Value = 0,35");
                builder.AppendLine("Aim Spotted = True");
                builder.AppendLine("Aim Spotted By = False");
                builder.AppendLine("Aim Enemies = True");
                builder.AppendLine("Aim Allies = False").AppendLine();
                //RCS
                builder.AppendLine("Rcs Enabled = True");
                builder.AppendLine("Rcs Start = 1");
                builder.AppendLine("Rcs Force Max = 100");
                builder.AppendLine("Rcs Force Min = 83").AppendLine();
                //Trigger
                builder.AppendLine("Trigger Enabled = True");
                builder.AppendLine("Trigger Key = MENU");
                builder.AppendLine("Trigger Toggle = False");
                builder.AppendLine("Trigger Hold = True");
                builder.AppendLine("Trigger Enemies = True");
                builder.AppendLine("Trigger Allies = True");
                builder.AppendLine("Trigger Burst Enabled = False");
                builder.AppendLine("Trigger Burst Shots = 0");
                builder.AppendLine("Trigger Burst Randomize = False");
                builder.AppendLine("Trigger Delay FirstShot = 21");
                builder.AppendLine("Trigger Delay Shots = 21").AppendLine();
            }
            builder.AppendLine("[Default]");
            builder.AppendLine("Aim Enabled = False");
            builder.AppendLine("Aim Key = LBUTTON");
            builder.AppendLine("Aim Fov = 0");
            builder.AppendLine("Aim Bone = 5");
            builder.AppendLine("Aim Smooth Enabled = False");
            builder.AppendLine("Aim Smooth Value = 0");
            builder.AppendLine("Aim Spotted = False");
            builder.AppendLine("Aim Spotted By = False");
            builder.AppendLine("Aim Enemies = False");
            builder.AppendLine("Aim Allies = False").AppendLine();
            //RCS
            builder.AppendLine("Rcs Enabled = True");
            builder.AppendLine("Rcs Start = 1");
            builder.AppendLine("Rcs Force Max = 100");
            builder.AppendLine("Rcs Force Min = 83").AppendLine();
            //Trigger
            builder.AppendLine("Trigger Enabled = False");
            builder.AppendLine("Trigger Key = MENU");
            builder.AppendLine("Trigger Toggle = False");
            builder.AppendLine("Trigger Hold = False");
            builder.AppendLine("Trigger Enemies = False");
            builder.AppendLine("Trigger Allies = False");
            builder.AppendLine("Trigger Burst Enabled = False");
            builder.AppendLine("Trigger Burst Shots = 0");
            builder.AppendLine("Trigger Burst Randomize = False");
            builder.AppendLine("Trigger Delay FirstShot = 21");
            builder.AppendLine("Trigger Delay Shots = 21");
            if (!File.Exists("Config.ini"))
            {
                Console.WriteLine("> Config does not exist. Creating..");
                StreamWriter sr = new StreamWriter(@"Config.ini");
                sr.WriteLine(builder);
                sr.Close();
            }
        }
    }

}