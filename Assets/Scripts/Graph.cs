using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Assets.Scripts.Enums;
using System;
using Random = UnityEngine.Random;

public class Graph : MonoBehaviour
{

	public TextAsset file;
	public GameObject personPrefab;
	public GameObject edgepf;
	public GameObject bubblepf;
	public GameObject capsuleBubblepf;
	public int numberOfNodes = 5;
	[SerializeField] [Tooltip("0.0 to 1.0")] public float chanceOfAnEdge = 0.5f;
	public float graphSize = 10f;	
	private List<PersonNode> gameObjectNodes = new List<PersonNode>();

	void Start()
	{
		var fruitnames = new string[] { "Apple", "Apricot", "Avocado", "Banana", "Bilberry", "Blackberry", "Blackcurrant", "Blueberry", "Boysenberry", "Currant", "Cherry", "Cherimoya", "Chico fruit", "Cloudberry", "Coconut", "Cranberry", "Cucumber", "Custard apple", "Damson", "Date", "Dragonfruit", "Durian", "Elderberry", "Feijoa", "Fig", "Goji berry", "Gooseberry", "Grape", "Raisin", "Grapefruit", "Guava", "Honeyberry", "Huckleberry", "Jabuticaba", "Jackfruit", "Jambul", "Jujube", "Juniper berry", "Kiwano", "Kiwifruit", "Kumquat", "Lemon", "Lime", "Loquat", "Longan", "Lychee", "Mango", "Mangosteen", "Marionberry", "Melon", "Cantaloupe", "Honeydew", "Watermelon", "Miracle fruit", "Mulberry", "Nectarine", "Nance", "Olive", "Orange", "Blood orange", "Clementine", "Mandarine", "Tangerine", "Papaya", "Passionfruit", "Peach", "Pear", "Persimmon", "Physalis", "Plantain", "Plum", "Prune", "Pineapple", "Plumcot", "Pomegranate", "Pomelo", "Purple mangosteen", "Quince", "Raspberry", "Salmonberry", "Rambutan", "Redcurrant", "Salal berry", "Salak", "Satsuma", "Soursop", "Star fruit", "Solanum", "quitoense", "Strawberry", "Tamarillo", "Tamarind", "Ugli fruit", "Yuzu" };

		if (file == null)
		{
			var adam = CreatePerson("Adam", PersonGenderType.Male, 1800, false, 1870, generation: 0);
			var eve = CreatePerson("Eve", PersonGenderType.Female, 1810, false, 1860, generation: 0);
			CreateMarriage(eve, adam, 1830);
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
		}
		else
		{
			LoadGMLFromFile(file);
		}
	}

	GameObject CreatePerson(string name, PersonGenderType personGender, int birthEventDate, bool livingFlag = true, int deathEventDate = 0, int generation = 0)
    {
		var currentYear = DateTime.Now.Year;
		var age = livingFlag ? currentYear - birthEventDate : deathEventDate - birthEventDate;

		var x = (float)birthEventDate / 10;
		var y = generation;
		
		var newPersonGameObject = Instantiate(personPrefab, new Vector3(x, y, birthEventDate), Quaternion.identity);		
		newPersonGameObject.transform.parent = transform;
		newPersonGameObject.name = name;
		var personObjectScript = newPersonGameObject.GetComponent<PersonNode>();

		personObjectScript.SetLifeSpan(birthEventDate, age);
		personObjectScript.SetPersonGender(personGender);
		personObjectScript.SetEdgePrefab(edgepf, bubblepf, capsuleBubblepf);
		//TODO use gender to set the color of the platform	
		//
		return newPersonGameObject;	
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

	void LoadGMLFromFile(TextAsset f)
	{
		string[] lines = f.text.Split('\n');
		int currentobject = -1; // 0 = graph, 1 = node, 2 = edge
		int stage = -1; // 0 waiting to open, 1 = waiting for attribute, 2 = waiting for id, 3 = waiting for label, 4 = waiting for source, 5 = waiting for target
		PersonNode n = null;
		Dictionary<string, PersonNode> nodes = new Dictionary<string, PersonNode>();
		foreach (string line in lines)
		{
			string l = line.Trim();
			string[] words = l.Split(' ');
			foreach (string word in words)
			{
				if (word == "graph" && stage == -1)
				{
					currentobject = 0;
				}
				if (word == "node" && stage == -1)
				{
					currentobject = 1;
					stage = 0;
				}
				if (word == "edge" && stage == -1)
				{
					currentobject = 2;
					stage = 0;
				}
				if (word == "[" && stage == 0 && currentobject == 2)
				{
					stage = 1;
				}
				if (word == "[" && stage == 0 && currentobject == 1)
				{
					stage = 1;
					GameObject go = Instantiate(personPrefab, Random.insideUnitSphere * graphSize, Quaternion.identity);
					n = go.GetComponent<PersonNode>();
					n.transform.parent = transform;
					n.SetEdgePrefab(edgepf, bubblepf, capsuleBubblepf);
					continue;
				}
				if (word == "]")
				{
					stage = -1;
				}
				if (word == "id" && stage == 1 && currentobject == 1)
				{
					stage = 2;
					continue;
				}
				if (word == "label" && stage == 1 && currentobject == 1)
				{
					stage = 3;
					continue;
				}
				if (stage == 2)
				{
					nodes.Add(word, n);
					stage = 1;
					break;
				}
				if (stage == 3)
				{
					n.name = word;
					stage = 1;
					break;
				}
				if (word == "source" && stage == 1 && currentobject == 2)
				{
					stage = 4;
					continue;
				}
				if (word == "target" && stage == 1 && currentobject == 2)
				{
					stage = 5;
					continue;
				}
				if (stage == 4)
				{
					n = nodes[word];
					stage = 1;
					break;
				}
				if (stage == 5)
				{
					n.AddEdge(nodes[word]);
					stage = 1;
					break;
				}
			}
		}
	}
}