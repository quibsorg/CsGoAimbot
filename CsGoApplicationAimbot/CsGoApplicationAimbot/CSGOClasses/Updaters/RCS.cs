using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class RCS
    {

        #region Fields
        static SettingsConfig _settings = new SettingsConfig();
        #endregion
        #region Properties
        public static Vector3 LastPunch { get; set; }
        public static Vector3 NewViewAngles { get; set; }
        public static Vector3 ViewAngles { get; set; }

        #endregion

        public void Update()
        {
            if (Memory.WindowTitle != Program.GameTitle)
                return;

            if (Memory.LocalPlayer == null || Memory.LocalPlayer.Health <= 0)
                return;

            ViewAngles = Program.MemUtils.Read<Vector3>((IntPtr)(Memory.ClientState + Offsets.ClientState.ViewAngles));
            NewViewAngles = ViewAngles;

            ControlRecoil();

            LastPunch = Memory.LocalPlayer.VecPunch;

            SetViewAngles(NewViewAngles);
        }

        public static void ControlRecoil(bool aimbot = false)
        {
            bool rcsEnabled = _settings.GetBool(Memory.WeaponSection, "Rcs Enabled");
            float rcsForceMax = _settings.GetFloat(Memory.WeaponSection, "Rcs Force Max");
            float rcsForceMin = _settings.GetFloat(Memory.WeaponSection, "Rcs Force Min");
            int rcsStart = _settings.GetInt(Memory.WeaponSection, "Rcs Start");
            Random random = new Random();

            float randomRcsForce = random.Next((int)rcsForceMin, (int)rcsForceMax);

            if (!rcsEnabled)
                return;

            if (Memory.LocalPlayerWeapon == null || Memory.LocalPlayerWeapon.Clip1 <= 0)
                return;

            //If we are shooting with the trigger bot, over ride rcsStart.
            if (!TriggerBot.TriggerbotActive)
            {
                if (Memory.LocalPlayer.ShotsFired <= rcsStart)
                    return;
            }

            if (aimbot)
            {
                var punch = Memory.LocalPlayer.VecPunch - LastPunch;
                if (punch.X != 0 || punch.Y != 0)
                    NewViewAngles -= punch * (2f / 100 * randomRcsForce);
            }
            else
            {
                var punch = Memory.LocalPlayer.VecPunch - LastPunch;
                if (punch.X != 0 || punch.Y != 0)
                    NewViewAngles -= punch * (2f / 100 * randomRcsForce);
            }
        }
        private void SetViewAngles(Vector3 viewAngles, bool clamp = true)
        {
            if (clamp)
                viewAngles = viewAngles.ClampAngle();
            Program.MemUtils.Write((IntPtr)(Memory.ClientState + Offsets.ClientState.ViewAngles), viewAngles);
        }
    }
}
