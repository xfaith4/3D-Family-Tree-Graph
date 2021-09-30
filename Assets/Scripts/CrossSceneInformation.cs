using Assets.Scripts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
