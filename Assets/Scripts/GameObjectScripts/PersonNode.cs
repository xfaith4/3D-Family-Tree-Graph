using Assets.Scripts.Enums;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PersonNode : MonoBehaviour
{
    public float lifeSpan;
    public int birthDate;
    public int deathDate;
    public PersonGenderType personGender;
    public int dataBaseOwnerID;
    public int arrayIndex;
    public (int original, int updated) birthDateQuality;
    public (int original, int updated) deathDateQuality;
    public string dateQualityInformationString = "using original dates, all is good";
    
    GameObject edgePrefabObject;
    float edgePrefabXScale;
    GameObject bubblePrefabObject;
    GameObject capsuleBubblePrefabObject;
    GameObject leftConnection;
    GameObject rightConnection;

    const int PlatformChildIndex = 0;

    private Color[] personGenderCapsuleBubbleColors = {
            new Color(0.4f, 0.4f, 0.4f, 0.7f),   // notset
            new Color(0.4f, 0.7f, 0.9f, 0.7f),   // male
            new Color(0.8f, 0.5f, 0.8f, 0.7f)    // female
        };
    private Color[] personDateQualityColors = {
            new Color(0.4f, 0.4f, 0.4f),   // date = orig
            new Color(0.9f, 0.1f, 0.1f)    // date != orig
        };
    private Color clearWhite = new Color(1.0f, 1.0f, 1.0f, 0.2f);
    private Color[] personGenderPlatformColors = {
            new Color(0.2f, 0.2f, 0.2f),   // notset
            new Color(0.3f, 0.6f, 0.9f),   // male
            new Color(0.7f, 0.4f, 0.9f)    // female
        };
    private Color[] childRelationshipColors = {            
            new Color(0.9f, 0.9f, 0.9f, 0.9f),   // biological
            new Color(0.0f, 0.0f, 0.0f, 0.9f)    // adopted
        };

    void Start()
    {
        //transform.GetChild(TextNodeChildIndex).GetComponent<TextMesh>().text = name;
        transform.GetComponentInChildren<TextMesh>().text = name;
    }

    void Update()
    {
       
    }

    public void SetEdgePrefab(GameObject edge, GameObject bubble, GameObject capsuleBubble, float edgeXScale)
    {
        this.edgePrefabObject = edge;
        this.edgePrefabXScale = edgeXScale;
        this.bubblePrefabObject = bubble;
        this.capsuleBubblePrefabObject = capsuleBubble;
    }

    public void Freeze(bool crazySprings)
    {    
        if (crazySprings)
            this.transform.GetChild(PlatformChildIndex).GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
        else
            this.transform.GetChild(PlatformChildIndex).GetComponent<Rigidbody>().constraints = 
                RigidbodyConstraints.FreezePositionY | 
                RigidbodyConstraints.FreezePositionZ | 
                RigidbodyConstraints.FreezeRotation;
    }

    public void SetIndexes(int dataBaseOwnerId, int arrayIndex)
    {
        this.dataBaseOwnerID = dataBaseOwnerId;
        this.arrayIndex = arrayIndex;
    }

    public void SetLifeSpan(int birthDate, float age)
    {
        var myPlatformComponent = gameObject.transform.GetChild(PlatformChildIndex);
        myPlatformComponent.transform.localScale = new Vector3(1.0f, 1.0f, age);
        myPlatformComponent.transform.localPosition = new Vector3(0, 0, age / 2f);
        lifeSpan = age;
        this.birthDate = birthDate;
        this.deathDate = birthDate + (int)age;
    }

    public void AddDateQualityInformation((int updated, int original) birthDateQuality, (int updated, int original) deathDateQuality, string dateQualityInformationString)
    {
        this.dateQualityInformationString = dateQualityInformationString;
        this.birthDateQuality = birthDateQuality;
        this.deathDateQuality = deathDateQuality;
    }

    public void addMyBirthQualityBubble()
    { 
        var parentPlatformTransform = gameObject.transform.GetChild(PlatformChildIndex);

        var birthConnection = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var colorToSet = personDateQualityColors[birthDateQuality.original == birthDateQuality.updated ? 0 : 1];
        birthConnection.GetComponent<Renderer>().material.SetColor("_Color", colorToSet);

        birthConnection.transform.localScale = Vector3.one * 1.5f;
        birthConnection.transform.parent = parentPlatformTransform;
        birthConnection.transform.localPosition = new Vector3(0, 0, - 0.5f);
    }

    public void SetPersonGender(PersonGenderType personGender)
    {        
        this.personGender = personGender;
        gameObject.transform.GetChild(PlatformChildIndex).GetComponent<Renderer>().material.SetColor("_Color",
            personGenderPlatformColors[(int)personGender]);
    }

    public void AddBirthEdge(PersonNode childPersonNode, float myAgeConnectionPointPercent = 0f, 
        ChildRelationshipType childRelationshipType = ChildRelationshipType.Biological, int birthDate = 0)
    {
        var foo = new Color(0.3f, 0.4f, 0.6f);
        var childAgeConnectionPointPercent = 0f;
        var parentPlatformTransform = gameObject.transform.GetChild(PlatformChildIndex);
        var parentRidgidbodyComponent = parentPlatformTransform.GetComponent<Rigidbody>();
        var childPlatformTransform = childPersonNode.transform.GetChild(PlatformChildIndex);
        var childRidgidbodyComponent = childPlatformTransform.GetComponent<Rigidbody>();
        SpringJoint sj = parentRidgidbodyComponent.gameObject.AddComponent<SpringJoint>();
        sj.autoConfigureConnectedAnchor = false;
        sj.anchor = new Vector3(0, 0.5f, myAgeConnectionPointPercent - 0.5f);
        sj.connectedAnchor = new Vector3(0, 0.5f, childAgeConnectionPointPercent - 0.5f);
        sj.enableCollision = true;
        sj.connectedBody = childRidgidbodyComponent;
        sj.spring = 10.0f;
        //sj.minDistance = 10.0f;
        //sj.maxDistance = 500.0f;

        leftConnection = //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Instantiate(this.capsuleBubblePrefabObject, Vector3.zero, Quaternion.identity);        
        //TODO Twins born at the same time are not handled well if one is a boy and the other a girl
        leftConnection.transform.GetChild(PlatformChildIndex).GetComponent<Renderer>().material.SetColor("_Color", 
            personGenderCapsuleBubbleColors[(int)childPersonNode.personGender]);

        leftConnection.transform.localScale = new Vector3(10.0f, 2.0f, 2.0f);
        //leftConnection.transform.localScale = Vector3.one * 2f;
        leftConnection.transform.parent = parentPlatformTransform;
        leftConnection.transform.localPosition = new Vector3(0, 0, myAgeConnectionPointPercent - 0.5f);

        rightConnection = //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Instantiate(this.bubblePrefabObject, Vector3.zero, Quaternion.identity);
        rightConnection.transform.GetChild(PlatformChildIndex).GetComponent<Renderer>().material.SetColor("_Color", clearWhite);
        rightConnection.transform.localScale = Vector3.one * 2f;
        rightConnection.transform.parent = childPlatformTransform;
        rightConnection.transform.localPosition = new Vector3(0, 0, childAgeConnectionPointPercent - 0.5f);
        //

        GameObject edge = Instantiate(this.edgePrefabObject, Vector3.zero, Quaternion.identity);
        edge.name = $"Birth {birthDate} {childPersonNode.name}";
        edge.GetComponent<Edge>().CreateEdge(leftConnection, rightConnection);
        edge.transform.GetChild(PlatformChildIndex).GetComponent<Renderer>().material.SetColor("_Color",
            childRelationshipColors[(int)childRelationshipType]);

        edge.transform.parent = transform;

    }
    public void AddMarriageEdge(PersonNode spousePersonNode, 
        float myAgeConnectionPointPercent = 0f, 
        float spouseAgeConnectionPointPercent = 0f, int marriageEventDate = 0, int marriageLength = 0)
    {
        var myPlatformTransform = gameObject.transform.GetChild(PlatformChildIndex);
        var myRidgidbodyComponent = myPlatformTransform.GetComponent<Rigidbody>();
        var spousePlatformTransform = spousePersonNode.transform.GetChild(PlatformChildIndex);
        var spouseRidgidbodyComponent = spousePlatformTransform.GetComponent<Rigidbody>();
        SpringJoint sj = myRidgidbodyComponent.gameObject.AddComponent<SpringJoint>();
        sj.autoConfigureConnectedAnchor = false;
        sj.anchor = new Vector3(0, 0.5f, myAgeConnectionPointPercent);   // - 0.5f
        sj.connectedAnchor = new Vector3(0, 0.5f, spouseAgeConnectionPointPercent);  // - 0.5f
        sj.enableCollision = true;
        sj.connectedBody = spouseRidgidbodyComponent;
        sj.spring = 50.0f;
        sj.minDistance = 30.0f;
        sj.maxDistance = 60.0f;

        leftConnection = new GameObject(); // GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //leftConnection.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);

        leftConnection.transform.localScale = Vector3.one * 2f;
        leftConnection.transform.parent = myPlatformTransform;
        leftConnection.transform.localPosition = new Vector3(0, 0, myAgeConnectionPointPercent - 0.5f);

        rightConnection = new GameObject(); // GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //rightConnection.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
        rightConnection.transform.localScale = Vector3.one * 2f;
        rightConnection.transform.parent = spousePlatformTransform;
        rightConnection.transform.localPosition = new Vector3(0, 0, spouseAgeConnectionPointPercent - 0.5f);
        //

        GameObject edge = Instantiate(this.edgePrefabObject, Vector3.zero, Quaternion.identity);
        edge.name = $"Marriage {marriageEventDate}, to {spousePersonNode.name}, duration {marriageLength}.";
        edge.GetComponent<Edge>().CreateEdge(leftConnection, rightConnection);
        edge.GetComponent<Edge>().SetEdgeEventLength(marriageLength, edgePrefabXScale);
        var material = edge.transform.GetChild(PlatformChildIndex).GetComponent<Renderer>().material;
        material.SetColor("_Color", new Color(1.0f, 0.92f, 0.01f,  0.2f));
        //material.SetOverrideTag("RenderMode", "Transparent");

        edge.transform.parent = transform;

    }
    public void AddEdge(PersonNode destinationPersonNode, float myAgeConnectionPointPercent = 0f, float destinationAgeConnectionPointPercent = 0f)
    {
        var myPlatformTransform = gameObject.transform.GetChild(PlatformChildIndex);
        var myRidgidbodyComponent = myPlatformTransform.GetComponent<Rigidbody>();
        var destinationPlatformTransform = destinationPersonNode.transform.GetChild(PlatformChildIndex);
        var destinationRidgidbodyComponent = destinationPlatformTransform.GetComponent<Rigidbody>();
        SpringJoint sj = myRidgidbodyComponent.gameObject.AddComponent<SpringJoint>();
        sj.autoConfigureConnectedAnchor = false;
        sj.anchor = new Vector3(0, 0.5f, myAgeConnectionPointPercent - 0.5f);
        sj.connectedAnchor = new Vector3(0, 0.5f, destinationAgeConnectionPointPercent - 0.5f);
        sj.enableCollision = true;
        sj.connectedBody = destinationRidgidbodyComponent;

        leftConnection = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leftConnection.GetComponent<Renderer>().material.SetColor("_Color", Color.green);

        leftConnection.transform.localScale = Vector3.one * 2f;
        leftConnection.transform.parent = myPlatformTransform;
        leftConnection.transform.localPosition = new Vector3(0, 0, myAgeConnectionPointPercent - 0.5f);

        rightConnection = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        rightConnection.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
        rightConnection.transform.localScale = Vector3.one * 2f;
        rightConnection.transform.parent = destinationPlatformTransform;
        rightConnection.transform.localPosition = new Vector3(0, 0, destinationAgeConnectionPointPercent - 0.5f);
        //

        GameObject edge = Instantiate(this.edgePrefabObject, Vector3.zero, Quaternion.identity);
        edge.GetComponent<Edge>().CreateEdge(leftConnection, rightConnection);
        // edge.transform.parent = transform;

    }
}
