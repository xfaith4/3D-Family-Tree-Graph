using Assets.Scripts.DataObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonDetailsHandler : MonoBehaviour
{
    public Sprite femaleImage;
    public Sprite maleImage;
    public Sprite unknownGenderImage;
    public Person personObject;
    public GameObject imageGameObject;
    public GameObject nameGameObject;
    public GameObject birthGameObject;
    public GameObject deathGameObject;
    public GameObject generationGameObject;
    public GameObject currentDateObject;
    public GameObject currentAgeObject;
    public GameObject dateQualityInformationGameObject;
    public GameObject recordIdGameObject;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ClearPersonDisplay()
    {
        personObject = null;
        DisplayThisPerson(personObject);
    }

    public void DisplayThisPerson(Person personToDisplay, int currentDate = 0)
    {
        personObject = personToDisplay;
        var tempBirthDate = (personObject == null) ? "" : $"{personObject.originalBirthEventDateMonth}/{personObject.originalBirthEventDateDay}/{personObject.originalBirthEventDateYear}";
        var tempDeathDate = (personObject == null) ? "" : $"{personObject.originalDeathEventDateMonth}/{personObject.originalDeathEventDateDay}/{personObject.originalDeathEventDateYear}";
        
        nameGameObject.GetComponent<Text>().text = (personObject == null) ? "" : personObject.givenName + " " + personObject.surName;
        birthGameObject.GetComponent<Text>().text = (personObject == null) ? "" : $"Birth: {personObject.birthEventDate}, orig: {tempBirthDate}";
        deathGameObject.GetComponent<Text>().text = (personObject == null) ? "" : personObject.isLiving ? "Living" : $"Death: {personObject.deathEventDate}, orig: {tempDeathDate}";
        generationGameObject.GetComponent<Text>().text = (personObject == null) ? "" : $"Generation: {personObject.generation}";
        UpdateCurrentDate(currentDate);
        dateQualityInformationGameObject.GetComponent<Text>().text = (personObject == null) ? "" : personObject.dateQualityInformationString;
        recordIdGameObject.GetComponent<Text>().text = (personObject == null) ? "" : $"RootsMagic DB ID: {personObject.dataBaseOwnerId}";
        imageGameObject.GetComponent<Image>().sprite = (personObject == null) ? unknownGenderImage :
            personObject.gender == Assets.Scripts.Enums.PersonGenderType.Male ? maleImage : 
            personObject.gender == Assets.Scripts.Enums.PersonGenderType.Female ? femaleImage : unknownGenderImage;
    }

    public void UpdateCurrentDate(int currentDate)
    {
        currentDateObject.GetComponent<Text>().text = (personObject == null) ? "" : $"Current Date: {currentDate}";
        currentAgeObject.GetComponent<Text>().text = (personObject == null) ? "" : $"Current Age: {Mathf.Max(0f, (currentDate - personObject.birthEventDate))}";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
