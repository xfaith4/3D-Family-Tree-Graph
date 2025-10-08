using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Mono.Data.Sqlite;
using UnityEngine;

namespace Assets.Scripts.DataProviders
{
    public class GedcomToSqlConverter
    {
        private GedcomParser _parser;
        private string _dbFilePath;
        private Dictionary<string, int> _gedcomIdToSqlId = new Dictionary<string, int>();

        public GedcomToSqlConverter()
        {
            _parser = new GedcomParser();
        }

        public void ConvertGedcomToDatabase(string gedcomFilePath, string outputDbPath)
        {
            Debug.Log($"Converting GEDCOM file {gedcomFilePath} to database {outputDbPath}");
            
            // Parse the GEDCOM file
            _parser.ParseFile(gedcomFilePath);

            // Create or overwrite the database
            _dbFilePath = outputDbPath;
            
            // Delete existing database if it exists
            if (File.Exists(_dbFilePath))
            {
                Debug.Log($"Deleting existing database: {_dbFilePath}");
                File.Delete(_dbFilePath);
            }

            // Create new database with schema
            CreateDatabaseSchema();

            // Import data
            ImportIndividuals();
            ImportFamilies();

            Debug.Log($"GEDCOM conversion complete. Database created at: {_dbFilePath}");
        }

        private void CreateDatabaseSchema()
        {
            string conn = "URI=file:" + _dbFilePath;
            using (IDbConnection dbconn = new SqliteConnection(conn))
            {
                dbconn.Open();
                using (IDbCommand dbcmd = dbconn.CreateCommand())
                {
                    // Create PersonTable
                    dbcmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS PersonTable (
                            PersonID INTEGER PRIMARY KEY AUTOINCREMENT,
                            UniqueID TEXT,
                            Sex INTEGER,
                            ParentID INTEGER,
                            SpouseID INTEGER,
                            Color INTEGER,
                            Color1 INTEGER,
                            Color2 INTEGER,
                            Color3 INTEGER,
                            Color4 INTEGER,
                            Color5 INTEGER,
                            Color6 INTEGER,
                            Color7 INTEGER,
                            Color8 INTEGER,
                            Color9 INTEGER,
                            Relate1 INTEGER,
                            Relate2 INTEGER,
                            Flags INTEGER,
                            Living INTEGER,
                            IsPrivate INTEGER,
                            Proof INTEGER,
                            Bookmark INTEGER,
                            Note TEXT,
                            UTCModDate FLOAT
                        )";
                    dbcmd.ExecuteNonQuery();

                    // Create NameTable
                    dbcmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS NameTable (
                            NameID INTEGER PRIMARY KEY AUTOINCREMENT,
                            OwnerID INTEGER,
                            Surname TEXT COLLATE RMNOCASE,
                            Given TEXT COLLATE RMNOCASE,
                            Prefix TEXT COLLATE RMNOCASE,
                            Suffix TEXT COLLATE RMNOCASE,
                            Nickname TEXT COLLATE RMNOCASE,
                            NameType INTEGER,
                            Date TEXT,
                            SortDate BIGINT,
                            IsPrimary INTEGER,
                            IsPrivate INTEGER,
                            Proof INTEGER,
                            Sentence TEXT,
                            Note TEXT,
                            BirthYear INTEGER,
                            DeathYear INTEGER,
                            Display INTEGER,
                            Language TEXT,
                            UTCModDate FLOAT,
                            SurnameMP TEXT,
                            GivenMP TEXT,
                            NicknameMP TEXT
                        )";
                    dbcmd.ExecuteNonQuery();

                    // Create EventTable
                    dbcmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS EventTable (
                            EventID INTEGER PRIMARY KEY AUTOINCREMENT,
                            EventType INTEGER,
                            OwnerType INTEGER,
                            OwnerID INTEGER,
                            FamilyID INTEGER,
                            PlaceID INTEGER,
                            SiteID INTEGER,
                            Date TEXT,
                            SortDate BIGINT,
                            IsPrimary INTEGER,
                            IsPrivate INTEGER,
                            Proof INTEGER,
                            Status INTEGER,
                            Sentence TEXT,
                            Details TEXT,
                            Note TEXT,
                            UTCModDate FLOAT
                        )";
                    dbcmd.ExecuteNonQuery();

                    // Create FamilyTable
                    dbcmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS FamilyTable (
                            FamilyID INTEGER PRIMARY KEY AUTOINCREMENT,
                            FatherID INTEGER,
                            MotherID INTEGER,
                            ChildID INTEGER,
                            HusbOrder INTEGER,
                            WifeOrder INTEGER,
                            IsPrivate INTEGER,
                            Proof INTEGER,
                            SpouseLabel INTEGER,
                            FatherLabel INTEGER,
                            MotherLabel INTEGER,
                            SpouseLabelStr TEXT,
                            FatherLabelStr TEXT,
                            MotherLabelStr TEXT,
                            Note TEXT,
                            UTCModDate FLOAT
                        )";
                    dbcmd.ExecuteNonQuery();

                    // Create ChildTable
                    dbcmd.CommandText = @"
                        CREATE TABLE IF NOT EXISTS ChildTable (
                            RecID INTEGER PRIMARY KEY AUTOINCREMENT,
                            ChildID INTEGER,
                            FamilyID INTEGER,
                            RelFather INTEGER,
                            RelMother INTEGER,
                            ChildOrder INTEGER,
                            IsPrivate INTEGER,
                            ProofFather INTEGER,
                            ProofMother INTEGER,
                            Note TEXT,
                            UTCModDate FLOAT
                        )";
                    dbcmd.ExecuteNonQuery();

                    Debug.Log("Database schema created successfully");
                }
            }
        }

        private void ImportIndividuals()
        {
            string conn = "URI=file:" + _dbFilePath;
            using (IDbConnection dbconn = new SqliteConnection(conn))
            {
                dbconn.Open();

                foreach (var kvp in _parser.Individuals)
                {
                    string gedcomId = kvp.Key;
                    GedcomPerson person = kvp.Value;

                    int personId;
                    using (IDbCommand dbcmd = dbconn.CreateCommand())
                    {
                        // Insert into PersonTable
                        int sex = person.Sex == "M" ? 0 : (person.Sex == "F" ? 1 : 2);
                        int living = string.IsNullOrEmpty(person.DeathDate) ? 1 : 0;

                        dbcmd.CommandText = @"
                            INSERT INTO PersonTable (UniqueID, Sex, Living, IsPrivate, Proof, Bookmark)
                            VALUES (@UniqueID, @Sex, @Living, 0, 0, 0)";
                        
                        AddParameter(dbcmd, "@UniqueID", gedcomId);
                        AddParameter(dbcmd, "@Sex", sex);
                        AddParameter(dbcmd, "@Living", living);
                        
                        dbcmd.ExecuteNonQuery();

                        // Get the PersonID
                        dbcmd.CommandText = "SELECT last_insert_rowid()";
                        personId = Convert.ToInt32(dbcmd.ExecuteScalar());
                        _gedcomIdToSqlId[gedcomId] = personId;
                    }

                    using (IDbCommand dbcmd = dbconn.CreateCommand())
                    {
                        // Insert into NameTable
                        int birthYear = ExtractYear(person.BirthDate);
                        int deathYear = ExtractYear(person.DeathDate);

                        dbcmd.CommandText = @"
                            INSERT INTO NameTable (OwnerID, Surname, Given, IsPrimary, BirthYear, DeathYear)
                            VALUES (@OwnerID, @Surname, @Given, 1, @BirthYear, @DeathYear)";
                        
                        AddParameter(dbcmd, "@OwnerID", personId);
                        AddParameter(dbcmd, "@Surname", person.Surname ?? "");
                        AddParameter(dbcmd, "@Given", person.GivenName ?? "");
                        AddParameter(dbcmd, "@BirthYear", birthYear);
                        AddParameter(dbcmd, "@DeathYear", deathYear);
                        
                        dbcmd.ExecuteNonQuery();
                    }

                    // Insert birth event
                    if (!string.IsNullOrEmpty(person.BirthDate))
                    {
                        InsertEvent(dbconn, personId, 1, person.BirthDate, person.BirthPlace);
                    }

                    // Insert death event
                    if (!string.IsNullOrEmpty(person.DeathDate))
                    {
                        InsertEvent(dbconn, personId, 2, person.DeathDate, person.DeathPlace);
                    }
                }

                Debug.Log($"Imported {_parser.Individuals.Count} individuals");
            }
        }

        private void ImportFamilies()
        {
            string conn = "URI=file:" + _dbFilePath;
            using (IDbConnection dbconn = new SqliteConnection(conn))
            {
                dbconn.Open();

                foreach (var kvp in _parser.Families)
                {
                    string gedcomFamilyId = kvp.Key;
                    GedcomFamily family = kvp.Value;

                    int? fatherId = null;
                    int? motherId = null;

                    if (!string.IsNullOrEmpty(family.HusbandId) && _gedcomIdToSqlId.ContainsKey(family.HusbandId))
                        fatherId = _gedcomIdToSqlId[family.HusbandId];

                    if (!string.IsNullOrEmpty(family.WifeId) && _gedcomIdToSqlId.ContainsKey(family.WifeId))
                        motherId = _gedcomIdToSqlId[family.WifeId];

                    if (!fatherId.HasValue && !motherId.HasValue)
                        continue;

                    int familyId;
                    using (IDbCommand dbcmd = dbconn.CreateCommand())
                    {
                        dbcmd.CommandText = @"
                            INSERT INTO FamilyTable (FatherID, MotherID, IsPrivate, Proof)
                            VALUES (@FatherID, @MotherID, 0, 0)";
                        
                        AddParameter(dbcmd, "@FatherID", fatherId.HasValue ? (object)fatherId.Value : DBNull.Value);
                        AddParameter(dbcmd, "@MotherID", motherId.HasValue ? (object)motherId.Value : DBNull.Value);
                        
                        dbcmd.ExecuteNonQuery();

                        dbcmd.CommandText = "SELECT last_insert_rowid()";
                        familyId = Convert.ToInt32(dbcmd.ExecuteScalar());
                    }

                    // Insert marriage event
                    if (!string.IsNullOrEmpty(family.MarriageDate))
                    {
                        InsertEvent(dbconn, 0, 3, family.MarriageDate, family.MarriagePlace, familyId);
                    }

                    // Insert children
                    int childOrder = 0;
                    foreach (var childGedcomId in family.ChildrenIds)
                    {
                        if (_gedcomIdToSqlId.ContainsKey(childGedcomId))
                        {
                            int childId = _gedcomIdToSqlId[childGedcomId];

                            using (IDbCommand dbcmd = dbconn.CreateCommand())
                            {
                                dbcmd.CommandText = @"
                                    INSERT INTO ChildTable (ChildID, FamilyID, RelFather, RelMother, ChildOrder, IsPrivate)
                                    VALUES (@ChildID, @FamilyID, 0, 0, @ChildOrder, 0)";
                                
                                AddParameter(dbcmd, "@ChildID", childId);
                                AddParameter(dbcmd, "@FamilyID", familyId);
                                AddParameter(dbcmd, "@ChildOrder", childOrder);
                                
                                dbcmd.ExecuteNonQuery();
                            }

                            childOrder++;
                        }
                    }
                }

                Debug.Log($"Imported {_parser.Families.Count} families");
            }
        }

        private void InsertEvent(IDbConnection dbconn, int ownerId, int eventType, string date, string place, int familyId = 0)
        {
            using (IDbCommand dbcmd = dbconn.CreateCommand())
            {
                string formattedDate = FormatDateForRootsMagic(date);
                
                dbcmd.CommandText = @"
                    INSERT INTO EventTable (EventType, OwnerType, OwnerID, FamilyID, Date, IsPrimary, IsPrivate)
                    VALUES (@EventType, 0, @OwnerID, @FamilyID, @Date, 1, 0)";
                
                AddParameter(dbcmd, "@EventType", eventType);
                AddParameter(dbcmd, "@OwnerID", ownerId);
                AddParameter(dbcmd, "@FamilyID", familyId);
                AddParameter(dbcmd, "@Date", formattedDate);
                
                dbcmd.ExecuteNonQuery();
            }
        }

        private void AddParameter(IDbCommand cmd, string name, object value)
        {
            var param = cmd.CreateParameter();
            param.ParameterName = name;
            param.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(param);
        }

        private int ExtractYear(string dateString)
        {
            if (string.IsNullOrEmpty(dateString))
                return 0;

            // Try to extract a 4-digit year from the date string
            var match = System.Text.RegularExpressions.Regex.Match(dateString, @"\b(\d{4})\b");
            if (match.Success)
            {
                return int.Parse(match.Groups[1].Value);
            }

            return 0;
        }

        private string FormatDateForRootsMagic(string gedcomDate)
        {
            if (string.IsNullOrEmpty(gedcomDate))
                return "";

            // GEDCOM date format examples: "17 January 1959", "6 June 1933", "Abt 1907"
            // RootsMagic date format: "D.+YYYYMMDD..+00000000.."
            
            // Try to parse the date
            var dayMatch = System.Text.RegularExpressions.Regex.Match(gedcomDate, @"\b(\d{1,2})\b");
            var monthMatch = System.Text.RegularExpressions.Regex.Match(gedcomDate, @"\b(January|February|March|April|May|June|July|August|September|October|November|December|Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            var yearMatch = System.Text.RegularExpressions.Regex.Match(gedcomDate, @"\b(\d{4})\b");

            string year = yearMatch.Success ? yearMatch.Groups[1].Value : "0000";
            string month = "00";
            string day = "00";

            if (monthMatch.Success)
            {
                string monthName = monthMatch.Groups[1].Value.ToLower();
                switch (monthName.Substring(0, 3))
                {
                    case "jan": month = "01"; break;
                    case "feb": month = "02"; break;
                    case "mar": month = "03"; break;
                    case "apr": month = "04"; break;
                    case "may": month = "05"; break;
                    case "jun": month = "06"; break;
                    case "jul": month = "07"; break;
                    case "aug": month = "08"; break;
                    case "sep": month = "09"; break;
                    case "oct": month = "10"; break;
                    case "nov": month = "11"; break;
                    case "dec": month = "12"; break;
                }
            }

            if (dayMatch.Success)
            {
                int dayNum = int.Parse(dayMatch.Groups[1].Value);
                day = dayNum.ToString("D2");
            }

            // RootsMagic format: "D.+YYYYMMDD..+00000000.."
            return $"D.+{year}{month}{day}..+00000000..";
        }
    }
}
