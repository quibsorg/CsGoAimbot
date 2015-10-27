using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CsGoApplicationAimbot.CSGOClasses.Enums;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class Framework
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        #region Constructor
        public Framework(ProcessModule engineDll, ProcessModule clientDll)
        {
            Scanner.ScanOffsets(clientDll, engineDll, Program.MemUtils);
            _clientDllBase = (int)clientDll.BaseAddress;
            _engineDllBase = (int)engineDll.BaseAddress;
            _entityList = _clientDllBase + Offsets.Misc.EntityList;
            _viewMatrix = _clientDllBase + Offsets.Misc.ViewMatrix;
            _clientState = Program.MemUtils.Read<int>((IntPtr)(_engineDllBase + Offsets.ClientState.Base));
            AimbotActive = false;
            TriggerbotActive = false;
        }
        #endregion

        #region Variables
        readonly SettingsConfig _settings = new SettingsConfig();
        private readonly int _entityList;
        private readonly int _viewMatrix;
        private readonly int _clientState;
        private readonly int _clientDllBase;
        private readonly int _engineDllBase;
        private int _localPlayer;
        private long _lastBeep;
        #endregion

        #region Properties
        private LocalPlayer LocalPlayer { get; set; }
        private string WeaponSection { get; set; }
        private Tuple<int, Player>[] Players { get; set; }
        private Tuple<int, BaseEntity>[] Entities { get; set; }
        public Tuple<int, Weapon>[] Weapons { get; private set; }
        private Matrix ViewMatrix { get; set; }
        private Vector3 ViewAngles { get; set; }
        private Vector3 NewViewAngles { get; set; }
        private SignOnState State { get; set; }
        private bool AimbotActive { get; set; }
        private bool TriggerbotActive { get; set; }
        private int LastShotsFired { get; set; }
        private int LastClip { get; set; }
        private Vector3 LastPunch { get; set; }
        private bool TriggerOnTarget { get; set; }
        private long TriggerLastTarget { get; set; }
        private long TriggerLastShot { get; set; }
        private int TriggerBurstFired { get; set; }
        private int TriggerBurstCount { get; set; }
        private bool TriggerShooting { get; set; }
        private Weapon LocalPlayerWeapon { get; set; }
        private float LastPercent { get; set; }
        private int CurrentJump { get; set; }
        #endregion

        public void Update()
        {
            Console.WriteLine("Updating Framework");

            //If the game processes is not running, close the cheat.
            if (!ProcUtils.ProcessIsRunning(Program.GameProcess))
                Environment.Exit(0);

            //If we are not focus on csgo no reason to update.
            var activeWindow = GetActiveWindowTitle();
            if (activeWindow != Program.GameTitle)
                return;

            var players = new List<Tuple<int, Player>>();
            //var entities = new List<Tuple<int, BaseEntity>>();
            var weapons = new List<Tuple<int, Weapon>>();

            State = (SignOnState)Program.MemUtils.Read<int>((IntPtr)(_clientState + Offsets.ClientState.InGame));
            _localPlayer = Program.MemUtils.Read<int>((IntPtr)(_clientDllBase + Offsets.Misc.LocalPlayer));
            ViewMatrix = Program.MemUtils.ReadMatrix((IntPtr)_viewMatrix, 4, 4);
            ViewAngles = Program.MemUtils.Read<Vector3>((IntPtr)(_clientState + Offsets.ClientState.ViewAngles));


            //If we are not ingame do not update  
            if (State != SignOnState.SignonstateFull)
                return;

            var data = new byte[16 * 8192];
            Program.MemUtils.Read((IntPtr)(_entityList), out data, data.Length);

            for (var i = 0; i < data.Length / 16; i++)
            {
                var address = BitConverter.ToInt32(data, 16 * i);
                if (address == 0) continue;
                var entity = new BaseEntity(address);
                if (!entity.IsValid())
                    continue;
                if (entity.IsPlayer())
                    players.Add(new Tuple<int, Player>(i, new Player(entity)));
                else if (entity.IsWeapon())
                    weapons.Add(new Tuple<int, Weapon>(i, new Weapon(entity)));
                //else
                //    entities.Add(new Tuple<int, BaseEntity>(i, entity));
            }

            Players = players.ToArray();
            //Entities = entities.ToArray();
            Weapons = weapons.ToArray();

            //Check if our player exists
            if (players.Exists(x => x.Item2.Address == _localPlayer))
            {
                LocalPlayer = new LocalPlayer(players.First(x => x.Item2.Address == _localPlayer).Item2);
                LocalPlayerWeapon = LocalPlayer.GetActiveWeapon();
                //Only gets the weapon name and formates it properly and retunrs a string. Used for Weapon Configs
                WeaponSection = LocalPlayer.GetActiveWeaponName();
            }
            if (LocalPlayer == null || LocalPlayer.Health <= 0)
                return;

            NewViewAngles = NewViewAngles.SmoothAngle(ViewAngles, 1f);

            #region Aimbot
            WinAPI.VirtualKeyShort aimKey = _settings.GetKey(WeaponSection, "Aim Key");
            bool aimEnaled = _settings.GetBool(WeaponSection, "Aim Enabled");
            bool aimScoped = _settings.GetBool(WeaponSection, "Aim When Scoped");
            bool aimToggle = _settings.GetBool(WeaponSection, "Aim Toggle");
            bool aimHold = _settings.GetBool(WeaponSection, "Aim Hold");
            int aimStart = _settings.GetInt(WeaponSection, "Aim Start");
            //Won't aim if we do not have any ammo in the clip.
            if (LocalPlayerWeapon != null && (aimEnaled && LocalPlayerWeapon.Clip1 > 0 && LocalPlayer.ShotsFired > aimStart))
            {
                if (aimScoped)
                {
                    if (LocalPlayerWeapon.ZoomLevel > 0)
                    {
                        AimToggleOrHold(aimToggle, aimHold, aimKey);
                    }
                }
                else
                {
                    AimToggleOrHold(aimToggle, aimHold, aimKey);
                }
            }
            #endregion

            #region Rcs
            ControlRecoil();
            #endregion

            #region Trigger
            WinAPI.VirtualKeyShort triggerKey = _settings.GetKey(WeaponSection, "Trigger Key");
            bool triggerEnabled = _settings.GetBool(WeaponSection, "Trigger Enabled");
            bool triggerScoped = _settings.GetBool(WeaponSection, "Trigger When Scoped");
            bool triggerBurstEnabled = _settings.GetBool(WeaponSection, "Trigger Burst Enabled");
            bool triggerToggle = _settings.GetBool(WeaponSection, "Trigger Toggle");
            bool triggerHold = _settings.GetBool(WeaponSection, "Trigger Hold");

            if (triggerEnabled)
            {
                //Trigger Scoped is only good for snipers.
                if (triggerScoped)
                {
                    //ZoomLevel 0 = No Zoom
                    if (LocalPlayerWeapon != null && LocalPlayerWeapon.ZoomLevel > 0)
                    {
                        TriggerToggleOrHold(triggerKey, triggerToggle, triggerHold);
                    }
                }
                else
                {
                    TriggerToggleOrHold(triggerKey, triggerToggle, triggerHold);
                }
            }

            //We set Trigger shooting to true later down the line.
            if (TriggerShooting)
            {
                //If our LocalPlayer weapon is null, do not shoot it will crash.
                if (LocalPlayerWeapon == null)
                {
                    TriggerShooting = false;
                }
                else
                {
                    if (triggerBurstEnabled)
                    {
                        if (TriggerBurstFired >= TriggerBurstCount)
                        {
                            TriggerShooting = false;
                            TriggerBurstFired = 0;
                        }
                        else
                        {
                            if (LocalPlayerWeapon.Clip1 != LastClip)
                            {
                                TriggerBurstFired += Math.Abs(LocalPlayerWeapon.Clip1 - LastClip);
                            }
                            else
                            {
                                Shoot();
                            }
                        }
                    }
                    else
                    {
                        Shoot();
                        TriggerShooting = false;
                    }
                }
            }
            LastClip = LocalPlayerWeapon?.Clip1 ?? 0;
            LastShotsFired = LocalPlayer.ShotsFired;
            LastPunch = LocalPlayer.VecPunch;
            #endregion

            #region Bunny Hop
            BunnyHop();
            #endregion

            #region Sonar
            Sonar();
            #endregion
        }
        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder Buff = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, Buff, nChars) > 0)
            {
                return Buff.ToString();
            }
            return null;
        }
        private void TriggerToggleOrHold(WinAPI.VirtualKeyShort triggerKey, bool triggerToggle, bool triggerHold)
        {
            if (triggerToggle)
            {
                if (Program.KeyUtils.KeyWentUp(triggerKey))
                    TriggerbotActive = !TriggerbotActive;
            }
            else if (triggerHold)
            {
                TriggerbotActive = Program.KeyUtils.KeyIsDown(triggerKey);
            }
            if (TriggerbotActive)
                Triggerbot();
        }
        private void AimToggleOrHold(bool aimToggle, bool aimHold, WinAPI.VirtualKeyShort aimKey)
        {
            if (aimToggle)
            {
                if (Program.KeyUtils.KeyWentUp(aimKey))
                    AimbotActive = !AimbotActive;
            }
            else if (aimHold)
            {
                AimbotActive = Program.KeyUtils.KeyIsDown(aimKey);
            }
            if (AimbotActive)
                ControlAim();
        }

        private void ControlAim()
        {
            bool aimSpotted = _settings.GetBool(WeaponSection, "Aim Spotted");
            bool aimSpottedBy = _settings.GetBool(WeaponSection, "Aim Spotted By");
            bool aimEnemies = _settings.GetBool(WeaponSection, "Aim Enemies");
            bool aimAllies = _settings.GetBool(WeaponSection, "Aim Allies");
            bool aimSmooth = _settings.GetBool(WeaponSection, "Aim Smooth Enabled");
            int aimBone = _settings.GetInt(WeaponSection, "Aim Bone");
            float aimFov = _settings.GetFloat(WeaponSection, "Aim Fov");
            float aimSmoothValue = _settings.GetFloat(WeaponSection, "Aim Smooth Value");

            //if (LocalPlayer == null)
            //    return;

            var valid = Players.Where(x => x.Item2.IsValid() && x.Item2.Health != 0 && x.Item2.Dormant != 1);

            if (aimSpotted)
                valid = valid.Where(x => x.Item2.SeenBy(LocalPlayer));

            if (aimSpottedBy)
                valid = valid.Where(x => LocalPlayer.SeenBy(x.Item2));

            if (aimEnemies)
                valid = valid.Where(x => x.Item2.TeamNum != LocalPlayer.TeamNum);

            if (aimAllies)
                valid = valid.Where(x => x.Item2.TeamNum == LocalPlayer.TeamNum);

            valid = valid.OrderBy(x => (x.Item2.VecOrigin - LocalPlayer.VecOrigin).Length());

            var closest = Vector3.Zero;
            var closestFov = float.MaxValue;
            foreach (var tpl in valid)
            {
                var player = tpl.Item2;

                var newAngles = (LocalPlayer.VecOrigin + LocalPlayer.VecViewOffset).CalcAngle(player.Bones.GetBoneByIndex(aimBone)) - NewViewAngles;
                newAngles = newAngles.ClampAngle();
                var fov = newAngles.Length() % 360f;

                if (!(fov < closestFov) || !(fov < aimFov))
                    continue;

                closestFov = fov;
                closest = newAngles;
            }
            if (closest == Vector3.Zero)
                return;



            if (aimSmooth)
                NewViewAngles = NewViewAngles.SmoothAngle(NewViewAngles + closest, aimSmoothValue);
            else
                NewViewAngles += closest;
            ControlRecoil(true);

            NewViewAngles = NewViewAngles;
        }

        private void ControlRecoil(bool aimbot = false)
        {
            bool rcsEnabled = _settings.GetBool(WeaponSection, "Rcs Enabled");
            float rcsForceMax = _settings.GetFloat(WeaponSection, "Rcs Force Max");
            float rcsForceMin = _settings.GetFloat(WeaponSection, "Rcs Force Min");
            int rcsStart = _settings.GetInt(WeaponSection, "Rcs Start");
            Random random = new Random();

            float randomRcsForce = random.Next((int)rcsForceMin, (int)rcsForceMax);

            if (!rcsEnabled)
                return;

            if (LocalPlayerWeapon == null || LocalPlayerWeapon.Clip1 <= 0)
                return;

            //If we are shooting with the trigger bot, over ride rcsStart.
            if (!TriggerbotActive)
            {
                if (LocalPlayer.ShotsFired <= rcsStart)
                    return;
            }
                                                                        
            if (aimbot)
            {
                NewViewAngles -= LocalPlayer.VecPunch * (2f / 100 * randomRcsForce);
                Program.MemUtils.Write((IntPtr)(_clientState + Offsets.ClientState.ViewAngles), NewViewAngles);
            }
            else
            {
                NewViewAngles -= (LocalPlayer.VecPunch - LastPunch) * (2f / 100 * randomRcsForce);
                Program.MemUtils.Write((IntPtr)(_clientState + Offsets.ClientState.ViewAngles), NewViewAngles);
            }
        }

        private void Triggerbot()
        {
            bool triggerTazer = _settings.GetBool("Misc", "Trigger Taser");
            bool autoKnife = _settings.GetBool("Misc", "Auto Knife");
            bool triggerEnemies = _settings.GetBool(WeaponSection, "Trigger Enemies");
            bool triggerAllies = _settings.GetBool(WeaponSection, "Trigger Allies");
            bool burstRandomize = _settings.GetBool(WeaponSection, "Trigger Burst Randomize");
            float firstShot = _settings.GetFloat(WeaponSection, "Trigger Delay FirstShot");
            float delayShot = _settings.GetFloat(WeaponSection, "Trigger Delay Shots");
            float burstShots = _settings.GetFloat(WeaponSection, "Trigger Burst Shots");

            //If our player is null, or if trigger is shooting we return.
            if (LocalPlayer == null || TriggerShooting)
                return;

            //If no one is in hour crosshair we return.(Not aiming at someone)
            if (Players.Count(x => x.Item2.Id == LocalPlayer.CrosshairIdx) <= 0)
                return;

            //We get the player that is in our crosshair.
            var player = Players.First(x => x.Item2.Id == LocalPlayer.CrosshairIdx).Item2;

            if (triggerTazer)
            {
                if (LocalPlayerWeapon.ClassName == "CWeaponTaser" && LocalPlayer.DistanceToOtherEntityInMetres(player) <= 3)
                {
                    Shoot();
                    return;
                }
            }
            if (autoKnife)
            {
                if (LocalPlayerWeapon.ClassName == "CKnife" && LocalPlayer.DistanceToOtherEntityInMetres(player) <= 1.4f)
                {
                    RightKnife();
                    return;
                }
            }

            //We check the players teamnum, if it matches ours teammate, if not enemy.
            if ((triggerEnemies && player.TeamNum != LocalPlayer.TeamNum) || (triggerAllies && player.TeamNum == LocalPlayer.TeamNum))
            {
                if (LocalPlayerWeapon.ClassName == "CWeaponTaser")
                    return;

                if (LocalPlayerWeapon.ClassName == "CKnife")
                      return;

                //If we got this far, we have a target in our crosshair.
                if (!TriggerOnTarget)
                {
                    TriggerOnTarget = true;
                    TriggerLastTarget = DateTime.Now.Ticks;
                }
                else
                {
                    if (!(new TimeSpan(DateTime.Now.Ticks - TriggerLastTarget).TotalMilliseconds >= firstShot))
                        return;
                    if (!(new TimeSpan(DateTime.Now.Ticks - TriggerLastShot).TotalMilliseconds >= delayShot))
                        return;
                    //Get the tick from our last shoot.
                    TriggerLastShot = DateTime.Now.Ticks;

                    //If we are not shooting.
                    if (!TriggerShooting)
                    {
                        if (burstRandomize)
                            TriggerBurstCount = new Random().Next(1, (int)burstShots);
                        else
                            TriggerBurstCount = (int)burstShots;
                    }
                    TriggerShooting = true;
                }
            }
            else
            {
                //No one is in our crosshair.
                TriggerOnTarget = false;
            }
        }

        private static void Shoot()
        {
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(10);
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTUP, 0, 0, 0, 0);
        }

        private static void RightKnife()
        {
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.RIGHTDOWN, 0, 0, 0, 0);
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.RIGHTUP, 0, 0, 0, 0);
        }

        private void BunnyHop()
        {
            WinAPI.VirtualKeyShort bunnyJumpKey = _settings.GetKey("Bunny Jump", "Bunny Jump Key");
            bool bunnyJump = _settings.GetBool("Bunny Jump", "Bunny Jump Enabled");
            int successfulJumps = _settings.GetInt("Bunny Jump", "Bunny Jump Jumps");

            if (!bunnyJump)
                return;

            if (Program.KeyUtils.KeyIsDown(bunnyJumpKey))
            {
                if (successfulJumps < CurrentJump)
                    return;

                if (LocalPlayer.Flags == 256)
                    Program.MemUtils.Write((IntPtr)(_clientDllBase + Offsets.Misc.Jump), 4);
                else
                {
                    Program.MemUtils.Write((IntPtr)(_clientDllBase + Offsets.Misc.Jump), 5);
                    //We +1 for each time we jump
                    CurrentJump++;
                }
            }
            else
            {
                //If we are not holding space we set currentJump to 0.
                CurrentJump = 0;
            }
        }

        private void Sonar()
        {
            bool sonarEnabled = _settings.GetBool("Sonar", "Sonar Enabled");
            int sonarSound = _settings.GetInt("Sonar", "Sonar Sound");
            float sonarRange = _settings.GetFloat("Sonar", "Sonar Range");
            float sonarInterval = _settings.GetFloat("Sonar", "Sonar Interval");
            float sonarVolume = _settings.GetFloat("Sonar", "Sonar Volume");

            if (!sonarEnabled)
                return;
            //Set's our sound volume
            Program.SoundManager.SetVolume(sonarVolume / 100f);

            TimeSpan span = new TimeSpan(DateTime.Now.Ticks - _lastBeep);

            if (span.TotalMilliseconds > sonarInterval)
            {
                _lastBeep = DateTime.Now.Ticks;
                return;
            }

            float minRange = sonarRange / sonarInterval * (float)span.TotalMilliseconds;
            LastPercent = 100f / sonarInterval * (float)span.TotalMilliseconds;

            float leastDist = float.MaxValue;

            foreach (var player in Players)
            {
                //If the ID does match it's our player
                if (player.Item2.Id == LocalPlayer.Id)
                    continue;

                //If the player is dead.
                if (player.Item2.Health == 0)
                    continue;

                //if the player is in the same team as us.
                if (player.Item2.TeamNum == LocalPlayer.TeamNum)
                    continue;

                float dist = LocalPlayer.DistanceToOtherEntityInMetres(player);
                if (dist <= minRange)
                {
                    leastDist = dist;
                    break;
                }
            }

            if (leastDist != float.MaxValue)
            {
                Program.SoundManager.Play(sonarSound - 1);
                Thread.Sleep(50);
                _lastBeep = DateTime.Now.Ticks;
            }
        }
    }
}