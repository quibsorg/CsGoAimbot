using System;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class BunnyJump
    {
        public void Update()
        {
            if (!Memory.ShouldUpdate())
                return;

            if (!Memory.LocalPlayer.IsMoving())
                return;

            //If _bunnyJumpKey is 0, settings has not been applied. 
            if (_bunnyJumpKey == 0)
            {
                _bunnyJumpKey = _settings.GetKey("Bunny Jump", "Bunny Jump Key");
                _bunnyJumpEnabled = _settings.GetBool("Bunny Jump", "Bunny Jump Enabled");
            }

            if (!_bunnyJumpEnabled)
                return;

            BunnyHop();
        }

        private void BunnyHop()
        {
            if (Program.KeyUtils.KeyIsDown(_bunnyJumpKey))
            {
                if (Memory.LocalPlayer.Flags == 256)
                {
                    Program.MemUtils.Write((IntPtr) (Memory.ClientDllBase + Offsets.Misc.Jump), 4);
                }
                else
                {
                    Program.MemUtils.Write((IntPtr) (Memory.ClientDllBase + Offsets.Misc.Jump), 5);
                }
            }
        }

        #region Fields

        private readonly Settings _settings = new Settings();
        private WinAPI.VirtualKeyShort _bunnyJumpKey;
        private bool _bunnyJumpEnabled;
        private int _successfulJumps;

        #endregion
    }
}