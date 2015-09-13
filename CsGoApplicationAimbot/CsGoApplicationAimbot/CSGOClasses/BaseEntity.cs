using System;
using System.Text;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class BaseEntity : Entity
    {
        #region VARIABLES
        private uint _iClientClass, _iClassId;
        private string _szClassName;
        #endregion

        #region PROPERTIES
        public uint MIClientClass
        {
            get
            {
                if (_iClientClass == 0)
                    _iClientClass = GetClientClass();
                return _iClientClass;
            }
            protected set { _iClientClass = value; }
        }
        public uint MIClassId
        {
            get
            {
                if (_iClassId == 0)
                    _iClassId = GetClassId();
                return _iClassId;
            }
            protected set { _iClassId = value; }
        }
        public string MSzClassName
        {
            get
            {
                if (_szClassName == "<none>")
                    _szClassName = GetClassName();
                return _szClassName;
            }
            protected set { _szClassName = value; }
        }
        #endregion

        #region FIELDS
        public int MIHealth
        {
            get { return ReadFieldProxy<int>("CSPlayer.m_iHealth"); }
        }
        public int MIVirtualTable
        {
            get { return ReadFieldProxy<int>("Entity.m_iVirtualTable"); }
        }
        public int M_IId
        {
            get { return ReadFieldProxy<int>("Entity.m_iID"); }
        }
        public byte MIDormant
        {
            get { return ReadFieldProxy<byte>("Entity.m_iDormant"); }
        }
        public int MHOwnerEntity
        {
            get { return ReadFieldProxy<int>("Entity.m_hOwnerEntity"); }
        }
        public int MITeamNum
        {
            get { return ReadFieldProxy<int>("Entity.m_iTeamNum"); }
        }
        public int MBSpotted
        {
            get { return ReadFieldProxy<int>("Entity.m_bSpotted"); }
        }
        public long MBSpottedByMask
        {
            get { return ReadFieldProxy<long>("Entity.m_bSpottedByMask"); }
        }
        public Vector3 MVecOrigin
        {
            get { return ReadFieldProxy<Vector3>("Entity.m_vecOrigin"); }
        }
        public Vector3 MAngRotation
        {
            get { return ReadFieldProxy<Vector3>("Entity.m_angRotation"); }
        }
        #endregion

        #region CONSTRUCTOR
        public BaseEntity(int address)
            : base(address)
        {
            _iClassId = 0;
            _iClientClass = 0;
            _szClassName = "<none>";
        }
        public BaseEntity(BaseEntity copyFrom)
            : base(copyFrom.Address)
        {
            Address = copyFrom.Address;
            CopyFieldsFrom(copyFrom);
            _iClassId = copyFrom.MIClassId;
            _iClientClass = copyFrom.MIClientClass;
            _szClassName = copyFrom.MSzClassName;
        }
        #endregion

        #region METHODS
        protected override void SetupFields()
        {
            base.SetupFields();
            AddField<int>("CSPlayer.m_iHealth", CsgoOffsets.NetVars.CBaseEntity.MIHealth);
            AddField<int>("Entity.m_iVirtualTable", 0x08);
            AddField<int>("Entity.m_iID", CsgoOffsets.NetVars.CBaseEntity.M_IId);
            AddField<byte>("Entity.m_iDormant", CsgoOffsets.NetVars.CBaseEntity.MBDormant);
            AddField<int>("Entity.m_hOwnerEntity", CsgoOffsets.NetVars.CBaseEntity.MHOwnerEntity);
            AddField<int>("Entity.m_iTeamNum", CsgoOffsets.NetVars.CBaseEntity.MITeamNum);
            AddField<int>("Entity.m_bSpotted", CsgoOffsets.NetVars.CBaseEntity.MBSpotted);
            AddField<long>("Entity.m_bSpottedByMask", CsgoOffsets.NetVars.CBaseEntity.MBSpottedByMask);
            AddField<Vector3>("Entity.m_vecOrigin", CsgoOffsets.NetVars.CBaseEntity.MVecOrigin);
            AddField<Vector3>("Entity.m_angRotation", CsgoOffsets.NetVars.CBaseEntity.MAngRotation);
        }
        public override string ToString()
        {
            return string.Format("[BaseEntity m_iID={0}, m_iClassID={3}, m_szClassName={4}, m_vecOrigin={1}]\n{2}", M_IId, MVecOrigin, base.ToString(), MIClassId, MSzClassName);
        }
        public virtual bool IsValid()
        {
            return Address != 0 /* && this.m_iDormant != 1*/ && M_IId > 0 && MIClassId > 0;
        }
        public bool SeenBy(int entityIndex)
        {
            return (MBSpottedByMask & (0x1 << entityIndex)) != 0;
        }
        public bool SeenBy(BaseEntity ent)
        {
            return SeenBy(ent.M_IId - 1);
        }
        protected uint GetClientClass()
        {
            try
            {
                if (MIVirtualTable == 0)
                    return 0;
                uint function = Program.MemUtils.Read<uint>((IntPtr)(MIVirtualTable + 2 * 0x04));
                if (function != 0xFFFFFFFF)
                    return Program.MemUtils.Read<uint>((IntPtr)(function + 0x01));
                else
                    return 0;
            }
            catch { return 0; }
        }
        protected uint GetClassId()
        {
            try
            {
                uint clientClass = GetClientClass();
                if (clientClass != 0)
                    return Program.MemUtils.Read<uint>((IntPtr)((long)clientClass + 20));
                return clientClass;
            }
            catch { return 0; }
        }
        protected string GetClassName()
        {
            try
            {
                uint clientClass = GetClientClass();
                if (clientClass != 0)
                {
                    int ptr = Program.MemUtils.Read<int>((IntPtr)(clientClass + 8));
                    return Program.MemUtils.ReadString((IntPtr)(ptr), 32, Encoding.ASCII);
                }
                return "none";
            }
            catch { return "none"; }
        }
        public bool IsPlayer()
        {
            return
                MIClassId == (int)CSGO.ClassId.CsPlayer;
        }
        public bool IsWeapon()
        {
            return
                MIClassId == (int)CSGO.ClassId.Ak47 ||
                MIClassId == (int)CSGO.ClassId.DEagle ||
                MIClassId == (int)CSGO.ClassId.Knife ||
                MIClassId == (int)CSGO.ClassId.KnifeGg ||
                MIClassId == (int)CSGO.ClassId.WeaponAug ||
                MIClassId == (int)CSGO.ClassId.WeaponAwp ||
                MIClassId == (int)CSGO.ClassId.WeaponBizon ||
                MIClassId == (int)CSGO.ClassId.WeaponDualBerettas ||
                MIClassId == (int)CSGO.ClassId.WeaponElite ||
                MIClassId == (int)CSGO.ClassId.WeaponFiveSeven ||
                MIClassId == (int)CSGO.ClassId.WeaponG3Sg1 ||
                MIClassId == (int)CSGO.ClassId.WeaponG3Sg1X ||
                MIClassId == (int)CSGO.ClassId.WeaponGalilAr ||
                MIClassId == (int)CSGO.ClassId.WeaponGlock ||
                MIClassId == (int)CSGO.ClassId.WeaponHkp2000 ||
                MIClassId == (int)CSGO.ClassId.WeaponM249 ||
                MIClassId == (int)CSGO.ClassId.WeaponM249X ||
                MIClassId == (int)CSGO.ClassId.WeaponM4 ||
                MIClassId == (int)CSGO.ClassId.WeaponM4A1 ||
                MIClassId == (int)CSGO.ClassId.WeaponMag ||
                MIClassId == (int)CSGO.ClassId.WeaponMag7 ||
                MIClassId == (int)CSGO.ClassId.WeaponMp7 ||
                MIClassId == (int)CSGO.ClassId.WeaponMp9 ||
                MIClassId == (int)CSGO.ClassId.WeaponNegev ||
                MIClassId == (int)CSGO.ClassId.WeaponNova ||
                MIClassId == (int)CSGO.ClassId.WeaponNOVA ||
                MIClassId == (int)CSGO.ClassId.WeaponP250 ||
                MIClassId == (int)CSGO.ClassId.WeaponP90 ||
                MIClassId == (int)CSGO.ClassId.WeaponP90X ||
                MIClassId == (int)CSGO.ClassId.WeaponPpBizon ||
                MIClassId == (int)CSGO.ClassId.WeaponScar20 ||
                MIClassId == (int)CSGO.ClassId.WeaponScar20X ||
                MIClassId == (int)CSGO.ClassId.WeaponSg556 ||
                MIClassId == (int)CSGO.ClassId.WeaponSsg08 ||
                MIClassId == (int)CSGO.ClassId.WeaponTaser ||
                MIClassId == (int)CSGO.ClassId.WeaponTec9 ||
                MIClassId == (int)CSGO.ClassId.WeaponTec9X ||
                MIClassId == (int)CSGO.ClassId.WeaponUmp45 ||
                MIClassId == (int)CSGO.ClassId.WeaponUmp45X ||
                MIClassId == (int)CSGO.ClassId.WeaponXm1014 ||
                MIClassId == (int)CSGO.ClassId.WeaponXm1014X ||
                MIClassId == (int)CSGO.ClassId.DecoyGrenade ||
                MIClassId == (int)CSGO.ClassId.HeGrenade ||
                MIClassId == (int)CSGO.ClassId.IncendiaryGrenade ||
                MIClassId == (int)CSGO.ClassId.MolotovGrenade ||
                MIClassId == (int)CSGO.ClassId.SmokeGrenade ||
                MIClassId == (int)CSGO.ClassId.Flashbang;
        }
        public bool IsProp()
        {
            return
                MIClassId == (int)CSGO.ClassId.DynamicProp ||
                MIClassId == (int)CSGO.ClassId.PhysicsProp ||
                MIClassId == (int)CSGO.ClassId.PhysicsPropMultiplayer;
        }
        #endregion
    }
}
