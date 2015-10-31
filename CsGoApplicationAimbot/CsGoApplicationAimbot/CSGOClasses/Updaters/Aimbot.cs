using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsGoApplicationAimbot.CSGOClasses.Enums;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class Aimbot
    {

        #region Properties
        #endregion

        #region Variables
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

            ControlAim();
        }

        private void ControlAim()
        {

        }
        #endregion
    }
}
