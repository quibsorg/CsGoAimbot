namespace CsGoApplicationAimbot.CSGOClasses
{
    public class Weapon : BaseEntity
    {
        #region VARIABLES
        #endregion

        #region FIELDS
        public int MIItemDefinitionIndex
        {
            get { return ReadFieldProxy<int>("Weapon.m_iItemDefinitionIndex"); }
        }
        public int MIState
        {
            get { return ReadFieldProxy<int>("Weapon.m_iState"); }
        }
        public int MIClip1
        {
            get { return ReadFieldProxy<int>("Weapon.m_iClip1"); }
        }
        public float MFlNextPrimaryAttack
        {
            get { return ReadFieldProxy<float>("Weapon.m_flNextPrimaryAttack"); }
        }
        public int MBCanReload
        {
            get { return ReadFieldProxy<int>("Weapon.m_bCanReload"); }
        }
        public int MIWeaponTableIndex
        {
            get { return ReadFieldProxy<int>("Weapon.m_iWeaponTableIndex"); }
        }
        public float MFAccuracyPenalty
        {
            get { return ReadFieldProxy<float>("Weapon.m_fAccuracyPenalty"); }
        }
        public int MIWeaponId
        {
            get { return ReadFieldProxy<int>("Weapon.m_iWeaponID"); }
        }
        #endregion

        #region CONSTRUCTORS
        public Weapon(int address) : base(address)
        {
        }
        public Weapon(BaseEntity baseEntity)
            : base(baseEntity)
        {
        }
        public Weapon(Weapon other) 
            : base(other)
        {
            CopyFieldsFrom(other);
        }
        #endregion

        #region METHODS
        protected override void SetupFields()
        {
            base.SetupFields();
            AddField<int>("Weapon.m_iItemDefinitionIndex", CsgoOffsets.NetVars.Weapon.MIItemDefinitionIndex);
            AddField<int>("Weapon.m_iState", CsgoOffsets.NetVars.Weapon.MIState);
            AddField<int>("Weapon.m_iClip1", CsgoOffsets.NetVars.Weapon.MIClip1);
            AddField<float>("Weapon.m_flNextPrimaryAttack", CsgoOffsets.NetVars.Weapon.MFlNextPrimaryAttack);
            AddField<int>("Weapon.m_bCanReload", CsgoOffsets.NetVars.Weapon.MBCanReload);
            AddField<int>("Weapon.m_iWeaponTableIndex", CsgoOffsets.NetVars.Weapon.MIWeaponTableIndex);
            AddField<float>("Weapon.m_fAccuracyPenalty", CsgoOffsets.NetVars.Weapon.MFAccuracyPenalty);
            AddField<int>("Weapon.m_iWeaponID", CsgoOffsets.NetVars.Weapon.MIWeaponId);
        }
        public override bool IsValid()
        {
            return base.IsValid() && MIWeaponId > 0 && MIItemDefinitionIndex > 0;
        }
        public bool IsCarried()
        {
            return MHOwnerEntity != 0;
        }
        #endregion
    }
}
