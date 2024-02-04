public static class PathOfResources
{
    public static class Prefabs
    {
        private const string FolderName = "Prefabs";

        public const string Arrow = FolderName + "/Arrow";
        public const string Archer01 = FolderName + "/FantasyArcher_01";
        public const string Archer02 = FolderName + "/FantasyArcher_02";
        public const string Barricade = FolderName + "/Barricade";
        public const string Zombie01 = FolderName + "/Zombie_01";
        public const string Zombie02 = FolderName + "/Zombie_02";
        public const string Zombie03 = FolderName + "/Zombie_03";
        public const string Zombie04 = FolderName + "/Zombie_04";

        public static class UI
        {
            private const string FolderName = "UI";

            public const string DmgFont = Prefabs.FolderName + "/" + FolderName + "/UI_DmgFont";
        }
    }

    public static class SO
    {
        private const string FolderName = "ScriptableObjects";

        public const string ColorSwatches = FolderName + "/ColorSwatchSO";
    }

    public static class SpriteAtlas
    {
        public const string GDR_Icons_Bright = "Modern GDR - Free icons pack/00_Atlas/BrightIcons";
        public const string GDR_Icons_Dark = "Modern GDR - Free icons pack/00_Atlas/DarkIcons";
    }
}
