using System.Linq;
using CsGoApplicationAimbot.CSGOClasses.Enums;
using CsGoApplicationAimbot.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class Aimbot
    {
        public Aimbot()
        {
            AimbotActive = false;
        }

        #region Properties

        public static bool AimbotActive { get; set; }

        #endregion

        #region Fields

        private static readonly Settings Settings = new Settings();
        private WinAPI.VirtualKeyShort _aimKey;
        private bool _aimEnaled;
        private bool _aimScoped;
        private bool _aimToggle;
        private bool _aimHold;
        private int _aimStart;
        private bool _aimSpotted;
        private bool _aimSpottedBy;
        private bool _aimEnemies;
        private bool _aimAllies;
        private bool _aimSmooth;
        private int _aimBone;
        private float _aimFov;
        private float _aimSmoothValue;

        #endregion

        #region Methods

        public void Update()
        {
            if (Memory.WindowTitle != Program.GameTitle)
                return;

            if (Memory.LocalPlayerWeapon == null)
                return;

            if (Memory.LocalPlayer == null || Memory.LocalPlayer.Health <= 0)
                return;

            if (Memory.State != SignOnState.SignonstateFull)
                return;

            #region Aimbot

            if (Memory.ActiveWeapon != Memory.WeaponSection)
            {
                _aimKey = Settings.GetKey(Memory.WeaponSection, "Aim Key");
                _aimEnaled = Settings.GetBool(Memory.WeaponSection, "Aim Enabled");
                _aimScoped = Settings.GetBool(Memory.WeaponSection, "Aim When Scoped");
                _aimToggle = Settings.GetBool(Memory.WeaponSection, "Aim Toggle");
                _aimHold = Settings.GetBool(Memory.WeaponSection, "Aim Hold");
                _aimStart = Settings.GetInt(Memory.WeaponSection, "Aim Start");
                _aimSpotted = Settings.GetBool(Memory.WeaponSection, "Aim Spotted");
                _aimSpottedBy = Settings.GetBool(Memory.WeaponSection, "Aim Spotted By");
                _aimEnemies = Settings.GetBool(Memory.WeaponSection, "Aim Enemies");
                _aimAllies = Settings.GetBool(Memory.WeaponSection, "Aim Allies");
                _aimSmooth = Settings.GetBool(Memory.WeaponSection, "Aim Smooth Enabled");
                _aimBone = Settings.GetInt(Memory.WeaponSection, "Aim Bone");
                _aimFov = Settings.GetFloat(Memory.WeaponSection, "Aim Fov");
                _aimSmoothValue = Settings.GetFloat(Memory.WeaponSection, "Aim Smooth Value");
            }
            Memory.ActiveWeapon = Memory.WeaponSection;

            if (!_aimEnaled)
                return;

            //Won't aim if we do not have any ammo in the clip.
            if (Memory.LocalPlayerWeapon.Clip1 <= 0 || Memory.LocalPlayer.ShotsFired < _aimStart)
                return;

            if (_aimScoped)
            {
                if (Memory.LocalPlayerWeapon.ZoomLevel > 0)
                {
                    AimToggleOrHold(_aimToggle, _aimHold, _aimKey);
                }
            }
            else
            {
                AimToggleOrHold(_aimToggle, _aimHold, _aimKey);
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
            var valid = Memory.Players.Where(x => x.Item2.IsValid() && x.Item2.Health != 0 && x.Item2.Dormant != 1);

            if (_aimSpotted)
            {
                valid = valid.Where(x => x.Item2.SeenBy(Memory.LocalPlayer));
            }
            if (_aimSpottedBy)
            {
                valid = valid.Where(x => Memory.LocalPlayer.SeenBy(x.Item2));
            }

            if (_aimEnemies)
            {
                valid = valid.Where(x => x.Item2.TeamNum != Memory.LocalPlayer.TeamNum);
            }

            if (_aimAllies)
            {
                valid = valid.Where(x => x.Item2.TeamNum == Memory.LocalPlayer.TeamNum);
            }

            valid = valid.OrderBy(x => (x.Item2.VecOrigin - Memory.LocalPlayer.VecOrigin).Length());

            var closest = Vector3.Zero;
            var closestFov = float.MaxValue;

            foreach (var tuple in valid)
            {
                var player = tuple.Item2;

                var newAngles =
                    (Memory.LocalPlayer.VecOrigin + Memory.LocalPlayer.VecViewOffset).CalcAngle(
                        player.Bones.GetBoneByIndex(_aimBone)) - Rcs.NewViewAngles;

                newAngles = newAngles.ClampAngle();
                var fov = newAngles.Length()%360f;

                if (!(fov < closestFov) || !(fov < _aimFov))
                    continue;

                closestFov = fov;
                closest = newAngles;
            }
            if (closest == Vector3.Zero)
                return;

            if (_aimSmooth)
                Rcs.NewViewAngles = Rcs.NewViewAngles.SmoothAngle(Rcs.NewViewAngles + closest, _aimSmoothValue);
            else
                Rcs.NewViewAngles += closest;

            Rcs.ControlRecoil(true);

            Rcs.NewViewAngles = Rcs.NewViewAngles;
        }

        #endregion
    }
}