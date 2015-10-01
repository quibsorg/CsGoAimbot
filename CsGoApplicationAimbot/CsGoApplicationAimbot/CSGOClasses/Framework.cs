using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using CsGoApplicationAimbot.CSGOClasses.Enums;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class Framework
    {
        #region Constructor
        public Framework(ProcessModule engineDll, ProcessModule clientDll)
        {
            Scanner.ScanOffsets(clientDll, engineDll, Program.MemUtils);
            _clientDllBase = (int)clientDll.BaseAddress;
            _engineDllBase = (int)engineDll.BaseAddress;
            _dwEntityList = _clientDllBase + Offsets.Misc.EntityList;
            _dwViewMatrix = _clientDllBase + Offsets.Misc.ViewMatrix;
            _dwClientState = Program.MemUtils.Read<int>((IntPtr)(_engineDllBase + Offsets.ClientState.Base));
            AimbotActive = false;
            TriggerbotActive = false;
        }
        #endregion

        #region Variables
        readonly SettingsConfig _settings = new SettingsConfig();
        private readonly int _dwEntityList;
        private readonly int _dwViewMatrix;
        private readonly int _dwClientState;
        private readonly int _clientDllBase;
        private readonly int _engineDllBase;
        private int _dwLocalPlayer;
        #endregion

        #region Properties
        private CsLocalPlayer LocalPlayer { get; set; }
        private string WeaponSection { get; set; }
        private BaseEntity Target { get; set; }
        private Tuple<int, CsPlayer>[] Players { get; set; }
        private Tuple<int, BaseEntity>[] Entities { get; set; }
        public Tuple<int, Weapon>[] Weapons { get; private set; }
        private Matrix ViewMatrix { get; set; }
        private Vector3 ViewAngles { get; set; }
        private Vector3 NewViewAngles { get; set; }
        private SignOnState State { get; set; }
        private bool AimbotActive { get; set; }
        private bool TriggerbotActive { get; set; }
        private bool RcsHandled { get; set; }
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
        #endregion

        public void Update()
        {
            var players = new List<Tuple<int, CsPlayer>>();
            var entities = new List<Tuple<int, BaseEntity>>();
            var weapons = new List<Tuple<int, Weapon>>();

            State = (SignOnState)Program.MemUtils.Read<int>((IntPtr)(_dwClientState + Offsets.ClientState.InGame));
            _dwLocalPlayer = Program.MemUtils.Read<int>((IntPtr)(_clientDllBase + Offsets.Misc.LocalPlayer));
            ViewMatrix = Program.MemUtils.ReadMatrix((IntPtr)_dwViewMatrix, 4, 4);
            ViewAngles = Program.MemUtils.Read<Vector3>((IntPtr)(_dwClientState + Offsets.ClientState.ViewAngles));

            //If we are not ingame do not update
            if (State != SignOnState.SignonstateFull)
                return;

            NewViewAngles = ViewAngles;
            RcsHandled = false;

            var data = new byte[16 * 8192];
            Program.MemUtils.Read((IntPtr)(_dwEntityList), out data, data.Length);

            for (var i = 0; i < data.Length / 16; i++)
            {
                var address = BitConverter.ToInt32(data, 16 * i);
                if (address == 0) continue;
                var ent = new BaseEntity(address);
                if (!ent.IsValid())
                    continue;
                if (ent.IsPlayer())
                    players.Add(new Tuple<int, CsPlayer>(i, new CsPlayer(ent)));
                else if (ent.IsWeapon())
                    weapons.Add(new Tuple<int, Weapon>(i, new Weapon(ent)));
                else
                    entities.Add(new Tuple<int, BaseEntity>(i, ent));
            }

            Players = players.ToArray();
            Entities = entities.ToArray();
            Weapons = weapons.ToArray();

            //Check if our player exists
            if (players.Exists(x => x.Item2.Address == _dwLocalPlayer))
            {
                LocalPlayer = new CsLocalPlayer(players.First(x => x.Item2.Address == _dwLocalPlayer).Item2);
                LocalPlayerWeapon = LocalPlayer.GetActiveWeapon();
                //Only gets the weapon name and formates it properly and retunrs a string. Used for Weapon Configs
                WeaponSection = LocalPlayer.GetActiveWeaponName();
            }
            //Localplayer does not exist, set it to null.
            else
            {
                LocalPlayer = null;
                LocalPlayerWeapon = null;
            }

            if (LocalPlayer == null)
                return;

            //Check the player in our crosshair
            if (entities.Exists(x => x.Item1 == LocalPlayer.CrosshairIdx - 1))
                Target = entities.First(x => x.Item1 == LocalPlayer.CrosshairIdx - 1).Item2;
            //Curret target in our crosshair
            Target = players.Exists(x => x.Item1 == LocalPlayer.CrosshairIdx - 1) ? players.First(x => x.Item1 == LocalPlayer.CrosshairIdx - 1).Item2 : null;

            #region Aimbot
            bool aimEnaled = _settings.GetBool(WeaponSection, "Aim Enabled");
            bool aimScoped = _settings.GetBool(WeaponSection, "Aim When Scoped");
            bool aimToggle = _settings.GetBool(WeaponSection, "Aim Toggle");
            bool aimHold = _settings.GetBool(WeaponSection, "Aim Hold");
            WinAPI.VirtualKeyShort aimKey = _settings.GetKey(WeaponSection, "Aim Key");

            //Won't aim if we do not have any ammo in the clip.
            if (LocalPlayerWeapon != null && (aimEnaled && LocalPlayerWeapon.Clip1 > 0 && LocalPlayer.ShotsFired > 1))
            {
                if (aimScoped)
                {
                    if (LocalPlayerWeapon.ZoomLevel > 0)
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
                }
                else if (!aimScoped)
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
            }
            #endregion

            #region Rcs
            ControlRecoil();
            #endregion

            #region Set View Angles 
            //Sets the view angles.
            if (NewViewAngles != ViewAngles)
                SetViewAngles(NewViewAngles);
            #endregion

            #region Trigger
            var triggerKey = _settings.GetKey(WeaponSection, "Trigger Key");
            bool triggerEnabled = _settings.GetBool(WeaponSection, "Trigger Enabled");
            bool triggerScoped = _settings.GetBool(WeaponSection, "Trigger When Scoped");
            bool triggerBurstEnabled = _settings.GetBool(WeaponSection, "Trigger Burst Enabled");
            bool triggerToggle = _settings.GetBool(WeaponSection, "Trigger Toggle");
            bool triggerHold = _settings.GetBool(WeaponSection, "Trigger Hold");

            if (triggerEnabled)
            {
                if (triggerScoped)
                {
                    //ZoomLevel 0 = No Zoom
                    if (LocalPlayerWeapon.ZoomLevel > 0)
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
                        if (TriggerbotActive && !Program.KeyUtils.KeyIsDown(WinAPI.VirtualKeyShort.LBUTTON))
                            Triggerbot();
                    }
                }
                else if (!triggerScoped)
                {
                    if (triggerToggle)
                    {
                        if (Program.KeyUtils.KeyWentUp(WinAPI.VirtualKeyShort.MENU))
                            TriggerbotActive = !TriggerbotActive;
                    }
                    else if (triggerHold)
                    {
                        TriggerbotActive = Program.KeyUtils.KeyIsDown(triggerKey);
                    }
                    if (TriggerbotActive && !Program.KeyUtils.KeyIsDown(WinAPI.VirtualKeyShort.LBUTTON))
                        Triggerbot();
                }
            }

            if (TriggerShooting)
            {
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

        }

        #region Aimbot
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

            if (LocalPlayer == null)
                return;
            var valid = Players.Where(x => x.Item2.IsValid() && x.Item2.MiHealth != 0 && x.Item2.MiDormant != 1);
            if (aimSpotted)
                valid = valid.Where(x => x.Item2.SeenBy(LocalPlayer));
            if (aimSpottedBy)
                valid = valid.Where(x => LocalPlayer.SeenBy(x.Item2));
            if (aimEnemies)
                valid = valid.Where(x => x.Item2.TeamNum != LocalPlayer.TeamNum);
            if (aimAllies)
                valid = valid.Where(x => x.Item2.TeamNum == LocalPlayer.TeamNum);

            valid = valid.OrderBy(x => (x.Item2.MVecOrigin - LocalPlayer.MVecOrigin).Length());
            var closest = Vector3.Zero;
            var closestFov = float.MaxValue;
            foreach (var tpl in valid)
            {
                var plr = tpl.Item2;
                var newAngles = (LocalPlayer.MVecOrigin + LocalPlayer.VecViewOffset).CalcAngle(plr.Bones.GetBoneByIndex(aimBone)) - NewViewAngles;
                newAngles = newAngles.ClampAngle();
                var fov = newAngles.Length() % 360f;
                if (!(fov < closestFov) || !(fov < aimFov)) continue;
                closestFov = fov;
                closest = newAngles;
            }
            if (closest == Vector3.Zero) return;

            ControlRecoil(true);

            if (aimSmooth)
                NewViewAngles = NewViewAngles.SmoothAngle(NewViewAngles + closest, aimSmoothValue);
            else
                NewViewAngles += closest;

            NewViewAngles = NewViewAngles;
        }
        #endregion

        #region Rcs
        private void ControlRecoil(bool aimbot = false)
        {
            var rcsEnabled = _settings.GetBool(WeaponSection, "Rcs Enabled");
            var rcsForceMax = _settings.GetFloat(WeaponSection, "Rcs Force Max");
            var rcsForceMin = _settings.GetFloat(WeaponSection, "Rcs Force Min");
            var rcsStart = _settings.GetInt(WeaponSection, "Rcs Start");

            var random = new Random();
            float randomRcsForce = random.Next((int)rcsForceMin, (int)rcsForceMax);

            if (!rcsEnabled) return;

            if (LocalPlayerWeapon == null || LocalPlayerWeapon.Clip1 <= 0)
                return;
            if ((RcsHandled || LocalPlayer.ShotsFired <= rcsStart))
                return;
            if (aimbot)
            {
                var aimbotForce = randomRcsForce / 1.6;
                NewViewAngles -= LocalPlayer.VecPunch * (float)(2f / 100f * aimbotForce);
            }
            else
            {
                var punch = LocalPlayer.VecPunch - LastPunch;
                NewViewAngles -= punch * (2f / 100f * randomRcsForce);
            }
            RcsHandled = true;
        }
        #endregion

        #region Set View Angles 
        private void SetViewAngles(Vector3 viewAngles, bool clamp = true)
        {
            if (clamp)
                viewAngles = viewAngles.ClampAngle();
            Program.MemUtils.Write((IntPtr)(_dwClientState + Offsets.ClientState.ViewAngles), viewAngles);
        }
        #endregion

        #region Trigger
        private void Triggerbot()
        {
            bool triggerEnemies = _settings.GetBool(WeaponSection, "Trigger Enemies");
            bool triggerAllies = _settings.GetBool(WeaponSection, "Trigger Allies");
            bool burstRandomize = _settings.GetBool(WeaponSection, "Trigger Burst Randomize");
            float firstShot = _settings.GetFloat(WeaponSection, "Trigger Delay FirstShot");
            float delayShot = _settings.GetFloat(WeaponSection, "Trigger Delay Shots");
            float burstShots = _settings.GetFloat(WeaponSection, "Trigger Burst Shots");

            if (LocalPlayer == null || TriggerShooting)
                return;
            if (Players.Count(x => x.Item2.MiId == LocalPlayer.CrosshairIdx) <= 0) return;
            var player = Players.First(x => x.Item2.MiId == LocalPlayer.CrosshairIdx).Item2;
            if ((triggerEnemies && player.TeamNum != LocalPlayer.TeamNum) || (triggerAllies && player.TeamNum == LocalPlayer.TeamNum))
            {
                if (!TriggerOnTarget)
                {
                    TriggerOnTarget = true;
                    TriggerLastTarget = DateTime.Now.Ticks;
                }
                else
                {
                    if (!(new TimeSpan(DateTime.Now.Ticks - TriggerLastTarget).TotalMilliseconds >= firstShot)) return;
                    if (!(new TimeSpan(DateTime.Now.Ticks - TriggerLastShot).TotalMilliseconds >= delayShot)) return;
                    TriggerLastShot = DateTime.Now.Ticks;
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
                TriggerOnTarget = false;
            }
        }
        #endregion

        #region Shoot
        private void Shoot()
        {
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(1);
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTUP, 0, 0, 0, 0);
        }
        #endregion

        #region Bunny Hop
        private void BunnyHop()
        {
            bool bunnyJump = _settings.GetBool("Bunny Jump", "Bunny Jump Enabled");
            WinAPI.VirtualKeyShort bunnyJumpKey = _settings.GetKey("Bunny Jump", "Bunny Jump Key");

            if (bunnyJump)
            {
                if (Program.KeyUtils.KeyIsDown(bunnyJumpKey))
                {
                    Program.MemUtils.Write((IntPtr)(_clientDllBase + Offsets.Misc.Jump),
                        LocalPlayer.Flags == 256 ? 4 : 5);
                }
            }
        }
        #endregion

    }
}