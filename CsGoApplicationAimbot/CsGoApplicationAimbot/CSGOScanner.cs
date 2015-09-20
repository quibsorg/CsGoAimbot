using System;
using System.Diagnostics;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MemObjects;

namespace CsGoApplicationAimbot
{
    public static class CsgoScanner
    {
        private static ScanResult _scan;

        private static ProcessModule _clientDll;
        private static int _clientDllBase;
        private static ProcessModule _engineDll;
        private static int _engineDllBase;

        public static void ScanOffsets(ProcessModule client, ProcessModule engine, MemUtils memUtils)
        {
            _clientDll = client;
            _engineDll = engine;
            _clientDllBase = _clientDll.BaseAddress.ToInt32();
            _engineDllBase = _engineDll.BaseAddress.ToInt32();
            EntityOff(memUtils);
            LocalPlayer(memUtils);
            Jump(memUtils);
            GameResources(memUtils);
            ClientState(memUtils);
            SetViewAngles(memUtils);
            SignOnState(memUtils);
            GlowManager(memUtils);
            WeaponTable(memUtils);
            EntityId(memUtils);
            EntityHealth(memUtils);
            EntityVecOrigin(memUtils);
            PlayerTeamNum(memUtils);
            PlayerBoneMatrix(memUtils);
            PlayerWeaponHandle(memUtils);
            VMatrix(memUtils);
            _clientDll = null;
            _engineDll = null;
            _clientDllBase = 0;
            _engineDllBase = 0;
        }

        #region MISC

        private static void GameResources(MemUtils memUtils)
        {
            _scan = memUtils.PerformSignatureScan(new byte[]
            {
                0x89, 0x4D, 0xF4, //mov [ebp-0C],ecx
                0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, //mov ecx,[engine.dll+xxxx]
                0x53, //push ebx
                0x56, //push esi
                0x57, //push edi
                0x8B, 0x01
            }, "xxxxx????xxxxx", _engineDll);
            if (_scan.Success)
            {
                int address, pointer, offset;
                pointer = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 5)) - _engineDllBase;

                _scan = memUtils.PerformSignatureScan(new byte[]
                {
                    0xCC, //int 3
                    0xCC, //int 3
                    0x55, //push ebp
                    0x8B, 0xEC, //mov ebp,esp
                    0x8B, 0x45, 0x08, //mov eax,[ebp+08]
                    0x8B, 0x44, 0xC1, 0x00, //mov eax,[acx+eax*8+xx]
                    0x5D, //pop ebp
                    0xC2, 0x00, 0x00, //ret 0004
                    0xCC, //int 3
                    0xCC
                }, "xxxxxxxxxxx?xx??xx", _clientDll);
                if (_scan.Success)
                {
                    offset = memUtils.Read<byte>((IntPtr) (_scan.Address.ToInt32() + 11));

                    address = memUtils.Read<int>((IntPtr) (_engineDllBase + pointer));
                    address = address + 0x46*8 + offset;
                    address -= _clientDllBase;
                    CsgoOffsets.GameResources.Base = address;
                }
            }
        }

        private static void VMatrix(MemUtils memUtils)
        {
            _scan = memUtils.PerformSignatureScan(new byte[]
            {
                0x53, 0x8B, 0xDC, 0x83, 0xEC, 0x08, 0x83, 0xE4,
                0xF0, 0x83, 0xC4, 0x04, 0x55, 0x8B, 0x6B, 0x04,
                0x89, 0x6C, 0x24, 0x04, 0x8B, 0xEC, 0xA1, 0x00,
                0x00, 0x00, 0x00, 0x81, 0xEC, 0x98, 0x03, 0x00,
                0x00
            }, "xxxxxxxxxxxxxxxxxxxxxxx????xxxxxx", _clientDll);
            if (_scan.Success)
            {
                var address = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + +0x4EE));
                address -= _clientDllBase;
                address += 0x80;
                CsgoOffsets.Misc.ViewMatrix = address;
            }
        }

        private static void EntityOff(MemUtils memUtils)
        {
            _scan =
                memUtils.PerformSignatureScan(
                    new byte[] {0x05, 0x00, 0x00, 0x00, 0x00, 0xC1, 0xe9, 0x00, 0x39, 0x48, 0x04}, "x????xx?xxx",
                    _clientDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 1));
                var tmp2 = memUtils.Read<byte>((IntPtr) (_scan.Address.ToInt32() + 7));
                CsgoOffsets.Misc.EntityList = tmp + tmp2 - _clientDllBase;
            }
        }

        private static void LocalPlayer(MemUtils memUtils)
        {
            _scan =
                memUtils.PerformSignatureScan(
                    new byte[]
                    {
                        0x8D, 0x34, 0x85, 0x00, 0x00, 0x00, 0x00, 0x89, 0x15, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x41, 0x08,
                        0x8B, 0x48
                    }, "xxx????xx????xxxxx", _clientDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 3));
                var tmp2 = memUtils.Read<byte>((IntPtr) (_scan.Address.ToInt32() + 18));
                CsgoOffsets.Misc.LocalPlayer = tmp + tmp2 - _clientDllBase;
            }
        }

        private static void Jump(MemUtils memUtils)
        {
            _scan =
                memUtils.PerformSignatureScan(
                    new byte[]
                    {
                        0x89, 0x15, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x15, 0x00, 0x00, 0x00, 0x00, 0xF6, 0xC2, 0x03, 0x74,
                        0x03, 0x83, 0xCE, 0x08, 0xA8, 0x08, 0xBF
                    }, "xx????xx????xxxxxxxxxxx", _clientDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 2));
                CsgoOffsets.Misc.Jump = tmp - _clientDllBase;
            }
        }

        private static void ClientState(MemUtils memUtils)
        {
            _scan =
                memUtils.PerformSignatureScan(
                    new byte[]
                    {0xC2, 0x00, 0x00, 0xCC, 0xCC, 0x8B, 0x0D, 0x00, 0x00, 0x00, 0x00, 0x33, 0xC0, 0x83, 0xB9},
                    "x??xxxx????xxxx", _engineDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 7));
                CsgoOffsets.ClientState.Base = tmp - _engineDllBase;
            }
        }

        private static void SetViewAngles(MemUtils memUtils)
        {
            _scan =
                memUtils.PerformSignatureScan(
                    new byte[]
                    {
                        0x8B, 0x15, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x4D, 0x08, 0x8B, 0x82, 0x00, 0x00, 0x00, 0x00, 0x89,
                        0x01, 0x8B, 0x82, 0x00, 0x00, 0x00, 0x00, 0x89, 0x41, 0x04
                    }, "xx????xxxxx????xxxx????xxx",
                    _engineDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 11));
                CsgoOffsets.ClientState.MDwViewAngles = tmp;
            }
        }

        private static void SignOnState(MemUtils memUtils)
        {
            _scan = memUtils.PerformSignatureScan(
                new byte[]
                {
                    0x51, 0xA1, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x51, 0x00, 0x83, 0xB8, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7C,
                    0x40, 0x3B, 0xD1
                },
                "xx????xx?xx?????xxxx", _engineDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 11));
                CsgoOffsets.Misc.SignOnState = tmp;
            }
        }

        private static void GlowManager(MemUtils memUtils)
        {
            _scan =
                memUtils.PerformSignatureScan(
                    new byte[]
                    {
                        0x8D, 0x8F, 0x00, 0x00, 0x00, 0x00, 0xA1, 0x00, 0x00, 0x00, 0x00, 0xC7, 0x04, 0x02, 0x00, 0x00,
                        0x00, 0x00, 0x89, 0x35, 0x00, 0x00, 0x00, 0x00, 0x8B, 0x51
                    }, "xx????x????xxx????xx????xx",
                    _clientDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 7));
                CsgoOffsets.Misc.GlowManager = tmp - _clientDllBase;
            }
        }

        private static void WeaponTable(MemUtils memUtils)
        {
            _scan =
                memUtils.PerformSignatureScan(
                    new byte[]
                    {0xA1, 0x00, 0x00, 0x00, 0x00, 0x0F, 0xB7, 0xC9, 0x03, 0xC9, 0x8B, 0x44, 0x00, 0x0C, 0xC3},
                    "x????xxxxxxx?xx", _clientDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 1));
                CsgoOffsets.Misc.WeaponTable = tmp - _clientDllBase;
            }
        }

        #endregion

        #region ENTITY

        private static void EntityId(MemUtils memUtils)
        {
            _scan = memUtils.PerformSignatureScan(
                new byte[] {0x74, 0x72, 0x80, 0x79, 0x00, 0x00, 0x8B, 0x56, 0x00, 0x89, 0x55, 0x00, 0x74, 0x17},
                "xxxx??xx?xx?xx", _clientDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<byte>((IntPtr) (_scan.Address.ToInt32() + 8));
                CsgoOffsets.NetVars.CBaseEntity.MiId = tmp;
            }
        }

        private static void EntityHealth(MemUtils memUtils)
        {
            _scan = memUtils.PerformSignatureScan(
                new byte[]
                {
                    0x8B, 0x41, 0x00, 0x89, 0x41, 0x00, 0x8B, 0x41, 0x00, 0x89, 0x41, 0x00, 0x8B, 0x41, 0x00, 0x89, 0x41,
                    0x00, 0x8B, 0x4F, 0x00, 0x83, 0xB9, 0x00, 0x00, 0x00, 0x00, 0x00, 0x7F, 0x2E
                },
                "xx?xx?xx?xx?xx?xx?xx?xx????xxx", _clientDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 23));
                CsgoOffsets.NetVars.CBaseEntity.MiHealth = tmp;
            }
        }

        private static void EntityVecOrigin(MemUtils memUtils)
        {
            _scan = memUtils.PerformSignatureScan(
                new byte[]
                {
                    0x8A, 0x0E, 0x80, 0xE1, 0xFC, 0x0A, 0xC8, 0x88, 0x0E, 0xF3, 0x00, 0x00, 0x87, 0x00, 0x00, 0x00, 0x00,
                    0x00, 0x00, 0x87, 0x00, 0x00, 0x00, 0x00, 0x9F
                },
                "xxxxxxxxxx??x??????x????x", _clientDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 13));
                CsgoOffsets.NetVars.CBaseEntity.MVecOrigin = tmp;
            }
        }

        #endregion

        #region PLAYER

        private static void PlayerTeamNum(MemUtils memUtils)
        {
            _scan = memUtils.PerformSignatureScan(
                new byte[]
                {
                    0xCC, 0xCC, 0xCC, 0x8B, 0x89, 0x00, 0x00, 0x00, 0x00, 0xE9, 0x00, 0x00, 0x00, 0x00, 0xCC, 0xCC, 0xCC,
                    0xCC, 0xCC, 0x8B, 0x81, 0x00, 0x00, 0x00, 0x00, 0xC3, 0xCC, 0xCC
                },
                "xxxxx????x????xxxxxxx????xxx", _clientDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 5));
                CsgoOffsets.NetVars.CBaseEntity.MiTeamNum = tmp;
            }
        }

        private static void PlayerBoneMatrix(MemUtils memUtils)
        {
            _scan = memUtils.PerformSignatureScan(
                new byte[]
                {
                    0x83, 0x3C, 0xB0, 0xFF, 0x75, 0x15, 0x8B, 0x87, 0x00, 0x00, 0x00, 0x00, 0x8B, 0xCF, 0x8B, 0x17, 0x03,
                    0x44, 0x24, 0x0C, 0x50
                },
                "xxxxxxxx????xxxxxxxxx", _clientDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 8));
                CsgoOffsets.NetVars.CCsPlayer.MhBoneMatrix = tmp;
            }
        }

        private static void PlayerWeaponHandle(MemUtils memUtils)
        {
            _scan = memUtils.PerformSignatureScan(
                new byte[] {0x0F, 0x45, 0xF7, 0x5F, 0x8B, 0x8E, 0x00, 0x00, 0x00, 0x00, 0x5E, 0x83, 0xF9, 0xFF},
                "xxxxxx????xxxx", _clientDll);
            if (_scan.Success)
            {
                var tmp = memUtils.Read<int>((IntPtr) (_scan.Address.ToInt32() + 6));
                CsgoOffsets.NetVars.CCsPlayer.MhActiveWeapon = tmp;
            }
        }

        #endregion
    }
}