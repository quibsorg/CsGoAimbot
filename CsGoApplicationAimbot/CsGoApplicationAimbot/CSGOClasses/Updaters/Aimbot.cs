using System;
using System.Collections.Generic;
using System.Linq;
using CsGoApplicationAimbot.CSGOClasses.Enums;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class Aimbot
    {
        private static readonly SettingsConfig Settings = new SettingsConfig();

        public Aimbot()
        {
            AimbotActive = false;
        }

        #region Properties

        private bool AimbotActive { get; set; }

        #endregion

        #region Methods

        public void Update()
        {
            if (Memory.WindowTitle != Program.GameTitle)
                return;

            if (Memory.LocalPlayer == null || Memory.LocalPlayer.Health <= 0)
                return;

            if (Memory.State != SignOnState.SignonstateFull)
                return;

            #region Aimbot

            var aimKey = Settings.GetKey(Memory.WeaponSection, "Aim Key");
            var aimEnaled = Settings.GetBool(Memory.WeaponSection, "Aim Enabled");
            var aimScoped = Settings.GetBool(Memory.WeaponSection, "Aim When Scoped");
            var aimToggle = Settings.GetBool(Memory.WeaponSection, "Aim Toggle");
            var aimHold = Settings.GetBool(Memory.WeaponSection, "Aim Hold");
            var aimStart = Settings.GetInt(Memory.WeaponSection, "Aim Start");

            //Won't aim if we do not have any ammo in the clip.
            if (Memory.LocalPlayerWeapon != null && (aimEnaled && Memory.LocalPlayerWeapon.Clip1 > 0 && Memory.LocalPlayer.ShotsFired > aimStart))
            {
                if (aimScoped)
                {
                    if (Memory.LocalPlayerWeapon.ZoomLevel > 0)
                    {
                        AimToggleOrHold(aimToggle, aimHold, aimKey);
                    }
                }
                else
                {
                    AimToggleOrHold(aimToggle, aimHold, aimKey);
                }
            }

            #endregion
        }

        private void AimToggleOrHold(bool aimToggle, bool aimHold, WinAPI.VirtualKeyShort aimKey)
        {
            if (aimToggle)
            {
                if (Program.KeyUtils.KeyWentUp(aimKey))
                    AimbotActive = !AimbotActive;
            }
            else if (aimHold)
            {
                AimbotActive = Program.KeyUtils.KeyIsDown(aimKey);
            }
            if (AimbotActive)
                ControlAim();
        }

        private void ControlAim()
        {
            var aimSpotted = Settings.GetBool(Memory.WeaponSection, "Aim Spotted");
            var aimSpottedBy = Settings.GetBool(Memory.WeaponSection, "Aim Spotted By");
            var aimEnemies = Settings.GetBool(Memory.WeaponSection, "Aim Enemies");
            var aimAllies = Settings.GetBool(Memory.WeaponSection, "Aim Allies");
            var aimSmooth = Settings.GetBool(Memory.WeaponSection, "Aim Smooth Enabled");
            var aimBone = Settings.GetInt(Memory.WeaponSection, "Aim Bone");
            var aimFov = Settings.GetFloat(Memory.WeaponSection, "Aim Fov");
            var aimSmoothValue = Settings.GetFloat(Memory.WeaponSection, "Aim Smooth Value");

            var valid = Memory.Players.Where(x => x.Item2.IsValid() && x.Item2.Health != 0 && x.Item2.Dormant != 1);

            if (aimSpotted)
            {
                valid = valid.Where(x => x.Item2.SeenBy(Memory.LocalPlayer));
            }
            if (aimSpottedBy)
            {
                valid = valid.Where(x => Memory.LocalPlayer.SeenBy(x.Item2));
            }

            if (aimEnemies)
            {
                valid = valid.Where(x => x.Item2.TeamNum != Memory.LocalPlayer.TeamNum);
            }

            if (aimAllies)
            {
                valid = valid.Where(x => x.Item2.TeamNum == Memory.LocalPlayer.TeamNum);
            }

            valid = valid.OrderBy(x => (x.Item2.VecOrigin - Memory.LocalPlayer.VecOrigin).Length());

            var closest = Vector3.Zero;
            var closestFov = float.MaxValue;

            foreach (var tuple in valid)
            {
                var player = tuple.Item2;

                var newAngles = (Memory.LocalPlayer.VecOrigin + Memory.LocalPlayer.VecViewOffset).CalcAngle(player.Bones.GetBoneByIndex(aimBone)) - RCS.NewViewAngles;

                newAngles = newAngles.ClampAngle();
                var fov = newAngles.Length() % 360f;

                if (!(fov < closestFov) || !(fov < aimFov))
                    continue;

                closestFov = fov;
                closest = newAngles;
            }
            if (closest == Vector3.Zero)
                return;

            if (aimSmooth)
                RCS.NewViewAngles = RCS.NewViewAngles.SmoothAngle(RCS.NewViewAngles + closest, aimSmoothValue);
            else
                RCS.NewViewAngles += closest;

            RCS.ControlRecoil(true);

            RCS.NewViewAngles = RCS.NewViewAngles;
        }

        #endregion
    }
}