using System;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class RCS
    {
        #region Fields

        private static readonly SettingsConfig _settings = new SettingsConfig();

        #endregion

        public void Update()
        {
            if (Memory.WindowTitle != Program.GameTitle)
                return;

            if (Memory.LocalPlayer == null || Memory.LocalPlayer.Health <= 0)
                return;

            ViewAngles = Program.MemUtils.Read<Vector3>((IntPtr) (Memory.ClientState + Offsets.ClientState.ViewAngles));
            NewViewAngles = ViewAngles;

            ControlRecoil();

            LastPunch = Memory.LocalPlayer.VecPunch;

            SetViewAngles(NewViewAngles);
        }

        public static void ControlRecoil(bool aimbot = false)
        {
            var rcsEnabled = _settings.GetBool(Memory.WeaponSection, "Rcs Enabled");
            var rcsForceMax = _settings.GetFloat(Memory.WeaponSection, "Rcs Force Max");
            var rcsForceMin = _settings.GetFloat(Memory.WeaponSection, "Rcs Force Min");
            var rcsStart = _settings.GetInt(Memory.WeaponSection, "Rcs Start");
            var random = new Random();

            float randomRcsForce = random.Next((int) rcsForceMin, (int) rcsForceMax);

            if (!rcsEnabled)
                return;

            if (Memory.LocalPlayerWeapon == null || Memory.LocalPlayerWeapon.Clip1 <= 0)
                return;

            //Ugly way to do it, we'll count the shots fired while trigger is active.
            if (!TriggerBot.TriggerbotActive)
            {
                if (Memory.LocalPlayer.ShotsFired <= rcsStart)
                    return;
            }

            if (aimbot)
            {
                NewViewAngles -= Memory.LocalPlayer.VecPunch*(2f/100f*randomRcsForce/3);
                SetViewAngles(NewViewAngles);
            }
            else
            {
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