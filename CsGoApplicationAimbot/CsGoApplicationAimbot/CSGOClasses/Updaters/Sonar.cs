using System;
using System.Threading;
using CsGoApplicationAimbot.CSGOClasses.Enums;

namespace CsGoApplicationAimbot.CSGOClasses.Updaters
{
    public class Sonar
    {
        #region Properties

        private float LastPercent { get; set; }

        #endregion

        public void Update()
        {
            if (Memory.WindowTitle != Program.GameTitle)
                return;

            if (Memory.LocalPlayer == null || Memory.LocalPlayer.Health <= 0)
                return;

            if (Memory.State != SignOnState.SignonstateFull)
                return;

            SoundEsp();
        }

        private void SoundEsp()
        {
            if (_sonarEnabled == null)
            {
                _sonarEnabled = _settings.GetBool("Sonar", "Sonar Enabled");
                _sonarSound = _settings.GetInt("Sonar", "Sonar Sound");
                _sonarRange = _settings.GetFloat("Sonar", "Sonar Range");
                _sonarInterval = _settings.GetFloat("Sonar", "Sonar Interval");
                _sonarVolume = _settings.GetFloat("Sonar", "Sonar Volume");
            }

            if ((bool) !_sonarEnabled)
                return;

            //Set's our sound volume
            Program.SoundManager.SetVolume(_sonarVolume/100f);

            var span = new TimeSpan(DateTime.Now.Ticks - _lastBeep);

            if (span.TotalMilliseconds > _sonarInterval)
            {
                _lastBeep = DateTime.Now.Ticks;
                return;
            }

            var minRange = _sonarRange/_sonarInterval*(float) span.TotalMilliseconds;

            LastPercent = 100f/_sonarInterval*(float) span.TotalMilliseconds;

            var leastDist = float.MaxValue;

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

                var dist = Memory.LocalPlayer.DistanceToOtherEntityInMetres(player);
                if (dist <= minRange)
                {
                    leastDist = dist;
                    break;
                }
            }

            if (leastDist != float.MaxValue)
            {
                Program.SoundManager.Play(_sonarSound - 1);
                Thread.Sleep(50);
                _lastBeep = DateTime.Now.Ticks;
            }
        }

        #region Variables

        private readonly Settings _settings = new Settings();
        private long _lastBeep;
        private bool? _sonarEnabled;
        private int _sonarSound;
        private float _sonarRange;
        private float _sonarInterval;
        private float _sonarVolume;

        #endregion
    }
}