﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using CsGoApplicationAimbot.CSGOClasses.Enums;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class Memory
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        #region Properties
        public static string WeaponSection { get; set; }
        public static string WindowTitle { get; set; }
        private Tuple<int, BaseEntity>[] Entities { get; set; }
        public Tuple<int, Weapon>[] Weapons { get; private set; }
        private Matrix ViewMatrix { get; set; }
        public static Vector3 ViewAngles { get; set; }
        public static SignOnState State { get; set; }
        public static Weapon LocalPlayerWeapon { get; set; }
        public static LocalPlayer LocalPlayer { get; set; }
        public static Tuple<int, Player>[] Players { get; set; }
        #endregion

        #region Variables
        private readonly int _entityList;
        private readonly int _viewMatrix;
        public static int ClientState;
        public static int ClientDllBase;
        private int _localPlayer;
        #endregion

        #region Consturctor
        public Memory(ProcessModule engineDll, ProcessModule clientDll)
        {
            Scanner.ScanOffsets(clientDll, engineDll, Program.MemUtils);
            ClientDllBase = (int)clientDll.BaseAddress;
            var engineDllBase = (int)engineDll.BaseAddress;
            _entityList = ClientDllBase + Offsets.Misc.EntityList;
            _viewMatrix = ClientDllBase + Offsets.Misc.ViewMatrix;
            ClientState = Program.MemUtils.Read<int>((IntPtr)(engineDllBase + Offsets.ClientState.Base));
        }
        #endregion

        #region Method
        public void Update()
        {
            //If the game processes is not running, close the cheat.
            if (!ProcUtils.ProcessIsRunning(Program.GameProcess))
                Environment.Exit(0);

            WindowTitle = GetActiveWindowTitle();
            if (WindowTitle != Program.GameTitle)
                return;

            var players = new List<Tuple<int, Player>>();
            //var entities = new List<Tuple<int, BaseEntity>>();
            var weapons = new List<Tuple<int, Weapon>>();

            State = (SignOnState)Program.MemUtils.Read<int>((IntPtr)(ClientState + Offsets.ClientState.InGame));
            _localPlayer = Program.MemUtils.Read<int>((IntPtr)(ClientDllBase + Offsets.Misc.LocalPlayer));
            ViewMatrix = Program.MemUtils.ReadMatrix((IntPtr)_viewMatrix, 4, 4);
            ViewAngles = Program.MemUtils.Read<Vector3>((IntPtr)(ClientState + Offsets.ClientState.ViewAngles));


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
        }

        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder builder = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();
        
            if (GetWindowText(handle, builder, nChars) > 0)
            {
                return builder.ToString();
            }
            return null;
        }

        #endregion

    }
}