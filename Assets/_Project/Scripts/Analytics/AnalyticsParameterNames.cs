namespace _Project.Scripts.Analytics
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable MemberCanBePrivate.Global
    public static class AnalyticsParameterNames
    {
        public const string PlacementName = "placementName";
        public const string ItemName = "itemName";
        public const string Killer = "killer";
        public const string PerkName = "PerkName";
        
        public const string LevelName = "LevelName";
        public const string RoomsBeaten = "RoomsBeaten";
        public const string RoomsToBeat = "RoomsToBeat";
        
        public const string WeaponUpgrades_WeaponName = "WeaponUpgrades_WeaponName";
        public const string WeaponUpgrades_WeaponRarity = "WeaponUpgrades_WeaponRarity";
        public const string WeaponUpgrades_UpgradeCost = "WeaponUpgrades_UpgradeCost";
        public const string WeaponUpgrades_UpgradedStatName1 = "WeaponUpgrades_UpgradedStatName1";
        public const string WeaponUpgrades_UpgradedStatValue1 = "WeaponUpgrades_UpgradedStatValueFloat1";
        public const string WeaponUpgrades_UpgradedStatName2 = "WeaponUpgrades_UpgradedStatName2";
        public const string WeaponUpgrades_UpgradedStatValue2 = "WeaponUpgrades_UpgradedStatValueFloat2";
        public const string WeaponUpgrades_UpgradedStatName3 = "WeaponUpgrades_UpgradedStatName3";
        public const string WeaponUpgrades_UpgradedStatValue3 = "WeaponUpgrades_UpgradedStatValueFloat3";
        public const string WeaponUpgrades_UpgradedStatName4 = "WeaponUpgrades_UpgradedStatName4";
        public const string WeaponUpgrades_UpgradedStatValue4 = "WeaponUpgrades_UpgradedStatValueFloat4";

        public static (string, string) GetWeaponUpgradeStat(int index)
        {
            return index switch
            {
                1 => (WeaponUpgrades_UpgradedStatName1, WeaponUpgrades_UpgradedStatValue1),
                2 => (WeaponUpgrades_UpgradedStatName2, WeaponUpgrades_UpgradedStatValue2),
                3 => (WeaponUpgrades_UpgradedStatName3, WeaponUpgrades_UpgradedStatValue3),
                4 => (WeaponUpgrades_UpgradedStatName4, WeaponUpgrades_UpgradedStatValue4),
                _ => ("", "")
            };
        }
    }
    // ReSharper restore InconsistentNaming
    // ReSharper restore MemberCanBePrivate.Global
}