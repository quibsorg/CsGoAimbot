using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using CsGoApplicationAimbot.CSGOClasses.Enums;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class Framework
    {
        #region CONSTRUCTOR

        public Framework(ProcessModule engineDll, ProcessModule clientDll)
        {
            CsgoScanner.ScanOffsets(clientDll, engineDll, Program.MemUtils);
            _clientDllBase = (int)clientDll.BaseAddress;
            var engineDllBase = (int)engineDll.BaseAddress;
            _dwEntityList = _clientDllBase + CsgoOffsets.Misc.EntityList;
            _dwViewMatrix = _clientDllBase + CsgoOffsets.Misc.ViewMatrix;
            _dwClientState = Program.MemUtils.Read<int>((IntPtr)(engineDllBase + CsgoOffsets.ClientState.Base));
            AimbotActive = false;
            TriggerbotActive = false;
        }

        readonly Settings _settings = new Settings();
        #endregion

        #region VARIABLES

        private readonly int
            _dwEntityList;

        private readonly int
            _dwViewMatrix;

        private int
            _dwLocalPlayer;

        private readonly int
            _dwClientState;

        private readonly int
            _clientDllBase;

        private int
            _dwIGameResources;

        #endregion

        #region PROPERTIES
        public float LastPercent { get; private set; }
        public CsLocalPlayer LocalPlayer { get; private set; }
        public string WeaponSection { get; set; }
        private BaseEntity Target { get; set; }
        public Tuple<int, CsPlayer>[] Players { get; private set; }
        private Tuple<int, BaseEntity>[] Entities { get; set; }
        public Tuple<int, Weapon>[] Weapons { get; private set; }
        private Matrix ViewMatrix { get; set; }
        private Vector3 ViewAngles { get; set; }
        private Vector3 NewViewAngles { get; set; }
        private int[] Kills { get; set; }
        private int[] Deaths { get; set; }
        private int[] Assists { get; set; }
        private int[] Armor { get; set; }
        private int[] Score { get; set; }
        private string[] Clantags { get; set; }
        public string[] Names { get; set; }
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

            _dwLocalPlayer = Program.MemUtils.Read<int>((IntPtr)(_clientDllBase + CsgoOffsets.Misc.LocalPlayer));
            _dwIGameResources = Program.MemUtils.Read<int>((IntPtr)(_clientDllBase + CsgoOffsets.GameResources.Base));

            State = (SignOnState)Program.MemUtils.Read<int>((IntPtr)(_dwClientState + CsgoOffsets.ClientState.MDwInGame));
            if (State != SignOnState.SignonstateFull)
                return;

            ViewMatrix = Program.MemUtils.ReadMatrix((IntPtr)_dwViewMatrix, 4, 4);
            ViewAngles = Program.MemUtils.Read<Vector3>((IntPtr)(_dwClientState + CsgoOffsets.ClientState.MDwViewAngles));
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


            if (players.Exists(x => x.Item2.Address == _dwLocalPlayer))
            {
                LocalPlayer = new CsLocalPlayer(players.First(x => x.Item2.Address == _dwLocalPlayer).Item2);
                LocalPlayerWeapon = LocalPlayer.GetActiveWeapon();
                //Only gets the weapon name and formates it properly and retunrs a string. Used for Weapon Configs
                WeaponSection = LocalPlayer.GetActiveWeaponName();
            }
            else
            {
                LocalPlayer = null;
                LocalPlayerWeapon = null;
            }

            if (LocalPlayer != null)
            {
                if (entities.Exists(x => x.Item1 == LocalPlayer.MiCrosshairIdx - 1))
                    Target = entities.First(x => x.Item1 == LocalPlayer.MiCrosshairIdx - 1).Item2;
                Target = players.Exists(x => x.Item1 == LocalPlayer.MiCrosshairIdx - 1) ? players.First(x => x.Item1 == LocalPlayer.MiCrosshairIdx - 1).Item2 : null;
            }

            if (LocalPlayer == null)
                return;

            #region Specator
            //We use this if we can to list all the specators in the console window. But it will not be used for now.

            //if (_dwIGameResources != 0)
            //{
            //    Kills = Program.MemUtils.ReadArray<int>((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Kills), 65);
            //    Deaths = Program.MemUtils.ReadArray<int>((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Deaths), 65);
            //    Armor = Program.MemUtils.ReadArray<int>((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Armor), 65);
            //    Assists = Program.MemUtils.ReadArray<int>((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Assists), 65);
            //    Score = Program.MemUtils.ReadArray<int>((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Score), 65);
            //
            //    var clantagsData = new byte[16 * 65];
            //    Program.MemUtils.Read((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Clantag), out clantagsData, clantagsData.Length);
            //    var clantags = new string[65];
            //    for (var i = 0; i < 65; i++)
            //        clantags[i] = Encoding.Unicode.GetString(clantagsData, i * 16, 16);
            //    Clantags = clantags;
            //
            //    var namePtrs = Program.MemUtils.ReadArray<int>((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Names), 65);
            //    var names = new string[65];
            //    for (var i = 0; i < 65; i++)
            //        try
            //        {
            //            names[i] = Program.MemUtils.ReadString((IntPtr)namePtrs[i], 32, Encoding.ASCII);
            //        }
            //        catch
            //        {
            //            //ignored
            //        }
            //    Names = names;
            //}
            #endregion

            #region Aimbot
            bool aimEnaled = _settings.GetBool(WeaponSection, "Aim Enabled");
            var aimKey = _settings.GetKey(WeaponSection, "Aim Key");
            if (aimEnaled)
            {
                AimbotActive = Program.KeyUtils.KeyIsDown(aimKey);
                if (AimbotActive)
                    ControlAim();
            }
            #endregion

            #region Rcs
            ControlRecoil();
            #endregion

            //Sets the view angles.
            if (NewViewAngles != ViewAngles)
                SetViewAngles(NewViewAngles);

            #region Trigger
            var triggerKey = _settings.GetKey(WeaponSection, "Trigger Key");
            bool triggerEnabled = _settings.GetBool(WeaponSection, "Trigger Enabled");
            bool triggerBurstEnabled = _settings.GetBool(WeaponSection, "Trigger Burst Enabled");
            bool triggerToggle = _settings.GetBool(WeaponSection, "Trigger Toggle");
            bool triggerHold = _settings.GetBool(WeaponSection, "Trigger Hold");

            if (triggerEnabled)
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
                            if (LocalPlayerWeapon.MiClip1 != LastClip)
                            {
                                TriggerBurstFired += Math.Abs(LocalPlayerWeapon.MiClip1 - LastClip);
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
            LastClip = LocalPlayerWeapon?.MiClip1 ?? 0;
            LastShotsFired = LocalPlayer.MiShotsFired;
            LastPunch = LocalPlayer.MVecPunch;
        }

        #endregion
        private void SetViewAngles(Vector3 viewAngles, bool clamp = true)
        {
            if (clamp)
                viewAngles = viewAngles.ClampAngle();
            Program.MemUtils.Write((IntPtr)(_dwClientState + CsgoOffsets.ClientState.MDwViewAngles), viewAngles);
        }

        public bool IsPlaying()
        {
            return State == SignOnState.SignonstateFull && LocalPlayer != null;
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
                valid = valid.Where(x => x.Item2.MiTeamNum != LocalPlayer.MiTeamNum);
            if (aimAllies)
                valid = valid.Where(x => x.Item2.MiTeamNum == LocalPlayer.MiTeamNum);

            valid = valid.OrderBy(x => (x.Item2.MVecOrigin - LocalPlayer.MVecOrigin).Length());
            var closest = Vector3.Zero;
            var closestFov = float.MaxValue;
            foreach (var tpl in valid)
            {
                var plr = tpl.Item2;
                var newAngles = (LocalPlayer.MVecOrigin + LocalPlayer.MVecViewOffset).CalcAngle(plr.Bones.GetBoneByIndex(aimBone)) - NewViewAngles;
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

            if (LocalPlayerWeapon == null || LocalPlayerWeapon.MiClip1 <= 0)
                return;
            if ((RcsHandled || LocalPlayer.MiShotsFired <= rcsStart))
                return;
            if (aimbot)
            {
                var aimbotForce = randomRcsForce / 1.6;
                NewViewAngles -= LocalPlayer.MVecPunch * (float)(2f / 100f * aimbotForce);
            }
            else
            {
                var punch = LocalPlayer.MVecPunch - LastPunch;
                NewViewAngles -= punch * (2f / 100f * randomRcsForce);
            }
            RcsHandled = true;
        }
        #endregion

        #region Trigger
        public void Triggerbot()
        {
            bool triggerEnemies = _settings.GetBool(WeaponSection, "Trigger Enemies");
            bool triggerAllies = _settings.GetBool(WeaponSection, "Trigger Allies");
            bool burstRandomize = _settings.GetBool(WeaponSection, "Trigger Burst Randomize");
            float firstShot = _settings.GetFloat(WeaponSection, "Trigger Delay FirstShot");
            float delayShot = _settings.GetFloat(WeaponSection, "Trigger Delay Shots");
            float burstShots = _settings.GetFloat(WeaponSection, "Trigger Burst Shots");

            if (LocalPlayer == null || TriggerShooting)
                return;
            if (Players.Count(x => x.Item2.MiId == LocalPlayer.MiCrosshairIdx) <= 0) return;
            var player = Players.First(x => x.Item2.MiId == LocalPlayer.MiCrosshairIdx).Item2;
            if ((triggerEnemies && player.MiTeamNum != LocalPlayer.MiTeamNum) || (triggerAllies && player.MiTeamNum == LocalPlayer.MiTeamNum))
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
        private void Shoot()
        {
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(1);
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTUP, 0, 0, 0, 0);
        }
        #endregion

    }
}