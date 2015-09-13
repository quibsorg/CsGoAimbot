using System.Linq;
using CsGoApplicationAimbot.CSGO.Enums;
using CsGoApplicationAimbot.CSGOClasses;
using ExternalUtilsCSharp.SharpDXRenderer.Controls;
using SharpDX;

namespace CsGoApplicationAimbot.UI
{
    public class PlayerRadar : SharpDXRadar
    {
        public override void Update(double secondsElapsed, ExternalUtilsCSharp.KeyUtils keyUtils, SharpDX.Vector2 cursorPoint, bool checkMouse = false)
        {
            base.Update(secondsElapsed, keyUtils, cursorPoint, checkMouse);
            Framework fw = Program.Framework;
            if (fw.LocalPlayer == null)
                return;
            if (!fw.LocalPlayer.IsValid())
                return;

            this.Scaling = Program.ConfigUtils.GetValue<float>("radarScale");
            this.Width = Program.ConfigUtils.GetValue<float>("radarWidth");
            this.Height = Program.ConfigUtils.GetValue<float>("radarHeight");

            if(fw.LocalPlayer.MITeamNum == (int)Team.Terrorists)
            {
                this.AlliesColor = Color.Red;
                this.EnemiesColor = Color.LightBlue;
            }
            else
            {
                this.AlliesColor = Color.LightBlue;
                this.EnemiesColor = Color.Red;
            }

            this.RotationDegrees = fw.ViewAngles.Y + 90;
            this.CenterCoordinate = new SharpDX.Vector2(fw.LocalPlayer.MVecOrigin.X, fw.LocalPlayer.MVecOrigin.Y);

            if (Program.ConfigUtils.GetValue<bool>("radarEnemies"))
            {
                var enemies = fw.Players.Where(x => x.Item2.IsValid() && x.Item2.MIHealth > 0 && x.Item2.MITeamNum != fw.LocalPlayer.MITeamNum);
                this.Enemies = enemies.Select(x => new Vector2(x.Item2.MVecOrigin.X, x.Item2.MVecOrigin.Y)).ToArray();
            }
            else { this.Enemies = null; }

            if (Program.ConfigUtils.GetValue<bool>("radarAllies"))
            {
                var allies = fw.Players.Where(x => x.Item2.IsValid() && x.Item2.MIHealth > 0 && x.Item2.MITeamNum == fw.LocalPlayer.MITeamNum);
                this.Allies = allies.Select(x => new Vector2(x.Item2.MVecOrigin.X, x.Item2.MVecOrigin.Y)).ToArray();
            }
            else { this.Allies = null; }
        }
    }
}
