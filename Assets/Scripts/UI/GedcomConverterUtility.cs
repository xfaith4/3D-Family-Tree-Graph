using UnityEngine;
using Assets.Scripts.DataProviders;
using System.IO;

namespace Assets.Scripts.UI
{
    public class GedcomConverterUtility : MonoBehaviour
    {
        public void ConvertGedcomFileInProjectRoot()
        {
            string projectRoot = Application.dataPath.Replace("/Assets", "");
            string gedcomPath = Path.Combine(projectRoot, "Hofstetter Family Tree.ged");
            string outputDbPath = Path.Combine(projectRoot, "HofstetterFamilyTree_from_gedcom.rmtree");

            if (File.Exists(gedcomPath))
            {
                Debug.Log($"Converting GEDCOM file: {gedcomPath}");
                var converter = new GedcomToSqlConverter();
                converter.ConvertGedcomToDatabase(gedcomPath, outputDbPath);
                Debug.Log($"Conversion complete. Database created at: {outputDbPath}");
            }
            else
            {
                Debug.LogError($"GEDCOM file not found: {gedcomPath}");
            }
        }
    }
}
