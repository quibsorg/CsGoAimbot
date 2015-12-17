using System;
using System.Linq;
using System.Threading;
using CsGoApplicationAimbot.CSGOClasses.Enums;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class TriggerBot
    {
        #region Constructor

        public TriggerBot()
        {
            TriggerbotActive = false;
        }

        #endregion

        public void Update()
        {
            if (!Memory.ShouldUpdate())
                return;

            if (Memory.ActiveWeapon != Memory.WeaponSection)
            {
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
                    if (Memory.LocalPlayer.IsMoving())
                        return;
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
                                if (Memory.LocalPlayerWeapon.Clip1 > 0)
                                {
                                    Console.WriteLine("Test");
                                    Shoot();
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Memory.LocalPlayerWeapon.Clip1 > 0)
                        {
                            Shoot();
                            TriggerShooting = false;
                        }
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
            if (Memory.LocalPlayer == null || TriggerShooting)
                return;

            if (Memory.Players.Count(x => x.Item2.Id == Memory.LocalPlayer.CrosshairIdx) <= 0)
                return;

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

            if ((_triggerEnemies && player.TeamNum != Memory.LocalPlayer.TeamNum) || (_triggerAllies && player.TeamNum == Memory.LocalPlayer.TeamNum))
            {
                if (Memory.LocalPlayerWeapon.ClassName == "CWeaponTaser")
                    return;

                if (Memory.LocalPlayerWeapon.ClassName == "CKnife")
                    return;

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

        #region Fields

        private readonly Settings _settings = new Settings();
        private WinAPI.VirtualKeyShort _triggerKey;
        private bool _triggerEnabled;
        private bool _triggerScoped;
        private bool _triggerBurstEnabled;
        private bool _triggerToggle;
        private bool _triggerHold;
        private bool _triggerDash;
        private bool _triggerTazer;
        private bool _autoKnife;
        private bool _triggerEnemies;
        private bool _triggerAllies;
        private bool _burstRandomize;
        private float _firstShot;
        private float _delayShot;
        private int _burstShotsMin;
        private int _burstShotsMax;

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

        #endregion
    }
}