using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class Framework
    {
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
        public CsLocalPlayer LocalPlayer { get; private set; }
        public BaseEntity Target { get; private set; }
        public Tuple<int, CsPlayer>[] Players { get; private set; }
        public Tuple<int, BaseEntity>[] Entities { get; private set; }
        public Tuple<int, Weapon>[] Weapons { get; private set; }
        public Matrix ViewMatrix { get; private set; }
        public Vector3 ViewAngles { get; private set; }
        public Vector3 NewViewAngles { get; private set; }
        public int[] Kills { get; private set; }
        public int[] Deaths { get; private set; }
        public int[] Assists { get; private set; }
        public int[] Armor { get; private set; }
        public int[] Score { get; private set; }
        public string[] Clantags { get; private set; }
        public string[] Names { get; private set; }
        public SignOnState State { get; set; }
        public bool AimbotActive { get; set; }
        public bool TriggerbotActive { get; set; }
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

        #region CONSTRUCTOR
        public Framework(ProcessModule clientDll, ProcessModule engineDll)
        {
            CsgoScanner.ScanOffsets(Program.MemUtils, clientDll, engineDll);
            _clientDllBase = (int)clientDll.BaseAddress;
            var engineDllBase = (int)engineDll.BaseAddress;
            _dwEntityList = _clientDllBase + CsgoOffsets.Misc.EntityList;
            _dwViewMatrix = _clientDllBase + CsgoOffsets.Misc.ViewMatrix;
            _dwClientState = Program.MemUtils.Read<int>((IntPtr)(engineDllBase + CsgoOffsets.ClientState.Base));
            AimbotActive = false;
            TriggerbotActive = false;
        }
        #endregion

        #region METHODS
        public void Update()
        {
            List<Tuple<int, CsPlayer>> players = new List<Tuple<int, CsPlayer>>();
            List<Tuple<int, BaseEntity>> entities = new List<Tuple<int, BaseEntity>>();
            List<Tuple<int, Weapon>> weapons = new List<Tuple<int, Weapon>>();

            _dwLocalPlayer = Program.MemUtils.Read<int>((IntPtr)(_clientDllBase + CsgoOffsets.Misc.LocalPlayer));
            _dwIGameResources = Program.MemUtils.Read<int>((IntPtr)(_clientDllBase + CsgoOffsets.GameResources.Base));

            State = (SignOnState)Program.MemUtils.Read<int>((IntPtr)(_dwClientState + CsgoOffsets.ClientState.MDwInGame));
            if (State != SignOnState.SignonstateFull)
                return;

            ViewMatrix = Program.MemUtils.ReadMatrix((IntPtr)_dwViewMatrix, 4, 4);
            ViewAngles = Program.MemUtils.Read<Vector3>((IntPtr)(_dwClientState + CsgoOffsets.ClientState.MDwViewAngles));
            NewViewAngles = ViewAngles;
            RcsHandled = false;

            #region Read entities
            byte[] data = new byte[16 * 8192];
            Program.MemUtils.Read((IntPtr)(_dwEntityList), out data, data.Length);

            for (int i = 0; i < data.Length / 16; i++)
            {
                int address = BitConverter.ToInt32(data, 16 * i);
                if (address != 0)
                {
                    BaseEntity ent = new BaseEntity(address);
                    if (!ent.IsValid())
                        continue;
                    if (ent.IsPlayer())
                        players.Add(new Tuple<int, CsPlayer>(i, new CsPlayer(ent)));
                    else if (ent.IsWeapon())
                        weapons.Add(new Tuple<int, Weapon>(i, new Weapon(ent)));
                    else
                        entities.Add(new Tuple<int, BaseEntity>(i, ent));
                }
            }

            Players = players.ToArray();
            Entities = entities.ToArray();
            Weapons = weapons.ToArray();
            #endregion

            #region LocalPlayer and Target
            if (players.Exists(x => x.Item2.Address == _dwLocalPlayer))
            {
                LocalPlayer = new CsLocalPlayer(players.First(x => x.Item2.Address == _dwLocalPlayer).Item2);
                LocalPlayerWeapon = LocalPlayer.GetActiveWeapon();
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
                if (players.Exists(x => x.Item1 == LocalPlayer.MiCrosshairIdx - 1))
                    Target = players.First(x => x.Item1 == LocalPlayer.MiCrosshairIdx - 1).Item2;
                else
                    Target = null;
            }
            #endregion

            if (LocalPlayer == null)
                return;

            #region IGameResources
            if (_dwIGameResources != 0)
            {
                Kills = Program.MemUtils.ReadArray<int>((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Kills), 65);
                Deaths = Program.MemUtils.ReadArray<int>((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Deaths), 65);
                Armor = Program.MemUtils.ReadArray<int>((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Armor), 65);
                Assists = Program.MemUtils.ReadArray<int>((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Assists), 65);
                Score = Program.MemUtils.ReadArray<int>((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Score), 65);

                byte[] clantagsData = new byte[16 * 65];
                Program.MemUtils.Read((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Clantag), out clantagsData, clantagsData.Length);
                string[] clantags = new string[65];
                for (int i = 0; i < 65; i++)
                    clantags[i] = Encoding.Unicode.GetString(clantagsData, i * 16, 16);
                Clantags = clantags;

                int[] namePtrs = Program.MemUtils.ReadArray<int>((IntPtr)(_dwIGameResources + CsgoOffsets.GameResources.Names), 65);
                string[] names = new string[65];
                for (int i = 0; i < 65; i++)
                    try
                    {
                        names[i] = Program.MemUtils.ReadString((IntPtr)namePtrs[i], 32, Encoding.ASCII);
                    }
                    catch { }
                Names = names;
            }
            #endregion

            #region Aimbot
            if (Program.ConfigUtils.GetValue<bool>("Aim Enabled"))
            {
                if (Program.ConfigUtils.GetValue<bool>("Aim Hold"))
                {
                    AimbotActive = Program.KeyUtils.KeyIsDown(Program.ConfigUtils.GetValue<WinAPI.VirtualKeyShort>("Aim Key"));
                }
                if (AimbotActive)
                    ControlAim();
            }
            #endregion

            #region RCS
            ControlRecoil();
            #endregion

            #region Set view angles
            if (NewViewAngles != ViewAngles)
                SetViewAngles(NewViewAngles);
            #endregion

            #region triggerbot
            if (Program.ConfigUtils.GetValue<bool>("Trigger Enabled"))
            {
                if (Program.ConfigUtils.GetValue<bool>("Trigger Toggle"))
                {
                    if (Program.KeyUtils.KeyWentUp(Program.ConfigUtils.GetValue<WinAPI.VirtualKeyShort>("Trigger Key")))
                        TriggerbotActive = !TriggerbotActive;
                }
                else if (Program.ConfigUtils.GetValue<bool>("Trigger Hold"))
                {
                    TriggerbotActive = Program.KeyUtils.KeyIsDown(Program.ConfigUtils.GetValue<WinAPI.VirtualKeyShort>("Trigger Key"));
                }
                if (TriggerbotActive && !Program.KeyUtils.KeyIsDown(WinAPI.VirtualKeyShort.LBUTTON))
                    Triggerbot();
            }
            #endregion
            
            #region triggerbot-burst
            if (TriggerShooting)
            {
                if (LocalPlayerWeapon == null)
                {
                    TriggerShooting = false;
                }
                else
                {
                    if (Program.ConfigUtils.GetValue<bool>("Trigger Burst Enabled"))
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

            #endregion
            LastClip = LocalPlayerWeapon?.MiClip1 ?? 0;
            LastShotsFired = LocalPlayer.MiShotsFired;
            LastPunch = LocalPlayer.MVecPunch;
        }

        public void SetViewAngles(Vector3 viewAngles, bool clamp = true)
        {
            if (clamp)
                viewAngles = viewAngles.ClampAngle();
            Program.MemUtils.Write((IntPtr)(_dwClientState + CsgoOffsets.ClientState.MDwViewAngles), viewAngles);
        }

        public bool IsPlaying()
        {
            return State == SignOnState.SignonstateFull && LocalPlayer != null;
        }

        public void ControlAim()
        {
            if (LocalPlayer == null)
                return;
            var valid = Players.Where(x => x.Item2.IsValid() && x.Item2.MiHealth != 0 && x.Item2.MiDormant != 1);
            if (Program.ConfigUtils.GetValue<bool>("Aim Spotted"))
                valid = valid.Where(x => x.Item2.SeenBy(LocalPlayer));
            if (Program.ConfigUtils.GetValue<bool>("Aim Spotted By"))
                valid = valid.Where(x => LocalPlayer.SeenBy(x.Item2));
            if (Program.ConfigUtils.GetValue<bool>("Aim Enemies"))
                valid = valid.Where(x => x.Item2.MiTeamNum != LocalPlayer.MiTeamNum);
            if (Program.ConfigUtils.GetValue<bool>("Aim Allies"))
                valid = valid.Where(x => x.Item2.MiTeamNum == LocalPlayer.MiTeamNum);

            valid = valid.OrderBy(x => (x.Item2.MVecOrigin - LocalPlayer.MVecOrigin).Length());
            Vector3 closest = Vector3.Zero;
            float closestFov = float.MaxValue;
            foreach (Tuple<int, CsPlayer> tpl in valid)
            {
                CsPlayer plr = tpl.Item2;
                Vector3 newAngles = (LocalPlayer.MVecOrigin + LocalPlayer.MVecViewOffset).CalcAngle(plr.Bones.GetBoneByIndex(Program.ConfigUtils.GetValue<int>("Aim Bone"))) - NewViewAngles;
                newAngles = newAngles.ClampAngle();
                float fov = newAngles.Length() % 360f;
                if (fov < closestFov && fov < Program.ConfigUtils.GetValue<float>("Aim Fov"))
                {
                    closestFov = fov;
                    closest = newAngles;
                }
            }
            if (closest != Vector3.Zero)
            {
                ControlRecoil(true);
                if (Program.ConfigUtils.GetValue<bool>("Aim Smooth Enabled"))
                    NewViewAngles = NewViewAngles.SmoothAngle(NewViewAngles + closest, Program.ConfigUtils.GetValue<float>("Aim Smooth Value"));
                else
                    NewViewAngles += closest;
                NewViewAngles = NewViewAngles;
            }
        }

        public void ControlRecoil(bool aimbot = false)
        {
            if (Program.ConfigUtils.GetValue<bool>("Rcs Enabled"))
            {
                float rcsForceMax = Program.ConfigUtils.GetValue<float>("Rcs Force Max");
                float rcsForceMin = Program.ConfigUtils.GetValue<float>("Rcs Force Min");
                int rcsStart = Program.ConfigUtils.GetValue<int>("Rcs Start");
                Random random = new Random();
                float randomRcsForce = random.Next((int)rcsForceMin, (int)rcsForceMax);

                if (LocalPlayerWeapon != null && LocalPlayerWeapon.MiClip1 > 0 && !LocalPlayerWeapon.IsPistol())
                {
                    if (!RcsHandled && LocalPlayer.MiShotsFired > rcsStart)
                    {
                        if (aimbot)
                        {
                            float aimbotForce = randomRcsForce/2;
                            NewViewAngles -= LocalPlayer.MVecPunch * (2f / 100f * aimbotForce);
                        }
                        else
                        {
                            Vector3 punch = LocalPlayer.MVecPunch - LastPunch;
                            NewViewAngles -= punch * (2f / 100f * randomRcsForce);
                        }
                        RcsHandled = true;
                    }
                }
            }
        }

        public void Triggerbot()
        {
            if (LocalPlayer != null && !TriggerShooting && !LocalPlayerWeapon.IsGrenade())
            {
                if (Players.Count(x => x.Item2.MIId == LocalPlayer.MiCrosshairIdx) > 0)
                {
                    CsPlayer player = Players.First(x=>x.Item2.MIId == LocalPlayer.MiCrosshairIdx).Item2;
                    if ((Program.ConfigUtils.GetValue<bool>("Trigger Enemies") && player.MiTeamNum != LocalPlayer.MiTeamNum) ||
                        (Program.ConfigUtils.GetValue<bool>("Trigger Allies") && player.MiTeamNum == LocalPlayer.MiTeamNum))
                    {
                        if (!TriggerOnTarget)
                        {
                            TriggerOnTarget = true;
                            TriggerLastTarget = DateTime.Now.Ticks;
                        }
                        else
                        {
                            if (new TimeSpan(DateTime.Now.Ticks - TriggerLastTarget).TotalMilliseconds >= Program.ConfigUtils.GetValue<float>("Trigger Delay FirstShot"))
                            {
                                if (new TimeSpan(DateTime.Now.Ticks - TriggerLastShot).TotalMilliseconds >= Program.ConfigUtils.GetValue<float>("Trigger Delay Shots"))
                                {
                                    TriggerLastShot = DateTime.Now.Ticks;
                                    if(!TriggerShooting)
                                    {
                                        if (Program.ConfigUtils.GetValue<bool>("Trigger Burst Randomize"))
                                            TriggerBurstCount = new Random().Next(1, (int)Program.ConfigUtils.GetValue<float>("Trigger Burst Shots"));
                                        else TriggerBurstCount = (int)Program.ConfigUtils.GetValue<float>("Trigger Burst Shots");
                                    }
                                    TriggerShooting = true;
                                }
                            }
                        }
                    }
                    else 
                    {
                        TriggerOnTarget = false; 
                    }
                }
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
