using System;
using CsGoApplicationAimbot.CSGOClasses.Updaters;
using Vector2 = CsGoApplicationAimbot.MathObjects.Vector2;
using Vector3 = CsGoApplicationAimbot.MathObjects.Vector3;

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

        public bool IsMoving()
        {
            Vector2 vector2 = new Vector2(Memory.LocalPlayer.VecVelocity.X, Memory.LocalPlayer.VecVelocity.Y);
            float length = vector2.Length();
            float speedMeters = length * 0.01905f;
            float speedKiloMetersPerHour = speedMeters * 60f * 60f / 1000f;

            //If speedKiloMeters is bigger than 0 we are moving and returning true, else false.
            return speedKiloMetersPerHour > 0;
        }

        #region FIELDS

        public Vector3 VecViewOffset => ReadFieldProxy<Vector3>("CSLocalPlayer.m_vecViewOffset");
        public Vector3 VecPunch => ReadFieldProxy<Vector3>("CSLocalPlayer.m_vecPunch");
        public int ShotsFired => ReadFieldProxy<int>("CSLocalPlayer.m_iShotsFired");
        public int CrosshairIdx => ReadFieldProxy<int>("CSLocalPlayer.m_iCrosshairIdx");

        #endregion

        #region CONSTRUCTORS

        public LocalPlayer(int address) : base(address)
        {
            AddField<Vector3>("CSLocalPlayer.m_vecViewOffset", Offsets.NetVars.LocalPlayer.VecViewOffset);
            AddField<Vector3>("CSLocalPlayer.m_vecPunch", Offsets.NetVars.LocalPlayer.VecPunch);
            AddField<int>("CSLocalPlayer.m_iShotsFired", Offsets.NetVars.LocalPlayer.ShotsFired);
            AddField<int>("CSLocalPlayer.m_iCrosshairIdx", Offsets.NetVars.LocalPlayer.CrosshairIdx);
        }

        public LocalPlayer(Player player) : base(player)
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