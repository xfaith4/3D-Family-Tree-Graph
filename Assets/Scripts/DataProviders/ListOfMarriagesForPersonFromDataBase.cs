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
    class ListOfMarriagesForPersonFromDataBase
    {
        public List<Marriage> marriageList;
        private string _dataBaseFileName;

        public ListOfMarriagesForPersonFromDataBase(string DataBaseFileName)           
        {
            _dataBaseFileName = DataBaseFileName;
            marriageList = new List<Marriage>();
        }

        public void GetListOfMarriagesWithEventsForPersonFromDataBase(int ownerId, bool useHusbandQuery = true)
        {
            string whereIdTypeToUse = useHusbandQuery ? "FatherID" : "MotherID";

            string conn = "URI=file:" + Application.dataPath + $"/RootsMagic/{_dataBaseFileName}";
            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();
            dbcmd.CommandText =
                "SELECT FM.FamilyID, FM.FatherID AS HusbandID \n" +
                "    , FM.MotherID AS WifeID \n" +
                "    , SUBSTR(Emar.Date, 8, 2) AS MarriedMonth \n" +
                "    , SUBSTR(Emar.Date, 10, 2) AS MarriedDay \n" +
                "    , SUBSTR(Emar.Date, 4, 4) AS MarriedYear \n" +
                "    , CASE WHEN SUBSTR(Eanl.Date,4,4) THEN SUBSTR(Eanl.Date,4,4) ELSE \"0\" END AS AnnulledDate \n" +
                "    , CASE WHEN SUBSTR(Ediv.Date, 4, 4) THEN SUBSTR(Ediv.Date,4,4) ELSE \"0\" END AS DivorcedDate \n" +
                "FROM FamilyTable FM \n" +
                "JOIN EventTable Emar ON FM.FamilyID = Emar.OwnerID AND Emar.EventType = 300 AND Emar.Date LIKE 'D%'-- must have Marriage event with date\n" +
                "LEFT JOIN EventTable Eanl ON FM.FamilyID = Eanl.OwnerID AND Eanl.EventType = 301-- to get Annullment event\n" +
                "LEFT JOIN EventTable Ediv ON FM.FamilyID = Ediv.OwnerID AND Ediv.EventType = 302-- to get Divorce event\n" +
                $"Where FM.{whereIdTypeToUse} = \"{ownerId}\";";

            IDataReader reader = dbcmd.ExecuteReader();   
            while (reader.Read())
            {
                var MarriageName = new Marriage(
                    familyId: reader.GetInt32(0),
                    husbandId: reader.GetInt32(1),
                    wifeId: reader.GetInt32(2),
                    marriageMonth: Int32.Parse(reader.GetString(3)),
                    marriageDay: Int32.Parse(reader.GetString(4)),
                    marriageYear: Int32.Parse(reader.GetString(5)),
                    annulledYear: Int32.Parse(reader.GetString(6)),
                    divorcedYear: Int32.Parse(reader.GetString(7)));

                marriageList.Add(MarriageName);                
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
        }

        public void GetListOfMarriagesWithNoEventsForPersonFromDataBase(int ownerId, bool useHusbandQuery = true)
        {
            string whereIdTypeToUse = useHusbandQuery ? "FatherID" : "MotherID";

            string conn = "URI=file:" + Application.dataPath + $"/RootsMagic/{_dataBaseFileName}";
            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();
            dbcmd.CommandText =
                "SELECT FM.FamilyID, FM.FatherID AS HusbandID \n" +
                "    , FM.MotherID AS WifeID \n" +
                "FROM FamilyTable FM \n" +
                $"Where FM.{whereIdTypeToUse} = \"{ownerId}\";";

            IDataReader reader = dbcmd.ExecuteReader();
            while (reader.Read())
            {
                var MarriageName = new Marriage(
                    familyId: reader.GetInt32(0),
                    husbandId: reader.GetInt32(1),
                    wifeId: reader.GetInt32(2),
                    marriageMonth: 0,
                    marriageDay: 0,
                    marriageYear: 0,
                    annulledYear: 0,
                    divorcedYear: 0);

                marriageList.Add(MarriageName);
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;
        }
    }        
}
