using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Enums;
using System;
using Random = UnityEngine.Random;
using Assets.Scripts.DataObjects;
using Assets.Scripts.DataProviders;
using Cinemachine;
using Cinemachine.Examples;
using StarterAssets;
using UnityEngine.SceneManagement;

public class Tribe : MonoBehaviour
{
	private TribeType tribeType;
	private String rootsMagicFileName;
	private bool dataLoadComplete = false;
	private int updateFramesToWaist = 120;
	private int startingIdForTree;
	private int numberOfGenerations = 5;
	public GameObject personPrefab;
	public GameObject playerControllerPrefab;
	public GameObject playerFollowCameraPrefab;
	public GameObject birthConnectionPrefab;
	public GameObject marriageConnectionPrefab;
	public float marriageEdgepfXScale = 0.4f;
	public GameObject bubblepf;
	public GameObject parentPlatformBirthBubble;
	public GameObject childPlatformReturnToParent;
	public int numberOfPeopleInTribe = 1000;
	public GlobalSpringType globalSpringType = GlobalSpringType.Normal;
	public int generationGap;
	public int spouseGap;
	public int personSpacing = 20;

	private int maximumNumberOfPeopleInAGeneration = 0;
	
	private ListOfPersonsFromDataBase[] listOfPersonsPerGeneration = new ListOfPersonsFromDataBase[11];
	private int personOfInterestDepth = 0;
	private int personOfInterestIndexInList = 0;

	const int PlatformChildIndex = 0;
	private ThirdPersonController thirdPersonContollerScript;

	void Start()
	{
		dataLoadComplete = false;
		updateFramesToWaist = 120;
		tribeType = Assets.Scripts.CrossSceneInformation.myTribeType;
		numberOfGenerations = Assets.Scripts.CrossSceneInformation.numberOfGenerations;
		startingIdForTree = Assets.Scripts.CrossSceneInformation.startingDataBaseId;
		rootsMagicFileName = Assets.Scripts.CrossSceneInformation.rootsMagicDataFileNameWithFullPath;

		if (tribeType == TribeType.MadeUpData || rootsMagicFileName == null)
		{
			var adam = CreatePersonGameObject("Adam", PersonGenderType.Male, 10, false,60, xOffset:10, generation: 0);

			//SetDemoMotionMode(adam);

			CreatePlayerOnThisPersonObject(adam);

			var eve = CreatePersonGameObject("Eve", PersonGenderType.Female, 10, false, 60, xOffset: 30, generation: 0);

            CreateMarriage(eve, adam, 30);
			dataLoadComplete = true;
      
        } 		
	}

	void NewUpEnoughListOfPersonsPerGeneration(int numberOfGenerations)
    {
		for(var depth = 0; depth <= numberOfGenerations; depth++)
        {
			listOfPersonsPerGeneration[depth] = new ListOfPersonsFromDataBase(rootsMagicFileName);
        }
    }

	void PositionTimeBarrier()
    {
		var timeBarrierObject = GameObject.FindGameObjectsWithTag("TimeBarrier")[0];
		timeBarrierObject.transform.position = new Vector3(0f, 0f, DateTime.Now.Year + 0.5f);
		timeBarrierObject.transform.localScale = new Vector3((maximumNumberOfPeopleInAGeneration * personSpacing * 10f), 0.1f, (maximumNumberOfPeopleInAGeneration * personSpacing * 10f));
	}

	private IEnumerator GetNextLevelOfAncestryForThisPersonIdDataBaseOnlyAsync(int personId, int depth, float xOffSet, float xRange)
	{
		if (personId == 26)
			Debug.Log("We made it to 26");

		listOfPersonsPerGeneration[depth].GetSinglePersonFromDataBase(personId, generation: depth, xOffSet + xRange / 2, spouseNumber: 0);
		var personWeAreAdding = getPersonForDataBaseOwnerId(personId, depth);

		var listOfFamilyIds = AddParentsAndFixUpDates(personWeAreAdding);
		//yield return new WaitForSeconds(0.1f);
		if (depth > 0)
		{

			var parentCount = listOfFamilyIds.Count;
			var parentIndex = 0;
			foreach (var familyId in listOfFamilyIds)
			{
				var newRange = xRange / parentCount;
				var newOffset = xOffSet + parentIndex * newRange;

				//if (depth > 4)
				//	yield return null; 
				StartCoroutine(GetNextLevelOfAncestryForThisPersonIdDataBaseOnlyAsync(familyId, depth - 1, newOffset, newRange));
				parentIndex++;				
			}

			//Debug.Log($"Depth {depth} complete with {parentCount} parents found for personId {personId}.");
		}
		yield return null;
	}

	private IEnumerator GetNextLevelOfDescendancyForThisPersonIdDataBaseOnlyAsync(int personId, int depth, float xOffSet, float xRange)
	{
		if (personId == 26)
			Debug.Log("We made it to 26");

		listOfPersonsPerGeneration[depth].GetSinglePersonFromDataBase(personId, generation: numberOfGenerations - depth, xOffSet + xRange / 2, spouseNumber: 0);
		var personWeAreAdding = getPersonForDataBaseOwnerId(personId, depth);
		var listOfFamilyIds = AddSpousesAndFixUpDates(personWeAreAdding, depth, xOffSet, xRange);
		//yield return new WaitForSeconds(0.1f);
		if (depth > 0)
		{
			var myChildrenList = new ListOfChildrenFromDataBase(rootsMagicFileName);
			foreach (var familyId in listOfFamilyIds)
				myChildrenList.GetListOfChildrenFromDataBase(familyId);

			var childCount = myChildrenList.childList.Count;
			var childIndex = 0;
			
			foreach (var child in myChildrenList.childList)
			{
				var newRange = xRange / childCount;
				var newOffset = xOffSet + childIndex * newRange;
				//if (depth > 4) 
				//	yield return null; 
				StartCoroutine(GetNextLevelOfDescendancyForThisPersonIdDataBaseOnlyAsync(child.childId, depth - 1, newOffset, newRange));
				childIndex++;		
			}
			//Debug.Log($"Depth {depth} complete with {childCount} children found for personId {personId}.");
		}
		yield return null;
	}

	List<int> AddParentsAndFixUpDates(Person forThisPerson)
    {
		var listOfPersonIdsToReturn = new List<int>();
		var myListOfParentages = new ListOfParentsFromDataBase(rootsMagicFileName);
		myListOfParentages.parentList.Clear();
		myListOfParentages.GetListOfParentsFromDataBase(forThisPerson.dataBaseOwnerId);
        foreach (var parentage in myListOfParentages.parentList)
        {
			if (parentage.fatherId != 0 && !listOfPersonIdsToReturn.Contains(parentage.fatherId))
				listOfPersonIdsToReturn.Add(parentage.fatherId);
			if (parentage.motherId != 0 && !listOfPersonIdsToReturn.Contains(parentage.motherId))
				listOfPersonIdsToReturn.Add(parentage.motherId);
		}
		return listOfPersonIdsToReturn;
	}

	List<int> AddSpousesAndFixUpDates(Person forThisPerson, int depth, float xOffset, float xRange)
    {
		var listOfFamilyIdsToReturn = new List<int>();
		var myListOfMarriages = new ListOfMarriagesForPersonFromDataBase(rootsMagicFileName);
		bool thisIsAHusbandQuery = (forThisPerson.gender == PersonGenderType.Male);
		
		myListOfMarriages.GetListOfMarriagesWithEventsForPersonFromDataBase(forThisPerson.dataBaseOwnerId, useHusbandQuery: thisIsAHusbandQuery);
		int spouseNumber = 1;

		foreach (var marriage in myListOfMarriages.marriageList)
		{
			var spouseIdWeAreAdding = thisIsAHusbandQuery ? marriage.wifeId : marriage.husbandId;

			var spouseXOffset = xOffset + (xRange / (myListOfMarriages.marriageList.Count + 2)) * spouseNumber;
			listOfPersonsPerGeneration[depth].GetSinglePersonFromDataBase(spouseIdWeAreAdding, generation: numberOfGenerations - depth, spouseXOffset, spouseNumber);
			var spousePersonWeAreAdding = getPersonForDataBaseOwnerId(spouseIdWeAreAdding, depth);
			
			forThisPerson.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear, spousePersonWeAreAdding);
			if (spousePersonWeAreAdding != null)
			{
				spousePersonWeAreAdding.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear, forThisPerson);
			}
			else
			{
				var errorPersonId = spouseIdWeAreAdding;
			}
			listOfFamilyIdsToReturn.Add(marriage.familyId);
			spouseNumber++;
		}
		return listOfFamilyIdsToReturn;
	}

	void FixUpDatesBasedOffMarriageDates()
    {
		// Lets fixup some more PersonDates based off off Marriage Event Dates
		var myListOfMarriages = new ListOfMarriagesForPersonFromDataBase(rootsMagicFileName);
		for (var depth = 0; depth <= numberOfGenerations; depth++)
		{
			foreach (var potentialMarriedPerson in listOfPersonsPerGeneration[depth].personsList)
			{
				if (potentialMarriedPerson.gender == PersonGenderType.Male)
				{
					myListOfMarriages.marriageList.Clear();
					myListOfMarriages.GetListOfMarriagesWithEventsForPersonFromDataBase(potentialMarriedPerson.dataBaseOwnerId, useHusbandQuery: true);
					foreach (var marriage in myListOfMarriages.marriageList)
						potentialMarriedPerson.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear, getPersonForDataBaseOwnerId(marriage.wifeId, depth));
				}
				else
				{
					myListOfMarriages.marriageList.Clear();
					myListOfMarriages.GetListOfMarriagesWithEventsForPersonFromDataBase(potentialMarriedPerson.dataBaseOwnerId, useHusbandQuery: false);
					foreach (var marriage in myListOfMarriages.marriageList)
						potentialMarriedPerson.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear, getPersonForDataBaseOwnerId(marriage.husbandId, depth));
				}
			}
		}
	}

	void CreatePersonGameObjectForMyTribeOfPeople(int startingID, GlobalSpringType globalSpringType = GlobalSpringType.Normal)
    {
		maximumNumberOfPeopleInAGeneration = 0;
		for (var depth = 0; depth <= numberOfGenerations; depth++)
		{
			var numberOfPersonsInThisGeneration = listOfPersonsPerGeneration[depth].personsList.Count;
			if (numberOfPersonsInThisGeneration > maximumNumberOfPeopleInAGeneration)
				maximumNumberOfPeopleInAGeneration = numberOfPersonsInThisGeneration;
			//Sort this by personList.xOffset
			var indexIntoPersonsInThisGeneration = 0;
			foreach (var personToAdd in listOfPersonsPerGeneration[depth].personsList.OrderBy(x=>x.xOffset))
			{
				// This two items (along with personToAdd.Generation) help place this personGameObject in x/y space
				personToAdd.numberOfPersonsInThisGeneration = numberOfPersonsInThisGeneration;
				personToAdd.indexIntoPersonsInThisGeneration = indexIntoPersonsInThisGeneration;

				personToAdd.personNodeGameObject = CreatePersonGameObject(personToAdd, globalSpringType);
				if (personToAdd.dataBaseOwnerId == startingID)
					CreatePlayerOnThisPersonObject(personToAdd.personNodeGameObject);
				indexIntoPersonsInThisGeneration++;
			}
		}
	}

	void HookUpTheMarriages()
	{
		// time to hook up some marriages
		var myListOfMarriages = new ListOfMarriagesForPersonFromDataBase(rootsMagicFileName);
		for (var depth = 0; depth <= numberOfGenerations; depth++)
		{
			foreach (var potentialHusbandPerson in listOfPersonsPerGeneration[depth].personsList)
			{
				if (potentialHusbandPerson.gender == PersonGenderType.Male)
				{
					myListOfMarriages.marriageList.Clear();
					myListOfMarriages.GetListOfMarriagesWithEventsForPersonFromDataBase(potentialHusbandPerson.dataBaseOwnerId);
					foreach (var marriage in myListOfMarriages.marriageList)
					{
						int marriageYearToUse = potentialHusbandPerson.FixUpAndReturnMarriageDate(marriage.marriageYear);
						bool divorcedOrAnnuledFlag = false;
						int divorcedOrAnnuledDate = 0;

						if (marriage.divorcedYear != 0 || marriage.annulledYear != 0)
						{
							divorcedOrAnnuledFlag = true;
							if (marriage.divorcedYear != 0)
								divorcedOrAnnuledDate = marriage.divorcedYear;
							if (marriage.annulledYear != 0 && marriage.annulledYear < marriage.divorcedYear)
								divorcedOrAnnuledDate = marriage.annulledYear;
						}
						CreateMarriage(
							getGameObjectForDataBaseOwnerId(marriage.wifeId, depth),
							getGameObjectForDataBaseOwnerId(marriage.husbandId, depth),
							marriageYearToUse,
							divorcedOrAnnuledFlag,
							divorcedOrAnnuledDate);
					}
				}
			}
		}
	}

	void NowAddChildrenAssignments(TribeType tribeType)
	{
		// first came love, then came marriage, then came a baby in a baby carriage
		var myListOfParents = new ListOfParentsFromDataBase(rootsMagicFileName);
		for (var depth = (tribeType == TribeType.Ancestry ? 1 : 0);
			depth <= (tribeType == TribeType.Ancestry ? numberOfGenerations : numberOfGenerations - 1);
			depth++)
		{			
			foreach (var child in listOfPersonsPerGeneration[depth].personsList)
			{
				myListOfParents.parentList.Clear();
				myListOfParents.GetListOfParentsFromDataBase(child.dataBaseOwnerId);

				foreach (var myParents in myListOfParents.parentList)
				{
					AssignParents(
						getGameObjectForDataBaseOwnerId(child.dataBaseOwnerId, 
							depth),
						getGameObjectForDataBaseOwnerId(myParents.motherId,
							tribeType == TribeType.Ancestry ? depth - 1 : depth + 1),
						getGameObjectForDataBaseOwnerId(myParents.fatherId,
							tribeType == TribeType.Ancestry ? depth - 1 : depth + 1),
						myParents.relationToMother,
						myParents.relationToFather);
				}
			}
		}
	}

	GameObject getGameObjectForDataBaseOwnerId(int ownerId, int depth) =>
				listOfPersonsPerGeneration[depth].personsList.Find(x => x.dataBaseOwnerId == ownerId)?.personNodeGameObject;
	Person getPersonForDataBaseOwnerId(int ownerId, int depth) =>
				listOfPersonsPerGeneration[depth].personsList.Find(x => x.dataBaseOwnerId == ownerId);

	GameObject CreatePersonGameObject(string name, PersonGenderType personGender, int birthEventDate,
		bool isLiving = true, int deathEventDate = 0,
		int generation = 0,
		int numberOfPersonsInThisGeneration = 0,
		int indexIntoPersonsInThisGeneration = 0,
		float xOffset = 0.0f,
		int spouseNumber = 0,
		int originalBirthDate = 0, int originalDeathDate = 0, string dateQualityInformationString = "",
		int databaseOwnerArry = 0, int tribeArrayIndex = 0, GlobalSpringType globalSpringType = GlobalSpringType.Normal,
		Person person = null)
    {
		var currentYear = DateTime.Now.Year;
	
		var age = isLiving ? currentYear - birthEventDate : deathEventDate - birthEventDate;

		// old way    var x = xOffset * (maximumNumberOfPeopleInAGeneration * personSpacing);
		var x = indexIntoPersonsInThisGeneration * personSpacing - (numberOfPersonsInThisGeneration * personSpacing) / 2 + personSpacing / 2;
		var y = generation * generationGap + spouseNumber * spouseGap;
		
		var newPersonGameObject = Instantiate(personPrefab, new Vector3(x, y, birthEventDate), Quaternion.identity);		
		newPersonGameObject.transform.parent = transform;
		newPersonGameObject.name = name;
		var personObjectScript = newPersonGameObject.GetComponent<PersonNode>();

		personObjectScript.SetIndexes(databaseOwnerArry, tribeArrayIndex, person);
		personObjectScript.SetLifeSpan(birthEventDate, age, isLiving);
		personObjectScript.AddDateQualityInformation((birthEventDate, originalBirthDate), (deathEventDate, originalDeathDate), dateQualityInformationString);
		personObjectScript.SetPersonGender(personGender);
		personObjectScript.SetEdgePrefab(birthConnectionPrefab, marriageConnectionPrefab, bubblepf, parentPlatformBirthBubble, childPlatformReturnToParent, marriageEdgepfXScale);
		personObjectScript.addMyBirthQualityBubble();
		personObjectScript.SetGlobalSpringType(globalSpringType);
		personObjectScript.SetRootsMagicFileName(rootsMagicFileName);

		//TODO use gender to set the color of the platform	
		//
		return newPersonGameObject;
	}

	void SetDemoMotionMode(GameObject personGameObject)
    {
		personGameObject.GetComponent<PersonNode>().SetDebugAddMotionSetting(true);
	}

	GameObject CreatePlayerOnThisPersonObject(GameObject personGameObject)
    {
		personGameObject.GetComponent<Rigidbody>().isKinematic = true;  // Lets make this on stay put

		GameObject playerGameObject = Instantiate(playerControllerPrefab);

		//playerGameObject.transform.SetParent(personGameObject.transform, false);
		
		//playerGameObject.transform.position = new Vector3(0f, 1f, 0f);

		//playerGameObject.transform.localPosition = new Vector3(0f, 0f, 0f);

		GameObject[] targets = GameObject.FindGameObjectsWithTag("CinemachineTarget");
		GameObject target = targets.FirstOrDefault(t => t.transform.IsChildOf(playerGameObject.transform));
		
		CreatePlayerFollowCameraObject(target);

		thirdPersonContollerScript = playerGameObject.GetComponent<ThirdPersonController>();
		thirdPersonContollerScript.TeleportTo(personGameObject.transform, new Vector3(0,0.5f,0), ticksToHoldHere: 100);

		return playerGameObject;
    }

	private void teleportToNextPersonOfInterest()
	{
		personOfInterestIndexInList++;
		for (var depth = personOfInterestDepth; depth <= numberOfGenerations; depth++)
		{
			for (var index = personOfInterestIndexInList; index < listOfPersonsPerGeneration[depth]?.personsList.Count; index++)
			{
				if (listOfPersonsPerGeneration[depth].personsList[index].dateQualityInformationString != "")
				{
					personOfInterestDepth = depth;
					personOfInterestIndexInList = index;
					
					thirdPersonContollerScript.TeleportTo(listOfPersonsPerGeneration[depth].personsList[index].personNodeGameObject.transform, new Vector3(0, 0.5f, 0), ticksToHoldHere: 100);
					return;
				}
			}
			personOfInterestIndexInList = 0;
		}
		personOfInterestDepth = 0;

		if (listOfPersonsPerGeneration[personOfInterestDepth]?.personsList.Count > personOfInterestIndexInList)
			thirdPersonContollerScript.TeleportTo(listOfPersonsPerGeneration[personOfInterestDepth].personsList[personOfInterestIndexInList].personNodeGameObject.transform, new Vector3(0, 0.5f, 0), ticksToHoldHere: 100);
	}

	private void CreatePlayerFollowCameraObject(GameObject target)
	{
		var playerFollowCameraGameObject = Instantiate(playerFollowCameraPrefab);

		var vCam = playerFollowCameraGameObject.GetComponent<CinemachineVirtualCamera>();
		vCam.Follow = target.transform;
		vCam.LookAt = target.transform;

		var vDistanceModifier = playerFollowCameraGameObject.GetComponent<ThirdPersonFollowDistanceModifier>();
		vDistanceModifier.SetFollow();
	}
		

	GameObject CreatePersonGameObject(Person person, GlobalSpringType globalSpringType = GlobalSpringType.Normal)
	{
		return CreatePersonGameObject($"{person.givenName} {person.surName}", person.gender, person.birthEventDate,
			person.isLiving, person.deathEventDate, person.generation, person.numberOfPersonsInThisGeneration, person.indexIntoPersonsInThisGeneration,
			person.xOffset, person.spouseNumber,
			person.originalBirthEventDateYear, person.originalDeathEventDateYear,
			person.dateQualityInformationString,
			person.dataBaseOwnerId, person.tribeArrayIndex, globalSpringType,
			person);
	}

	void CreateMarriage(GameObject wifePerson, GameObject husbandPerson, int marriageEventDate, bool divorcedFlag= false,int divorcedEventDate=0)
    {
		// We may not have loaded a full set of family information
		// If the husband or wife is not found, skip the marriage
		if (ReferenceEquals(wifePerson, null)
			|| ReferenceEquals(husbandPerson, null))
			return;
		var husbandPersonNode = husbandPerson.GetComponent<PersonNode>();
		var wifePersonNode = wifePerson.GetComponent<PersonNode>();
		var wifeAge = wifePersonNode.lifeSpan;

		// We have some married people with no birthdates
		var wifeAgeAtMarriage = (float)(marriageEventDate - wifePersonNode.birthDate);	
		var husbandAge = husbandPersonNode.lifeSpan;
		// We have some married people with no birthdates
		var husbandAgeAtMarriage = (float)(marriageEventDate - husbandPersonNode.birthDate);
		// TODO does not work for divorcedEventDate = 0
		var marriageLength = divorcedFlag ?
			divorcedEventDate - marriageEventDate : (int)Mathf.Min(wifePersonNode.birthDate + wifeAge, husbandPersonNode.birthDate + husbandAge) - marriageEventDate;
		// Just in case birthdate and ages are zero
		if (marriageLength < 0)
			marriageLength = 1;

		var wifeMarriageConnectionPointPercent = wifeAge != 0f ? wifeAgeAtMarriage / wifeAge : 0.5f;
		var husbandMarriageConnectionPointPercent = husbandAge != 0f ? husbandAgeAtMarriage / husbandAge: 0.5f; 
		
		wifePersonNode.AddMarriageEdge(
			husbandPersonNode, 
			wifeMarriageConnectionPointPercent, 
			husbandMarriageConnectionPointPercent, 
			marriageEventDate,
			marriageLength);
	}

	void AssignParents(GameObject childPerson, GameObject motherPerson, GameObject fatherPerson,
		ChildRelationshipType motherChildRelationshipType = ChildRelationshipType.Biological,
		ChildRelationshipType fatherChildRelationshipType = ChildRelationshipType.Biological)
    {
		var childPersonNode = childPerson.GetComponent<PersonNode>();

		if (motherPerson != null && !motherPerson.Equals(null))
		{
			var motherPersonNode = motherPerson.GetComponent<PersonNode>();
			var motherAge = motherPersonNode.lifeSpan;
			var motherAgeAtChildBirth = (float)(childPersonNode.birthDate - motherPersonNode.birthDate);
			motherPersonNode.AddBirthEdge(childPersonNode, motherAgeAtChildBirth / motherAge, motherChildRelationshipType, childPersonNode.birthDate);
		}

		if (fatherPerson != null && !fatherPerson.Equals(null))
		{
			var fatherPersonNode = fatherPerson.GetComponent<PersonNode>();
			var fatherAge = fatherPersonNode.lifeSpan;
			var fatherAgeAtChildBirth = (float)(childPersonNode.birthDate - fatherPersonNode.birthDate);
			fatherPersonNode.AddBirthEdge(childPersonNode, fatherAgeAtChildBirth / fatherAge, fatherChildRelationshipType, childPersonNode.birthDate);
		}
	}

	void Update() 
	{

		//Detect when the F arrow key is pressed down
		if (Input.GetKeyDown(KeyCode.F))
		{
			Debug.Log("F key was pressed.");
			teleportToNextPersonOfInterest();
		}

		if (dataLoadComplete)
			return;

		if (updateFramesToWaist-- > 0)
			return;

		if (tribeType == TribeType.Ancestry)
		{
			NewUpEnoughListOfPersonsPerGeneration(numberOfGenerations);
			StartCoroutine(GetNextLevelOfAncestryForThisPersonIdDataBaseOnlyAsync(startingIdForTree, numberOfGenerations, xOffSet: 0.0f, xRange: 1.0f));			
			//Debug.Log("We are done with Ancestry Recurrsion.");
			
			FixUpDatesBasedOffMarriageDates();
			//Debug.Log("We are done with Fix Up Dates Based off marriage.");

			CreatePersonGameObjectForMyTribeOfPeople(startingIdForTree, globalSpringType);
			//Debug.Log("We are done with creating game objects.");

			HookUpTheMarriages();
			//Debug.Log("We are done with hooking up marriages.");

			NowAddChildrenAssignments(tribeType);
			//Debug.Log("We are done adding children assignments.");

			PositionTimeBarrier();

			dataLoadComplete = true;
		}
		else if (tribeType == TribeType.Descendancy)
		{
			NewUpEnoughListOfPersonsPerGeneration(numberOfGenerations);
			StartCoroutine(GetNextLevelOfDescendancyForThisPersonIdDataBaseOnlyAsync(startingIdForTree, numberOfGenerations, xOffSet: 0.0f, xRange: 1.0f));
			//Debug.Log("We are done with Descendacy Recurrsion.");

			FixUpDatesBasedOffMarriageDates();
			//Debug.Log("We are done with Fix Up Dates Based off marriage.");

			CreatePersonGameObjectForMyTribeOfPeople(startingIdForTree, globalSpringType);
			//Debug.Log("We are done with creating game objects.");

			HookUpTheMarriages();
			//Debug.Log("We are done with hooking up marriages.");

			NowAddChildrenAssignments(tribeType);
			//Debug.Log("We are done adding children assignments.");

			PositionTimeBarrier();

			dataLoadComplete = true;
		}
	}
}