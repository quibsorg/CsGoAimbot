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

        public uint MiClientClass
        {
            get
            {
                if (_iClientClass == 0)
                    _iClientClass = GetClientClass();
                return _iClientClass;
            }
            protected set { _iClientClass = value; }
        }

        public uint MiClassId
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

        public int MiHealth => ReadFieldProxy<int>("CSPlayer.m_iHealth");

        public int MiVirtualTable => ReadFieldProxy<int>("Entity.m_iVirtualTable");

        public int MiId => ReadFieldProxy<int>("Entity.m_iID");

        public byte MiDormant => ReadFieldProxy<byte>("Entity.m_iDormant");

        public int MhOwnerEntity => ReadFieldProxy<int>("Entity.m_hOwnerEntity");
        public int MiTeamNum => ReadFieldProxy<int>("Entity.m_iTeamNum");

        public int MbSpotted => ReadFieldProxy<int>("Entity.m_bSpotted");
        public long MbSpottedByMask => ReadFieldProxy<long>("Entity.m_bSpottedByMask");

        public Vector3 MVecOrigin => ReadFieldProxy<Vector3>("Entity.m_vecOrigin");

        public Vector3 MAngRotation => ReadFieldProxy<Vector3>("Entity.m_angRotation");

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
            _iClassId = copyFrom.MiClassId;
            _iClientClass = copyFrom.MiClientClass;
            _szClassName = copyFrom.MSzClassName;
        }

        #endregion

        #region METHODS

        protected override void SetupFields()
        {
            base.SetupFields();
            AddField<int>("CSPlayer.m_iHealth", CsgoOffsets.NetVars.CBaseEntity.MiHealth);
            AddField<int>("Entity.m_iVirtualTable", 0x08);
            AddField<int>("Entity.m_iID", CsgoOffsets.NetVars.CBaseEntity.MiId);
            AddField<byte>("Entity.m_iDormant", CsgoOffsets.NetVars.CBaseEntity.MbDormant);
            AddField<int>("Entity.m_hOwnerEntity", CsgoOffsets.NetVars.CBaseEntity.MhOwnerEntity);
            AddField<int>("Entity.m_iTeamNum", CsgoOffsets.NetVars.CBaseEntity.MiTeamNum);
            AddField<int>("Entity.m_bSpotted", CsgoOffsets.NetVars.CBaseEntity.MbSpotted);
            AddField<long>("Entity.m_bSpottedByMask", CsgoOffsets.NetVars.CBaseEntity.MbSpottedByMask);
            AddField<Vector3>("Entity.m_vecOrigin", CsgoOffsets.NetVars.CBaseEntity.MVecOrigin);
            AddField<Vector3>("Entity.m_angRotation", CsgoOffsets.NetVars.CBaseEntity.MAngRotation);
        }

        public override string ToString()
        {
            return string.Format("[BaseEntity m_iID={0}, m_iClassID={3}, m_szClassName={4}, m_vecOrigin={1}]\n{2}", MiId,
                MVecOrigin, base.ToString(), MiClassId, MSzClassName);
        }

        public virtual bool IsValid()
        {
            return Address != 0 /* && this.m_iDormant != 1*/&& MiId > 0 && MiClassId > 0;
        }

        public bool SeenBy(int entityIndex)
        {
            return (MbSpottedByMask & (0x1 << entityIndex)) != 0;
        }

        public bool SeenBy(BaseEntity ent)
        {
            return SeenBy(ent.MiId - 1);
        }

        protected uint GetClientClass()
        {
            try
            {
                if (MiVirtualTable == 0)
                    return 0;
                var function = Program.MemUtils.Read<uint>((IntPtr) (MiVirtualTable + 2*0x04));
                if (function != 0xFFFFFFFF)
                    return Program.MemUtils.Read<uint>((IntPtr) (function + 0x01));
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        protected uint GetClassId()
        {
            try
            {
                var clientClass = GetClientClass();
                if (clientClass != 0)
                    return Program.MemUtils.Read<uint>((IntPtr) ((long) clientClass + 20));
                return clientClass;
            }
            catch
            {
                return 0;
            }
        }

        protected string GetClassName()
        {
            try
            {
                var clientClass = GetClientClass();
                if (clientClass != 0)
                {
                    var ptr = Program.MemUtils.Read<int>((IntPtr) (clientClass + 8));
                    return Program.MemUtils.ReadString((IntPtr) (ptr), 32, Encoding.ASCII);
                }
                return "none";
            }
            catch
            {
                return "none";
            }
        }

        public bool IsPlayer()
        {
            return
                MiClassId == (int) ClassId.CsPlayer;
        }

        public bool IsWeapon()
        {
            return
                MiClassId == (int) ClassId.Ak47 ||
                MiClassId == (int) ClassId.DEagle ||
                MiClassId == (int) ClassId.WeaponAug ||
                MiClassId == (int) ClassId.WeaponAwp ||
                MiClassId == (int) ClassId.WeaponDualBerettas ||
                MiClassId == (int) ClassId.WeaponElite ||
                MiClassId == (int) ClassId.WeaponFiveSeven ||
                MiClassId == (int) ClassId.WeaponGlock ||
                MiClassId == (int) ClassId.WeaponHkp2000 ||
                MiClassId == (int) ClassId.WeaponM4A1 ||
                MiClassId == (int) ClassId.WeaponMp7 ||
                MiClassId == (int) ClassId.WeaponMp9 ||
                MiClassId == (int) ClassId.WeaponP250 ||
                MiClassId == (int) ClassId.WeaponP90 ||
                MiClassId == (int) ClassId.WeaponSg556 ||
                MiClassId == (int) ClassId.WeaponSsg08 ||
                MiClassId == (int) ClassId.WeaponTaser ||
                MiClassId == (int) ClassId.WeaponTec9 ||
                MiClassId == (int) ClassId.WeaponUmp45;
        }

        public bool IsGrenade()
        {
            return
                MiClassId == (int) ClassId.DecoyGrenade ||
                MiClassId == (int) ClassId.HeGrenade ||
                MiClassId == (int) ClassId.IncendiaryGrenade ||
                MiClassId == (int) ClassId.MolotovGrenade ||
                MiClassId == (int) ClassId.SmokeGrenade ||
                MiClassId == (int) ClassId.Flashbang;
        }

        public bool IsMelee()
        {
            return
            MiClassId == (int) ClassId.Knife ||
            MiClassId == (int) ClassId.Knife;
        }

        public bool IsProp()
        {
            return
                MiClassId == (int) ClassId.DynamicProp ||
                MiClassId == (int) ClassId.PhysicsProp ||
                MiClassId == (int) ClassId.PhysicsPropMultiplayer;
        }

        #endregion
    }
}