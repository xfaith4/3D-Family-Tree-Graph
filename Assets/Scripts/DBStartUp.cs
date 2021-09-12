using UnityEngine;
using System.Collections;
using Mono.Data.Sqlite;
using System.Data;
using System;
using Assets.Scripts.DataBase.DBObjects;
using System.Collections.Generic;


public class DBStartUp : MonoBehaviour {

    public UnityEngine.Object MyPersonPlatformObject;
    public string RootsMagicFileName = ""; 
    //this sets up the over all scaling of our world
    public float PersonWidth = 50.0f;  // The width of a person platform
    public float InterPersonSpacing = 5.0f;  // The spacing between person platforms
    public float ZScale = 2.0f;  // Number of meters per year

    //private GUIManager gui;

    private void Awake()
    {
        // Let the GUI Manager know about our ZScale
        //gui = FindObjectOfType(typeof(GUIManager)) as GUIManager;
        //gui.SendMessage("myInitZScale", ZScale);
    }

    void Start () {
        var myNameList = new List<NameSimple>();

        string conn = "URI=file:" + Application.dataPath + "/RootsMagic/Huskey08D.rmgc";
        IDbConnection dbconn;
        dbconn = (IDbConnection)new SqliteConnection(conn);        
        dbconn.Open();
        IDbCommand dbcmd = dbconn.CreateCommand();
        //const string QUERYALL = "SELECT * FROM NameTable";
        const string QUERYNAMES = "SELECT name.OwnerID," +
            "case when Sex = 0 then 'M' when Sex = 1 then 'F' else 'U' end," +
            "name.Given, name.Surname, name.BirthYear, name.DeathYear " +
            "FROM NameTable name " +
            "JOIN PersonTable person " +
            "on name.OwnerID = person.PersonID";
        string sqlQuery = QUERYNAMES;
        dbcmd.CommandText = sqlQuery;
        IDataReader reader = dbcmd.ExecuteReader();
        while (reader.Read())
        {
            var nextName = new NameSimple(reader.GetInt32(0),
                                            reader.GetString(1)[0],
                                            reader.GetString(2),
                                            reader.GetString(3),
                                            reader.GetInt32(4),
                                            reader.GetInt32(5));
            myNameList.Add(nextName);

            Debug.Log("nameId= "+nextName._ownerId +
                ", sex= " + nextName._sex +
                ", given= " + nextName._given +
                ", surname= " + nextName._surname +
                ", birthyear= " + nextName._birthYear +
                ", deathyear= " + nextName._deathYear 
                );
        }
        reader.Close();
        reader = null;
        dbcmd.Dispose();
        dbcmd = null;
        dbconn.Close();
        dbconn = null;

        float personXLocation = 0.0f;
        foreach (var nameSimple in myNameList)
        {
            GameObject newPersonPlatform =
                        (GameObject)
                            Instantiate(MyPersonPlatformObject, new Vector3(personXLocation, 0.0f, 0.0f), transform.rotation);

            newPersonPlatform.SendMessage("myInit", nameSimple);
            personXLocation += InterPersonSpacing;
        }
    }
	
	// Update is called once per frame
	void Update () {
	
	}
}
