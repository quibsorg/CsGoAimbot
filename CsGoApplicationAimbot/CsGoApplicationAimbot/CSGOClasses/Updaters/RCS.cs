using System;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class Rcs
    {
        #region Fields

        private static readonly Settings Settings = new Settings();
        static bool _rcsEnabled;
        static float _rcsForceMax;
        static float _rcsForceMin;
        static int _rcsStart;
        static Random _random = new Random();
        #endregion

        public void Update()
        {
            if (Memory.WindowTitle != Program.GameTitle)
                return;

            if (Memory.LocalPlayer == null || Memory.LocalPlayer.Health <= 0)
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

            SetViewAngles(NewViewAngles);
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

            if (aimbot)
            {
                NewViewAngles -= Memory.LocalPlayer.VecPunch*(2f/100f*randomRcsForce/3);
                SetViewAngles(NewViewAngles);
            }
            else
            {
                //Ugly way to do it, we'll count the shots fired while trigger is active.
                if (!TriggerBot.TriggerbotActive)
                {
                    if (Memory.LocalPlayer.ShotsFired <= _rcsStart)
                        return;
                }

                var punch = Memory.LocalPlayer.VecPunch - LastPunch;
                if (punch.X != 0 || punch.Y != 0)
                    NewViewAngles -= punch*(2f/100*randomRcsForce);
            }
        }

        private static void SetViewAngles(Vector3 viewAngles, bool clamp = true)
        {
            if (clamp)
                viewAngles = viewAngles.ClampAngle();
            Program.MemUtils.Write((IntPtr) (Memory.ClientState + Offsets.ClientState.ViewAngles), viewAngles);
        }

        #region Properties

        public static Vector3 LastPunch { get; set; }
        public static Vector3 NewViewAngles { get; set; }
        public static Vector3 ViewAngles { get; set; }

        #endregion
    }
}