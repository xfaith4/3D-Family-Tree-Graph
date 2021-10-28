using Assets.Scripts.Enums;

namespace Assets.Scripts
{
    public static class CrossSceneInformation
    {
        public static int startingDataBaseId { get; set; }
        public static TribeType myTribeType { get; set; }
        public static int numberOfGenerations { get; set; }
        public static string rootsMagicDataFileNameWithFullPath { get; set; }
    }
}
