namespace CsGoApplicationAimbot
{
    internal class Offsets
    {
        public class Misc
        {
            //Auto Updated
            public static int EntityList = 0x04A587D4;
            //Auto Updated
            public static int LocalPlayer = 0x00A6A444;
            //Auto Updated
            public static int Jump = 0x04AD01E8;
            //Auto Updated
            public static int GlowManager = 0x00008698;
            //Auto Updated
            public static int SignOnState = 0xE8;
            //Auto Updated
            public static int WeaponTable = 0x04A9F88C;
            //Auto Updated
            public static int ViewMatrix = 0x04A30F24;
            public static int MouseEnable = 0xA7A4C0;
        }

        public class ClientState
        {
            public static int Base = 0x00;
            public static int InGame = 0x100;
            public static int ViewAngles = 0x00;
            public static int Mapname = 0x180;
            public static int MapDirectory = 0x00000180;
        }

        public class GameResources
        {
            public static int Base = 0x04A38E2C;
            public static int Names = 0x9D0;
            public static int Kills = 0x00000BE0;
            public static int Assists = 0x00000CE4;
            public static int Deaths = 0x00000DE8;
            public static int Armor = 0x1834;
            public static int Score = 0x00001938;
            public static int Clantag = 0x00004118;
        }

        public class NetVars
        {
            public class CBaseEntity
            {
                public static int Health = 0x000000FC;
                public static int Id = 0x00;
                public static int TeamNum = 0x000000F0;
                public static int VecOrigin = 0x00000134;
                public static int AngRotation = 0x128;
                public static int Spotted = 0x00000935;
                public static int SpottedByMask = 0x00000978;
                public static int OwnerEntity = 0x00000148;
                public static int Dormant = 0x000000E9;
            }

            public class CCsPlayer
            {
                public static int LifeState = 0x0000025B;
                public static int BoneMatrix = 0x0000267C;
                public static int ActiveWeapon = 0x00004AF8; // m_hActiveWeapon
                public static int Flags = 0x00000100;
                public static int ObserverTarget = 0x173C;
                public static int ObserverMode = 0x1728;
                public static int VecVelocity = 0x00000110;
            }

            public class LocalPlayer
            {
                public static int VecViewOffset = 0x00000104;
                public static int VecPunch = 0x00004C28;
                public static int ShotsFired = 0x0000BEB0;
                public static int CrosshairIdx = 0xC550;
            }

            public class Weapon
            {
                public static int ItemDefinitionIndex = 0x000032B4;
                public static int State = 0x00004DF8;
                public static int Clip1 = 0x00004E04;
                public static int NextPrimaryAttack = 0x00004DD8;
                public static int WeaponId = 0x00004EE4; // Search for weaponid
                public static int CanReload = 0x00004E45;
                public static int WeaponTableIndex = 0x00004E70;
                public static int AccuracyPenalty = 0x00004EC0;
                public static int ZoomLevel = 0x00004F40;
            }
        }
    }
}