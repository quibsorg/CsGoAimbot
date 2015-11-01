using System;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class LocalPlayer : Player
    {
        #region METHODS

        public override string ToString()
        {
            return string.Format("[CSLocalPlayer m_iCrosshairIdx={1}, m_iShotsFired={2}, m_vecPunch={0}]\n{3}",
                VecPunch, CrosshairIdx, ShotsFired, base.ToString());
        }

        #endregion

        public float DistanceToOtherEntityInMetres(Tuple<int, Player> player)
        {
            return Geometry.GetDistanceToPoint(VecOrigin, player.Item2.VecOrigin)*0.01905f;
        }

        public float DistanceToOtherEntityInMetres(Player player)
        {
            return Geometry.GetDistanceToPoint(VecOrigin, player.VecOrigin)*0.01905f;
        }

        #region FIELDS

        public Vector3 VecViewOffset => ReadFieldProxy<Vector3>("CSLocalPlayer.m_vecViewOffset");
        public Vector3 VecPunch => ReadFieldProxy<Vector3>("CSLocalPlayer.m_vecPunch");
        public int ShotsFired => ReadFieldProxy<int>("CSLocalPlayer.m_iShotsFired");
        public int CrosshairIdx => ReadFieldProxy<int>("CSLocalPlayer.m_iCrosshairIdx");

        #endregion

        #region CONSTRUCTORS

        public LocalPlayer(int address)
            : base(address)
        {
            AddField<Vector3>("CSLocalPlayer.m_vecViewOffset", Offsets.NetVars.LocalPlayer.VecViewOffset);
            AddField<Vector3>("CSLocalPlayer.m_vecPunch", Offsets.NetVars.LocalPlayer.VecPunch);
            AddField<int>("CSLocalPlayer.m_iShotsFired", Offsets.NetVars.LocalPlayer.ShotsFired);
            AddField<int>("CSLocalPlayer.m_iCrosshairIdx", Offsets.NetVars.LocalPlayer.CrosshairIdx);
        }

        public LocalPlayer(Player player)
            : base(player)
        {
            CopyFieldsFrom(player);
            AddField<Vector3>("CSLocalPlayer.m_vecViewOffset", Offsets.NetVars.LocalPlayer.VecViewOffset);
            AddField<Vector3>("CSLocalPlayer.m_vecPunch", Offsets.NetVars.LocalPlayer.VecPunch);
            AddField<int>("CSLocalPlayer.m_iShotsFired", Offsets.NetVars.LocalPlayer.ShotsFired);
            AddField<int>("CSLocalPlayer.m_iCrosshairIdx", Offsets.NetVars.LocalPlayer.CrosshairIdx);
        }

        #endregion
    }
}