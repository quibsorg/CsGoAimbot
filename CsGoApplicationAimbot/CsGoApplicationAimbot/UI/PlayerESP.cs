using System;
using System.Linq;
using CsGoApplicationAimbot.CSGO.Enums;
using CsGoApplicationAimbot.CSGOClasses;
using ExternalUtilsCSharp;
using ExternalUtilsCSharp.SharpDXRenderer;
using ExternalUtilsCSharp.SharpDXRenderer.Controls;
using SharpDX;

namespace CsGoApplicationAimbot.UI
{
    public class PlayerEsp : SharpDXControl
    {
        public static float MaxDistance = float.MaxValue;
        public static float BorderSize = 2f;
        public static float BorderMargin = 8f;
        public CsPlayer Player { get; set; }
        public override void Draw(ExternalUtilsCSharp.SharpDXRenderer.SharpDXRenderer renderer)
        {
            if (!WithOverlay.ConfigUtils.GetValue<bool>("espEnabled"))
                return;
            Framework fw = WithOverlay.Framework;
            
            if (!fw.IsPlaying())
                return;
            if (Player == null || fw.LocalPlayer == null)
                return;
            if (Player.Address == fw.LocalPlayer.Address)
                return;

            float distance = Player.MVecOrigin.DistanceTo(fw.LocalPlayer.MVecOrigin);
            if (!Player.IsValid() || Player.MVecOrigin == ExternalUtilsCSharp.MathObjects.Vector3.Zero || distance > MaxDistance || Player.MIHealth == 0)
                return;
            
            #region Bones + W2S
            ExternalUtilsCSharp.MathObjects.Vector3[] arms = new ExternalUtilsCSharp.MathObjects.Vector3[] 
            {
                Player.Bones.LeftHand, 
                Player.Bones.LeftElbow,
                Player.Bones.LeftShoulder,
                Player.Bones.Spine5,
                Player.Bones.RightShoulder, 
                Player.Bones.RightElbow, 
                Player.Bones.RightHand 
            };
            ExternalUtilsCSharp.MathObjects.Vector3[] legs = new ExternalUtilsCSharp.MathObjects.Vector3[] 
            { 
                Player.Bones.LeftFoot,
                Player.Bones.LeftKnee,
                Player.Bones.LeftHip,
                Player.Bones.Spine1,
                Player.Bones.RightHip,
                Player.Bones.RightKnee,
                Player.Bones.RightFoot
            };
            ExternalUtilsCSharp.MathObjects.Vector3[] spine = new ExternalUtilsCSharp.MathObjects.Vector3[] 
            {
                Player.Bones.Spine1,
                Player.Bones.Spine2,
                Player.Bones.Spine3,
                Player.Bones.Spine4,
                Player.Bones.Spine5,
                Player.Bones.Neck + new ExternalUtilsCSharp.MathObjects.Vector3(0,0,5)
            };
            ExternalUtilsCSharp.MathObjects.Vector3[] body = MiscUtils.MergeArrays(arms, legs, spine);

            if (body.Count(x=>x == ExternalUtilsCSharp.MathObjects.Vector3.Zero) > 0)
                return;
            if (body.Count(x => x.DistanceTo(Player.MVecOrigin) > 100) > 0)
                return;

            Vector2[] w2SArms = W2S(arms);
            Vector2[] w2SLegs = W2S(legs);
            Vector2[] w2SSpine = W2S(spine);

            Vector2[] w2SBody = MiscUtils.MergeArrays(w2SArms, w2SLegs, w2SSpine);
            if (w2SBody.Count(x=>x == Vector2.Zero) > 0)
                return;
            Vector2 left = w2SBody.First(x => x.X == w2SBody.Min(x2 => x2.X));
            Vector2 right = w2SBody.First(x => x.X == w2SBody.Max(x2 => x2.X));
            Vector2 upper = w2SBody.First(x => x.Y == w2SBody.Min(x2 => x2.Y));
            Vector2 lower = w2SBody.First(x => x.Y == w2SBody.Max(x2 => x2.Y));

            Vector2 outerSize = new Vector2(right.X - left.X + BorderMargin * 2, lower.Y - upper.Y + BorderMargin * 2) + Vector2.One * BorderSize * 2;
            Vector2 outerLocation = new Vector2(left.X - BorderMargin, upper.Y - BorderMargin) - Vector2.One * BorderSize;
            #endregion

            #region Color
            if (this.Player.MITeamNum == (int)Team.Terrorists)
                this.BackColor = new Color(1f, 0f, 0f, 1f);
            else
                this.BackColor = new Color(0.5f, 0.8f, 0.9f, 0.9f);
            #endregion

            #region Box
            if (WithOverlay.ConfigUtils.GetValue<bool>("espBox"))
            {
                renderer.DrawRectangle(this.ForeColor, outerLocation, outerSize, BorderSize + 2f);
                renderer.DrawRectangle(this.BackColor, outerLocation, outerSize, BorderSize);
            }
            #endregion

            #region Skeleton
            if (WithOverlay.ConfigUtils.GetValue<bool>("espSkeleton"))
            {
                renderer.DrawLines(this.ForeColor, 3f, w2SArms);
                renderer.DrawLines(this.ForeColor, 3f, w2SLegs);
                renderer.DrawLines(this.ForeColor, 3f, w2SSpine);
                renderer.DrawLines(this.BackColor, w2SArms);
                renderer.DrawLines(this.BackColor, w2SLegs);
                renderer.DrawLines(this.BackColor, w2SSpine);
            }
            #endregion

            #region Name + Stats
            string name = string.Format("{0} [{1}/{2}]", fw.Names[Player.M_IId], fw.Kills[Player.M_IId], fw.Deaths[Player.M_IId]);
            Vector2 nameSize = renderer.MeasureString(name, this.Font);
            Vector2 nameBoxSize = new Vector2((float)Math.Max(outerSize.X, nameSize.X), nameSize.Y);
            Vector2 nameBoxLocation = outerLocation - Vector2.UnitY * (BorderSize + 2f) - Vector2.UnitY * nameSize.Y + Vector2.UnitX * (outerSize.X/2f - nameBoxSize.X/2f);
            Vector2 nameLocation = nameBoxLocation + Vector2.UnitX * (nameBoxSize.X / 2f - nameSize.X / 2f);
            
            if (WithOverlay.ConfigUtils.GetValue<bool>("espName"))
            {
                renderer.FillRectangle(this.BackColor, nameBoxLocation, nameBoxSize);
                renderer.DrawRectangle(this.ForeColor, nameBoxLocation, nameBoxSize);

                renderer.DrawText(name, this.ForeColor, this.Font, nameLocation);
            }
            #endregion

            #region Health
            Vector2 hpLocation = outerLocation + (Vector2.UnitX * outerSize.X) + (Vector2.UnitX * BorderMargin);
            Vector2 hpSize = new Vector2(1, outerSize.Y);
            Vector2 hpFillSize = new Vector2(1, hpSize.Y / 100f * (float)(Math.Min(100, Player.MIHealth)));
            Vector2 hpFillLocation = hpLocation + Vector2.UnitY * (hpSize.Y - hpFillSize.Y);
            
            if (WithOverlay.ConfigUtils.GetValue<bool>("espHealth"))
            {
                renderer.DrawLine(this.ForeColor, hpLocation, hpLocation + hpSize, BorderSize * 2f + 2f);
                renderer.DrawLine(Color.Green, hpFillLocation, hpFillLocation + hpFillSize, BorderSize * 2f);
            }
            #endregion
            base.Draw(renderer);
        }

        private Vector2 W2S(ExternalUtilsCSharp.MathObjects.Vector3 point)
        {
            ExternalUtilsCSharp.MathObjects.Matrix vMatrix = WithOverlay.Framework.ViewMatrix;
            ExternalUtilsCSharp.MathObjects.Vector2 screenSize = new ExternalUtilsCSharp.MathObjects.Vector2(WithOverlay.ShdxOverlay.Width, WithOverlay.ShdxOverlay.Height);

            return SharpDXConverter.Vector2EUCtoSDX(MathUtils.WorldToScreen(vMatrix, screenSize, point));
        }

        private Vector2[] W2S(ExternalUtilsCSharp.MathObjects.Vector3[] points)
        {
            ExternalUtilsCSharp.MathObjects.Matrix vMatrix = WithOverlay.Framework.ViewMatrix;
            ExternalUtilsCSharp.MathObjects.Vector2 screenSize = new ExternalUtilsCSharp.MathObjects.Vector2(WithOverlay.ShdxOverlay.Width, WithOverlay.ShdxOverlay.Height);
            ExternalUtilsCSharp.MathObjects.Vector3 origin = Player.MVecOrigin;

            return SharpDXConverter.Vector2EUCtoSDX(MathUtils.WorldToScreen(vMatrix, screenSize, points));
        }

        private Color FadeColor(Color color, float amount)
        {
            return new Color(this.BackColor.R, this.BackColor.G, this.BackColor.B, (byte)(this.BackColor.A * amount));
        }
    }
}
