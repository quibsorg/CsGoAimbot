using System;
using CsGoApplicationAimbot.CSGOClasses.Enums;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class BunnyJump
    {
        #region Variables

        private readonly SettingsConfig _settings = new SettingsConfig();

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

            BunnyHop();
        }

        private void BunnyHop()
        {
            var bunnyJumpKey = _settings.GetKey("Bunny Jump", "Bunny Jump Key");
            var bunnyJump = _settings.GetBool("Bunny Jump", "Bunny Jump Enabled");
            var successfulJumps = _settings.GetInt("Bunny Jump", "Bunny Jump Jumps");

            if (!bunnyJump)
                return;

            if (Program.KeyUtils.KeyIsDown(bunnyJumpKey))
            {
                if (successfulJumps < CurrentJump)
                    return;

                if (Memory.LocalPlayer.Flags == 256)
                    Program.MemUtils.Write((IntPtr) (Memory.ClientDllBase + Offsets.Misc.Jump), 4);
                else
                {
                    Program.MemUtils.Write((IntPtr) (Memory.ClientDllBase + Offsets.Misc.Jump), 5);
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