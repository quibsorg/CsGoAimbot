using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class CsLocalPlayer : CsPlayer
    {
        #region FIELDS
        public Vector3 MVecViewOffset
        {
            get { return ReadFieldProxy<Vector3>("CSLocalPlayer.m_vecViewOffset"); }
        }
        public Vector3 MVecPunch
        {
            get { return ReadFieldProxy<Vector3>("CSLocalPlayer.m_vecPunch"); }
        }
        public int MIShotsFired
        {
            get { return ReadFieldProxy<int>("CSLocalPlayer.m_iShotsFired"); }
        }
        public int MICrosshairIdx
        {
            get { return ReadFieldProxy<int>("CSLocalPlayer.m_iCrosshairIdx"); }
        }
        #endregion

        #region CONSTRUCTORS
        public CsLocalPlayer(int address)
            : base(address)
        {
            AddField<Vector3>("CSLocalPlayer.m_vecViewOffset", CsgoOffsets.NetVars.LocalPlayer.MVecViewOffset);
            AddField<Vector3>("CSLocalPlayer.m_vecPunch", CsgoOffsets.NetVars.LocalPlayer.MVecPunch);
            AddField<int>("CSLocalPlayer.m_iShotsFired", CsgoOffsets.NetVars.LocalPlayer.MIShotsFired);
            AddField<int>("CSLocalPlayer.m_iCrosshairIdx", CsgoOffsets.NetVars.LocalPlayer.MICrosshairIdx);
        }
        public CsLocalPlayer(CsPlayer player)
            : base(player)
        {
            CopyFieldsFrom(player);
            AddField<Vector3>("CSLocalPlayer.m_vecViewOffset", CsgoOffsets.NetVars.LocalPlayer.MVecViewOffset);
            AddField<Vector3>("CSLocalPlayer.m_vecPunch", CsgoOffsets.NetVars.LocalPlayer.MVecPunch);
            AddField<int>("CSLocalPlayer.m_iShotsFired", CsgoOffsets.NetVars.LocalPlayer.MIShotsFired);
            AddField<int>("CSLocalPlayer.m_iCrosshairIdx", CsgoOffsets.NetVars.LocalPlayer.MICrosshairIdx);
        }
        #endregion

        #region METHODS
        public override string ToString()
        {
            return string.Format("[CSLocalPlayer m_iCrosshairIdx={1}, m_iShotsFired={2}, m_vecPunch={0}]\n{3}", MVecPunch, MICrosshairIdx, MIShotsFired, base.ToString());
        }
        #endregion
    }
}
