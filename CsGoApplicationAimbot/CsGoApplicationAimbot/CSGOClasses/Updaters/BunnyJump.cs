using System;
using CsGoApplicationAimbot.CSGOClasses.Enums;
using ExternalUtilsCSharp;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class BunnyJump
    {
        #region Fields

        private readonly Settings _settings = new Settings();
        WinAPI.VirtualKeyShort _bunnyJumpKey;
        bool _bunnyJump;
        int _successfulJumps;

        #endregion

        #region Properties

        private int CurrentJump { get; set; }

        #endregion

        public void Update()
        {
            if (Memory.WindowTitle != Program.GameTitle)
                return;

            if (Memory.LocalPlayer == null || Memory.LocalPlayer.Health <= 0)
                return;

            if (Memory.State != SignOnState.SignonstateFull)
                return;

            if (!Memory.LocalPlayer.IsMoving())
                return;

            //If _bunnyJumpKey is 0, settings has not been applied. 
            if (_bunnyJumpKey == 0)
            {
                _bunnyJumpKey = _settings.GetKey("Bunny Jump", "Bunny Jump Key");
                _bunnyJump = _settings.GetBool("Bunny Jump", "Bunny Jump Enabled");
                _successfulJumps = _settings.GetInt("Bunny Jump", "Bunny Jump Jumps");
            }

            if (!_bunnyJump)
                return;

            BunnyHop();
        }

        private void BunnyHop()
        {
            if (Program.KeyUtils.KeyIsDown(_bunnyJumpKey))
            {
                if (_successfulJumps < CurrentJump)
                    return;

                if (Memory.LocalPlayer.Flags == 256)
                    Program.MemUtils.Write((IntPtr)(Memory.ClientDllBase + Offsets.Misc.Jump), 4);
                else
                {
                    Program.MemUtils.Write((IntPtr)(Memory.ClientDllBase + Offsets.Misc.Jump), 5);
                    //We +1 for each time we jump
                    CurrentJump++;
                }
            }
            else
            {
                //If we are not holding space we set currentJump to 0.
                CurrentJump = 0;
            }
        }
    }
}