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
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);
        #region Properties
        private LocalPlayer LocalPlayer { get; set; }
        private Tuple<int, Player>[] Players { get; set; }
        private float LastPercent { get; set; }

        #endregion

        #region Variables
        private readonly SettingsConfig _settings = new SettingsConfig();
        private readonly int _entityList;
        private readonly int _clientDllBase;
        private int _localPlayer;
        private long _lastBeep;
        #endregion

        public Sonar(ProcessModule engineDll, ProcessModule clientDll)
        {
            Scanner.ScanOffsets(clientDll, engineDll, Program.MemUtils);
            _clientDllBase = (int)clientDll.BaseAddress;
            _entityList = _clientDllBase + Offsets.Misc.EntityList;
        }

        public void Update()
        {
            var activeWindow = GetActiveWindowTitle();
            if (activeWindow != Program.GameTitle)
                return;

            var players = new List<Tuple<int, Player>>();
            _localPlayer = Program.MemUtils.Read<int>((IntPtr)(_clientDllBase + Offsets.Misc.LocalPlayer));

            var data = new byte[16 * 8192];
            Program.MemUtils.Read((IntPtr)(_entityList), out data, data.Length);

            for (var i = 0; i < data.Length / 16; i++)
            {
                var address = BitConverter.ToInt32(data, 16 * i);
                if (address == 0) continue;
                var entity = new BaseEntity(address);
                if (!entity.IsValid())
                    continue;
                if (entity.IsPlayer())
                    players.Add(new Tuple<int, Player>(i, new Player(entity)));
            }

            Players = players.ToArray();

            if (players.Exists(x => x.Item2.Address == _localPlayer))
            {
                LocalPlayer = new LocalPlayer(players.First(x => x.Item2.Address == _localPlayer).Item2);
            }
            if (LocalPlayer == null || LocalPlayer.Health <= 0)
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

            foreach (var player in Players)
            {
                //If the ID does match it's our player
                if (player.Item2.Id == LocalPlayer.Id)
                    continue;

                //If the player is dead.
                if (player.Item2.Health == 0)
                    continue;

                //if the player is in the same team as us.
                if (player.Item2.TeamNum == LocalPlayer.TeamNum)
                    continue;

                float dist = LocalPlayer.DistanceToOtherEntityInMetres(player);
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
        private static string GetActiveWindowTitle()
        {
            const int nChars = 256;
            StringBuilder builder = new StringBuilder(nChars);
            IntPtr handle = GetForegroundWindow();

            if (GetWindowText(handle, builder, nChars) > 0)
            {
                return builder.ToString();
            }
            return null;
        }

    }
}
