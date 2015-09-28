namespace CsGoApplicationAimbot
{
    internal class Offsets
    {
        public class Misc
        {
            public static int EntityList = 0x04a35a14;
            public static int LocalPlayer = 0x00a932cc;
            public static int Jump = 0x04AC69B8;
            public static int GlowManager = 0x00008698;
            public static int SignOnState = 0xE8;
            public static int WeaponTable = 0x04A5DC4C;
            public static int ViewMatrix = 0x04a2af54;
            public static int MouseEnable = 0xA7A4C0;
        }

        public class ClientState
        {
            public static int Base = 0x00;
            public static int InGame = 0xE8;
            public static int ViewAngles = 0x4ce0;
            public static int Mapname = 0x26c;
            public static uint MapDirectory = 0xf5010000;
        }

        public class GameResources
        {
            public static int Base = 0x04A38E2C;
            public static int Names = 0x9D0;
            public static int Kills = 0xBD8;
            public static int Assists = 0xCDC;
            public static int Deaths = 0xDE0;
            public static int Armor = 0x182C;
            public static int Score = 0x192C;
            public static int Clantag = 0x4110;
        }

        public class NetVars
        {
            public class CBaseEntity
            {
                public static int Health = 0xfc;
                public static int Id = 0x00;
                public static int TeamNum = 0xf0;
                public static int VecOrigin = 0x134;
                public static int AngRotation = 0x128;
                public static int Spotted = 0x935;
                public static int SpottedByMask = 0x978;
                public static int OwnerEntity = 0x148;
                public static int Dormant = 0xE9;
            }

            public class CCsPlayer
            {
                public static int LifeState = 0x25B;
                public static int BoneMatrix = 0xa74;
                public static int ActiveWeapon = 0x12b8; // m_hActiveWeapon
                public static int Flags = 0x100;
                public static int ObserverTarget = 0x173C;
                public static int ObserverMode = 0x1728;
                public static int VecVelocity = 0x110;
            }

            public class LocalPlayer
            {
                public static int VecViewOffset = 0x104;
                public static int VecPunch = 0x13E0;
                public static int ShotsFired = 0x8640;
                public static int CrosshairIdx = 0x8CE4;
            }

            public class Weapon
            {
                public static int ItemDefinitionIndex = 0x131C;
                public static int State = 0x15ac;
                public static int Clip1 = 0x15b8;
                public static int NextPrimaryAttack = 0x1590;
                public static int WeaponId = 0x1684; // Search for weaponid
                public static int CanReload = 0x15ed;
                public static int WeaponTableIndex = 0x1624;
                public static int AccuracyPenalty = 0x1668;
                public static int MZoomLevel = 0x16E0; //Gets out soom level, 0 no zoom, 1 first zoom, 2 secound zoom.
            }
        }
    }
}