using Assets.Scripts.DataObjects;
using Assets.Scripts.DataProviders;
using Assets.Scripts.Enums;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NamePickerHandler : MonoBehaviour
{
    public Text searchStatusText;
    public InputField lastNameFilterField;
    public Toggle ancestryToggle;
    public Toggle descendancyToggle;
    public Dropdown generationsDropdown;
    public Button quitButton;
    public Button startButton;
    public int numberOfPeopleInTribe = 1000;

    private ListOfPersonsFromDataBase myTribeOfPeople;
    private Person selectedPerson;


    // Start is called before the first frame update
    void Start()
    {
        startButton.transform.Find("LoadingCircle").gameObject.SetActive(false);
        startButton.interactable = false;
        FileSelectedNowEnableUserInterface(false);

        if (PlayerPrefs.HasKey("LastUsedRootsMagicDataFilePath"))
        {
            Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath = PlayerPrefs.GetString("LastUsedRootsMagicDataFilePath");
            FileSelectedNowEnableUserInterface();
            Debug.Log("Game data loaded!");
        }
        else
            Debug.Log("There is no save data!");

        generationsDropdown.value = 0;

        quitButton.onClick.AddListener(delegate { quitClicked(); });

        startButton.onClick.AddListener(delegate { StartClicked(); });

        lastNameFilterField.onEndEdit.AddListener(delegate { LastNameFilterFieldEndEdit(lastNameFilterField); });
        lastNameFilterField.onSubmit.AddListener(delegate { LastNameFilterFieldEndEdit(lastNameFilterField); });

        lastNameFilterField.onValueChanged.AddListener(delegate { LastNameFilterFieldChanging(lastNameFilterField); });

        ancestryToggle.onValueChanged.AddListener(delegate { ToggleControl(ancestryToggle); });
        descendancyToggle.onValueChanged.AddListener(delegate { ToggleControl(descendancyToggle); });

        transform.GetComponent<Dropdown>().onValueChanged.AddListener(delegate { DropDownItemSelected(transform.GetComponent<Dropdown>()); });
        ResetDropDown();
    }

    public void FileSelectedNowEnableUserInterface(bool enableFlag = true)
    {
        lastNameFilterField.interactable = enableFlag;
        transform.GetComponent<Dropdown>().interactable = enableFlag;
    }

    void quitClicked()
    {
        Application.Quit();
    }

    void StartClicked()
    {
        startButton.transform.Find("LoadingCircle").gameObject.SetActive(true);
        Assets.Scripts.CrossSceneInformation.startingDataBaseId = selectedPerson.dataBaseOwnerId;
        Assets.Scripts.CrossSceneInformation.numberOfGenerations = Int32.Parse(generationsDropdown.options[generationsDropdown.value].text);
        Assets.Scripts.CrossSceneInformation.myTribeType = ancestryToggle.isOn ? TribeType.Ancestry : TribeType.Descendancy;
        SceneManager.LoadScene("MyTribeScene"); 
        //StartCoroutine(LoadSceneAsync());
    }

    //private IEnumerator LoadSceneAsync()
    //{
    //    var progress = SceneManager.LoadSceneAsync("MyTribeScene", LoadSceneMode.Single);
    //    while (!progress.isDone)
    //        yield return null;
    //}

    void ToggleControl(Toggle toggleThatToggled)
    {
        if (toggleThatToggled.name.StartsWith("A"))
        {
            descendancyToggle.isOn = !ancestryToggle.isOn;
        }
        if (toggleThatToggled.name.StartsWith("D"))
        {
            ancestryToggle.isOn = !descendancyToggle.isOn;
        }
    }

    void LastNameFilterFieldChanging(InputField input)
    {
        ResetDropDown();
    }

    void ResetDropDown()
    {
        var dropdown = transform.GetComponent<Dropdown>();
        dropdown.options.Clear();
        dropdown.value = 0;
        dropdown.RefreshShownValue();
        dropdown.options.Add(new Dropdown.OptionData() { text = $"Enter Last Name Filter Text, to populate this" });

        searchStatusText.text = "";

        startButton.interactable = false;
    }

    void LastNameFilterFieldEndEdit(InputField input)
    {
        if (input.text.Length > 0)
        {
            Debug.Log($"Text: {input.text} has been entered");
            PopulateDropDownWithMyTribeSubSet(input.text);
            var dropdown = transform.GetComponent<Dropdown>();

            if (myTribeOfPeople.personsList.Count > 0)
            {
                searchStatusText.text = $"{myTribeOfPeople.personsList.Count} Search Results Available.";                
                dropdown.value = 0;
                DropDownItemSelected(dropdown);
                dropdown.Show();
            }
            else
                searchStatusText.text = $"No results available for that search string.";

        }
        else if (input.text.Length == 0)
        {
            Debug.Log("Main Input Empty");
            searchStatusText.text = $"No results available for that search string.";
        }
    }

    void DropDownItemSelected(Dropdown dropdown)
    {
        var index = dropdown.value;
        selectedPerson = myTribeOfPeople.personsList[index];
        startButton.interactable = true;
        searchStatusText.text = $"{selectedPerson.givenName} {selectedPerson.surName} selected. Press Start.";
    }

    void PopulateDropDownWithMyTribeSubSet(string filterText)
    {
        var dropdown = transform.GetComponent<Dropdown>();
        dropdown.options.Clear();
        dropdown.value = 0;        
        dropdown.RefreshShownValue();

        myTribeOfPeople = new ListOfPersonsFromDataBase(Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath);
        myTribeOfPeople.GetListOfPersonsFromDataBaseWithLastNameFilter(numberOfPeopleInTribe, lastNameFilterString: filterText);
        foreach (var person in myTribeOfPeople.personsList)
        {
            dropdown.options.Add(new Dropdown.OptionData() { text = $"{person.surName}, {person.givenName} b{person.birthEventDate} id {person.dataBaseOwnerId}" });
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
