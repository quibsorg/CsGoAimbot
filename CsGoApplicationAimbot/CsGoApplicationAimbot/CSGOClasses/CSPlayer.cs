﻿using System;
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

        #region CLASSES

        public class Skeleton : Entity
        {
            #region CONSTRUCTORS

            public Skeleton(int address) : base(address)
            {
                AddBone("Head", 11);
                AddBone("Neck", 10);
                AddBone("Spine1", 1);
                AddBone("Spine2", 2);
                AddBone("Spine3", 3);
                AddBone("Spine4", 4);
                AddBone("Spine5", 5);
                AddBone("LeftHand", 21);
                AddBone("LeftElbow", 31);
                AddBone("LeftShoulder", 36);
                AddBone("RightShoulder", 37);
                AddBone("RightElbow", 38);
                AddBone("RightHand", 15);
                AddBone("LeftToe", 38);
                AddBone("LeftFoot", 28);
                AddBone("LeftKnee", 27);
                AddBone("LeftHip", 26);
                AddBone("RightHip", 23);
                AddBone("RightKnee", 24);
                AddBone("RightFoot", 25);
                AddBone("RightToe", 37);
                AddBone("Weapon1", 16);
                AddBone("Weapon2", 21);
            }

            #endregion

            #region FIELDS

            public Vector3 Head => ReadFieldProxy<Vector3>("Head");

            public Vector3 Neck => ReadFieldProxy<Vector3>("Neck");

            public Vector3 Spine1 => ReadFieldProxy<Vector3>("Spine1");

            public Vector3 Spine2 => ReadFieldProxy<Vector3>("Spine2");

            public Vector3 Spine3 => ReadFieldProxy<Vector3>("Spine3");

            public Vector3 Spine4 => ReadFieldProxy<Vector3>("Spine4");
            public Vector3 Spine5 => ReadFieldProxy<Vector3>("Spine5");

            public Vector3 LeftHand => ReadFieldProxy<Vector3>("LeftHand");

            public Vector3 LeftElbow => ReadFieldProxy<Vector3>("LeftElbow");
            public Vector3 LeftShoulder => ReadFieldProxy<Vector3>("LeftShoulder");

            public Vector3 RightShoulder => ReadFieldProxy<Vector3>("RightShoulder");

            public Vector3 RightElbow => ReadFieldProxy<Vector3>("RightElbow");

            public Vector3 RightHand => ReadFieldProxy<Vector3>("RightHand");

            public Vector3 LeftToe => ReadFieldProxy<Vector3>("LeftToe");

            public Vector3 LeftFoot => ReadFieldProxy<Vector3>("LeftFoot");
            public Vector3 LeftKnee => ReadFieldProxy<Vector3>("LeftKnee");

            public Vector3 LeftHip => ReadFieldProxy<Vector3>("LeftHip");

            public Vector3 RightHip => ReadFieldProxy<Vector3>("RightHip");
            public Vector3 RightKnee => ReadFieldProxy<Vector3>("RightKnee");

            public Vector3 RightFoot => ReadFieldProxy<Vector3>("RightFoot");

            public Vector3 RightToe => ReadFieldProxy<Vector3>("RightToe");

            public Vector3 Weapon1 => ReadFieldProxy<Vector3>("Weapon1");

            public Vector3 Weapon2 => ReadFieldProxy<Vector3>("Weapon2");

            #endregion

            #region METHODS

            public Vector3 GetBoneByIndex(int index)
            {
                var fields = Fields.Values.Cast<BonesField>();
                if (fields.Count(x => x.Offset == index) == 0)
                    return Vector3.Zero;
                var field = fields.First(x => x.Offset == index);
                foreach (string name in Fields.Keys)
                    if (Fields[name] == field)
                        return ReadFieldProxy<Vector3>(name);
                return Vector3.Zero;
            }

            protected void AddBone(string name, int index)
            {
                Fields[name] = new BonesField(index);
            }

            #endregion
        }

        #endregion

        #region FIELDS

        public int MhBoneMatrix => ReadFieldProxy<int>("CSPlayer.m_hBoneMatrix");

        public int MiFlags => ReadFieldProxy<int>("CSPlayer.m_iFlags");
        public uint MhActiveWeapon => ReadFieldProxy<uint>("CSPlayer.m_hActiveWeapon");
        public Vector3 MVecVelocity => ReadFieldProxy<Vector3>("CSPlayer.m_vecVelocity");

        public int MhObserverTarget => ReadFieldProxy<int>("CSPlayer.m_hObserverTarget") & 0xFFF;

        public int MiObserverMode => ReadFieldProxy<int>("CSPlayer.m_iObserverMode");

        public uint MiWeaponIndex
        {
            get
            {
                if (_iWeaponIndex == 0)
                {
                    if (MhActiveWeapon != 0xFFFFFFFF)
                        _iWeaponIndex = MhActiveWeapon & 0xFFF;
                }
                return _iWeaponIndex;
            }
        }

        public Skeleton Bones { get; }

        #endregion

        #region CONSTRUCTORS

        public CsPlayer(int address) : base(address)
        {
            _iWeaponIndex = 0;
            Bones = new Skeleton(MhBoneMatrix);
        }

        public CsPlayer(BaseEntity baseEntity)
            : base(baseEntity)
        {
            _iWeaponIndex = 0;
            Bones = new Skeleton(MhBoneMatrix);
        }

        public CsPlayer(CsPlayer copyFrom) : base(copyFrom)
        {
            CopyFieldsFrom(copyFrom);
            _iWeaponIndex = 0;
            Bones = copyFrom.Bones;
        }

        #endregion

        #region METHODS

        protected override void SetupFields()
        {
            base.SetupFields();
            AddField<int>("CSPlayer.m_hBoneMatrix", CsgoOffsets.NetVars.CCsPlayer.MhBoneMatrix);
            AddField<uint>("CSPlayer.m_hActiveWeapon", CsgoOffsets.NetVars.CCsPlayer.MhActiveWeapon);
            AddField<int>("CSPlayer.m_iFlags", CsgoOffsets.NetVars.CCsPlayer.MiFlags);
            AddField<int>("CSPlayer.m_hObserverTarget", CsgoOffsets.NetVars.CCsPlayer.MhObserverTarget);
            AddField<int>("CSPlayer.m_iObserverMode", CsgoOffsets.NetVars.CCsPlayer.MiObserverMode);
            AddField<Vector3>("CSPlayer.m_vecVelocity", CsgoOffsets.NetVars.CCsPlayer.MVecVelocity);
        }

        public override bool IsValid()
        {
            return base.IsValid() && (MiTeamNum == 2 || MiTeamNum == 3);
        }

        public override string ToString()
        {
            return string.Format("[CSPlayer m_iHealth={0}, m_iTeamNum={3}, m_iFlags={1}]\n{2}", MiHealth,
                Convert.ToString(MiFlags, 2).PadLeft(32, '0'), base.ToString(), MiTeamNum);
        }

        public Weapon GetActiveWeapon()
        {
            if (MhActiveWeapon == 0xFFFFFFFF)
                return null;

            var handle = MhActiveWeapon & 0xFFF;
            if (Program.Framework.Weapons.Count(x => x.Item1 == handle - 1) > 0)
            {
                return Program.Framework.Weapons.First(x => x.Item1 == handle - 1).Item2;
            }
            return null;
        }

        #endregion
    }
}