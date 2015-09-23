namespace CsGoApplicationAimbot
{
    internal class CsgoOffsets
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
            public static int MDwInGame = 0xE8;
            public static int MDwViewAngles = 0x4ce0;
            public static int MDwMapname = 0x26c;
            public static uint MDwMapDirectory = 0xf5010000;
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
                public static int MiHealth = 0xfc;
                public static int MiId = 0x00;
                public static int MiTeamNum = 0xf0;
                public static int MVecOrigin = 0x134;
                public static int MAngRotation = 0x128;
                public static int MbSpotted = 0x935;
                public static int MbSpottedByMask = 0x978;
                public static int MhOwnerEntity = 0x148;
                public static int MbDormant = 0xE9;
            }

            public class CCsPlayer
            {
                public static int MLifeState = 0x25B;
                public static int MhBoneMatrix = 0xa74;
                public static int MhActiveWeapon = 0x12b8; // m_hActiveWeapon
                public static int MiFlags = 0x100;
                public static int MhObserverTarget = 0x173C;
                public static int MiObserverMode = 0x1728;
                public static int MVecVelocity = 0x110;
            }

            public class LocalPlayer
            {
                public static int MVecViewOffset = 0x104;
                public static int MVecPunch = 0x13fc;
                public static int MiShotsFired = 0x8660;
                public static int MiCrosshairIdx = 0x8d04;
            }

            public class Weapon
            {
                public static int MiItemDefinitionIndex = 0x131C;
                public static int MiState = 0x15ac;
                public static int MiClip1 = 0x15b8;
                public static int MFlNextPrimaryAttack = 0x1590;
                public static int MiWeaponId = 0x1684; // Search for weaponid
                public static int MbCanReload = 0x15ed;
                public static int MiWeaponTableIndex = 0x1624;
                public static int MfAccuracyPenalty = 0x1668;
                public static int MZoonLevel = 0x16E0; //Gets out soom level, 0 no zoom, 1 first zoom, 2 secound zoom.
            }
        }
    }
}