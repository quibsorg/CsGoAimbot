using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            PrintSuccess("Smurf bot");
            KeyUtils = new KeyUtils();
            ConfigUtils = new CsgoConfigUtils();

            //Aim
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
            ConfigUtils.KeySettings.Add("aimKey");
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
                ConfigUtils.SaveSettingsToFile("Config.cfg");
            ConfigUtils.ReadSettingsFromFile("Config.cfg");

            PrintInfo("> Waiting for CSGO to start up...");
            while (!ProcUtils.ProcessIsRunning(GameProcess))
                Thread.Sleep(250);

            ProcUtils = new ProcUtils(GameProcess, WinAPI.ProcessAccessFlags.VirtualMemoryRead | WinAPI.ProcessAccessFlags.VirtualMemoryWrite | WinAPI.ProcessAccessFlags.VirtualMemoryOperation);
            MemUtils = new MemUtils();
            MemUtils.Handle = ProcUtils.Handle;

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
                InitializeComponents();
                SharpDXRenderer renderer = ShdxOverlay.Renderer;
                TextFormat smallFont = renderer.CreateFont("smallFont", "Century Gothic", 10f);
                TextFormat largeFont = renderer.CreateFont("largeFont", "Century Gothic", 14f);
                TextFormat heavyFont = renderer.CreateFont("heavyFont", "Century Gothic", 14f, FontStyle.Normal, FontWeight.Heavy);

                _windowMenu.Font = smallFont;
                _windowMenu.Caption.Font = largeFont;
                _windowGraphs.Font = smallFont;
                _windowGraphs.Caption.Font = largeFont;
                _windowSpectators.Font = smallFont;
                _windowSpectators.Caption.Font = largeFont;
                _windowBots.Font = smallFont;
                _windowBots.Caption.Font = largeFont;
                _graphMemRead.Font = smallFont;
                _graphMemWrite.Font = smallFont;

                _windowMenu.ApplySettings(ConfigUtils);

                ShdxOverlay.ChildControls.Add(_ctrlCrosshair);
                ShdxOverlay.ChildControls.Add(_windowMenu);
                ShdxOverlay.ChildControls.Add(_windowGraphs);
                ShdxOverlay.ChildControls.Add(_windowSpectators);
                ShdxOverlay.ChildControls.Add(_windowBots);
                ShdxOverlay.ChildControls.Add(_cursor);
                PrintInfo("> Running overlay");
                System.Windows.Forms.Application.Run();
            }
            ConfigUtils.SaveSettingsToFile("Config.cfg");
        }

        private static void overlay_TickEvent(object sender, SharpDXOverlay.DeltaEventArgs e)
        {
            _seconds += e.SecondsElapsed;
            //Update logic
            KeyUtils.Update();
            Framework.Update();
            ShdxOverlay.UpdateControls(e.SecondsElapsed, KeyUtils);

            //Process input
            if (KeyUtils.KeyWentUp(WinAPI.VirtualKeyShort.DELETE))
                ShdxOverlay.Kill();
            if (KeyUtils.KeyWentUp(WinAPI.VirtualKeyShort.INSERT))
                Framework.MouseEnabled = !Framework.MouseEnabled;
            if (KeyUtils.KeyWentUp(WinAPI.VirtualKeyShort.HOME))
                _windowMenu.Visible = !_windowMenu.Visible;

            //Update UI
            _cursor.Visible = !Framework.MouseEnabled;

            if (_seconds >= 1)
            {
                _seconds = 0;
                _graphMemRead.AddValue(MemUtils.BytesRead);
                _graphMemWrite.AddValue(MemUtils.BytesWritten);
            }

            _ctrlCrosshair.X = ShdxOverlay.Width / 2f;
            _ctrlCrosshair.Y = ShdxOverlay.Height / 2f;


            _labelAimbot.Text = string.Format("Aimbot: {0}", Framework.AimbotActive ? "ON" : "OFF");
            _labelAimbot.ForeColor = Framework.AimbotActive ? SharpDX.Color.Green : _windowBots.Caption.ForeColor;
            _labelTriggerbot.Text = string.Format("Triggerbot: {0}", Framework.TriggerbotActive ? "ON" : "OFF");
            _labelTriggerbot.ForeColor = Framework.TriggerbotActive ? SharpDX.Color.Green : _windowBots.Caption.ForeColor;

            if (_ctrlCrosshair.PrimaryColor.ToRgba() != _colorControlCrosshairPrimary.SDXColor.ToRgba())
                _ctrlCrosshair.PrimaryColor = _colorControlCrosshairPrimary.SDXColor;
            if (_ctrlCrosshair.SecondaryColor.ToRgba() != _colorControlCrosshairSecondary.SDXColor.ToRgba())
                _ctrlCrosshair.SecondaryColor = _colorControlCrosshairSecondary.SDXColor;

            if (Framework.LocalPlayer != null)
            {
                Weapon wpn = Framework.LocalPlayer.GetActiveWeapon();
                if (wpn != null)
                    _ctrlCrosshair.Spread = wpn.MFAccuracyPenalty * 10000;
                else
                    _ctrlCrosshair.Spread = 1f;
            }
            else { _ctrlCrosshair.Spread = 1f; }
            if (Framework.IsPlaying())
            {
                #region Spectators
                if (Framework.LocalPlayer != null)
                {
                    var spectators = Framework.Players.Where(x => x.Item2.MHObserverTarget == Framework.LocalPlayer.M_IId && x.Item2.MIHealth == 0 && x.Item2.MIDormant != 1);
                    StringBuilder builder = new StringBuilder();
                    foreach (Tuple<int, CsPlayer> spec in spectators)
                    {
                        CsPlayer player = spec.Item2;
                        builder.AppendFormat("{0} [{1}]{2}", Framework.Names[player.M_IId], (SpectatorView)player.MIObserverMode, builder.Length > 0 ? "\n" : "");
                    }
                    if (builder.Length > 0)
                        _labelSpectators.Text = builder.ToString();
                    else
                        _labelSpectators.Text = "<none>";
                }
                else
                {
                    _labelSpectators.Text = "<none>";
                }
                #endregion
            }
            else
            {
                _labelSpectators.Text = "<none>";
                //ctrlRadar.Visible = false;
            }
        }

        private static void InitializeComponents()
        {
            PrintInfo("> Initializing controls");

            _cursor = new SharpDXCursor();

            _windowGraphs = new SharpDXWindow();
            _windowGraphs.Caption.Text = "Performance";

            _graphMemRead = new SharpDXGraph();
            _graphMemRead.DynamicMaximum = true;
            _graphMemRead.Width = 256;
            _graphMemRead.Height = 48;
            _graphMemRead.Text = "RPM data/s";
            _graphMemWrite = new SharpDXGraph();
            _graphMemWrite.DynamicMaximum = true;
            _graphMemWrite.Width = 256;
            _graphMemWrite.Height = 48;
            _graphMemWrite.Text = "WPM data/s";

            _windowGraphs.Panel.AddChildControl(_graphMemRead);
            _windowGraphs.Panel.AddChildControl(_graphMemWrite);  

            _windowMenu = new SharpDXWindow();
            _windowMenu.Caption.Text = "[UC|CSGO] Zat's Multihack v3";
            _windowMenu.X = 500;
            _windowMenu.Panel.DynamicWidth = true;

            InitLabel(ref _labelHotkeys, "[INS] Toggle mouse [HOME] Toggle menu\n[DEL] Terminate hack", false, 150, SharpDXLabel.TextAlignment.Center);

            _tabsMenu = new SharpDXTabControl();
            _tabsMenu.FillParent = false;

            InitPanel(ref _panelEspContent, "ESP", false, true, true, false);
            _panelEspContent.ContentLayout = new TableLayout(2);
            InitCheckBox(ref _checkBoxEspEnabled, "Enabled", "espEnabled", true);
            InitCheckBox(ref _checkBoxEspBox, "Draw box", "espBox", false);
            InitCheckBox(ref _checkBoxEspSkeleton, "Draw skeleton", "espSkeleton", true);
            InitCheckBox(ref _checkBoxEspName, "Draw name", "espName", false);
            InitCheckBox(ref _checkBoxEspHealth, "Draw health", "espHealth", true);
            InitCheckBox(ref _checkBoxEspAllies, "Filter: Draw allies", "espAllies", true);
            InitCheckBox(ref _checkBoxEspEnemies, "Filter: Draw enemies", "espEnemies", true);

            InitPanel(ref _panelAimContent, "Aim", false, true, true, false);
            _panelAimContent.ContentLayout = TableLayout.TwoColumns;
            InitCheckBox(ref _checkBoxAimEnabled, "Enabled", "aimEnabled", true);
            InitCheckBox(ref _checkBoxAimDrawFov, "Draw fov", "aimDrawFov", true);
            InitButtonKey(ref _keyAimKey, "Key", "aimKey");
            InitTrackBar(ref _trackBarAimFov, "Aimbot FOV", "aimFov", 1, 180, 20, 0);
            InitRadioButton(ref _radioAimHold, "Mode: Hold key", "aimHold", true);
            InitRadioButton(ref _radioAimToggle, "Mode: Toggle", "aimToggle", false);
            InitCheckBox(ref _checkBoxAimSmoothEnaled, "Smoothing", "aimSmoothEnabled", true);
            InitTrackBar(ref _trackBarAimSmoothValue, "Smooth-factor", "aimSmoothValue", 0, 1, 0.2f, 4);
            InitCheckBox(ref _checkBoxAimFilterSpotted, "Filter: Spotted by me", "aimFilterSpotted", false);
            InitCheckBox(ref _checkBoxAimFilterSpottedBy, "Filter: Spotted me", "aimFilterSpottedBy", false);
            InitCheckBox(ref _checkBoxAimFilterEnemies, "Filter: Enemies", "aimFilterEnemies", true);
            InitCheckBox(ref _checkBoxAimFilterAllies, "Filter: Allies", "aimFilterAllies", false);
            InitComboValue(ref _comboValueAimBone, "Bone", "aimBone", new Tuple<string, int>("Neck", 10), new Tuple<string, int>("Chest", 4), new Tuple<string, int>("Hips", 1));

            InitPanel(ref _panelRcsContent, "RCS", false, true, true, false);
            InitCheckBox(ref _checkBoxRcsEnabled, "Enabled", "rcsEnabled", true);
            InitTrackBar(ref _trackBarRcsForce, "Force (%)", "rcsForce", 1, 100, 100, 2);
            
            InitPanel(ref _panelTriggerContent, "Trigger", false, true, true, false);
            _panelTriggerContent.ContentLayout = TableLayout.TwoColumns;
            InitCheckBox(ref _checkBoxTriggerEnabled, "Enabled", "triggerEnabled", true);
            InitButtonKey(ref _keyTriggerKey, "Key", "triggerKey");
            InitTrackBar(ref _trackBarTriggerDelayFirstShot, "Delay (ms, first shot)", "triggerDelayFirstShot", 1, 1000, 30, 0);
            InitTrackBar(ref _trackBarTriggerDelayShots, "Delay (ms)", "triggerDelayShots", 1, 1000, 30, 0);
            InitRadioButton(ref _radioTriggerHold, "Mode: Hold key", "triggerHold", true);
            InitRadioButton(ref _radioTriggerToggle, "Mode: Toggle", "triggerToggle", false);
            InitCheckBox(ref _checkBoxTriggerFilterEnemies, "Filter: Enemies", "triggerFilterEnemies", true);
            InitCheckBox(ref _checkBoxTriggerFilterAllies, "Filter: Allies", "triggerFilterAllies", false);
            InitCheckBox(ref _checkBoxTriggerBurstEnabled, "Burst enabled", "triggerBurstEnabled", true);
            InitCheckBox(ref _checkBoxTriggerBurstRandomize, "Burst randomization", "triggerBurstRandomize", true);
            InitTrackBar(ref _trackBarTriggerBurstShots, "No. of burst-fire shots", "triggerBurstShots", 1, 10, 3, 0);

            InitPanel(ref _panelRadarContent, "Radar", false, true, true, false);
            _panelRadarContent.ContentLayout = TableLayout.ThreeColumns;
            InitCheckBox(ref _checkBoxRadarEnabled, "Enabled", "radarEnabled", true);
            InitCheckBox(ref _checkBoxRadarAllies, "Filter: Draw allies", "radarAllies", true);
            InitCheckBox(ref _checkBoxRadarEnemies, "Filter: Draw enemies", "radarEnemies", true);
            InitTrackBar(ref _trackBarRadarScale, "Scale", "radarScale", 0, 0.25f, 0.02f, 4);
            InitTrackBar(ref _trackBarRadarWidth, "Width (px)", "radarWidth", 16, 1024, 256, 0);
            InitTrackBar(ref _trackBarRadarHeight, "Height (px)", "radarHeight", 16, 1024, 256, 0);

            InitPanel(ref _panelCrosshairContent, "Crosshair", false, true, true, false);
            _panelCrosshairContent.ContentLayout = TableLayout.TwoColumns;
            InitCheckBox(ref _checkBoxCrosshairEnabled, "Enabled", "crosshairEnabled", true);
            InitTrackBar(ref _trackBarCrosshairRadius, "Radius (px)", "crosshairRadius", 1, 128, 16f, 1);
            InitTrackBar(ref _trackBarCrosshairWidth, "Width (px)", "crosshairWidth", 0.1f, 32f, 1f, 1);
            InitTrackBar(ref _trackBarCrosshairSpreadScale, "Spread-scale", "crosshairSpreadScale", 0.01f, 1f, 1f, 2);
            InitCheckBox(ref _checkBoxCrosshairOutline, "Outline", "crosshairOutline", true);
            InitComboValue(ref _comboValueCrosshairType, "Type", "crosshairType", 
                new Tuple<string, int>("Default", 0),
                new Tuple<string, int>("Default tilted", 1),
                new Tuple<string, int>("Rectangle", 2),
                new Tuple<string, int>("Rectangle tilted", 3),
                new Tuple<string, int>("Circle", 4));
            InitColorControl(ref _colorControlCrosshairPrimary, "Primary color", "crosshairPrimaryColor", new Color(255, 255, 255, 255));
            InitColorControl(ref _colorControlCrosshairSecondary, "Secondary color", "crosshairSecondaryColor", new Color(255, 255, 255, 255));


            InitPanel(ref _panelWindows, "Windows", true, true, true, true);
            InitCheckBox(ref _checkBoxGraphsEnabled, "Performance-window enabled", "windowPerformanceEnabled", true);
            InitCheckBox(ref _checkBoxSpectatorsEnabled, "Spectators-window enabled", "windowSpectatorsEnabled", true);
            InitCheckBox(ref _checkBoxBotsEnabled, "Bots-window enabled", "windowBotsEnabled", true);
            InitCheckBox(ref _checkBoxEnemiesEnabled, "Enemies-info enaled", "windowEnemiesEnabled", true);

            _tabsMenu.AddChildControl(_panelAimContent);
            _tabsMenu.AddChildControl(_panelEspContent);
            _tabsMenu.AddChildControl(_panelRadarContent);
            _tabsMenu.AddChildControl(_panelRcsContent);
            _tabsMenu.AddChildControl(_panelTriggerContent);
            _tabsMenu.AddChildControl(_panelCrosshairContent);
            _tabsMenu.AddChildControl(_panelWindows);
            _windowMenu.Panel.AddChildControl(_labelHotkeys);
            _windowMenu.Panel.AddChildControl(_tabsMenu);

            _panelEspContent.AddChildControl(_checkBoxEspEnabled);
            _panelEspContent.AddChildControl(_checkBoxEspBox);
            _panelEspContent.AddChildControl(_checkBoxEspSkeleton);
            _panelEspContent.AddChildControl(_checkBoxEspName);
            _panelEspContent.AddChildControl(_checkBoxEspHealth);
            _panelEspContent.AddChildControl(_checkBoxEspAllies);
            _panelEspContent.AddChildControl(_checkBoxEspEnemies);

            _panelAimContent.AddChildControl(_checkBoxAimEnabled);
            _panelAimContent.AddChildControl(_checkBoxAimSmoothEnaled);
            _panelAimContent.AddChildControl(_trackBarAimFov);
            _panelAimContent.AddChildControl(_trackBarAimSmoothValue);
            _panelAimContent.AddChildControl(_radioAimHold);
            _panelAimContent.AddChildControl(_radioAimToggle);
            _panelAimContent.AddChildControl(_checkBoxAimFilterSpotted);
            _panelAimContent.AddChildControl(_checkBoxAimFilterSpottedBy);
            _panelAimContent.AddChildControl(_checkBoxAimFilterEnemies);
            _panelAimContent.AddChildControl(_checkBoxAimFilterAllies);
            _panelAimContent.AddChildControl(_checkBoxAimDrawFov);
            _panelAimContent.AddChildControl(_comboValueAimBone);

            _panelAimContent.AddChildControl(_keyAimKey);

            _panelRcsContent.AddChildControl(_checkBoxRcsEnabled);
            _panelRcsContent.AddChildControl(_trackBarRcsForce);

            _panelTriggerContent.AddChildControl(_checkBoxTriggerEnabled);
            _panelTriggerContent.AddChildControl(_keyTriggerKey);
            _panelTriggerContent.AddChildControl(_trackBarTriggerDelayFirstShot);
            _panelTriggerContent.AddChildControl(_trackBarTriggerDelayShots);
            _panelTriggerContent.AddChildControl(_radioTriggerHold);
            _panelTriggerContent.AddChildControl(_radioTriggerToggle);
            _panelTriggerContent.AddChildControl(_checkBoxTriggerFilterEnemies);
            _panelTriggerContent.AddChildControl(_checkBoxTriggerFilterAllies);
            _panelTriggerContent.AddChildControl(_checkBoxTriggerBurstEnabled);
            _panelTriggerContent.AddChildControl(_checkBoxTriggerBurstRandomize);
            _panelTriggerContent.AddChildControl(_trackBarTriggerBurstShots);

            _panelRadarContent.AddChildControl(_checkBoxRadarEnabled);
            _panelRadarContent.AddChildControl(_checkBoxRadarAllies);
            _panelRadarContent.AddChildControl(_checkBoxRadarEnemies);
            _panelRadarContent.AddChildControl(_trackBarRadarScale);
            _panelRadarContent.AddChildControl(_trackBarRadarWidth);
            _panelRadarContent.AddChildControl(_trackBarRadarHeight);

            _panelCrosshairContent.AddChildControl(_checkBoxCrosshairEnabled);
            _panelCrosshairContent.AddChildControl(_checkBoxCrosshairOutline);
            _panelCrosshairContent.AddChildControl(_trackBarCrosshairRadius);
            _panelCrosshairContent.AddChildControl(_trackBarCrosshairWidth);
            _panelCrosshairContent.AddChildControl(_trackBarCrosshairSpreadScale);
            _panelCrosshairContent.AddChildControl(_comboValueCrosshairType);
            _panelCrosshairContent.AddChildControl(_colorControlCrosshairPrimary);
            _panelCrosshairContent.AddChildControl(_colorControlCrosshairSecondary);

            _panelWindows.AddChildControl(_checkBoxGraphsEnabled);
            _panelWindows.AddChildControl(_checkBoxSpectatorsEnabled);
            _panelWindows.AddChildControl(_checkBoxBotsEnabled);
            _panelWindows.AddChildControl(_checkBoxEnemiesEnabled);

            _windowSpectators = new SharpDXWindow();
            _windowSpectators.Caption.Text = "Spectators";
            _windowSpectators.Y = 500;
            InitLabel(ref _labelSpectators, "<none>", false, 200f, SharpDXLabel.TextAlignment.Left);
            _windowSpectators.Panel.AddChildControl(_labelSpectators);

            _windowBots = new SharpDXWindow();
            _windowBots.Caption.Text = "Bot-status";
            _windowBots.Y = 300;
            InitLabel(ref _labelAimbot, "Aimbot: -", false, 200f, SharpDXLabel.TextAlignment.Left);
            InitLabel(ref _labelTriggerbot, "Triggerbot: -", false, 200f, SharpDXLabel.TextAlignment.Left);
            _windowBots.Panel.AddChildControl(_labelAimbot);
            _windowBots.Panel.AddChildControl(_labelTriggerbot);

            _ctrlCrosshair = new Crosshair();
            _ctrlCrosshair.Radius = 16f;
            _ctrlCrosshair.Width = 2f;
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
        private static void InitColorControl(ref SharpDXColorControl control, string text, object tag, Color color)
        {
            control = new SharpDXColorControl();
            control.Text = text;
            control.Tag = tag;
            control.Color = color;
            control.ColorChangedEvent += control_ColorChangedEvent;
        }
        
        private static void InitComboValue<T>(ref SharpDXComboValue<T> control, string text, object tag, params Tuple<string, T>[] values)
        {
            control = new SharpDXComboValue<T>();
            control.Text = text;
            control.Tag = tag;
            control.Values = values;
            control.SelectedIndexChangedEvent += comboValue_SelectedIndexChangedEvent;
        }
        private static void InitButtonKey(ref SharpDXButtonKey control, string text, object tag)
        {
            control = new SharpDXButtonKey();
            control.Text = text;
            control.Tag = tag;
            control.KeyChangedEvent += buttonKey_KeyChangedEvent;
        }
        private static void InitPanel(ref SharpDXPanel control, string text, bool dynamicWidth = true, bool dynamicHeight = true, bool fillParent = true, bool visible = true)
        {
            control = new SharpDXPanel();
            control.Text = text;
            control.DynamicHeight = dynamicHeight;
            control.DynamicWidth = dynamicWidth;
            control.FillParent = fillParent;
            control.Visible = visible;
        }
        private static void InitToggleButton(ref SharpDXButton control, string text, SharpDXPanel tag)
        {
            control = new SharpDXButton();
            control.Text = text;
            control.Tag = tag;
            control.MouseClickEventUp += button_MouseClickEventUp;
        }
        private static void InitTrackBar(ref SharpDXTrackbar control, string text, object tag, float min =0, float max = 100, float value = 50, int numberofdecimals = 2)
        {
            control = new SharpDXTrackbar();
            control.Text = text;
            control.Tag = tag;
            control.Minimum = min;
            control.Maximum = max;
            control.Value = value;
            control.NumberOfDecimals = numberofdecimals;
            control.ValueChangedEvent += trackBar_ValueChangedEvent;
        }

        private static void InitRadioButton(ref SharpDXRadioButton control, string text, object tag, bool bChecked)
        {
            control = new SharpDXRadioButton();
            control.Text = text;
            control.Tag = tag;
            control.Checked = bChecked;
            control.CheckedChangedEvent += radioButton_CheckedChanged;
        }
        private static void InitLabel(ref SharpDXLabel control, string text, bool fixedWidth = false, float width = 0f, SharpDXLabel.TextAlignment alignment = SharpDXLabel.TextAlignment.Left)
        {
            control = new SharpDXLabel();
            control.FixedWidth = fixedWidth;
            control.Width = width;
            control.TextAlign = alignment;
            control.Text = text;
            control.Tag = null;
        }
        private static void InitCheckBox(ref SharpDXCheckBox control, string text, object tag, bool bChecked)
        {
            control = new SharpDXCheckBox();
            control.Text = text;
            control.Tag = tag;
            control.Checked = bChecked;
            control.CheckedChangedEvent += checkBox_CheckedChanged;
        }
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
