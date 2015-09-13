using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class CsLocalPlayer : CsPlayer
    {
        #region FIELDS
        public Vector3 MVecViewOffset => ReadFieldProxy<Vector3>("CSLocalPlayer.m_vecViewOffset");
        public Vector3 MVecPunch => ReadFieldProxy<Vector3>("CSLocalPlayer.m_vecPunch");

        public int MiShotsFired => ReadFieldProxy<int>("CSLocalPlayer.m_iShotsFired");
        public int MiCrosshairIdx => ReadFieldProxy<int>("CSLocalPlayer.m_iCrosshairIdx");

        #endregion

        #region CONSTRUCTORS
        public CsLocalPlayer(int address)
            : base(address)
        {
            AddField<Vector3>("CSLocalPlayer.m_vecViewOffset", CsgoOffsets.NetVars.LocalPlayer.MVecViewOffset);
            AddField<Vector3>("CSLocalPlayer.m_vecPunch", CsgoOffsets.NetVars.LocalPlayer.MVecPunch);
            AddField<int>("CSLocalPlayer.m_iShotsFired", CsgoOffsets.NetVars.LocalPlayer.MiShotsFired);
            AddField<int>("CSLocalPlayer.m_iCrosshairIdx", CsgoOffsets.NetVars.LocalPlayer.MiCrosshairIdx);
        }
        public CsLocalPlayer(CsPlayer player)
            : base(player)
        {
            CopyFieldsFrom(player);
            AddField<Vector3>("CSLocalPlayer.m_vecViewOffset", CsgoOffsets.NetVars.LocalPlayer.MVecViewOffset);
            AddField<Vector3>("CSLocalPlayer.m_vecPunch", CsgoOffsets.NetVars.LocalPlayer.MVecPunch);
            AddField<int>("CSLocalPlayer.m_iShotsFired", CsgoOffsets.NetVars.LocalPlayer.MiShotsFired);
            AddField<int>("CSLocalPlayer.m_iCrosshairIdx", CsgoOffsets.NetVars.LocalPlayer.MiCrosshairIdx);
        }
        #endregion

        #region METHODS
        public override string ToString()
        {
            return string.Format("[CSLocalPlayer m_iCrosshairIdx={1}, m_iShotsFired={2}, m_vecPunch={0}]\n{3}", MVecPunch, MiCrosshairIdx, MiShotsFired, base.ToString());
        }
        #endregion
    }
}
