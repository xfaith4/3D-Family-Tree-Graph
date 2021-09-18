using Assets.Scripts.DataObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using Assets.Scripts.Enums;

namespace Assets.Scripts.DataProviders
{
    class ListOfPersonsFromDataBase
    {
        public List<Person> personsList;
        private string _dataBaseFileName;

        public ListOfPersonsFromDataBase(string DataBaseFileName)           
        {
            _dataBaseFileName = DataBaseFileName;
            personsList = new List<Person>();
        }
        public void GetSinglePersonFromDataBase(int ownerId, int generation)
        {
            GetListOfPersonsFromDataBase(limitListSizeTo: 1, ownerId, generation);
        }

        public void GetListOfPersonsFromDataBase(int limitListSizeTo, int? JustThisOwnerId = null, int generation = 0)
        {
            string conn = "URI=file:" + Application.dataPath + $"/RootsMagic/{_dataBaseFileName}";
            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();
            string QUERYNAMES =
                "SELECT name.OwnerID \n" +
                "    , case when Sex = 0 then 'M' when Sex = 1 then 'F' else 'U' end \n" +
                "    , name.Given, name.Surname, name.BirthYear, name.DeathYear, person.Living \n" +
                "FROM NameTable name \n" +
                "JOIN PersonTable person \n" +
                "ON name.OwnerID = person.PersonID \n";
            if (JustThisOwnerId != null)
                QUERYNAMES +=
                    $"WHERE name.OwnerID = \"{JustThisOwnerId}\"";
            string sqlQuery = QUERYNAMES;
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int currentArrayIndex = 0;
            while (reader.Read() && currentArrayIndex < limitListSizeTo)
            {
                var nextName = new Person(
                    arrayIndex: currentArrayIndex,
                    ownerId: reader.GetInt32(0),
                    gender: charToPersonGenderType(reader.GetString(1)[0]),
                    given: reader.GetString(2),
                    surname: reader.GetString(3),
                    birthYear: reader.GetInt32(4),
                    deathYear: reader.GetInt32(5),
                    isLiving: reader.GetBoolean(6),
                    generation: generation);

                if (nextName.dataBaseOwnerId == 218)
                    Debug.Log($"We just read in OwnerId {nextName.dataBaseOwnerId}");

                nextName.FixUpDatesForViewing();

                personsList.Add(nextName);
                currentArrayIndex++;
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
            
            PersonGenderType charToPersonGenderType(char sex) =>
                sex.Equals('M') ? PersonGenderType.Male : (sex.Equals('F') ? PersonGenderType.Female : PersonGenderType.NotSet);
        }
    }        
}
