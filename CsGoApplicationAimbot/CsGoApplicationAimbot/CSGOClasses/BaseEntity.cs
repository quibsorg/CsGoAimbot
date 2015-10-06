using System;
using System.Text;
using CsGoApplicationAimbot.CSGOClasses.Enums;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class BaseEntity : Entity
    {
        #region VARIABLES

        private uint _clientClass, _classId;
        public string _className;

        #endregion

        #region PROPERTIES

        private uint ClientClass
        {
            get
            {
                if (_clientClass == 0)
                    _clientClass = GetClientClass();
                return _clientClass;
            }
            set { _clientClass = value; }
        }

        private uint ClassId
        {
            get
            {
                if (_classId == 0)
                    _classId = GetClassId();
                return _classId;
            }
            set { _classId = value; }
        }

        public string ClassName
        {
            get
            {
                if (_className == "<none>")
                    _className = GetClassName();
                return _className;
            }
            set { _className = value; }
        }

        #endregion

        #region FIELDS

        public int Health => ReadFieldProxy<int>("CSPlayer.m_iHealth");

        private int VirtualTable => ReadFieldProxy<int>("Entity.m_iVirtualTable");

        public int Id => ReadFieldProxy<int>("Entity.m_iID");

        public byte Dormant => ReadFieldProxy<byte>("Entity.m_iDormant");

        protected int OwnerEntity => ReadFieldProxy<int>("Entity.m_hOwnerEntity");
        public int TeamNum => ReadFieldProxy<int>("Entity.m_iTeamNum");

        public int Spotted => ReadFieldProxy<int>("Entity.m_bSpotted");
        private long SpottedByMask => ReadFieldProxy<long>("Entity.m_bSpottedByMask");

        public Vector3 VecOrigin => ReadFieldProxy<Vector3>("Entity.m_vecOrigin");

        public Vector3 AngRotation => ReadFieldProxy<Vector3>("Entity.m_angRotation");

        #endregion

        #region CONSTRUCTOR

        public BaseEntity(int address) : base(address)
        {
            _classId = 0;
            _clientClass = 0;
            _className = "<none>";
        }

        public BaseEntity(BaseEntity copyFrom) : base(copyFrom.Address)
        {
            Address = copyFrom.Address;
            CopyFieldsFrom(copyFrom);
            _classId = copyFrom.ClassId;
            _clientClass = copyFrom.ClientClass;
            _className = copyFrom.ClassName;
        }

        #endregion

        #region METHODS

        protected override void SetupFields()
        {
            base.SetupFields();
            AddField<int>("CSPlayer.m_iHealth", Offsets.NetVars.CBaseEntity.Health);
            AddField<int>("Entity.m_iVirtualTable", 0x08);
            AddField<int>("Entity.m_iID", Offsets.NetVars.CBaseEntity.Id);
            AddField<byte>("Entity.m_iDormant", Offsets.NetVars.CBaseEntity.Dormant);
            AddField<int>("Entity.m_hOwnerEntity", Offsets.NetVars.CBaseEntity.OwnerEntity);
            AddField<int>("Entity.m_iTeamNum", Offsets.NetVars.CBaseEntity.TeamNum);
            AddField<int>("Entity.m_bSpotted", Offsets.NetVars.CBaseEntity.Spotted);
            AddField<long>("Entity.m_bSpottedByMask", Offsets.NetVars.CBaseEntity.SpottedByMask);
            AddField<Vector3>("Entity.m_vecOrigin", Offsets.NetVars.CBaseEntity.VecOrigin);
            AddField<Vector3>("Entity.m_angRotation", Offsets.NetVars.CBaseEntity.AngRotation);
        }

        public override string ToString()
        {
            return string.Format("[BaseEntity m_iID={0}, m_iClassID={3}, m_szClassName={4}, m_vecOrigin={1}]\n{2}", Id,
                VecOrigin, base.ToString(), ClassId, ClassName);
        }

        public virtual bool IsValid()
        {
            return Address != 0 && Id > 0 && ClassId > 0;
        }

        private bool SeenBy(int entityIndex)
        {
            return (SpottedByMask & (0x1 << entityIndex)) != 0;
        }

        public bool SeenBy(BaseEntity ent)
        {
            return SeenBy(ent.Id - 1);
        }

        private uint GetClientClass()
        {
            try
            {
                if (VirtualTable == 0)
                    return 0;
                var function = Program.MemUtils.Read<uint>((IntPtr)(VirtualTable + 2 * 0x04));
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
                ClassId == (int)Enums.ClassId.CsPlayer;
        }

        public bool IsWeapon()
        {
            return
                ClassId == (int) Enums.ClassId.Ak47 ||
                ClassId == (int) Enums.ClassId.DEagle ||
                ClassId == (int) Enums.ClassId.Aug ||
                ClassId == (int) Enums.ClassId.Awp ||
                ClassId == (int) Enums.ClassId.Sawedoff ||
                ClassId == (int) Enums.ClassId.G3Sg1 ||
                ClassId == (int) Enums.ClassId.Scar20 ||
                ClassId == (int) Enums.ClassId.DualBerettas ||
                ClassId == (int) Enums.ClassId.Elite ||
                ClassId == (int) Enums.ClassId.FiveSeven ||
                ClassId == (int) Enums.ClassId.Glock ||
                ClassId == (int) Enums.ClassId.Hkp2000 ||
                ClassId == (int) Enums.ClassId.M4A1 ||
                ClassId == (int) Enums.ClassId.Mp7 ||
                ClassId == (int) Enums.ClassId.Mp9 ||
                ClassId == (int) Enums.ClassId.P250 ||
                ClassId == (int) Enums.ClassId.P90 ||
                ClassId == (int) Enums.ClassId.Sg556 ||
                ClassId == (int) Enums.ClassId.Ssg08 ||
                ClassId == (int) Enums.ClassId.Taser ||
                ClassId == (int) Enums.ClassId.Tec9 ||
                ClassId == (int) Enums.ClassId.Ump45 ||
                ClassId == (int) Enums.ClassId.DynamicProp ||
                ClassId == (int) Enums.ClassId.PhysicsProp ||
                ClassId == (int) Enums.ClassId.PhysicsPropMultiplayer ||
                ClassId == (int) Enums.ClassId.Ssg08 ||
                ClassId == (int) Enums.ClassId.G3Sg1 ||
                ClassId == (int) Enums.ClassId.Scar20 ||
                ClassId == (int) Enums.ClassId.Knife ||
                ClassId == (int) Enums.ClassId.DecoyGrenade ||
                ClassId == (int) Enums.ClassId.HeGrenade ||
                ClassId == (int) Enums.ClassId.IncendiaryGrenade ||
                ClassId == (int) Enums.ClassId.MolotovGrenade ||
                ClassId == (int) Enums.ClassId.SmokeGrenade ||
                ClassId == (int) Enums.ClassId.Flashbang ||
                ClassId == (int) Enums.ClassId.Famas ||
                ClassId == (int) Enums.ClassId.Mac10 ||
                ClassId == (int) Enums.ClassId.Galil ||
                ClassId == (int) Enums.ClassId.M249 ||
                ClassId == (int) Enums.ClassId.M249X ||
                ClassId == (int) Enums.ClassId.Mag7 ||
                ClassId == (int) Enums.ClassId.Nova ||
                ClassId == (int) Enums.ClassId.Negev ||
                ClassId == (int) Enums.ClassId.Ump45X ||
                ClassId == (int) Enums.ClassId.Xm1014 ||
                ClassId == (int) Enums.ClassId.Xm1014X ||
                ClassId == (int) Enums.ClassId.M4 ||
                ClassId == (int) Enums.ClassId.Mag ||
                ClassId == (int) Enums.ClassId.G3Sg1 ||
                ClassId == (int) Enums.ClassId.G3Sg1X ||
                ClassId == (int) Enums.ClassId.Tec9X ||
                ClassId == (int) Enums.ClassId.Bizon ||
                ClassId == (int) Enums.ClassId.P90X ||
                ClassId == (int) Enums.ClassId.Usp ||
                ClassId == (int) Enums.ClassId.Scar20;

        }
        #endregion
    }
}