using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ExternalUtilsCSharp;
using IniParser;
using IniParser.Model;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class SettingsConfig
    {
        static readonly FileIniDataParser Parser = new FileIniDataParser();
        readonly IniData _data;
        public SettingsConfig()
        {
            if (!File.Exists("Config.ini"))
            {
                CreateAndSaveSettings();
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
        public WinAPI.VirtualKeyShort GetKey(string section, string key)
        {
            string keyValue = _data[section][key];
            WinAPI.VirtualKeyShort button = (WinAPI.VirtualKeyShort) int.Parse(keyValue);
            return button;
        }

        private static void CreateAndSaveSettings()
        {
            var weaponList = new List<string>
            {
                //Pistols
                //"CZ75",
                "DEagle",
                "Elite",
                "FiveSeven",
                "Glock",
                "P228",
                "P250",
                "HKP2000",
                "Tec9",

                //Heavy
                "Nova",
                "XM1014",
                "Sawed-Off",
                "Mag-7",

                //SMG
                "MAC10",
                "MP9",
                "MP7",
                "UMP45",
                "PP-Bizon",
                "P90",

                //Rifles
                "Galil AR",
                "AK47",
                "SG556",
                "Famas",
                "M4A1",
                "Aug",

                //Snipers
                "AWP",
                "SSG08",
                "Scar-20",
                "G3SG1",

                //Machine Guins
                "M249",
                "Negev",

                //Default for unkown weapons
                "Default"

            };

            StringBuilder builder = new StringBuilder();
            //Comments
            builder.AppendLine(";Smurf Bot Config.");
            builder.AppendLine(";Key codes can be found here: http://pastebin.com/kr8PT0Hh");
            builder.AppendLine(";Just remove the 0x part. 0x01 > 01");
            builder.AppendLine(";Trigger When Scoped is only good on weapons with scopes.");

            //Misc
            builder.AppendLine("[Bunny Jump]");
            builder.AppendLine("Bunny Jump Enabled = True");
            builder.AppendLine("Bunny Jump Jumps = 5");
            builder.AppendLine("Bunny Jump Key = 32").AppendLine();

            //Sound ESP
            builder.AppendLine("[Sound Esp]");
            builder.AppendLine("Sound Enabled = False");
            builder.AppendLine("Sound Key = 0");
            builder.AppendLine("Sound Range = 0");
            builder.AppendLine("Sound Intverval = 0");
            builder.AppendLine("Sound Voulme = 0").AppendLine();

            foreach (var weapon in weaponList)
            {
                builder.AppendLine("[" + weapon + "]");
                builder.AppendLine("Aim Enabled = True");
                builder.AppendLine("Aim Start = 1");
                builder.AppendLine("Aim Key = 01");
                builder.AppendLine("Aim Toggle = False");
                builder.AppendLine("Aim Hold = True");
                builder.AppendLine("Aim When Scoped = False");
                builder.AppendLine("Aim Fov = 2");
                builder.AppendLine("Aim Bone = 4");
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
                builder.AppendLine("Trigger Key = 18");
                builder.AppendLine("Trigger Toggle = False");
                builder.AppendLine("Trigger Hold = True");
                builder.AppendLine("Trigger When Scoped = False");
                builder.AppendLine("Trigger Enemies = True");
                builder.AppendLine("Trigger Allies = True");
                builder.AppendLine("Trigger Burst Enabled = False");
                builder.AppendLine("Trigger Burst Shots = 0");
                builder.AppendLine("Trigger Burst Randomize = False");
                builder.AppendLine("Trigger Delay FirstShot = 21");
                builder.AppendLine("Trigger Delay Shots = 21").AppendLine();
            }
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