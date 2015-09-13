namespace CsGoApplicationAimbot.CSGOClasses
{
    public class Weapon : BaseEntity
    {
        #region VARIABLES
        #endregion

        #region FIELDS
        public int MIItemDefinitionIndex
        {
            get { return this.ReadFieldProxy<int>("Weapon.m_iItemDefinitionIndex"); }
        }
        public int MIState
        {
            get { return this.ReadFieldProxy<int>("Weapon.m_iState"); }
        }
        public int MIClip1
        {
            get { return this.ReadFieldProxy<int>("Weapon.m_iClip1"); }
        }
        public float MFlNextPrimaryAttack
        {
            get { return this.ReadFieldProxy<float>("Weapon.m_flNextPrimaryAttack"); }
        }
        public int MBCanReload
        {
            get { return this.ReadFieldProxy<int>("Weapon.m_bCanReload"); }
        }
        public int MIWeaponTableIndex
        {
            get { return this.ReadFieldProxy<int>("Weapon.m_iWeaponTableIndex"); }
        }
        public float MFAccuracyPenalty
        {
            get { return this.ReadFieldProxy<float>("Weapon.m_fAccuracyPenalty"); }
        }
        public int MIWeaponId
        {
            get { return this.ReadFieldProxy<int>("Weapon.m_iWeaponID"); }
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
            this.CopyFieldsFrom(other);
        }
        #endregion

        #region METHODS
        protected override void SetupFields()
        {
            base.SetupFields();
            this.AddField<int>("Weapon.m_iItemDefinitionIndex", CsgoOffsets.NetVars.Weapon.MIItemDefinitionIndex);
            this.AddField<int>("Weapon.m_iState", CsgoOffsets.NetVars.Weapon.MIState);
            this.AddField<int>("Weapon.m_iClip1", CsgoOffsets.NetVars.Weapon.MIClip1);
            this.AddField<float>("Weapon.m_flNextPrimaryAttack", CsgoOffsets.NetVars.Weapon.MFlNextPrimaryAttack);
            this.AddField<int>("Weapon.m_bCanReload", CsgoOffsets.NetVars.Weapon.MBCanReload);
            this.AddField<int>("Weapon.m_iWeaponTableIndex", CsgoOffsets.NetVars.Weapon.MIWeaponTableIndex);
            this.AddField<float>("Weapon.m_fAccuracyPenalty", CsgoOffsets.NetVars.Weapon.MFAccuracyPenalty);
            this.AddField<int>("Weapon.m_iWeaponID", CsgoOffsets.NetVars.Weapon.MIWeaponId);
        }
        public override bool IsValid()
        {
            return base.IsValid() && this.MIWeaponId > 0 && this.MIItemDefinitionIndex > 0;
        }
        public bool IsCarried()
        {
            return this.MHOwnerEntity != 0;
        }
        #endregion
    }
}
