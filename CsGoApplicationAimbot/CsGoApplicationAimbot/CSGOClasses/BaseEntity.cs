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
            get { return this.ReadFieldProxy<int>("CSPlayer.m_iHealth"); }
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
            this._iClassId = 0;
            this._iClientClass = 0;
            this._szClassName = "<none>";
        }
        public BaseEntity(BaseEntity copyFrom)
            : base(copyFrom.Address)
        {
            this.Address = copyFrom.Address;
            this.CopyFieldsFrom(copyFrom);
            this._iClassId = copyFrom.MIClassId;
            this._iClientClass = copyFrom.MIClientClass;
            this._szClassName = copyFrom.MSzClassName;
        }
        #endregion

        #region METHODS
        protected override void SetupFields()
        {
            base.SetupFields();
            this.AddField<int>("CSPlayer.m_iHealth", CsgoOffsets.NetVars.CBaseEntity.MIHealth);
            this.AddField<int>("Entity.m_iVirtualTable", 0x08);
            this.AddField<int>("Entity.m_iID", CsgoOffsets.NetVars.CBaseEntity.M_IId);
            this.AddField<byte>("Entity.m_iDormant", CsgoOffsets.NetVars.CBaseEntity.MBDormant);
            this.AddField<int>("Entity.m_hOwnerEntity", CsgoOffsets.NetVars.CBaseEntity.MHOwnerEntity);
            this.AddField<int>("Entity.m_iTeamNum", CsgoOffsets.NetVars.CBaseEntity.MITeamNum);
            this.AddField<int>("Entity.m_bSpotted", CsgoOffsets.NetVars.CBaseEntity.MBSpotted);
            this.AddField<long>("Entity.m_bSpottedByMask", CsgoOffsets.NetVars.CBaseEntity.MBSpottedByMask);
            this.AddField<Vector3>("Entity.m_vecOrigin", CsgoOffsets.NetVars.CBaseEntity.MVecOrigin);
            this.AddField<Vector3>("Entity.m_angRotation", CsgoOffsets.NetVars.CBaseEntity.MAngRotation);
        }
        public override string ToString()
        {
            return string.Format("[BaseEntity m_iID={0}, m_iClassID={3}, m_szClassName={4}, m_vecOrigin={1}]\n{2}", this.M_IId, this.MVecOrigin, base.ToString(), this.MIClassId, this.MSzClassName);
        }
        public virtual bool IsValid()
        {
            return this.Address != 0 /* && this.m_iDormant != 1*/ && this.M_IId > 0 && this.MIClassId > 0;
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
                uint function = WithOverlay.MemUtils.Read<uint>((IntPtr)(MIVirtualTable + 2 * 0x04));
                if (function != 0xFFFFFFFF)
                    return WithOverlay.MemUtils.Read<uint>((IntPtr)(function + 0x01));
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
                    return WithOverlay.MemUtils.Read<uint>((IntPtr)((long)clientClass + 20));
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
                    int ptr = WithOverlay.MemUtils.Read<int>((IntPtr)(clientClass + 8));
                    return WithOverlay.MemUtils.ReadString((IntPtr)(ptr), 32, Encoding.ASCII);
                }
                return "none";
            }
            catch { return "none"; }
        }
        public bool IsPlayer()
        {
            return
                this.MIClassId == (int)CSGO.ClassId.CsPlayer;
        }
        public bool IsWeapon()
        {
            return
                this.MIClassId == (int)CSGO.ClassId.Ak47 ||
                this.MIClassId == (int)CSGO.ClassId.DEagle ||
                this.MIClassId == (int)CSGO.ClassId.Knife ||
                this.MIClassId == (int)CSGO.ClassId.KnifeGg ||
                this.MIClassId == (int)CSGO.ClassId.WeaponAug ||
                this.MIClassId == (int)CSGO.ClassId.WeaponAwp ||
                this.MIClassId == (int)CSGO.ClassId.WeaponBizon ||
                this.MIClassId == (int)CSGO.ClassId.WeaponDualBerettas ||
                this.MIClassId == (int)CSGO.ClassId.WeaponElite ||
                this.MIClassId == (int)CSGO.ClassId.WeaponFiveSeven ||
                this.MIClassId == (int)CSGO.ClassId.WeaponG3Sg1 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponG3Sg1X ||
                this.MIClassId == (int)CSGO.ClassId.WeaponGalilAr ||
                this.MIClassId == (int)CSGO.ClassId.WeaponGlock ||
                this.MIClassId == (int)CSGO.ClassId.WeaponHkp2000 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponM249 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponM249X ||
                this.MIClassId == (int)CSGO.ClassId.WeaponM4 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponM4A1 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponMag ||
                this.MIClassId == (int)CSGO.ClassId.WeaponMag7 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponMp7 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponMp9 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponNegev ||
                this.MIClassId == (int)CSGO.ClassId.WeaponNova ||
                this.MIClassId == (int)CSGO.ClassId.WeaponNOVA ||
                this.MIClassId == (int)CSGO.ClassId.WeaponP250 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponP90 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponP90X ||
                this.MIClassId == (int)CSGO.ClassId.WeaponPpBizon ||
                this.MIClassId == (int)CSGO.ClassId.WeaponScar20 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponScar20X ||
                this.MIClassId == (int)CSGO.ClassId.WeaponSg556 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponSsg08 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponTaser ||
                this.MIClassId == (int)CSGO.ClassId.WeaponTec9 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponTec9X ||
                this.MIClassId == (int)CSGO.ClassId.WeaponUmp45 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponUmp45X ||
                this.MIClassId == (int)CSGO.ClassId.WeaponXm1014 ||
                this.MIClassId == (int)CSGO.ClassId.WeaponXm1014X ||
                this.MIClassId == (int)CSGO.ClassId.DecoyGrenade ||
                this.MIClassId == (int)CSGO.ClassId.HeGrenade ||
                this.MIClassId == (int)CSGO.ClassId.IncendiaryGrenade ||
                this.MIClassId == (int)CSGO.ClassId.MolotovGrenade ||
                this.MIClassId == (int)CSGO.ClassId.SmokeGrenade ||
                this.MIClassId == (int)CSGO.ClassId.Flashbang;
        }
        public bool IsProp()
        {
            return
                this.MIClassId == (int)CSGO.ClassId.DynamicProp ||
                this.MIClassId == (int)CSGO.ClassId.PhysicsProp ||
                this.MIClassId == (int)CSGO.ClassId.PhysicsPropMultiplayer;
        }
        #endregion
    }
}
