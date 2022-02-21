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
    public Texture2D noEventsThisYear_Texture;
    public Texture2D noImageThisEvent_Texture;
    
    private ListOfTopEventsFromDataBase topEventsDataProvider;
    private List<TopEvent> topEventsForYear;
    private int year;
    private int currentEventIndex = 0;
    private int numberOfEvents = 0;
    private TextMeshPro dateTextFieldName;
    private TextMeshPro titleTextFieldName;
    private EventDetailsHandler eventDetailsHandlerScript;
    private Texture2D eventImage_Texture;

    // Awake is called when instantiated
    void Awake()
    {
        var textMeshProObjects = gameObject.GetComponentsInChildren<TextMeshPro>();

        dateTextFieldName = textMeshProObjects[0];
        titleTextFieldName = textMeshProObjects[1];
        eventImage_Texture = noImageThisEvent_Texture;
    }

    private void Start()
    {
        GameObject[] eventDetailsPanel = GameObject.FindGameObjectsWithTag("EventDetailsPanel");
        eventDetailsHandlerScript = eventDetailsPanel[0].transform.GetComponent<EventDetailsHandler>();
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
        dateTextFieldName.text = year.ToString();
        titleTextFieldName.text = currentlySelectedEventTitle();
    }

    public void DisplayDetailsInEventDetailsPanel()
    {
        eventDetailsHandlerScript.DisplayThisEvent(topEventsForYear[currentEventIndex],
                                                   currentEventIndex,
                                                   numberOfEvents,
                                                   eventImage_Texture);
    }

    public void ClearEventDetailsPanel()
    {
        eventDetailsHandlerScript.ClearEventDisplay();
    }

    public void DisplayHallPanelImageTexture()
    {
        if (numberOfEvents == 0)
        {
            setPanelTexture(noEventsThisYear_Texture);
            return;
        }
        var eventToShow = topEventsForYear[currentEventIndex];
        if (string.IsNullOrEmpty(eventToShow.picture))
        {
            setPanelTexture(noImageThisEvent_Texture);
            return;
        }

        StartCoroutine(DownloadImage(eventToShow.picture + "?width=400px"));
    }

    public string currentlySelectedEventTitle()
    {
        if (numberOfEvents == 0)
            return $"Year {year}: No events.";
        var stringToReturn = topEventsForYear[currentEventIndex].itemLabel;
        if (string.IsNullOrEmpty(stringToReturn))
            return "No title found for this event";
        return stringToReturn[0].ToString().ToUpper() + stringToReturn.Substring(1);
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
            setPanelTexture(((DownloadHandlerTexture)request.downloadHandler).texture);
    }

    void setPanelTexture(Texture textureToSet)
    {
        this.gameObject.transform.Find("ImagePanel").GetComponent<Renderer>().material.mainTexture = textureToSet;
        eventImage_Texture = (Texture2D)textureToSet;
    }
}
