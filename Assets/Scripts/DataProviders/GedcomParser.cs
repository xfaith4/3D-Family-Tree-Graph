using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Assets.Scripts.DataProviders
{
    public class GedcomPerson
    {
        public string Id { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string Sex { get; set; }
        public string BirthDate { get; set; }
        public string BirthPlace { get; set; }
        public string DeathDate { get; set; }
        public string DeathPlace { get; set; }
        public List<string> FamilyAsChild { get; set; } = new List<string>();
        public List<string> FamilyAsSpouse { get; set; } = new List<string>();
    }

    public class GedcomFamily
    {
        public string Id { get; set; }
        public string HusbandId { get; set; }
        public string WifeId { get; set; }
        public List<string> ChildrenIds { get; set; } = new List<string>();
        public string MarriageDate { get; set; }
        public string MarriagePlace { get; set; }
    }

    public class GedcomParser
    {
        public Dictionary<string, GedcomPerson> Individuals { get; private set; } = new Dictionary<string, GedcomPerson>();
        public Dictionary<string, GedcomFamily> Families { get; private set; } = new Dictionary<string, GedcomFamily>();

        public void ParseFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"GEDCOM file not found: {filePath}");
                return;
            }

            Debug.Log($"Parsing GEDCOM file: {filePath}");
            string[] lines = File.ReadAllLines(filePath);
            
            string currentRecord = null;
            string currentRecordId = null;
            GedcomPerson currentPerson = null;
            GedcomFamily currentFamily = null;
            string lastTag = null;
            int lastLevel = -1;

            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                    continue;

                var parts = ParseLine(line);
                if (parts == null)
                    continue;

                int level = parts.Level;
                string tag = parts.Tag;
                string value = parts.Value;
                string xref = parts.XRef;

                // Start of a new record
                if (level == 0)
                {
                    // Save previous record
                    if (currentPerson != null && currentRecordId != null)
                        Individuals[currentRecordId] = currentPerson;
                    if (currentFamily != null && currentRecordId != null)
                        Families[currentRecordId] = currentFamily;

                    currentPerson = null;
                    currentFamily = null;
                    currentRecord = tag;
                    currentRecordId = xref;

                    if (tag == "INDI")
                    {
                        currentPerson = new GedcomPerson { Id = xref };
                    }
                    else if (tag == "FAM")
                    {
                        currentFamily = new GedcomFamily { Id = xref };
                    }
                }
                else
                {
                    // Handle level 1 tags
                    if (level == 1)
                    {
                        lastTag = tag;
                        
                        if (currentPerson != null)
                        {
                            switch (tag)
                            {
                                case "NAME":
                                    ParseName(value, currentPerson);
                                    break;
                                case "SEX":
                                    currentPerson.Sex = value;
                                    break;
                                case "FAMC":
                                    currentPerson.FamilyAsChild.Add(value);
                                    break;
                                case "FAMS":
                                    currentPerson.FamilyAsSpouse.Add(value);
                                    break;
                            }
                        }
                        else if (currentFamily != null)
                        {
                            switch (tag)
                            {
                                case "HUSB":
                                    currentFamily.HusbandId = value;
                                    break;
                                case "WIFE":
                                    currentFamily.WifeId = value;
                                    break;
                                case "CHIL":
                                    currentFamily.ChildrenIds.Add(value);
                                    break;
                            }
                        }
                    }
                    // Handle level 2 tags
                    else if (level == 2)
                    {
                        if (currentPerson != null)
                        {
                            if (lastTag == "BIRT")
                            {
                                if (tag == "DATE")
                                    currentPerson.BirthDate = value;
                                else if (tag == "PLAC")
                                    currentPerson.BirthPlace = value;
                            }
                            else if (lastTag == "DEAT")
                            {
                                if (tag == "DATE")
                                    currentPerson.DeathDate = value;
                                else if (tag == "PLAC")
                                    currentPerson.DeathPlace = value;
                            }
                            else if (lastTag == "NAME")
                            {
                                if (tag == "GIVN")
                                    currentPerson.GivenName = value;
                                else if (tag == "SURN")
                                    currentPerson.Surname = value;
                            }
                        }
                        else if (currentFamily != null)
                        {
                            if (lastTag == "MARR")
                            {
                                if (tag == "DATE")
                                    currentFamily.MarriageDate = value;
                                else if (tag == "PLAC")
                                    currentFamily.MarriagePlace = value;
                            }
                        }
                    }
                }

                if (level == 1)
                    lastLevel = level;
            }

            // Save last record
            if (currentPerson != null && currentRecordId != null)
                Individuals[currentRecordId] = currentPerson;
            if (currentFamily != null && currentRecordId != null)
                Families[currentRecordId] = currentFamily;

            Debug.Log($"Parsed {Individuals.Count} individuals and {Families.Count} families from GEDCOM file");
        }

        private class GedcomLine
        {
            public int Level { get; set; }
            public string XRef { get; set; }
            public string Tag { get; set; }
            public string Value { get; set; }
        }

        private GedcomLine ParseLine(string line)
        {
            // GEDCOM line format: level [xref] tag [value]
            var match = Regex.Match(line, @"^(\d+)\s+(?:(@[^@]+@)\s+)?(\S+)(?:\s+(.*))?$");
            if (!match.Success)
                return null;

            return new GedcomLine
            {
                Level = int.Parse(match.Groups[1].Value),
                XRef = match.Groups[2].Value,
                Tag = match.Groups[3].Value,
                Value = match.Groups[4].Value
            };
        }

        private void ParseName(string nameValue, GedcomPerson person)
        {
            // GEDCOM name format: given /surname/
            var match = Regex.Match(nameValue, @"^([^/]*)\s*/([^/]*)/");
            if (match.Success)
            {
                person.GivenName = match.Groups[1].Value.Trim();
                person.Surname = match.Groups[2].Value.Trim();
            }
            else
            {
                person.GivenName = nameValue.Trim();
            }
        }
    }
}
