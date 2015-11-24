using System;
using System.Linq;
using System.Threading;
using CsGoApplicationAimbot.CSGOClasses.Enums;
using ExternalUtilsCSharp;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class TriggerBot
    {
        #region Fields

        private readonly Settings _settings = new Settings();
        private WinAPI.VirtualKeyShort _triggerKey;
        private bool _triggerEnabled;
        private bool _triggerScoped;
        private bool _triggerBurstEnabled;
        private bool _triggerToggle;
        bool _triggerHold;
        bool _triggerDash;
        private bool _triggerTazer;
        private bool _autoKnife;
        private bool _triggerEnemies;
        private bool _triggerAllies;
        private bool _burstRandomize;
        float _firstShot;
        float _delayShot;
        int _burstShotsMin;
        int _burstShotsMax;
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

            if (Memory.State != SignOnState.SignonstateFull)
                return;


            if (Memory.ActiveWeapon != Memory.WeaponSection)
            {
                Console.WriteLine("Trigger I changed weapon to {0}", Memory.WeaponSection);
                _triggerKey = _settings.GetKey(Memory.WeaponSection, "Trigger Key");
                _triggerEnabled = _settings.GetBool(Memory.WeaponSection, "Trigger Enabled");
                _triggerScoped = _settings.GetBool(Memory.WeaponSection, "Trigger When Scoped");
                _triggerBurstEnabled = _settings.GetBool(Memory.WeaponSection, "Trigger Burst Enabled");
                _triggerToggle = _settings.GetBool(Memory.WeaponSection, "Trigger Toggle");
                _triggerHold = _settings.GetBool(Memory.WeaponSection, "Trigger Hold");
                _triggerDash = _settings.GetBool(Memory.WeaponSection, "Trigger Only When Standing Still");
                _triggerTazer = _settings.GetBool("Misc", "Trigger Taser");
                _autoKnife = _settings.GetBool("Misc", "Auto Knife");
                _triggerEnemies = _settings.GetBool(Memory.WeaponSection, "Trigger Enemies");
                _triggerAllies = _settings.GetBool(Memory.WeaponSection, "Trigger Allies");
                _burstRandomize = _settings.GetBool(Memory.WeaponSection, "Trigger Burst Randomize");
                _firstShot = _settings.GetFloat(Memory.WeaponSection, "Trigger Delay FirstShot");
                _delayShot = _settings.GetFloat(Memory.WeaponSection, "Trigger Delay Shots");
                _burstShotsMin = _settings.GetInt(Memory.WeaponSection, "Trigger Burst Shots Min");
                _burstShotsMax = _settings.GetInt(Memory.WeaponSection, "Trigger Burst Shots Max");
            }

            if (_triggerEnabled)
            {
                if (_triggerDash)
                {
                    if (Memory.LocalPlayer.IsMoving())
                    {
                        return;
                    }
                }
                //Trigger Scoped is only good for snipers.
                if (_triggerScoped)
                {
                    //ZoomLevel 0 = No Zoom
                    if (Memory.LocalPlayerWeapon != null && Memory.LocalPlayerWeapon.ZoomLevel > 0)
                    {
                        TriggerToggleOrHold(_triggerKey, _triggerToggle, _triggerHold);
                    }
                }
                else
                {
                    TriggerToggleOrHold(_triggerKey, _triggerToggle, _triggerHold);
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
                    if (_triggerBurstEnabled)
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


            //If our player is null, or if trigger is shooting we return.
            if (Memory.LocalPlayer == null || TriggerShooting)
                return;

            //If no one is in hour crosshair we return.(Not aiming at someone)
            if (Memory.Players.Count(x => x.Item2.Id == Memory.LocalPlayer.CrosshairIdx) <= 0)
                return;

            //We get the player that is in our crosshair.
            var player = Memory.Players.First(x => x.Item2.Id == Memory.LocalPlayer.CrosshairIdx).Item2;

            if (_triggerTazer)
            {
                if (Memory.LocalPlayerWeapon.ClassName == "CWeaponTaser" &&
                    Memory.LocalPlayer.DistanceToOtherEntityInMetres(player) <= 3)
                {
                    Shoot();
                    return;
                }
            }
            if (_autoKnife)
            {
                if (Memory.LocalPlayerWeapon.ClassName == "CKnife" &&
                    Memory.LocalPlayer.DistanceToOtherEntityInMetres(player) <= 1.4f)
                {
                    RightKnife();
                    return;
                }
            }

            //We check the players teamnum, if it matches ours teammate, if not enemy.
            if ((_triggerEnemies && player.TeamNum != Memory.LocalPlayer.TeamNum) ||
                (_triggerAllies && player.TeamNum == Memory.LocalPlayer.TeamNum))
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
                    if (!(new TimeSpan(DateTime.Now.Ticks - TriggerLastTarget).TotalMilliseconds >= _firstShot))
                        return;
                    if (!(new TimeSpan(DateTime.Now.Ticks - TriggerLastShot).TotalMilliseconds >= _delayShot))
                        return;

                    //Get the tick from our last shoot.
                    TriggerLastShot = DateTime.Now.Ticks;

                    //If we are not shooting.
                    if (!TriggerShooting)
                    {
                        if (_burstRandomize)
                            TriggerBurstCount = new Random().Next(_burstShotsMin, _burstShotsMax);
                        else
                            TriggerBurstCount = _burstShotsMax;
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

        #endregion
    }
}