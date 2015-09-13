namespace CsGoApplicationAimbot
{
    class CsgoOffsets
    {
        public class Misc
        {
            public static int EntityList = 0x00;
            public static int LocalPlayer = 0x00;
            public static int Jump = 0x00;
            public static int GlowManager = 0x00;
            public static int SignOnState = 0xE8;
            public static int WeaponTable = 0x04A5DC4C;
            public static int ViewMatrix = 0x00;
            public static int MouseEnable = 0xA7A4C0;
        }
        public class ClientState
        {
            public static int Base = 0x00;
            public static int MDwInGame = 0xE8;
            public static int MDwViewAngles = 0x00;
            public static int MDwMapname = 0x26c;
            public static int MDwMapDirectory = 0x168;
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
                public static int MIHealth = 0x00;
                public static int M_IId = 0x00;
                public static int MITeamNum = 0x00;
                public static int MVecOrigin = 0x134;
                public static int MAngRotation = 0x128;
                public static int MBSpotted = 0x935;
                public static int MBSpottedByMask = 0x978;
                public static int MHOwnerEntity = 0x148;
                public static int MBDormant = 0xE9;
            }

            public class CCsPlayer
            {
                public static int MLifeState = 0x25B;
                public static int MHBoneMatrix = 0x00;
                public static int MHActiveWeapon = 0x12C0;   // m_hActiveWeapon
                public static int MIFlags = 0x100;
                public static int MHObserverTarget = 0x173C;
                public static int MIObserverMode = 0x1728;
                public static int MVecVelocity = 0x110;
            }

            public class LocalPlayer
            {
                public static int MVecViewOffset = 0x104;
                public static int MVecPunch = 0x13E8;
                public static int MIShotsFired = 0x1d6C;
                public static int MICrosshairIdx = 0x2410;
            }

            public class Weapon
            {
                public static int MIItemDefinitionIndex = 0x131C;
                public static int MIState = 0x15B4;
                public static int MIClip1 = 0x15c0;
                public static int MFlNextPrimaryAttack = 0x159C;
                public static int MIWeaponId = 0x1690;   // Search for weaponid
                public static int MBCanReload = 0x15F9;
                public static int MIWeaponTableIndex = 0x162C;
                public static int MFAccuracyPenalty = 0x1670;
            }
        }
    }
}
