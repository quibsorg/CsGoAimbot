using System;
using System.Linq;
using CsGoApplicationAimbot.CSGOClasses.Fields;
using CsGoApplicationAimbot.MathObjects;

namespace CsGoApplicationAimbot.CSGOClasses
{
    public class Player : BaseEntity
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
                AddBone("pelvis", 0);
                AddBone("spine_0", 1);
                AddBone("spine_1", 2);
                AddBone("spine_2", 3);
                AddBone("spine_3", 4);
                AddBone("neck_0", 5);
                AddBone("head_0", 6);
                AddBone("clavicle_L", 7);
                AddBone("arm_upper_L", 8);
                AddBone("arm_lower_L", 9);
                AddBone("hand_L", 10);
                AddBone("finger_middle_meta_L", 11);
                AddBone("finger_middle_0_L", 12);
                AddBone("finger_middle_1_L", 13);
                AddBone("finger_middle_2_L", 14);
                AddBone("finger_pinky_meta_L", 15);
                AddBone("finger_pinky_0_L", 16);
                AddBone("finger_pinky_1_L", 17);
                AddBone("finger_pinky_2_L", 18);
                AddBone("finger_index_meta_L", 19);
                AddBone("finger_index_0_L", 20);
                AddBone("finger_index_1_L", 21);
                AddBone("finger_index_2_L", 22);
                AddBone("finger_thumb_0_L", 23);
                AddBone("finger_thumb_1_L", 24);
                AddBone("finger_thumb_2_L", 25);
                AddBone("finger_ring_meta_L", 26);
                AddBone("finger_ring_0_L", 27);
                AddBone("finger_ring_1_L", 28);
                AddBone("finger_ring_2_L", 29);
                AddBone("weapon_hand_L", 30);
                AddBone("arm_lower_L_TWIST", 31);
                AddBone("arm_lower_L_TWIST1", 32);
                AddBone("arm_upper_L_TWIST", 33);
                AddBone("arm_upper_L_TWIST1", 34);
                AddBone("clavicle_R", 35);
                AddBone("arm_upper_R", 36);
                AddBone("arm_lower_R", 37);
                AddBone("hand_R", 38);
                AddBone("finger_middle_meta_R", 39);
                AddBone("finger_middle_0_R", 40);
                AddBone("finger_middle_1_R", 41);
                AddBone("finger_middle_2_R", 42);
                AddBone("finger_pinky_meta_R", 43);
                AddBone("finger_pinky_0_R", 44);
                AddBone("finger_pinky_1_R", 45);
                AddBone("finger_pinky_2_R", 46);
                AddBone("finger_index_meta_R", 47);
                AddBone("finger_index_0_R", 48);
                AddBone("finger_index_1_R", 49);
                AddBone("finger_index_2_R", 50);
                AddBone("finger_thumb_0_R", 51);
                AddBone("finger_thumb_1_R", 52);
                AddBone("finger_thumb_2_R", 53);
                AddBone("finger_ring_meta_R", 54);
                AddBone("finger_ring_0_R", 55);
                AddBone("finger_ring_1_R", 56);
                AddBone("finger_ring_2_R", 57);
                AddBone("weapon_hand_R", 58);
                AddBone("arm_lower_R_TWIST", 59);
                AddBone("arm_lower_R_TWIST1", 60);
                AddBone("arm_upper_R_TWIST", 61);
                AddBone("arm_upper_R_TWIST1", 62);
                AddBone("leg_upper_L", 63);
                AddBone("leg_lower_L", 64);
                AddBone("ankle_L", 65);
                AddBone("ball_L", 66);
                AddBone("leg_upper_L_TWIST", 67);
                AddBone("leg_upper_L_TWIST1", 68);
                AddBone("leg_upper_R", 69);
                AddBone("leg_lower_R", 70);
                AddBone("ankle_R", 71);
                AddBone("ball_R", 72);
                AddBone("leg_upper_R_TWIST", 73);
                AddBone("leg_upper_R_TWIST1", 74);
                AddBone("ValveBiped.weapon_bone", 75);
                AddBone("lh_ik_driver", 76);
                AddBone("lean_root", 77);
                AddBone("lfoot_lock", 78);
                AddBone("rfoot_lock", 79);
                AddBone("primary_jiggle_jnt", 80);
                AddBone("primary_smg_jiggle_jnt", 81);
            }

            #endregion

            #region FIELDS

            public Vector3 Head => ReadFieldProxy<Vector3>("head_0");
            public Vector3 Neck => ReadFieldProxy<Vector3>("neck_0");
            public Vector3 Spine0 => ReadFieldProxy<Vector3>("spine_0");
            public Vector3 Spine1 => ReadFieldProxy<Vector3>("spine_1");
            public Vector3 Spine2 => ReadFieldProxy<Vector3>("spine_2");
            public Vector3 Spine3 => ReadFieldProxy<Vector3>("spine_3");

            #endregion

            #region METHODS

            public Vector3 GetBoneByIndex(int index)
            {
                var fields = Fields.Values.Cast<BonesField>();
                var bonesFields = fields as BonesField[] ?? fields.ToArray();
                if (bonesFields.Count(x => x.Offset == index) == 0)
                    return Vector3.Zero;
                var field = bonesFields.First(x => x.Offset == index);
                foreach (var name in Fields.Keys.Cast<string>().Where(name => Fields[name] == field))
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

        private int BoneMatrix => ReadFieldProxy<int>("CSPlayer.m_hBoneMatrix");
        public int Flags => ReadFieldProxy<int>("CSPlayer.m_iFlags");
        private uint ActiveWeapon => ReadFieldProxy<uint>("CSPlayer.m_hActiveWeapon");
        public Vector3 VecVelocity => ReadFieldProxy<Vector3>("CSPlayer.m_vecVelocity");
        public int ObserverTarget => ReadFieldProxy<int>("CSPlayer.m_hObserverTarget") & 0xFFF;
        public int ObserverMode => ReadFieldProxy<int>("CSPlayer.m_iObserverMode");
        public uint WeaponIndex
        {
            get
            {
                if (_iWeaponIndex != 0) return _iWeaponIndex;
                if (ActiveWeapon != 0xFFFFFFFF)
                    _iWeaponIndex = ActiveWeapon & 0xFFF;
                return _iWeaponIndex;
            }
        }

        public Skeleton Bones { get; }

        #endregion

        #region CONSTRUCTORS

        public Player(int address) : base(address)
        {
            _iWeaponIndex = 0;
            Bones = new Skeleton(BoneMatrix);
        }

        public Player(BaseEntity baseEntity) : base(baseEntity)
        {
            _iWeaponIndex = 0;
            Bones = new Skeleton(BoneMatrix);
        }

        public Player(Player copyFrom) : base(copyFrom)
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
            //AddField<float>("CSPlayer.m_flFlashMaxAlpha", Offsets.NetVars.CCsPlayer.FlashMaxAlpha);
            //AddField<float>("CSPlayer.m_flFlashDuration", Offsets.NetVars.CCsPlayer.FlashDuriation);
            AddField<int>("CSPlayer.m_hBoneMatrix", Offsets.NetVars.CCsPlayer.BoneMatrix);
            AddField<uint>("CSPlayer.m_hActiveWeapon", Offsets.NetVars.CCsPlayer.ActiveWeapon);
            AddField<int>("CSPlayer.m_iFlags", Offsets.NetVars.CCsPlayer.Flags);
            AddField<int>("CSPlayer.m_hObserverTarget", Offsets.NetVars.CCsPlayer.ObserverTarget);
            AddField<int>("CSPlayer.m_iObserverMode", Offsets.NetVars.CCsPlayer.ObserverMode);
            AddField<Vector3>("CSPlayer.m_vecVelocity", Offsets.NetVars.CCsPlayer.VecVelocity);
        }

        public override bool IsValid()
        {
            return base.IsValid() && (TeamNum == 2 || TeamNum == 3);
        }

        public override string ToString()
        {
            return string.Format("[CSPlayer m_iHealth={0}, m_iTeamNum={3}, m_iFlags={1}]\n{2}", Health,
                Convert.ToString(Flags, 2).PadLeft(32, '0'), base.ToString(), TeamNum);
        }

        public Weapon GetActiveWeapon()
        {
            if (ActiveWeapon == 0xFFFFFFFF)
                return null;

            var handle = ActiveWeapon & 0xFFF;
            return Program.Memory.Weapons.Count(x => x.Item1 == handle - 1) > 0
                ? Program.Memory.Weapons.First(x => x.Item1 == handle - 1).Item2
                : null;
        }

        public string GetActiveWeaponName()
        {
            if (ActiveWeapon == 0xFFFFFFFF)
            {
                //If we return null it will crash.
                return "Default";
            }

            var handle = ActiveWeapon & 0xFFF;
            if (Program.Memory.Weapons.Count(x => x.Item1 == handle - 1) > 0)
            {
                var weapon = Program.Memory.Weapons.First(x => x.Item1 == handle - 1).Item2;
                var weaponName = weapon.ClassName;
                weaponName = weaponName.Remove(0, 1);
                if (weaponName.Contains("Weapon"))
                {
                    weaponName = weaponName.Replace("Weapon", "");
                }
                if (weaponName.Contains("Knife") || weaponName.Contains("Taser"))
                {
                    return "Default";
                }

                return weaponName;
            }
            return "Default";
        }

        #endregion
    }
}