using Assets.Scripts.DataObjects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PersonDetailsHandler : MonoBehaviour
{
    public Person personObject;
    public GameObject imageGameObject;
    public GameObject nameGameObject;
    public GameObject birthGameObject;
    public GameObject deathGameObject;
    public GameObject generationGameObject;
    public GameObject currentDateObject;
    public GameObject currentAgeObject;
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
        nameGameObject.GetComponent<Text>().text = (personObject == null) ? "" : personObject.givenName + " " + personObject.surName;
        birthGameObject.GetComponent<Text>().text = (personObject == null) ? "" : $"Birth: {personObject.birthEventDate}";
        deathGameObject.GetComponent<Text>().text = (personObject == null) ? "" : personObject.isLiving ? "Living" : $"Death: {personObject.deathEventDate}";
        generationGameObject.GetComponent<Text>().text = (personObject == null) ? "" : $"Generation: {personObject.generation}";
        UpdateCurrentDate(currentDate);
        recordIdGameObject.GetComponent<Text>().text = (personObject == null) ? "" : $"RootsMagic DB ID: {personObject.dataBaseOwnerId}";
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
