namespace CsGoApplicationAimbot.CSGOClasses
{
    public class Weapon : BaseEntity
    {
        #region FIELDS

        private int MiItemDefinitionIndex => ReadFieldProxy<int>("Weapon.m_iItemDefinitionIndex");
        public int MiState => ReadFieldProxy<int>("Weapon.m_iState");
        public int MiClip1 => ReadFieldProxy<int>("Weapon.m_iClip1");
        public float MFlNextPrimaryAttack => ReadFieldProxy<float>("Weapon.m_flNextPrimaryAttack");
        public int MbCanReload => ReadFieldProxy<int>("Weapon.m_bCanReload");
        public int MiWeaponTableIndex => ReadFieldProxy<int>("Weapon.m_iWeaponTableIndex");
        public float MfAccuracyPenalty => ReadFieldProxy<float>("Weapon.m_fAccuracyPenalty");
        public int MiWeaponId => ReadFieldProxy<int>("Weapon.m_iWeaponID");

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
            AddField<int>("Weapon.m_iItemDefinitionIndex", CsgoOffsets.NetVars.Weapon.MiItemDefinitionIndex);
            AddField<int>("Weapon.m_iState", CsgoOffsets.NetVars.Weapon.MiState);
            AddField<int>("Weapon.m_iClip1", CsgoOffsets.NetVars.Weapon.MiClip1);
            AddField<float>("Weapon.m_flNextPrimaryAttack", CsgoOffsets.NetVars.Weapon.MFlNextPrimaryAttack);
            AddField<int>("Weapon.m_bCanReload", CsgoOffsets.NetVars.Weapon.MbCanReload);
            AddField<int>("Weapon.m_iWeaponTableIndex", CsgoOffsets.NetVars.Weapon.MiWeaponTableIndex);
            AddField<float>("Weapon.m_fAccuracyPenalty", CsgoOffsets.NetVars.Weapon.MfAccuracyPenalty);
            AddField<int>("Weapon.m_iWeaponID", CsgoOffsets.NetVars.Weapon.MiWeaponId);
        }

        public override bool IsValid()
        {
            return base.IsValid() && MiWeaponId > 0 && MiItemDefinitionIndex > 0;
        }

        public bool IsCarried()
        {
            return MhOwnerEntity != 0;
        }

        #endregion
    }
}