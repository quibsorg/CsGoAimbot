using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    class Aimbot
    {
        private void Update()
        {
            if (!Memory.ShouldUpdate())
                return;
        }
    }
}
