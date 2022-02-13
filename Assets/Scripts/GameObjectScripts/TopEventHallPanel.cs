using Assets.Scripts.DataObjects;
using Assets.Scripts.DataProviders;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class TopEventHallPanel : MonoBehaviour
{    
    public string topEventsDataBaseFileName;
    public Texture2D NoEventsThisYear_Texture;
    public Texture2D NoImageThisEvent_Texture;

    private ListOfTopEventsFromDataBase topEventsDataProvider;
    private List<TopEvent> topEventsForYear;
    private int year;
    private int currentEventIndex = 0;
    private int numberOfEvents = 0;
    private TextMeshPro titleTextFieldName;
    private TextMeshPro descriptionTextFieldName;


    // Awake is called when instantiated
    void Awake()
    {
        var textMeshProObjects = gameObject.GetComponentsInChildren<TextMeshPro>();
        titleTextFieldName = textMeshProObjects[0];
        descriptionTextFieldName = textMeshProObjects[1];
    }

    public void LoadTopEventsForYear_fromDataBase(int year)
    {
        var dataPath = Application.streamingAssetsPath + "/";
        topEventsDataProvider = new ListOfTopEventsFromDataBase(dataPath + topEventsDataBaseFileName);

        this.year = year;
        topEventsDataProvider.GetListOfTopEventsFromDataBase(this.year);
        topEventsForYear = topEventsDataProvider.topEventsList;
        numberOfEvents = topEventsForYear.Count;
        DisplayHallPanelImageTexture();
        titleTextFieldName.text = currentlySelectedEventTitle();
    }

    public void DisplayDescriptionText()
    {
        descriptionTextFieldName.text = currentlySelectedEventDescription();
    }

    public void ClearDescriptionText()
    {
        descriptionTextFieldName.text = null;
    }

    public void DisplayHallPanelImageTexture()
    {
        if (numberOfEvents == 0)
        {
            this.gameObject.GetComponent<Renderer>().material.mainTexture = NoEventsThisYear_Texture;
            return;
        }
        var eventToShow = topEventsForYear[currentEventIndex];
        if (string.IsNullOrEmpty(eventToShow.picture))
        {
            this.gameObject.GetComponent<Renderer>().material.mainTexture = NoImageThisEvent_Texture;
            return;
        }

        StartCoroutine(DownloadImage(eventToShow.picture + "?width=400px"));
    }

    public string currentlySelectedEventDescription()
    {
        if (numberOfEvents == 0)
            return $"Year {year}: No top events this year";

        return topEventsForYear[currentEventIndex].description;
    }

    public string currentlySelectedEventTitle()
    {
        if (numberOfEvents == 0)
            return null;
        var eventTally = numberOfEvents == 0 ? "0 / 0" : $"{currentEventIndex + 1} / {numberOfEvents}";
        var stringToReturn = $"Year {year}, Event {eventTally}";
        stringToReturn += "\n" + topEventsForYear[currentEventIndex].itemLabel;
        if (!String.IsNullOrEmpty(topEventsForYear[currentEventIndex].eventStartDate))
            stringToReturn = stringToReturn + "\n" + JsonConvert.DeserializeObject<DateTime>(("\"" + topEventsForYear[currentEventIndex].eventStartDate + "\"")).ToString("dd MMM yyyy");
        if (!String.IsNullOrEmpty(topEventsForYear[currentEventIndex].eventEndDate))
            stringToReturn = stringToReturn + " - " + JsonConvert.DeserializeObject<DateTime>(("\"" + topEventsForYear[currentEventIndex].eventEndDate + "\"")).ToString("dd MMM yyyy");

        return stringToReturn;
    }

    public void NextEventInPanel()
    {
        currentEventIndex++;
        if (currentEventIndex >= numberOfEvents)
            currentEventIndex = 0;
        DisplayHallPanelImageTexture();
        titleTextFieldName.text = currentlySelectedEventTitle();
    }

    public void PreviousEventInPanel()
    {
        currentEventIndex--;
        if (currentEventIndex < 0)
            currentEventIndex = numberOfEvents - 1;
        DisplayHallPanelImageTexture();
        titleTextFieldName.text = currentlySelectedEventTitle();
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator DownloadImage(string MediaUrl)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(MediaUrl);
        yield return request.SendWebRequest();
        //if (request.isNetworkError || request.isHttpError)
        if (request.result == UnityWebRequest.Result.ProtocolError)
            Debug.Log(request.error);
        else
            this.gameObject.GetComponent<Renderer>().material.mainTexture = ((DownloadHandlerTexture)request.downloadHandler).texture;
    }
}
