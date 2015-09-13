using System;
using System.Linq;
using CsGoApplicationAimbot.CSGOClasses.Fields;
using ExternalUtilsCSharp.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class CsPlayer : BaseEntity
    {
        #region VARIABLES
        private uint _iWeaponIndex;
        #endregion

        #region FIELDS
        public int MHBoneMatrix
        {
            get { return this.ReadFieldProxy<int>("CSPlayer.m_hBoneMatrix"); }
        }
        public int MIFlags
        {
            get { return this.ReadFieldProxy<int>("CSPlayer.m_iFlags"); }
        }
        public uint MHActiveWeapon
        {
            get { return this.ReadFieldProxy<uint>("CSPlayer.m_hActiveWeapon"); }
        }
        public Vector3 MVecVelocity
        {
            get { return this.ReadFieldProxy<Vector3>("CSPlayer.m_vecVelocity"); }
        }
        public int MHObserverTarget
        {
            get { return this.ReadFieldProxy<int>("CSPlayer.m_hObserverTarget") & 0xFFF; }
        }
        public int MIObserverMode
        {
            get { return this.ReadFieldProxy<int>("CSPlayer.m_iObserverMode"); }
        }
        public uint MIWeaponIndex
        {
            get
            {
                if (_iWeaponIndex == 0)
                {
                    if (this.MHActiveWeapon != 0xFFFFFFFF)
                    _iWeaponIndex = this.MHActiveWeapon & 0xFFF;
                }
                return _iWeaponIndex;
            }
        }
        public Skeleton Bones { get; private set; }
        #endregion

        #region CONSTRUCTORS
        public CsPlayer(int address) : base(address)
        {
            _iWeaponIndex = 0;
            this.Bones = new Skeleton(this.MHBoneMatrix);
        }
        public CsPlayer(BaseEntity baseEntity)
            : base(baseEntity)
        {
            _iWeaponIndex = 0;
            this.Bones = new Skeleton(this.MHBoneMatrix);
        }
        public CsPlayer(CsPlayer copyFrom) : base(copyFrom)
        {
            this.CopyFieldsFrom(copyFrom);
            _iWeaponIndex = 0;
            this.Bones = copyFrom.Bones;
        }
        #endregion

        #region METHODS
        protected override void SetupFields()
        {
            base.SetupFields();
            this.AddField<int>("CSPlayer.m_hBoneMatrix", CsgoOffsets.NetVars.CCsPlayer.MHBoneMatrix);
            this.AddField<uint>("CSPlayer.m_hActiveWeapon", CsgoOffsets.NetVars.CCsPlayer.MHActiveWeapon);
            this.AddField<int>("CSPlayer.m_iFlags", CsgoOffsets.NetVars.CCsPlayer.MIFlags);
            this.AddField<int>("CSPlayer.m_hObserverTarget", CsgoOffsets.NetVars.CCsPlayer.MHObserverTarget);
            this.AddField<int>("CSPlayer.m_iObserverMode", CsgoOffsets.NetVars.CCsPlayer.MIObserverMode);
            this.AddField<Vector3>("CSPlayer.m_vecVelocity", CsgoOffsets.NetVars.CCsPlayer.MVecVelocity);
        }
        public override bool IsValid()
        {
            return base.IsValid() && (this.MITeamNum == 2 ||this.MITeamNum == 3);
        }
        public override string ToString()
        {
            return string.Format("[CSPlayer m_iHealth={0}, m_iTeamNum={3}, m_iFlags={1}]\n{2}", this.MIHealth, Convert.ToString(this.MIFlags, 2).PadLeft(32, '0'), base.ToString(), this.MITeamNum);
        }
        public Weapon GetActiveWeapon()
        {
            if (this.MHActiveWeapon == 0xFFFFFFFF)
                return null;

            uint handle = this.MHActiveWeapon & 0xFFF;
            if (Program.Framework.Weapons.Count(x=>x.Item1 == handle - 1) > 0)
            {
                return Program.Framework.Weapons.First(x => x.Item1 == handle - 1).Item2;
            }
            return null;
        }
        #endregion

        #region CLASSES
        public class Skeleton : Entity
        {
            #region FIELDS
            public Vector3 Head
            {
                get { return ReadFieldProxy<Vector3>("Head"); }
            }
            public Vector3 Neck
            {
                get { return ReadFieldProxy<Vector3>("Neck"); }
            }
            public Vector3 Spine1
            {
                get { return ReadFieldProxy<Vector3>("Spine1"); }
            }
            public Vector3 Spine2
            {
                get { return ReadFieldProxy<Vector3>("Spine2"); }
            }
            public Vector3 Spine3
            {
                get { return ReadFieldProxy<Vector3>("Spine3"); }
            }
            public Vector3 Spine4
            {
                get { return ReadFieldProxy<Vector3>("Spine4"); }
            }
            public Vector3 Spine5
            {
                get { return ReadFieldProxy<Vector3>("Spine5"); }
            }
            public Vector3 LeftHand
            {
                get { return ReadFieldProxy<Vector3>("LeftHand"); }
            }
            public Vector3 LeftElbow
            {
                get { return ReadFieldProxy<Vector3>("LeftElbow"); }
            }
            public Vector3 LeftShoulder
            {
                get { return ReadFieldProxy<Vector3>("LeftShoulder"); }
            }
            public Vector3 RightShoulder
            {
                get { return ReadFieldProxy<Vector3>("RightShoulder"); }
            }
            public Vector3 RightElbow
            {
                get { return ReadFieldProxy<Vector3>("RightElbow"); }
            }
            public Vector3 RightHand
            {
                get { return ReadFieldProxy<Vector3>("RightHand"); }
            }
            public Vector3 LeftToe
            {
                get { return ReadFieldProxy<Vector3>("LeftToe"); }
            }
            public Vector3 LeftFoot
            {
                get { return ReadFieldProxy<Vector3>("LeftFoot"); }
            }
            public Vector3 LeftKnee
            {
                get { return ReadFieldProxy<Vector3>("LeftKnee"); }
            }
            public Vector3 LeftHip
            {
                get { return ReadFieldProxy<Vector3>("LeftHip"); }
            }
            public Vector3 RightHip
            {
                get { return ReadFieldProxy<Vector3>("RightHip"); }
            }
            public Vector3 RightKnee
            {
                get { return ReadFieldProxy<Vector3>("RightKnee"); }
            }
            public Vector3 RightFoot
            {
                get { return ReadFieldProxy<Vector3>("RightFoot"); }
            }
            public Vector3 RightToe
            {
                get { return ReadFieldProxy<Vector3>("RightToe"); }
            }
            public Vector3 Weapon1
            {
                get { return ReadFieldProxy<Vector3>("Weapon1"); }
            }
            public Vector3 Weapon2
            {
                get { return ReadFieldProxy<Vector3>("Weapon2"); }
            }
            #endregion

            #region CONSTRUCTORS
            public Skeleton(int address) : base(address)
            {
                this.AddBone("Head", 11);
                this.AddBone("Neck", 10);
                this.AddBone("Spine1", 1);
                this.AddBone("Spine2", 2);
                this.AddBone("Spine3", 3);
                this.AddBone("Spine4", 4);
                this.AddBone("Spine5", 5);
                this.AddBone("LeftHand", 21);
                this.AddBone("LeftElbow", 31);
                this.AddBone("LeftShoulder", 36);
                this.AddBone("RightShoulder", 37);
                this.AddBone("RightElbow", 38);
                this.AddBone("RightHand", 15);
                this.AddBone("LeftToe", 38);
                this.AddBone("LeftFoot", 28);
                this.AddBone("LeftKnee", 27);
                this.AddBone("LeftHip", 26);
                this.AddBone("RightHip", 23);
                this.AddBone("RightKnee", 24);
                this.AddBone("RightFoot", 25);
                this.AddBone("RightToe", 37);
                this.AddBone("Weapon1", 16);
                this.AddBone("Weapon2", 21);
            }
            #endregion

            #region METHODS
            public Vector3 GetBoneByIndex(int index)
            {
                var fields = this.Fields.Values.Cast<BonesField>();
                if (fields.Count(x => x.Offset == index) == 0)
                    return Vector3.Zero;
                BonesField field = fields.First(x => x.Offset == index);
                foreach (string name in this.Fields.Keys)
                    if (this.Fields[name] == field)
                        return ReadFieldProxy<Vector3>(name);
                return Vector3.Zero;
            }
            protected void AddBone(string name, int index)
            {
                this.Fields[name] = new BonesField(index);
            }
            #endregion
        }
        #endregion
    }
}
