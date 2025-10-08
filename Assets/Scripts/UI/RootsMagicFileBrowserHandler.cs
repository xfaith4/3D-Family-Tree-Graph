using UnityEngine;
using System.Collections;
using System.IO;
using SimpleFileBrowser;
using UnityEngine.UI;
using Assets.Scripts.DataProviders;

public class RootsMagicFileBrowserHandler : MonoBehaviour
{

	public Text fileSelectedText;
	public string initialPath = null;
	public string initialFilename = null;
	public GameObject personPickerDropdownGameObject;

	private ListOfPersonsFromDataBase myTribeOfPeople;
	// Warning: paths returned by FileBrowser dialogs do not contain a trailing '\' character
	// Warning: FileBrowser can only show 1 dialog at a time

	void Start()
	{

		// Set filters (optional)
		// It is sufficient to set the filters just once (instead of each time before showing the file browser dialog), 
		// if all the dialogs will be using the same filters
		FileBrowser.SetFilters(true, new FileBrowser.Filter("RootsMagic", ".rmgc", ".rmtree"), new FileBrowser.Filter("GEDCOM", ".ged"));

		// Set default filter that is selected when the dialog is shown (optional)
		// Returns true if the default filter is set successfully
		// In this case, set Images filter as the default filter
		FileBrowser.SetDefaultFilter(".rmgc");

		// Set excluded file extensions (optional) (by default, .lnk and .tmp extensions are excluded)
		// Note that when you use this function, .lnk and .tmp extensions will no longer be
		// excluded unless you explicitly add them as parameters to the function
		FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");

		// Add a new quick link to the browser (optional) (returns true if quick link is added successfully)
		// It is sufficient to add a quick link just once
		// Name: Users
		// Path: C:\Users
		// Icon: default (folder icon)
		FileBrowser.AddQuickLink("Users", "C:\\Users", null);
		
		// Check for default HofstetterFamilyTree.rmtree file in the project root
		string projectRoot = Application.dataPath.Replace("/Assets", "");
		string defaultDbPath = Path.Combine(projectRoot, "HofstetterFamilyTree.rmtree");
		
		if (PlayerPrefs.HasKey("LastUsedRootsMagicDataFilePath"))
		{
			Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath = PlayerPrefs.GetString("LastUsedRootsMagicDataFilePath");
			initialFilename = Path.GetFileName(Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath);
			initialPath = Path.GetDirectoryName(Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath);
			fileSelectedText.text = initialFilename;
	//		personPickerDropdownGameObject.GetComponent<PersonPickerHandler>().FileSelectedNowEnableUserInterface(true);
            Debug.Log("Game data from RootsMagic loaded!");

		}
		else if (File.Exists(defaultDbPath))
		{
			// Auto-load the default HofstetterFamilyTree.rmtree file
			Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath = defaultDbPath;
			initialFilename = Path.GetFileName(defaultDbPath);
			initialPath = Path.GetDirectoryName(defaultDbPath);
			fileSelectedText.text = initialFilename;
			PlayerPrefs.SetString("LastUsedRootsMagicDataFilePath", defaultDbPath);
			PlayerPrefs.Save();
			Debug.Log("Default HofstetterFamilyTree.rmtree loaded!");
		}
		else
			Debug.Log("There is no RootsMagic save data!");

		// Show a save file dialog 
		// onSuccess event: not registered (which means this dialog is pretty useless)
		// onCancel event: not registered
		// Save file/folder: file, Allow multiple selection: false
		// Initial path: "C:\", Initial filename: "Screenshot.png"
		// Title: "Save As", Submit button text: "Save"
		// FileBrowser.ShowSaveDialog( null, null, FileBrowser.PickMode.Files, false, "C:\\", "Screenshot.png", "Save As", "Save" );

		// Show a select folder dialog 
		// onSuccess event: print the selected folder's path
		// onCancel event: print "Canceled"
		// Load file/folder: folder, Allow multiple selection: false
		// Initial path: default (Documents), Initial filename: empty
		// Title: "Select Folder", Submit button text: "Select"
		// FileBrowser.ShowLoadDialog( ( paths ) => { Debug.Log( "Selected: " + paths[0] ); },
		//						   () => { Debug.Log( "Canceled" ); },
		//						   FileBrowser.PickMode.Folders, false, null, null, "Select Folder", "Select" );

		transform.GetComponent<Button>().onClick.AddListener(delegate { ShowLoadFileDialog(); });

	}

	void ShowLoadFileDialog()
    {
		StartCoroutine(ShowLoadDialogCoroutine());
	}

	IEnumerator ShowLoadDialogCoroutine()
	{
		// Show a load file dialog and wait for a response from user
		// Load file/folder: both, Allow multiple selection: true
		// Initial path: default (Documents), Initial filename: empty
		// Title: "Load File", Submit button text: "Load"
		yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, allowMultiSelection: false, initialPath: initialPath, initialFilename: initialFilename, "Open RootMagic File", "Open");

		// Dialog is closed
		// Print whether the user has selected some files/folders or cancelled the operation (FileBrowser.Success)
		Debug.Log(FileBrowser.Success);

		if (FileBrowser.Success)
		{
			string result = null;
			for (int i = 0; i < FileBrowser.Result.Length; i++)
			{
				result = FileBrowser.Result[i];
			}
            try
            {
				if (!File.Exists(result))
					throw new FileNotFoundException($"{result} file was not found.");

				string databasePath = result;
				
				// Check if this is a GEDCOM file
				if (Path.GetExtension(result).ToLower() == ".ged")
				{
					Debug.Log("GEDCOM file detected. Converting to SQLite database...");
					
					// Create output database path
					string gedcomFileName = Path.GetFileNameWithoutExtension(result);
					databasePath = Path.Combine(Path.GetDirectoryName(result), gedcomFileName + "_converted.rmtree");
					
					// Convert GEDCOM to SQLite
					var converter = new GedcomToSqlConverter();
					converter.ConvertGedcomToDatabase(result, databasePath);
					
					Debug.Log($"GEDCOM converted to database: {databasePath}");
				}

				myTribeOfPeople = new ListOfPersonsFromDataBase(databasePath);
				if (myTribeOfPeople.QuickDataBaseIntergetyCheck())
				{
					Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath = databasePath;
					fileSelectedText.text = Path.GetFileName(databasePath);
					Debug.Log("Data File Path Chosen: " + fileSelectedText.text);
					PlayerPrefs.SetString("LastUsedRootsMagicDataFilePath", Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath);
                    PlayerPrefs.Save();
					Debug.Log("Game data saved!");
					personPickerDropdownGameObject.GetComponent<PersonPickerHandler>().CheckIfFileSelectedAndEnableUserInterface();
				}
				
			}
            catch (System.Exception ex)
            {
				Debug.Log($"{ex.Message} Exception thrown, database file is not valid.");
				Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath = null;
				fileSelectedText.text = "> File Failure <  Please try again.";
				Debug.Log("Bad Data File Path Chosen: " + result);
				personPickerDropdownGameObject.GetComponent<PersonPickerHandler>().CheckIfFileSelectedAndEnableUserInterface();
			}

			
#if NOTNOW

			// Read the bytes of the first file via FileBrowserHelpers
			// Contrary to File.ReadAllBytes, this function works on Android 10+, as well
			byte[] bytes = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);

			// Or, copy the first file to persistentDataPath
			string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
			FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
#endif
		}
	}
}
