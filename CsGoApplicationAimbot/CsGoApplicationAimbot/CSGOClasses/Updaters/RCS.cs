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
        #endregion

        public void Update()
        {
            if (Memory.WindowTitle != Program.GameTitle)
                return;

            NewViewAngles = NewViewAngles.SmoothAngle(Memory.ViewAngles, 1f);

            ControlRecoil();

            LastPunch = Memory.LocalPlayer.VecPunch;
        }
        private void ControlRecoil(bool aimbot = false)
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
            if (!Framework.TriggerbotActive)
            {
                if (Memory.LocalPlayer.ShotsFired <= rcsStart)
                    return;
            }

            if (aimbot)
            {
                NewViewAngles -= Memory.LocalPlayer.VecPunch * (2f / 100 * randomRcsForce);
                Program.MemUtils.Write((IntPtr)(Memory.ClientState + Offsets.ClientState.ViewAngles), NewViewAngles);
            }
            else
            {
                NewViewAngles -= (Memory.LocalPlayer.VecPunch - LastPunch) * (2f / 100 * randomRcsForce);
                Program.MemUtils.Write((IntPtr)(Memory.ClientState + Offsets.ClientState.ViewAngles), NewViewAngles);
            }
        }

    }
}
