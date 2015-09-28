using System;
using System.Text;
using CsGoApplicationAimbot.CSGOClasses.Enums;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class BaseEntity : Entity
    {
        #region VARIABLES

        private uint _iClientClass, _iClassId;
        public string _szClassName;

        #endregion

        #region PROPERTIES

        private uint MiClientClass
        {
            get
            {
                if (_iClientClass == 0)
                    _iClientClass = GetClientClass();
                return _iClientClass;
            }
            set { _iClientClass = value; }
        }

        private uint MiClassId
        {
            get
            {
                if (_iClassId == 0)
                    _iClassId = GetClassId();
                return _iClassId;
            }
            set { _iClassId = value; }
        }

        public string MSzClassName
        {
            get
            {
                if (_szClassName == "<none>")
                    _szClassName = GetClassName();
                return _szClassName;
            }
            set { _szClassName = value; }
        }

        #endregion

        #region FIELDS

        public int MiHealth => ReadFieldProxy<int>("CSPlayer.m_iHealth");

        private int MiVirtualTable => ReadFieldProxy<int>("Entity.m_iVirtualTable");

        public int MiId => ReadFieldProxy<int>("Entity.m_iID");

        public byte MiDormant => ReadFieldProxy<byte>("Entity.m_iDormant");

        protected int OwnerEntity => ReadFieldProxy<int>("Entity.m_hOwnerEntity");
        public int TeamNum => ReadFieldProxy<int>("Entity.m_iTeamNum");

        public int MbSpotted => ReadFieldProxy<int>("Entity.m_bSpotted");
        private long MbSpottedByMask => ReadFieldProxy<long>("Entity.m_bSpottedByMask");

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
            AddField<int>("CSPlayer.m_iHealth", CsgoOffsets.NetVars.CBaseEntity.Health);
            AddField<int>("Entity.m_iVirtualTable", 0x08);
            AddField<int>("Entity.m_iID", CsgoOffsets.NetVars.CBaseEntity.Id);
            AddField<byte>("Entity.m_iDormant", CsgoOffsets.NetVars.CBaseEntity.Dormant);
            AddField<int>("Entity.m_hOwnerEntity", CsgoOffsets.NetVars.CBaseEntity.OwnerEntity);
            AddField<int>("Entity.m_iTeamNum", CsgoOffsets.NetVars.CBaseEntity.TeamNum);
            AddField<int>("Entity.m_bSpotted", CsgoOffsets.NetVars.CBaseEntity.Spotted);
            AddField<long>("Entity.m_bSpottedByMask", CsgoOffsets.NetVars.CBaseEntity.SpottedByMask);
            AddField<Vector3>("Entity.m_vecOrigin", CsgoOffsets.NetVars.CBaseEntity.VecOrigin);
            AddField<Vector3>("Entity.m_angRotation", CsgoOffsets.NetVars.CBaseEntity.AngRotation);
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

        private bool SeenBy(int entityIndex)
        {
            return (MbSpottedByMask & (0x1 << entityIndex)) != 0;
        }

        public bool SeenBy(BaseEntity ent)
        {
            return SeenBy(ent.MiId - 1);
        }

        private uint GetClientClass()
        {
            try
            {
                if (MiVirtualTable == 0)
                    return 0;
                var function = Program.MemUtils.Read<uint>((IntPtr)(MiVirtualTable + 2 * 0x04));
                if (function != 0xFFFFFFFF)
                    return Program.MemUtils.Read<uint>((IntPtr)(function + 0x01));
                return 0;
            }
            catch
            {
                return 0;
            }
        }

        private uint GetClassId()
        {
            try
            {
                var clientClass = GetClientClass();
                if (clientClass != 0)
                    return Program.MemUtils.Read<uint>((IntPtr)((long)clientClass + 20));
                return clientClass;
            }
            catch
            {
                return 0;
            }
        }

        private string GetClassName()
        {
            try
            {
                var clientClass = GetClientClass();
                if (clientClass != 0)
                {
                    var ptr = Program.MemUtils.Read<int>((IntPtr)(clientClass + 8));
                    return Program.MemUtils.ReadString((IntPtr)(ptr), 32, Encoding.ASCII);
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
                MiClassId == (int)ClassId.CsPlayer;
        }

        public bool IsWeapon()
        {
            return
                MiClassId == (int) ClassId.Ak47 ||
                MiClassId == (int) ClassId.DEagle ||
                MiClassId == (int) ClassId.Aug ||
                MiClassId == (int) ClassId.Awp ||
                MiClassId == (int) ClassId.G3Sg1 ||
                MiClassId == (int) ClassId.Scar20 ||
                MiClassId == (int) ClassId.DualBerettas ||
                MiClassId == (int) ClassId.Elite ||
                MiClassId == (int) ClassId.FiveSeven ||
                MiClassId == (int) ClassId.Glock ||
                MiClassId == (int) ClassId.Hkp2000 ||
                MiClassId == (int) ClassId.M4A1 ||
                MiClassId == (int) ClassId.Mp7 ||
                MiClassId == (int) ClassId.Mp9 ||
                MiClassId == (int) ClassId.P250 ||
                MiClassId == (int) ClassId.P90 ||
                MiClassId == (int) ClassId.Sg556 ||
                MiClassId == (int) ClassId.Ssg08 ||
                MiClassId == (int) ClassId.Taser ||
                MiClassId == (int) ClassId.Tec9 ||
                MiClassId == (int) ClassId.Ump45 ||
                MiClassId == (int) ClassId.DynamicProp ||
                MiClassId == (int) ClassId.PhysicsProp ||
                MiClassId == (int) ClassId.PhysicsPropMultiplayer ||
                MiClassId == (int) ClassId.Awp ||
                MiClassId == (int) ClassId.Ssg08 ||
                MiClassId == (int) ClassId.G3Sg1 ||
                MiClassId == (int) ClassId.Scar20 ||
                MiClassId == (int) ClassId.Knife ||
                MiClassId == (int) ClassId.DecoyGrenade ||
                MiClassId == (int) ClassId.HeGrenade ||
                MiClassId == (int) ClassId.IncendiaryGrenade ||
                MiClassId == (int) ClassId.MolotovGrenade ||
                MiClassId == (int) ClassId.SmokeGrenade ||
                MiClassId == (int) ClassId.Flashbang ||
                MiClassId == (int) ClassId.Famas ||
                MiClassId == (int) ClassId.Mac10 ||
                MiClassId == (int) ClassId.GalilAr ||
                MiClassId == (int) ClassId.M249 ||
                MiClassId == (int) ClassId.M249X ||
                MiClassId == (int) ClassId.Mag7 ||
                MiClassId == (int) ClassId.NOVA ||
                MiClassId == (int) ClassId.Negev ||
                MiClassId == (int) ClassId.Ump45X ||
                MiClassId == (int) ClassId.Xm1014 ||
                MiClassId == (int) ClassId.Xm1014X ||
                MiClassId == (int) ClassId.M4 ||
                MiClassId == (int) ClassId.Nova ||
                MiClassId == (int) ClassId.Mag ||
                MiClassId == (int) ClassId.G3Sg1 ||
                MiClassId == (int) ClassId.G3Sg1X ||
                MiClassId == (int) ClassId.Tec9X ||
                MiClassId == (int) ClassId.PpBizon ||
                MiClassId == (int) ClassId.P90X ||
                MiClassId == (int) ClassId.Scar20X;

        }
        #endregion
    }
}