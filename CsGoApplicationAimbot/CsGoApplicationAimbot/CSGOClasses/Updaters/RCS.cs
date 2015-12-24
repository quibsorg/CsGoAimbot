using System;
using CsGoApplicationAimbot.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class Rcs
    {
        public void Update()
        {
            if (!Memory.ShouldUpdate())
                return;

            if (Memory.ActiveWeapon != Memory.WeaponSection)
            {
                _rcsEnabled = Settings.GetBool(Memory.WeaponSection, "Rcs Enabled");
                _rcsForceMax = Settings.GetFloat(Memory.WeaponSection, "Rcs Force Max");
                _rcsForceMin = Settings.GetFloat(Memory.WeaponSection, "Rcs Force Min");
                _rcsStart = Settings.GetInt(Memory.WeaponSection, "Rcs Start");
            }
            ViewAngles = Program.MemUtils.Read<Vector3>((IntPtr) (Memory.ClientState + Offsets.ClientState.ViewAngles));
            NewViewAngles = ViewAngles;

            ControlRecoil();

            LastPunch = Memory.LocalPlayer.VecPunch;
        }

        public static void ControlRecoil(bool aimbot = false)
        {
            _random = new Random();

            //Todo random RcsForce Each click, not every update.
            float randomRcsForce = _random.Next((int) _rcsForceMin, (int) _rcsForceMax);

            if (!_rcsEnabled)
                return;

            if (Memory.LocalPlayerWeapon == null || Memory.LocalPlayerWeapon.Clip1 <= 0)
                return;


            if (!TriggerBot.TriggerbotActive)
            {
                if (Memory.LocalPlayer.ShotsFired <= _rcsStart)
                    return;
            }

            var punch = Memory.LocalPlayer.VecPunch - LastPunch;
            if (punch.X != 0 || punch.Y != 0)
            {
                NewViewAngles -= punch*(2f/100*randomRcsForce);
                Memory.SetViewAngles(NewViewAngles);
            }
        }

        #region Fields

        private static readonly Settings Settings = new Settings();
        private static bool _rcsEnabled;
        private static float _rcsForceMax;
        private static float _rcsForceMin;
        private static int _rcsStart;
        private static Random _random = new Random();

        #endregion

        #region Properties

        private static Vector3 LastPunch { get; set; }
        public static Vector3 NewViewAngles { get; set; }
        private static Vector3 ViewAngles { get; set; }

        #endregion
    }
}