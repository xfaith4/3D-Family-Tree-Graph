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
	public int numberOfNodes = 5;
	[SerializeField] [Tooltip("0.0 to 1.0")] public float chanceOfAnEdge = 0.5f;
	public float graphSize = 10f;	
	private List<PersonNode> gameObjectNodes = new List<PersonNode>();
	private ListOfPersonsFromDataBase myTribeOfPeople;

	void Start()
	{
		var fruitnames = new string[] { "Apple", "Apricot", "Avocado", "Banana", "Bilberry", "Blackberry", "Blackcurrant", "Blueberry", "Boysenberry", "Currant", "Cherry", "Cherimoya", "Chico fruit", "Cloudberry", "Coconut", "Cranberry", "Cucumber", "Custard apple", "Damson", "Date", "Dragonfruit", "Durian", "Elderberry", "Feijoa", "Fig", "Goji berry", "Gooseberry", "Grape", "Raisin", "Grapefruit", "Guava", "Honeyberry", "Huckleberry", "Jabuticaba", "Jackfruit", "Jambul", "Jujube", "Juniper berry", "Kiwano", "Kiwifruit", "Kumquat", "Lemon", "Lime", "Loquat", "Longan", "Lychee", "Mango", "Mangosteen", "Marionberry", "Melon", "Cantaloupe", "Honeydew", "Watermelon", "Miracle fruit", "Mulberry", "Nectarine", "Nance", "Olive", "Orange", "Blood orange", "Clementine", "Mandarine", "Tangerine", "Papaya", "Passionfruit", "Peach", "Pear", "Persimmon", "Physalis", "Plantain", "Plum", "Prune", "Pineapple", "Plumcot", "Pomegranate", "Pomelo", "Purple mangosteen", "Quince", "Raspberry", "Salmonberry", "Rambutan", "Redcurrant", "Salal berry", "Salak", "Satsuma", "Soursop", "Star fruit", "Solanum", "quitoense", "Strawberry", "Tamarillo", "Tamarind", "Ugli fruit", "Yuzu" };

		if (rootsMagicFileName == null)
		{
			var adam = CreatePerson("Adam", PersonGenderType.Male, 1800, false, 1870, generation: 0);
			var eve = CreatePerson("Eve", PersonGenderType.Female, 1810, false, 1860, generation: 0);
			CreateMarriage(eve, adam, 1820);
			var leisha = CreatePerson("Leisha", PersonGenderType.Female, 1832, false, 1900, generation: 1);
			var bob = CreatePerson("Bob", PersonGenderType.Male, 1834, false, 1837, generation: 1);
			var grace = CreatePerson("Grace", PersonGenderType.Female, 1836, false, 1910, generation: 1);
			var olivia = CreatePerson("Olivia", PersonGenderType.Female, 1837, false, 1920, generation: 1);
			var laura = CreatePerson("Laura", PersonGenderType.Female, 1840, false, 1930, generation: 1);
			var emily = CreatePerson("Emily", PersonGenderType.Female, 1842, false, 1940, generation: 1);
			var gary = CreatePerson("Gary", PersonGenderType.NotSet, 1848, false, 1932, generation: 1);
			AssignParents(leisha, eve, adam);
			AssignParents(bob, eve, adam, ChildRelationshipType.Biological, ChildRelationshipType.Adopted);
			AssignParents(grace, eve, adam);
			AssignParents(olivia, eve, adam);
			AssignParents(laura, eve, adam);
			AssignParents(emily, eve, adam);
			AssignParents(gary, eve, null);

			var vahe = CreatePerson("Vahe", PersonGenderType.Male, 1822, false, 1880, generation: 1);
			CreateMarriage(leisha, vahe, 1840);
			var kyah = CreatePerson("Kyah", PersonGenderType.Female, 1841, false, 1980, generation: 2);
			var hanna = CreatePerson("Hannah", PersonGenderType.Female, 1843, false, 1960, generation: 2);
			var jude = CreatePerson("Jude", PersonGenderType.Male, 1846, false, 1846, generation: 2);
			var arie = CreatePerson("Arie", PersonGenderType.Male, 1848, false, 1930, generation: 2);
			AssignParents(kyah, leisha, vahe);
			AssignParents(hanna, leisha, vahe);
			AssignParents(jude, leisha, vahe);
			AssignParents(arie, leisha, vahe);
		}
		else
		{
			myTribeOfPeople = new ListOfPersonsFromDataBase(rootsMagicFileName);
			myTribeOfPeople.GetListOfPersonsFromDataBase(1000);
			int counter = 0;
			int generationWith = 50;
            foreach (var personToAdd in myTribeOfPeople.PersonsList)
            {
				CreatePerson(personToAdd, generation: counter / generationWith);
				counter++;
			}
		}
	}

	GameObject CreatePerson(string name, PersonGenderType personGender, int birthEventDate,
		bool livingFlag = true, int deathEventDate = 0, int generation = 0,
		int databaseOwnerArry = 0, int tribeArrayIndex = 0)
    {
		var currentYear = DateTime.Now.Year;
		var age = livingFlag ? currentYear - birthEventDate : deathEventDate - birthEventDate;

		var x = (float)birthEventDate / 10;
		var y = generation * 10;
		
		var newPersonGameObject = Instantiate(personPrefab, new Vector3(x, y, birthEventDate), Quaternion.identity);		
		newPersonGameObject.transform.parent = transform;
		newPersonGameObject.name = name;
		var personObjectScript = newPersonGameObject.GetComponent<PersonNode>();

		personObjectScript.SetIndexes(databaseOwnerArry, tribeArrayIndex);
		personObjectScript.SetLifeSpan(birthEventDate, age);
		personObjectScript.SetPersonGender(personGender);
		personObjectScript.SetEdgePrefab(edgepf, bubblepf, capsuleBubblepf);
		//TODO use gender to set the color of the platform	
		//
		return newPersonGameObject;	
	}

	GameObject CreatePerson(Person person, int generation = 0)
	{
		return CreatePerson($"{person.givenName} {person.surName}", person.gender, person.birthEventDate,
			person.isLiving, person.deathEventDate, generation,
			person.dataBaseOwnerId, person.tribeArrayIndex);
	}

	void CreateMarriage(GameObject wifePerson, GameObject husbandPerson, int marriageEventDate, bool divorcedFlag= false,int divorcedEventDate=0)
    {
		var husbandPersonNode = husbandPerson.GetComponent<PersonNode>();
		var wifePersonNode = wifePerson.GetComponent<PersonNode>();
		var wifeAge = wifePersonNode.lifeSpan;
		var wifeAgeAtMarriage = (float)(marriageEventDate - wifePersonNode.birthDate);
		var husbandAge = husbandPersonNode.lifeSpan;
		var husbandAgeAtMarriage = (float)(marriageEventDate - husbandPersonNode.birthDate);
		// TODO does not work for divorcedEventDate = 0
		var marriageLength = divorcedFlag ?
			divorcedEventDate - marriageEventDate : (int)Mathf.Min(wifePersonNode.birthDate + wifeAge, husbandPersonNode.birthDate + husbandAge) - marriageEventDate;
		wifePersonNode.AddMarriageEdge(husbandPersonNode, wifeAgeAtMarriage / wifeAge, husbandAgeAtMarriage / husbandAge, marriageLength);
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