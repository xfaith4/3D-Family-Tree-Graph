using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Enums;
using System;
using Random = UnityEngine.Random;
using Assets.Scripts.DataObjects;
using Assets.Scripts.DataProviders;

public class Tribe : MonoBehaviour
{

	public String rootsMagicFileName;
	public GameObject personPrefab;
	public GameObject edgepf;
	public GameObject bubblepf;
	public GameObject capsuleBubblepf;
	public int numberOfPeopleInTribe = 1000;
	
	private List<PersonNode> gameObjectNodes = new List<PersonNode>();
	private ListOfPersonsFromDataBase myTribeOfPeople;

	void Start()
	{
		var fruitnames = new string[] { "Apple", "Apricot", "Avocado", "Banana", "Bilberry", "Blackberry", "Blackcurrant", "Blueberry", "Boysenberry", "Currant", "Cherry", "Cherimoya", "Chico fruit", "Cloudberry", "Coconut", "Cranberry", "Cucumber", "Custard apple", "Damson", "Date", "Dragonfruit", "Durian", "Elderberry", "Feijoa", "Fig", "Goji berry", "Gooseberry", "Grape", "Raisin", "Grapefruit", "Guava", "Honeyberry", "Huckleberry", "Jabuticaba", "Jackfruit", "Jambul", "Jujube", "Juniper berry", "Kiwano", "Kiwifruit", "Kumquat", "Lemon", "Lime", "Loquat", "Longan", "Lychee", "Mango", "Mangosteen", "Marionberry", "Melon", "Cantaloupe", "Honeydew", "Watermelon", "Miracle fruit", "Mulberry", "Nectarine", "Nance", "Olive", "Orange", "Blood orange", "Clementine", "Mandarine", "Tangerine", "Papaya", "Passionfruit", "Peach", "Pear", "Persimmon", "Physalis", "Plantain", "Plum", "Prune", "Pineapple", "Plumcot", "Pomegranate", "Pomelo", "Purple mangosteen", "Quince", "Raspberry", "Salmonberry", "Rambutan", "Redcurrant", "Salal berry", "Salak", "Satsuma", "Soursop", "Star fruit", "Solanum", "quitoense", "Strawberry", "Tamarillo", "Tamarind", "Ugli fruit", "Yuzu" };

		if (rootsMagicFileName == null)
		{
			var adam = CreatePersonGameObject("Adam", PersonGenderType.Male, 1800, false, 1870, generation: 0);
			var eve = CreatePersonGameObject("Eve", PersonGenderType.Female, 1810, false, 1860, generation: 0);
			CreateMarriage(eve, adam, 1820);
			var leisha = CreatePersonGameObject("Leisha", PersonGenderType.Female, 1832, false, 1900, generation: 1);
			var bob = CreatePersonGameObject("Bob", PersonGenderType.Male, 1834, false, 1837, generation: 1);
			var grace = CreatePersonGameObject("Grace", PersonGenderType.Female, 1836, false, 1910, generation: 1);
			var olivia = CreatePersonGameObject("Olivia", PersonGenderType.Female, 1837, false, 1920, generation: 1);
			var laura = CreatePersonGameObject("Laura", PersonGenderType.Female, 1840, false, 1930, generation: 1);
			var emily = CreatePersonGameObject("Emily", PersonGenderType.Female, 1842, false, 1940, generation: 1);
			var gary = CreatePersonGameObject("Gary", PersonGenderType.NotSet, 1848, false, 1932, generation: 1);
			AssignParents(leisha, eve, adam);
			AssignParents(bob, eve, adam, ChildRelationshipType.Biological, ChildRelationshipType.Adopted);
			AssignParents(grace, eve, adam);
			AssignParents(olivia, eve, adam);
			AssignParents(laura, eve, adam);
			AssignParents(emily, eve, adam);
			AssignParents(gary, eve, null);

			var vahe = CreatePersonGameObject("Vahe", PersonGenderType.Male, 1822, false, 1880, generation: 1);
			CreateMarriage(leisha, vahe, 1840);
			var kyah = CreatePersonGameObject("Kyah", PersonGenderType.Female, 1841, false, 1980, generation: 2);
			var hanna = CreatePersonGameObject("Hannah", PersonGenderType.Female, 1843, false, 1960, generation: 2);
			var jude = CreatePersonGameObject("Jude", PersonGenderType.Male, 1846, false, 1846, generation: 2);
			var arie = CreatePersonGameObject("Arie", PersonGenderType.Male, 1848, false, 1930, generation: 2);
			AssignParents(kyah, leisha, vahe);
			AssignParents(hanna, leisha, vahe);
			AssignParents(jude, leisha, vahe);
			AssignParents(arie, leisha, vahe);
		}
		else
		{
			myTribeOfPeople = new ListOfPersonsFromDataBase(rootsMagicFileName);
			myTribeOfPeople.GetListOfPersonsFromDataBase(numberOfPeopleInTribe);
			// Lets fixup some more PersonDates based off off Marriage Event Dates
			var myListOfMarriages = new ListOfMarriagesForPersonFromDataBase(rootsMagicFileName);
			foreach (var potentialMarriedPerson in myTribeOfPeople.personsList)
			{
				if (potentialMarriedPerson.gender == PersonGenderType.Male)
				{
					myListOfMarriages.marriageList.Clear();
					myListOfMarriages.GetListOfMarriagesForPersonFromDataBase(potentialMarriedPerson.dataBaseOwnerId, useHusbandQuery: true);
					foreach (var marriage in myListOfMarriages.marriageList)
						potentialMarriedPerson.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear);
				}
				else
				{
					myListOfMarriages.marriageList.Clear();
					myListOfMarriages.GetListOfMarriagesForPersonFromDataBase(potentialMarriedPerson.dataBaseOwnerId, useHusbandQuery: false);
					foreach (var marriage in myListOfMarriages.marriageList)
						potentialMarriedPerson.FixUpDatesForViewingWithMarriageDate(marriage.marriageYear);
				}
			}
			#region Create Person GameObjects
			int counter = 0;
			int generationWith = (int)Math.Sqrt((double)numberOfPeopleInTribe);
            foreach (var personToAdd in myTribeOfPeople.personsList)
            {
				personToAdd.personNodeGameObject = CreatePersonGameObject(personToAdd, generation: counter / generationWith);
				counter++;
			}
            #endregion
            // time to hook up some marriages
            foreach (var potentialHusbandPerson in myTribeOfPeople.personsList)
            {
                if (potentialHusbandPerson.gender == PersonGenderType.Male)
                {
                    myListOfMarriages.marriageList.Clear();
                    myListOfMarriages.GetListOfMarriagesForPersonFromDataBase(potentialHusbandPerson.dataBaseOwnerId);
                    foreach (var marriage in myListOfMarriages.marriageList)
                    {
                        int marriageYearToUse = potentialHusbandPerson.FixUpAndReturnMarriageDate(marriage.marriageYear);

                        CreateMarriage(
                            getGameObjectForDataBaseOwnerId(marriage.wifeId),
                            getGameObjectForDataBaseOwnerId(marriage.husbandId),
                            marriageYearToUse);
                    }
                }
            }

            GameObject getGameObjectForDataBaseOwnerId(int ownerId) => 
				myTribeOfPeople.personsList.Find(x => x.dataBaseOwnerId == ownerId)?.personNodeGameObject;           
		}
	}

	GameObject CreatePersonGameObject(string name, PersonGenderType personGender, int birthEventDate,
		bool livingFlag = true, int deathEventDate = 0, int generation = 0,
		int originalBirthDate = 0, int originalDeathDate = 0, string dateQualityInformationString = "",
		int databaseOwnerArry = 0, int tribeArrayIndex = 0)
    {
		var currentYear = DateTime.Now.Year;
	
		var age = livingFlag ? currentYear - birthEventDate : deathEventDate - birthEventDate;

		var x = (Random.value - 0.5f) * 2000;
		var y = (Random.value * negativeOffsetForMales(personGender)) * 30 + generation * 60;
		
		var newPersonGameObject = Instantiate(personPrefab, new Vector3(x, y, birthEventDate), Quaternion.identity);		
		newPersonGameObject.transform.parent = transform;
		newPersonGameObject.name = name;
		var personObjectScript = newPersonGameObject.GetComponent<PersonNode>();

		personObjectScript.SetIndexes(databaseOwnerArry, tribeArrayIndex);
		personObjectScript.SetLifeSpan(birthEventDate, age);
		personObjectScript.AddDateQualityInformation((birthEventDate, originalBirthDate), (deathEventDate, originalDeathDate), dateQualityInformationString);
		personObjectScript.SetPersonGender(personGender);
		personObjectScript.SetEdgePrefab(edgepf, bubblepf, capsuleBubblepf);
		personObjectScript.addMyBirthQualityBubble();
		//TODO use gender to set the color of the platform	
		//
		return newPersonGameObject;

		// generally have the males for this generation lower then the females
		float negativeOffsetForMales(PersonGenderType gender) =>
			gender == PersonGenderType.Male ? -1f : 1f;

	}

	GameObject CreatePersonGameObject(Person person, int generation = 0)
	{
		return CreatePersonGameObject($"{person.givenName} {person.surName}", person.gender, person.birthEventDate,
			person.isLiving, person.deathEventDate, generation,
			person.originalBirthEventDate, person.originalDeathEventDate,
			person.dateQualityInformationString,
			person.dataBaseOwnerId, person.tribeArrayIndex);
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
			motherPersonNode.AddBirthEdge(childPersonNode, motherAgeAtChildBirth / motherAge, motherChildRelationshipType);
		}

		if (fatherPerson != null && !fatherPerson.Equals(null))
		{
			var fatherPersonNode = fatherPerson.GetComponent<PersonNode>();
			var fatherAge = fatherPersonNode.lifeSpan;
			var fatherAgeAtChildBirth = (float)(childPersonNode.birthDate - fatherPersonNode.birthDate);
			fatherPersonNode.AddBirthEdge(childPersonNode, fatherAgeAtChildBirth / fatherAge, fatherChildRelationshipType);
		}
	}

	void Update() { }

}