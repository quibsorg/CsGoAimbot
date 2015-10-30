using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CsGoApplicationAimbot.CSGOClasses.Enums;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class Framework
    {


        #region Variables
        readonly SettingsConfig _settings = new SettingsConfig();
        #endregion

        #region Properties
        private bool AimbotActive { get; set; }
        public static bool TriggerbotActive { get; set; }
        public static Vector3 NewViewAngles { get; set; }
        private int LastShotsFired { get; set; }
        private int LastClip { get; set; }
        private bool TriggerOnTarget { get; set; }
        private long TriggerLastTarget { get; set; }
        private long TriggerLastShot { get; set; }
        private int TriggerBurstFired { get; set; }
        private int TriggerBurstCount { get; set; }
        private bool TriggerShooting { get; set; }

        #endregion
        #region Constructor
        public Framework()
        {
            AimbotActive = false;
            TriggerbotActive = false;
        }
        #endregion

        public void Update()
        {
            NewViewAngles = NewViewAngles.SmoothAngle(Memory.ViewAngles, 1f);

            #region Aimbot
            //WinAPI.VirtualKeyShort aimKey = _settings.GetKey(Memory.WeaponSection, "Aim Key");
            //bool aimEnaled = _settings.GetBool(Memory.WeaponSection, "Aim Enabled");
            //bool aimScoped = _settings.GetBool(Memory.WeaponSection, "Aim When Scoped");
            //bool aimToggle = _settings.GetBool(Memory.WeaponSection, "Aim Toggle");
            //bool aimHold = _settings.GetBool(Memory.WeaponSection, "Aim Hold");
            //int aimStart = _settings.GetInt(Memory.WeaponSection, "Aim Start");
            //Won't aim if we do not have any ammo in the clip.
            //if (Memory.LocalPlayerWeapon != null && (aimEnaled && Memory.LocalPlayerWeapon.Clip1 > 0 && Memory.LocalPlayer.ShotsFired > aimStart))
            //{
            //    if (aimScoped)
            //    {
            //        if (Memory.LocalPlayerWeapon.ZoomLevel > 0)
            //        {
            //            AimToggleOrHold(aimToggle, aimHold, aimKey);
            //        }
            //    }
            //    else
            //    {
            //        AimToggleOrHold(aimToggle, aimHold, aimKey);
            //    }
            //}
            #endregion

            #region Trigger
            WinAPI.VirtualKeyShort triggerKey = _settings.GetKey(Memory.WeaponSection, "Trigger Key");
            bool triggerEnabled = _settings.GetBool(Memory.WeaponSection, "Trigger Enabled");
            bool triggerScoped = _settings.GetBool(Memory.WeaponSection, "Trigger When Scoped");
            bool triggerBurstEnabled = _settings.GetBool(Memory.WeaponSection, "Trigger Burst Enabled");
            bool triggerToggle = _settings.GetBool(Memory.WeaponSection, "Trigger Toggle");
            bool triggerHold = _settings.GetBool(Memory.WeaponSection, "Trigger Hold");

            if (triggerEnabled)
            {
                //Trigger Scoped is only good for snipers.
                if (triggerScoped)
                {
                    //ZoomLevel 0 = No Zoom
                    if (Memory.LocalPlayerWeapon != null && Memory.LocalPlayerWeapon.ZoomLevel > 0)
                    {
                        TriggerToggleOrHold(triggerKey, triggerToggle, triggerHold);
                    }
                }
                else
                {
                    TriggerToggleOrHold(triggerKey, triggerToggle, triggerHold);
                }
            }

            //We set Trigger shooting to true later down the line.
            if (TriggerShooting)
            {
                //If our LocalPlayer weapon is null, do not shoot it will crash.
                if (Memory.LocalPlayerWeapon == null)
                {
                    TriggerShooting = false;
                }
                else
                {
                    if (triggerBurstEnabled)
                    {
                        if (TriggerBurstFired >= TriggerBurstCount)
                        {
                            TriggerShooting = false;
                            TriggerBurstFired = 0;
                        }
                        else
                        {
                            if (Memory.LocalPlayerWeapon.Clip1 != LastClip)
                            {
                                TriggerBurstFired += Math.Abs(Memory.LocalPlayerWeapon.Clip1 - LastClip);
                            }
                            else
                            {
                                Shoot();
                            }
                        }
                    }
                    else
                    {
                        Shoot();
                        TriggerShooting = false;
                    }
                }
            }
            LastClip = Memory.LocalPlayerWeapon?.Clip1 ?? 0;
            LastShotsFired = Memory.LocalPlayer.ShotsFired;
            #endregion
        }
        private void TriggerToggleOrHold(WinAPI.VirtualKeyShort triggerKey, bool triggerToggle, bool triggerHold)
        {
            if (triggerToggle)
            {
                if (Program.KeyUtils.KeyWentUp(triggerKey))
                    TriggerbotActive = !TriggerbotActive;
            }
            else if (triggerHold)
            {
                TriggerbotActive = Program.KeyUtils.KeyIsDown(triggerKey);
            }
            if (TriggerbotActive)
                Triggerbot();
        }

        //private void AimToggleOrHold(bool aimToggle, bool aimHold, WinAPI.VirtualKeyShort aimKey)
        //{
        //    if (aimToggle)
        //    {
        //        if (Program.KeyUtils.KeyWentUp(aimKey))
        //            AimbotActive = !AimbotActive;
        //    }
        //    else if (aimHold)
        //    {
        //        AimbotActive = Program.KeyUtils.KeyIsDown(aimKey);
        //    }
        //    if (AimbotActive)
        //        ControlAim();
        //}
        //private void ControlAim()
        //{
        //    bool aimSpotted = _settings.GetBool(Memory.WeaponSection, "Aim Spotted");
        //    bool aimSpottedBy = _settings.GetBool(Memory.WeaponSection, "Aim Spotted By");
        //    bool aimEnemies = _settings.GetBool(Memory.WeaponSection, "Aim Enemies");
        //    bool aimAllies = _settings.GetBool(Memory.WeaponSection, "Aim Allies");
        //    bool aimSmooth = _settings.GetBool(Memory.WeaponSection, "Aim Smooth Enabled");
        //    int aimBone = _settings.GetInt(Memory.WeaponSection, "Aim Bone");
        //    float aimFov = _settings.GetFloat(Memory.WeaponSection, "Aim Fov");
        //    float aimSmoothValue = _settings.GetFloat(Memory.WeaponSection, "Aim Smooth Value");
        //
        //    //if (LocalPlayer == null)
        //    //    return;
        //
        //    var valid = Memory.Players.Where(x => x.Item2.IsValid() && x.Item2.Health != 0 && x.Item2.Dormant != 1);
        //
        //    if (aimSpotted)
        //        valid = valid.Where(x => x.Item2.SeenBy(Memory.LocalPlayer));
        //
        //    if (aimSpottedBy)
        //        valid = valid.Where(x => Memory.LocalPlayer.SeenBy(x.Item2));
        //
        //    if (aimEnemies)
        //        valid = valid.Where(x => x.Item2.TeamNum != Memory.LocalPlayer.TeamNum);
        //
        //    if (aimAllies)
        //        valid = valid.Where(x => x.Item2.TeamNum == Memory.LocalPlayer.TeamNum);
        //
        //    valid = valid.OrderBy(x => (x.Item2.VecOrigin - Memory.LocalPlayer.VecOrigin).Length());
        //
        //    var closest = Vector3.Zero;
        //    var closestFov = float.MaxValue;
        //    foreach (var tpl in valid)
        //    {
        //        var player = tpl.Item2;
        //
        //        var newAngles = (Memory.LocalPlayer.VecOrigin + Memory.LocalPlayer.VecViewOffset).CalcAngle(player.Bones.GetBoneByIndex(aimBone)) - NewViewAngles;
        //        newAngles = newAngles.ClampAngle();
        //        var fov = newAngles.Length() % 360f;
        //
        //        if (!(fov < closestFov) || !(fov < aimFov))
        //            continue;
        //
        //        closestFov = fov;
        //        closest = newAngles;
        //    }
        //    if (closest == Vector3.Zero)
        //        return;
        //
        //
        //
        //    if (aimSmooth)
        //        NewViewAngles = NewViewAngles.SmoothAngle(NewViewAngles + closest, aimSmoothValue);
        //    else
        //        NewViewAngles += closest;
        //    ControlRecoil(true);
        //
        //    NewViewAngles = NewViewAngles;
        //}

        private void Triggerbot()
        {
            bool triggerTazer = _settings.GetBool("Misc", "Trigger Taser");
            bool autoKnife = _settings.GetBool("Misc", "Auto Knife");
            bool triggerEnemies = _settings.GetBool(Memory.WeaponSection, "Trigger Enemies");
            bool triggerAllies = _settings.GetBool(Memory.WeaponSection, "Trigger Allies");
            bool burstRandomize = _settings.GetBool(Memory.WeaponSection, "Trigger Burst Randomize");
            float firstShot = _settings.GetFloat(Memory.WeaponSection, "Trigger Delay FirstShot");
            float delayShot = _settings.GetFloat(Memory.WeaponSection, "Trigger Delay Shots");
            float burstShots = _settings.GetFloat(Memory.WeaponSection, "Trigger Burst Shots");

            //If our player is null, or if trigger is shooting we return.
            if (Memory.LocalPlayer == null || TriggerShooting)
                return;

            //If no one is in hour crosshair we return.(Not aiming at someone)
            if (Memory.Players.Count(x => x.Item2.Id == Memory.LocalPlayer.CrosshairIdx) <= 0)
                return;

            //We get the player that is in our crosshair.
            var player = Memory.Players.First(x => x.Item2.Id == Memory.LocalPlayer.CrosshairIdx).Item2;

            if (triggerTazer)
            {
                if (Memory.LocalPlayerWeapon.ClassName == "CWeaponTaser" && Memory.LocalPlayer.DistanceToOtherEntityInMetres(player) <= 3)
                {
                    Shoot();
                    return;
                }
            }
            if (autoKnife)
            {
                if (Memory.LocalPlayerWeapon.ClassName == "CKnife" && Memory.LocalPlayer.DistanceToOtherEntityInMetres(player) <= 1.4f)
                {
                    RightKnife();
                    return;
                }
            }

            //We check the players teamnum, if it matches ours teammate, if not enemy.
            if ((triggerEnemies && player.TeamNum != Memory.LocalPlayer.TeamNum) || (triggerAllies && player.TeamNum == Memory.LocalPlayer.TeamNum))
            {
                if (Memory.LocalPlayerWeapon.ClassName == "CWeaponTaser")
                    return;

                if (Memory.LocalPlayerWeapon.ClassName == "CKnife")
                    return;

                //If we got this far, we have a target in our crosshair.
                if (!TriggerOnTarget)
                {
                    TriggerOnTarget = true;
                    TriggerLastTarget = DateTime.Now.Ticks;
                }
                else
                {
                    if (!(new TimeSpan(DateTime.Now.Ticks - TriggerLastTarget).TotalMilliseconds >= firstShot))
                        return;
                    if (!(new TimeSpan(DateTime.Now.Ticks - TriggerLastShot).TotalMilliseconds >= delayShot))
                        return;
                    //Get the tick from our last shoot.
                    TriggerLastShot = DateTime.Now.Ticks;

                    //If we are not shooting.
                    if (!TriggerShooting)
                    {
                        if (burstRandomize)
                            TriggerBurstCount = new Random().Next(1, (int)burstShots);
                        else
                            TriggerBurstCount = (int)burstShots;
                    }
                    TriggerShooting = true;
                }
            }
            else
            {
                //No one is in our crosshair.
                TriggerOnTarget = false;
            }
        }
        private static void Shoot()
        {
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTDOWN, 0, 0, 0, 0);
            Thread.Sleep(10);
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.LEFTUP, 0, 0, 0, 0);
        }
        private static void RightKnife()
        {
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.RIGHTDOWN, 0, 0, 0, 0);
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.RIGHTUP, 0, 0, 0, 0);
        }
    }
}