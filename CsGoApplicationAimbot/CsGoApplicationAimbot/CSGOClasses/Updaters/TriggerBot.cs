using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ExternalUtilsCSharp;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class TriggerBot
    {
        #region Variables
        readonly SettingsConfig _settings = new SettingsConfig();
        #endregion

        #region Properties
        public static bool TriggerbotActive { get; set; }
        private int LastShotsFired { get; set; }
        private int LastClip { get; set; }
        private bool TriggerOnTarget { get; set; }
        private long TriggerLastTarget { get; set; }
        private long TriggerLastShot { get; set; }
        private int TriggerBurstFired { get; set; }
        private int TriggerBurstCount { get; set; }
        private bool TriggerShooting { get; set; }
        public int TriggerShotCount { get; set; }
        #endregion

        #region Constructor
        public TriggerBot()
        {
            TriggerbotActive = false;
        }
        #endregion

        public void Update()
        {
            if (Memory.WindowTitle != Program.GameTitle)
                return;

            if (Memory.LocalPlayer == null || Memory.LocalPlayer.Health <= 0)
                return;

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

        private void Triggerbot()
        {
            bool triggerTazer = _settings.GetBool("Misc", "Trigger Taser");
            bool autoKnife = _settings.GetBool("Misc", "Auto Knife");
            bool triggerEnemies = _settings.GetBool(Memory.WeaponSection, "Trigger Enemies");
            bool triggerAllies = _settings.GetBool(Memory.WeaponSection, "Trigger Allies");
            bool burstRandomize = _settings.GetBool(Memory.WeaponSection, "Trigger Burst Randomize");
            float firstShot = _settings.GetFloat(Memory.WeaponSection, "Trigger Delay FirstShot");
            float delayShot = _settings.GetFloat(Memory.WeaponSection, "Trigger Delay Shots");
            int burstShotsMin = _settings.GetInt(Memory.WeaponSection, "Trigger Burst Shots Min");
            int burstShotsMax = _settings.GetInt(Memory.WeaponSection, "Trigger Burst Shots Max");

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
                            TriggerBurstCount = new Random().Next(burstShotsMin, burstShotsMax);
                        else
                            TriggerBurstCount = burstShotsMax;
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
            Thread.Sleep(10);
            WinAPI.mouse_event(WinAPI.MOUSEEVENTF.RIGHTUP, 0, 0, 0, 0);
        }
    }
}
