using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using CsGoApplicationAimbot.CSGO.Enums;
using CsGoApplicationAimbot.CSGOClasses;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.SharpDXRenderer;
using ExternalUtilsCSharp.SharpDXRenderer.Controls;
using ExternalUtilsCSharp.SharpDXRenderer.Controls.Crosshairs;
using ExternalUtilsCSharp.SharpDXRenderer.Controls.Layouts;
using ExternalUtilsCSharp.UI.UIObjects;
using SharpDX.DirectWrite;

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
        private static double _seconds = 0;
        public static Framework Framework;
        public static ProcUtils ProcUtils;
        public static MemUtils MemUtils;
        public static CsgoConfigUtils ConfigUtils;
        #endregion

        #region CONTROLS
        public static SharpDXOverlay ShdxOverlay;

        private static SharpDXCursor _cursor;
        //Menu-window
        private static SharpDXWindow _windowMenu;
        private static SharpDXTabControl _tabsMenu;

        private static SharpDXLabel _labelHotkeys;
        private static SharpDXPanel _panelEspContent;
        private static SharpDXCheckBox _checkBoxEspEnabled;
        private static SharpDXCheckBox _checkBoxEspBox;
        private static SharpDXCheckBox _checkBoxEspSkeleton;
        private static SharpDXCheckBox _checkBoxEspName;
        private static SharpDXCheckBox _checkBoxEspHealth;
        private static SharpDXCheckBox _checkBoxEspAllies;
        private static SharpDXCheckBox _checkBoxEspEnemies;

        private static SharpDXPanel _panelAimContent;
        private static SharpDXCheckBox _checkBoxAimEnabled;
        private static SharpDXCheckBox _checkBoxAimDrawFov;
        private static SharpDXCheckBox _checkBoxAimFilterSpotted;
        private static SharpDXCheckBox _checkBoxAimFilterSpottedBy;
        private static SharpDXCheckBox _checkBoxAimFilterEnemies;
        private static SharpDXCheckBox _checkBoxAimFilterAllies;
        private static SharpDXRadioButton _radioAimToggle;
        private static SharpDXRadioButton _radioAimHold;
        private static SharpDXTrackbar _trackBarAimFov;
        private static SharpDXCheckBox _checkBoxAimSmoothEnaled;
        private static SharpDXTrackbar _trackBarAimSmoothValue;
        private static SharpDXButtonKey _keyAimKey;
        private static SharpDXComboValue<int> _comboValueAimBone;

        private static SharpDXPanel _panelRcsContent;
        private static SharpDXCheckBox _checkBoxRcsEnabled;
        private static SharpDXTrackbar _trackBarRcsForce;

        private static SharpDXPanel _panelTriggerContent;
        private static SharpDXCheckBox _checkBoxTriggerEnabled;
        private static SharpDXCheckBox _checkBoxTriggerFilterEnemies;
        private static SharpDXCheckBox _checkBoxTriggerFilterAllies;
        private static SharpDXRadioButton _radioTriggerToggle;
        private static SharpDXRadioButton _radioTriggerHold;
        private static SharpDXButtonKey _keyTriggerKey;
        private static SharpDXTrackbar _trackBarTriggerDelayFirstShot;
        private static SharpDXTrackbar _trackBarTriggerDelayShots;
        private static SharpDXCheckBox _checkBoxTriggerBurstEnabled;
        private static SharpDXCheckBox _checkBoxTriggerBurstRandomize;
        private static SharpDXTrackbar _trackBarTriggerBurstShots;

        private static SharpDXPanel _panelRadarContent;
        private static SharpDXCheckBox _checkBoxRadarEnabled;
        private static SharpDXCheckBox _checkBoxRadarAllies;
        private static SharpDXCheckBox _checkBoxRadarEnemies;
        private static SharpDXTrackbar _trackBarRadarScale;
        private static SharpDXTrackbar _trackBarRadarWidth;
        private static SharpDXTrackbar _trackBarRadarHeight;

        private static SharpDXPanel _panelCrosshairContent;
        private static SharpDXCheckBox _checkBoxCrosshairEnabled;
        private static SharpDXTrackbar _trackBarCrosshairRadius;
        private static SharpDXTrackbar _trackBarCrosshairWidth;
        private static SharpDXTrackbar _trackBarCrosshairSpreadScale;
        private static SharpDXCheckBox _checkBoxCrosshairOutline;
        private static SharpDXComboValue<int> _comboValueCrosshairType;
        private static SharpDXColorControl _colorControlCrosshairPrimary;
        private static SharpDXColorControl _colorControlCrosshairSecondary;

        private static SharpDXPanel _panelWindows;
        private static SharpDXCheckBox _checkBoxGraphsEnabled;
        private static SharpDXCheckBox _checkBoxSpectatorsEnabled;
        private static SharpDXCheckBox _checkBoxBotsEnabled;
        private static SharpDXCheckBox _checkBoxEnemiesEnabled;

        //Performance-window
        private static SharpDXWindow _windowGraphs;
        private static SharpDXGraph _graphMemRead;
        private static SharpDXGraph _graphMemWrite;

        //Spectators-window
        private static SharpDXWindow _windowSpectators;
        private static SharpDXLabel _labelSpectators;

        //Aimbot/Triggerbot window
        private static SharpDXWindow _windowBots;
        private static SharpDXLabel _labelAimbot;
        private static SharpDXLabel _labelTriggerbot;

        //Others
        private static Crosshair _ctrlCrosshair;
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
            _seconds += e.SecondsElapsed;
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

        static void checkBox_CheckedChanged(object sender, EventArgs e)
        {
            SharpDXCheckBox control = (SharpDXCheckBox)sender;
            ConfigUtils.SetValue(control.Tag.ToString(), control.Checked);
        }
        private static void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            SharpDXRadioButton control = (SharpDXRadioButton)sender;
            ConfigUtils.SetValue(control.Tag.ToString(), control.Checked);
        }
        private static void trackBar_ValueChangedEvent(object sender, EventArgs e)
        {
            SharpDXTrackbar control = (SharpDXTrackbar)sender;
            ConfigUtils.SetValue(control.Tag.ToString(), control.Value);
        }
        static void buttonKey_KeyChangedEvent(object sender, EventArgs e)
        {
            SharpDXButtonKey control = (SharpDXButtonKey)sender;
            ConfigUtils.SetValue(control.Tag.ToString(), control.Key);
        }
        static void button_MouseClickEventUp(object sender, ExternalUtilsCSharp.UI.Control<SharpDXRenderer, SharpDX.Color, SharpDX.Vector2, TextFormat>.MouseEventArgs e)
        {
            if (!e.LeftButton)
                return;
            SharpDXPanel panel = (SharpDXPanel)((SharpDXButton)sender).Tag;
            panel.Visible = !panel.Visible;
        }

        private static void comboValue_SelectedIndexChangedEvent<T>(object sender, SharpDXComboValue<T>.ComboValueEventArgs e)
        {
            ConfigUtils.SetValue(e.Tag.ToString(), e.Value);
        }

        static void control_ColorChangedEvent(object sender, EventArgs e)
        {
            SharpDXColorControl control = (SharpDXColorControl)sender;
            ConfigUtils.SetValue(control.Tag.ToString(), control.Color.ToRGBA());
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
