using Assets.Scripts.DataObjects;
using System.Collections.Generic;
using System.Linq;
using Mono.Data.Sqlite;
using System.Data;
using UnityEngine;
using Assets.Scripts.Enums;
using System;

namespace Assets.Scripts.DataProviders
{
    class PrimaryThumbnailForPersonFromDigiKam : DataProviderBase
    {
        private string _rootsMagicDataBaseFileName;
        private string _digiKamDataBaseFileName;
        private string _digiKamDataBaseFolderPath;

        public PrimaryThumbnailForPersonFromDigiKam(string DataBaseFileName, string DigiKamFileName)
        {
            _rootsMagicDataBaseFileName = DataBaseFileName;
            _digiKamDataBaseFileName = DigiKamFileName;
            // Get the path to the database file
            _digiKamDataBaseFolderPath = System.IO.Path.GetDirectoryName(_digiKamDataBaseFileName);

        }

        public byte[] GetPrimaryPhotoForPersonFromDataBase(int ownerId)
        {
            byte[] imageToReturn = null;

            int limitListSizeTo = 1;
            string conn = "URI=file:" + _digiKamDataBaseFileName;
            IDbConnection dbconn;
            dbconn = (IDbConnection)new SqliteConnection(conn);
            dbconn.Open();
            IDbCommand dbcmd = dbconn.CreateCommand();
            string QUERYTHUMBNAILS =
                "SELECT r2d.PersonID, r2d.tagid, tags.name, paths.thumbId, images.id as \"imageId\",\n" +
                "tnails.type, tnails.modificationDate, tnails.orientationHint,\n" +
                "\"C:\" || (SELECT specificPath FROM digikam4.AlbumRoots WHERE digikam4.AlbumRoots.label = \"Pictures\") ||\n" +
                "albums.relativePath || \"/\" || images.name as \"fullPathToFileName\",\n" +
                "region.value as \"region\", tnails.data as 'PGFImageData'\n" +
                "FROM PersonDigiKamTag r2d \n" +
                "JOIN digikam4.Tags tags ON r2d.tagid = tags.id\n" +
                "LEFT JOIN digikam4.Images images ON tags.icon = images.id\n" +
                "LEFT JOIN digikam4.Albums albums ON images.album = albums.id\n" +
                "LEFT JOIN digikam4.ImageTagProperties region ON tags.icon = region.imageid AND tags.id = region.tagid\n" +
                "INNER JOIN [thumbnails-digikam].FilePaths paths ON fullPathToFileName = paths.path\n" +
                "INNER JOIN [thumbnails-digikam].Thumbnails tnails ON paths.thumbId = tnails.id\n";
            QUERYTHUMBNAILS +=
                $"WHERE r2d.PersonID = \"{ownerId}\" AND images.album IS NOT NULL;";

            string sqlQuery = QUERYTHUMBNAILS;
            dbcmd.CommandText = sqlQuery;
            IDataReader reader = dbcmd.ExecuteReader();
            int currentArrayIndex = 0;
            while (reader.Read() && currentArrayIndex < limitListSizeTo) {
                string pathToFullResolutionImage = (string)reader["fullPathToFileName"];
                
                if (System.IO.File.Exists(pathToFullResolutionImage)) {
                    try {
                        imageToReturn = System.IO.File.ReadAllBytes(pathToFullResolutionImage);
                    }
                    catch (Exception ex) {
                        Debug.LogError($"Error reading image file {pathToFullResolutionImage}: {ex.Message}");
                        imageToReturn = null;
                    }
                }
                else {
                    Debug.LogWarning($"Image file not found: {pathToFullResolutionImage}");
                    imageToReturn = null;
                }

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

        public byte[] GetPrimaryThumbnailForPersonFromDataBase(int ownerId)
        {
            var imageToReturn = new byte[0];
            var connectionString = "URI=file:" + _digiKamDataBaseFileName;
            using (IDbConnection dbConnection = new SqliteConnection(connectionString)) {
                dbConnection.Open();
                try {
                    IDbCommand dbcmd = dbConnection.CreateCommand();
                    // Step 1: Attach secondary database
                   var attachCommand = "ATTACH DATABASE 'c:\\Users\\shuskey\\OneDrive\\Pictures\\thumbnails-digikam.db' AS 'thumbnails-digikam';";
                   dbcmd.CommandText = attachCommand;
                   IDataReader reader = dbcmd.ExecuteReader();


                }
                catch (Exception ex) {                    
                    Debug.Log("An error occurred:" + ex.Message);
                }
                finally {
                    if (dbConnection.State == ConnectionState.Open) {
                        dbConnection.Close();
                    }
                }
            }
            return imageToReturn;
        }
    }
}

