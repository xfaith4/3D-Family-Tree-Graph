using Assets.Scripts.Enums;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PersonNode : MonoBehaviour
{
    public float lifeSpan;
    public int birthDate;
    public int endOfPlatformDate;
    public bool isLiving;
    public PersonGenderType personGender;
    public int dataBaseOwnerID;
    public int arrayIndex;
    public (int original, int updated) birthDateQuality;
    public (int original, int updated) deathDateQuality;
    public string dateQualityInformationString = "using original dates, all is good";

    private bool debugAddMotion = false;
    private Rigidbody myRigidbody;

    GameObject birthConnectionPrefabObject;
    GameObject marriageConnectionPrefabObject;

    float marriageConnectionXScale;
    GameObject bubblePrefabObject;
    GameObject capsuleBubblePrefabObject;
    GameObject leftConnection;
    GameObject rightConnection;

    const int ScaleThisChildIndex = 0;

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
        if (debugAddMotion)
        {
            myRigidbody = this.transform.GetComponent<Rigidbody>();
            var vVelocityDirection = new Vector3(-1, 0, 0);
            myRigidbody.velocity = vVelocityDirection * 1f;
        }
    }

    public void SetEdgePrefab(GameObject birthConnectionPrefab, GameObject marriageConnectionPrefab, GameObject bubble, GameObject capsuleBubble, float edgeXScale)
    {
        this.birthConnectionPrefabObject = birthConnectionPrefab;
        this.marriageConnectionPrefabObject = marriageConnectionPrefab;
        this.marriageConnectionXScale = edgeXScale;
        this.bubblePrefabObject = bubble;
        this.capsuleBubblePrefabObject = capsuleBubble;
    }

    public void SetGlobalSpringType(GlobalSpringType globalSpringType)
    {
        if (globalSpringType == GlobalSpringType.Crazy)
        {
            this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
            return;
        }
        if (globalSpringType == GlobalSpringType.Normal)
        {
            this.transform.GetComponent<Rigidbody>().constraints =
                RigidbodyConstraints.FreezePositionY |
                RigidbodyConstraints.FreezePositionZ |
                RigidbodyConstraints.FreezeRotation;
            return;
        }
        if (globalSpringType == GlobalSpringType.Freeze)
        {
            this.transform.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
        }
    }

    public void SetIndexes(int dataBaseOwnerId, int arrayIndex)
    {
        this.dataBaseOwnerID = dataBaseOwnerId;
        this.arrayIndex = arrayIndex;
    }

    public void SetLifeSpan(int birthDate, float age, bool isLiving)
    {
        var myScaleThisPlatformComponent = gameObject.transform.GetChild(ScaleThisChildIndex);
        myScaleThisPlatformComponent.transform.localScale = new Vector3(1.0f, 1.0f, age);
        //myPlatformComponent.transform.localPosition = new Vector3(0, 0, age / 2f);
        lifeSpan = age;
        this.birthDate = birthDate;
        this.endOfPlatformDate = birthDate + (int)age;
        this.isLiving = isLiving;
    }

    public void AddDateQualityInformation((int updated, int original) birthDateQuality, (int updated, int original) deathDateQuality, string dateQualityInformationString)
    {
        this.dateQualityInformationString = dateQualityInformationString;
        this.birthDateQuality = birthDateQuality;
        this.deathDateQuality = deathDateQuality;
    }

    public void addMyBirthQualityBubble()
    {
        var myScaleThisPlatformTransform = gameObject.transform.GetChild(ScaleThisChildIndex);

        var birthConnection = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        var colorToSet = personDateQualityColors[birthDateQuality.original == birthDateQuality.updated ? 0 : 1];
        birthConnection.GetComponent<Renderer>().material.SetColor("_Color", colorToSet);

        birthConnection.transform.localScale = Vector3.one * 1.5f;
        birthConnection.transform.parent = myScaleThisPlatformTransform;
        birthConnection.transform.localPosition = new Vector3(0, 0, 0);
    }

    public void SetPersonGender(PersonGenderType personGender)
    {
        this.personGender = personGender;
        //  var renderer = gameObject.GetComponentsInChildren<Renderer>().Where(r => r.CompareTag("GenderColor")).ToArray()[0];
        var renderer = gameObject.GetComponentInChildren<Renderer>();
        renderer.material.SetColor("_Color", personGenderPlatformColors[(int)personGender]);
    }

    public void AddBirthEdge(PersonNode childPersonNode, float myAgeConnectionPointPercent = 0f,
        ChildRelationshipType childRelationshipType = ChildRelationshipType.Biological, int birthDate = 0)
    {
        var foo = new Color(0.3f, 0.4f, 0.6f);
        var childAgeConnectionPointPercent = 0f;
        var parentPlatformTransform = gameObject.transform;
        var parentRidgidbodyComponent = parentPlatformTransform.transform.GetComponent<Rigidbody>();
        var childPlatformTransform = childPersonNode.transform;
        var childRidgidbodyComponent = childPlatformTransform.transform.GetComponent<Rigidbody>();
        SpringJoint sj = parentRidgidbodyComponent.gameObject.AddComponent<SpringJoint>();
        sj.autoConfigureConnectedAnchor = false;
        sj.anchor = new Vector3(0, 0.5f, myAgeConnectionPointPercent);
        sj.connectedAnchor = new Vector3(0, 0.5f, childAgeConnectionPointPercent);
        sj.enableCollision = true;
        sj.connectedBody = childRidgidbodyComponent;
        sj.spring = 1f; // 10.0f;
        //sj.minDistance = 10.0f;
        //sj.maxDistance = 500.0f;

        leftConnection = //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Instantiate(this.capsuleBubblePrefabObject, Vector3.zero, Quaternion.identity);
        //TODO Twins born at the same time are not handled well if one is a boy and the other a girl
        var renderer = leftConnection.GetComponentInChildren<Renderer>(); //.Where(r => r.CompareTag("GenderColor")).ToArray()[0];
        renderer.material.SetColor("_Color",
            personGenderCapsuleBubbleColors[(int)childPersonNode.personGender]);

        leftConnection.transform.localScale = new Vector3(10.0f, 2.0f, 2.0f);
        //leftConnection.transform.localScale = Vector3.one * 2f;
        leftConnection.transform.parent = parentPlatformTransform.GetChild(0);  // Point to the ScaleThis Section
        leftConnection.transform.localPosition = new Vector3(0, 0, myAgeConnectionPointPercent);

        rightConnection = //GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Instantiate(this.bubblePrefabObject, Vector3.zero, Quaternion.identity);
        renderer = rightConnection.GetComponentInChildren<Renderer>(); //.Where(r => r.CompareTag("GenderColor")).ToArray()[0];
        renderer.material.SetColor("_Color", clearWhite);
        rightConnection.transform.localScale = Vector3.one * 2f;
        rightConnection.transform.parent = childPlatformTransform.GetChild(0); // Point to the ScaleThis Section
        rightConnection.transform.localPosition = new Vector3(0, 0, childAgeConnectionPointPercent);
        //

        GameObject edge = Instantiate(this.birthConnectionPrefabObject, Vector3.zero, Quaternion.identity);
        edge.name = $"Birth {birthDate} {childPersonNode.name}";
        edge.GetComponent<Edge>().CreateEdge(leftConnection, rightConnection);

        edge.transform.GetChild(ScaleThisChildIndex).GetComponent<Renderer>().material.SetColor("_Color",
            childRelationshipColors[(int)childRelationshipType]);

        edge.transform.parent = transform;
    }

    public void AddMarriageEdge(PersonNode spousePersonNode,
        float myAgeConnectionPointPercent = 0f,
        float spouseAgeConnectionPointPercent = 0f, int marriageEventDate = 0, int marriageLength = 0)
    {
        var myPositionThisPlatformTransform = gameObject.transform;
        var myRidgidbodyComponent = myPositionThisPlatformTransform.GetComponent<Rigidbody>();
        var spousePositionThisPlatformTransform = spousePersonNode.transform;
        var spouseRidgidbodyComponent = spousePositionThisPlatformTransform.GetComponent<Rigidbody>();
        SpringJoint sj = myRidgidbodyComponent.gameObject.AddComponent<SpringJoint>();
        sj.autoConfigureConnectedAnchor = false;
        sj.anchor = new Vector3(0, 0.5f, myAgeConnectionPointPercent);   // - 0.5f
        sj.connectedAnchor = new Vector3(0, 0.5f, spouseAgeConnectionPointPercent);  // - 0.5f
        sj.enableCollision = true;
        sj.connectedBody = spouseRidgidbodyComponent;
        sj.spring = 5f; // 50.0f;
        sj.minDistance = 10.0f;
        sj.maxDistance = 40.0f;

        leftConnection = new GameObject(); // GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //leftConnection.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);

        leftConnection.transform.localScale = Vector3.one * 2f;
        leftConnection.transform.parent = myPositionThisPlatformTransform.GetChild(0);  // Point to the ScaleThis Section
        leftConnection.transform.localPosition = new Vector3(0, 0, myAgeConnectionPointPercent);

        rightConnection = new GameObject(); // GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //rightConnection.GetComponent<Renderer>().material.SetColor("_Color", Color.yellow);
        rightConnection.transform.localScale = Vector3.one * 2f;
        rightConnection.transform.parent = spousePositionThisPlatformTransform.GetChild(0);  // Point to the ScaleThis Section
        rightConnection.transform.localPosition = new Vector3(0, 0, spouseAgeConnectionPointPercent);
        //

        GameObject edge = Instantiate(this.marriageConnectionPrefabObject, Vector3.zero, Quaternion.identity);
        edge.name = $"Marriage {marriageEventDate}, to {spousePersonNode.name}, duration {marriageLength}.";
        edge.GetComponent<Edge>().CreateEdge(leftConnection, rightConnection);
        edge.GetComponent<Edge>().SetEdgeEventLength(marriageLength, marriageConnectionXScale);
        var material = edge.transform.GetChild(0).GetComponent<Renderer>().material;
        material.SetColor("_Color", new Color(1.0f, 0.92f, 0.01f, 0.2f));
        //material.SetOverrideTag("RenderMode", "Transparent");

        edge.transform.parent = myPositionThisPlatformTransform;
    }

    public void SetDebugAddMotionSetting(bool newDebugAddMotionSetting)
    {
        this.debugAddMotion = newDebugAddMotionSetting;
    }
}