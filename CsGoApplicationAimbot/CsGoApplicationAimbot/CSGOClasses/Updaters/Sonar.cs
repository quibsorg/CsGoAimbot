using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class Sonar
    {
        #region Properties
        private float LastPercent { get; set; }

        #endregion

        #region Variables
        private readonly SettingsConfig _settings = new SettingsConfig();
        private int _localPlayer;
        private long _lastBeep;
        #endregion

        public void Update()
        {
            if (Memory.WindowTitle != Program.GameTitle)
                return;

            SoundEsp();
        }

        private void SoundEsp()
        {
            bool sonarEnabled = _settings.GetBool("Sonar", "Sonar Enabled");
            int sonarSound = _settings.GetInt("Sonar", "Sonar Sound");
            float sonarRange = _settings.GetFloat("Sonar", "Sonar Range");
            float sonarInterval = _settings.GetFloat("Sonar", "Sonar Interval");
            float sonarVolume = _settings.GetFloat("Sonar", "Sonar Volume");

            if (!sonarEnabled)
                return;
            //Set's our sound volume
            Program.SoundManager.SetVolume(sonarVolume / 100f);

            TimeSpan span = new TimeSpan(DateTime.Now.Ticks - _lastBeep);

            if (span.TotalMilliseconds > sonarInterval)
            {
                _lastBeep = DateTime.Now.Ticks;
                return;
            }

            float minRange = sonarRange / sonarInterval * (float)span.TotalMilliseconds;
            LastPercent = 100f / sonarInterval * (float)span.TotalMilliseconds;

            float leastDist = float.MaxValue;

            foreach (var player in Memory.Players)
            {
                //If the ID does match it's our player
                if (player.Item2.Id == Memory.LocalPlayer.Id)
                    continue;

                //If the player is dead.
                if (player.Item2.Health == 0)
                    continue;

                //if the player is in the same team as us.
                if (player.Item2.TeamNum == Memory.LocalPlayer.TeamNum)
                    continue;

                float dist = Memory.LocalPlayer.DistanceToOtherEntityInMetres(player);
                if (dist <= minRange)
                {
                    leastDist = dist;
                    break;
                }
            }

            if (leastDist != float.MaxValue)
            {
                Program.SoundManager.Play(sonarSound - 1);
                Thread.Sleep(50);
                _lastBeep = DateTime.Now.Ticks;
            }
        }
    }
}
