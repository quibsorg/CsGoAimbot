namespace CsGoApplicationAimbot.CSGOClasses
{
    public class Weapon : BaseEntity
    {
        #region FIELDS

        private int ItemDefinitionIndex => ReadFieldProxy<int>("Weapon.m_iItemDefinitionIndex");
        public int State => ReadFieldProxy<int>("Weapon.m_iState");
        public int Clip1 => ReadFieldProxy<int>("Weapon.m_iClip1");
        public float NextPrimaryAttack => ReadFieldProxy<float>("Weapon.m_flNextPrimaryAttack");
        public bool CanReload => ReadFieldProxy<bool>("Weapon.m_bCanReload");
        public int WeaponTableIndex => ReadFieldProxy<int>("Weapon.m_iWeaponTableIndex");
        public float AccuracyPenalty => ReadFieldProxy<float>("Weapon.m_fAccuracyPenalty");
        public int WeaponId => ReadFieldProxy<int>("Weapon.m_iWeaponID");
        public int ZoomLevel => ReadFieldProxy<int>("Weapon.m_zoomLevel");
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
            AddField<int>("Weapon.m_iItemDefinitionIndex", CsgoOffsets.NetVars.Weapon.ItemDefinitionIndex);
            AddField<int>("Weapon.m_iState", CsgoOffsets.NetVars.Weapon.State);
            AddField<int>("Weapon.m_iClip1", CsgoOffsets.NetVars.Weapon.Clip1);
            AddField<float>("Weapon.m_flNextPrimaryAttack", CsgoOffsets.NetVars.Weapon.NextPrimaryAttack);
            AddField<int>("Weapon.m_bCanReload", CsgoOffsets.NetVars.Weapon.CanReload);
            AddField<int>("Weapon.m_iWeaponTableIndex", CsgoOffsets.NetVars.Weapon.WeaponTableIndex);
            AddField<float>("Weapon.m_fAccuracyPenalty", CsgoOffsets.NetVars.Weapon.AccuracyPenalty);
            AddField<int>("Weapon.m_iWeaponID", CsgoOffsets.NetVars.Weapon.WeaponId);
            AddField<int>("Weapon.m_zoomLevel", CsgoOffsets.NetVars.Weapon.MZoomLevel);
        }

        public override bool IsValid()
        {
            return base.IsValid() && WeaponId > 0 && ItemDefinitionIndex > 0;
        }

        public bool IsCarried()
        {
            return OwnerEntity != 0;
        }

        #endregion
    }
}