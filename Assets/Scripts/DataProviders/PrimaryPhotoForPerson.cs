using Assets.Scripts.DataObjects;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using Assets.Scripts.Enums;

namespace Assets.Scripts.DataProviders
{
    class PrimaryPhotoForPerson : DataProviderBase
    {       
        private string _dataBaseFileName;

        public PrimaryPhotoForPerson(string DataBaseFileName)           
        {
            _dataBaseFileName = DataBaseFileName;            
        }

        public byte[] GetPrimaryPhotoForPersonFromDataBase(int ownerId)
        {
            byte[] imageToReturn = null;

            int limitListSizeTo = 1;
            string conn = "URI=file:" + _dataBaseFileName;
            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();
            string QUERYPHOTOS =
                "SELECT media.Thumbnail, media.MediaPath \n" +
                "FROM MultimediaTable media \n" +
                "Join MediaLinkTable link on media.MediaID = link.MediaID \n" +
                "Where link.IsPrimary = 1 \n";
            QUERYPHOTOS +=
                    $"AND link.OwnerID = \"{ownerId}\" LIMIT 1;";
            
            string sqlQuery = QUERYPHOTOS;
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int currentArrayIndex = 0;
            while (reader.Read() && currentArrayIndex < limitListSizeTo)
            {
                if (reader["Thumbnail"].ToString().Length == 0)
                {
                    imageToReturn = null;
                }
                else
                {
                    imageToReturn = (byte[])reader["Thumbnail"];
                }
                string pathToFullResolutionImage = reader.GetString(1);

                currentArrayIndex++;
            }
            reader.Close();
            reader = null;
            dbcmd.Dispose();
            dbcmd = null;
            dbconn.Close();
            dbconn = null;

            return imageToReturn;
        }
    }
}
