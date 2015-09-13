using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ExternalUtilsCSharp;

namespace CsGoApplicationAimbot
{
    public class CsgoConfigUtils : ConfigUtils
    {
        #region PROPERTIES
        public List<string> UIntegerSettings { get; set; }
        public List<string> IntegerSettings { get; set; }
        public List<string> FloatSettings { get; set; }
        public List<string> KeySettings { get; set; }
        public List<string> BooleanSettings { get; set; }
        #endregion

        #region CONSTRUCTOR
        public CsgoConfigUtils() : base()
        {
            IntegerSettings = new List<string>();
            UIntegerSettings = new List<string>();
            FloatSettings = new List<string>();
            KeySettings = new List<string>();
            BooleanSettings = new List<string>();
        }
        #endregion

        #region METHODS
        public void FillDefaultValues()
        {
            foreach (string integerV in IntegerSettings)
                SetValue(integerV, 0);
            foreach (string uintegerV in UIntegerSettings)
                SetValue(uintegerV, 0);
            foreach (string floatV in FloatSettings)
                SetValue(floatV, 0f);
            foreach (string keyV in KeySettings)
                SetValue(keyV, WinAPI.VirtualKeyShort.LBUTTON);
            foreach (string booleanV in BooleanSettings)
                SetValue(booleanV, false);
        }
        public override void ReadSettings(byte[] data)
        {
            string text = Encoding.Unicode.GetString(data);

            //Split text into lines
            string[] lines = text.Contains("\r\n") ? text.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries) : text.Split("\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

            foreach (string line in lines)
            {
                //Trim current line
                string tmpLine = line.Trim();
                //Skip invalid ones
                if (tmpLine.StartsWith("#")) // comment
                    continue;
                else if (!tmpLine.Contains("=")) // it's no key-value pair!
                    continue;

                //Trim both parts of the key-value pair
                string[] parts = tmpLine.Split('=');
                parts[0] = parts[0].Trim();
                parts[1] = parts[1].Trim();
                if (string.IsNullOrEmpty(parts[0]) || string.IsNullOrEmpty(parts[1]))
                    continue;
                if (parts[1].Contains('#')) //If value-part contains comment, split it
                    parts[1] = parts[1].Split('#')[0];
                InterpretSetting(parts[0], parts[1]);
            }
        }

        private void InterpretSetting(string name, string value)
        {
            try
            {
                if (FloatSettings.Contains(name))
                    SetValue(name, Convert.ToSingle(value));
                else if (IntegerSettings.Contains(name))
                    SetValue(name, Convert.ToInt32(value));
                else if (UIntegerSettings.Contains(name))
                    SetValue(name, Convert.ToUInt32(value));
                else if (BooleanSettings.Contains(name))
                    SetValue(name, Convert.ToBoolean(value));
                else if (KeySettings.Contains(name))
                    SetValue(name, ParseEnum<WinAPI.VirtualKeyShort>(value));
                else
                    Program.PrintError("Unknown settings-field \"{0}\" (value: \"{1}\")", name, value);
            }
            catch(Exception ex)
            {
                Program.PrintException(ex);
            }
        }

        public override byte[] SaveSettings()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine(@"#Smurf Bot 0.4");
            builder.AppendLine(@"#Made By Carlsson");
            object[] keys = new object[GetKeys().Count];
            GetKeys().CopyTo(keys, 0);
            var keysSorted = keys.OrderBy(x => x);
            foreach (string key in keysSorted)
            {
                builder.AppendFormat("{0} = {1}\n", key, GetValue(key));
            }
            return Encoding.Unicode.GetBytes(builder.ToString());
        }
        #endregion
    }
}
