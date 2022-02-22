using Assets.Scripts.DataObjects;
using Assets.Scripts.DataProviders;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HallOfHistory : MonoBehaviour
{
    public PersonNode focusPerson;
    public PersonNode previousFocusPerson;
    public GameObject topEventHallPanelPrefab;

    // Start is called before the first frame update
    void Start()
    {
    }

    public void SetFocusPersonNode(PersonNode newfocusPerson)
    {
        if (previousFocusPerson != null && newfocusPerson.dataBaseOwnerID == previousFocusPerson.dataBaseOwnerID)
            return;

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        previousFocusPerson = newfocusPerson;
        focusPerson = newfocusPerson;

        var birthDate = focusPerson.birthDate;
        var lifeSpan = focusPerson.lifeSpan;
        var x = focusPerson.transform.position.x;
        var y = focusPerson.transform.position.y;


        for (int age = 0; age < lifeSpan; age++)
        {
            GameObject newPanel = Instantiate(topEventHallPanelPrefab, new Vector3(x + 5.5f, y + 2f, (birthDate + age) * 5 + 2.5f), Quaternion.Euler(90, -180, -90));

            newPanel.transform.parent = transform;
            newPanel.name = $"HistoryPanelfor{birthDate+age}";

            var topEventHallPanelScript = newPanel.GetComponent<TopEventHallPanel>();
            topEventHallPanelScript.LoadTopEventsForYear_fromDataBase(birthDate + age);   
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
