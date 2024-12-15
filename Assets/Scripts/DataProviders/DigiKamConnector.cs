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
    class DigiKamConnector : DataProviderBase
    {
        public List<DigiKamFaceTag> faceTagList;
        private string _rootsMagicDataBaseFileNameWithFullPath;  // usually *.rmtree, *.rmgc, or *.sqlite
        private string _digiKamDataBaseFileNameWithFullPath;     // usually digikam4.db
        private string _rootsMagicToDigiKamDataBaseFileNameWithFullPath;  // usually rootsmagic-digikam.db
        private string _digiKamThumbnailsDataBaseFileNameWithFullPath;  // usually thumbnails-digikam.db

        static string DigiKam_RootsMagic_DataBaseFileNameOnly = "rootsmagic-digikam.db";
        static string DigiKam_Thumbnails_DataBaseFileNameOnly = "thumbnails-digikam.db";

        public DigiKamConnector(string RootMagicDataBaseFileName, string DigiKamDataBaseFileName)           
        {
            _rootsMagicDataBaseFileNameWithFullPath = RootMagicDataBaseFileName;
            _digiKamDataBaseFileNameWithFullPath = DigiKamDataBaseFileName;
            var justThePath = System.IO.Path.GetDirectoryName(_digiKamDataBaseFileNameWithFullPath);
            _rootsMagicToDigiKamDataBaseFileNameWithFullPath = justThePath + "\\" + DigiKam_RootsMagic_DataBaseFileNameOnly;
            _digiKamThumbnailsDataBaseFileNameWithFullPath = justThePath + "\\" + DigiKam_Thumbnails_DataBaseFileNameOnly;
            faceTagList = new List<DigiKamFaceTag>();
        }

        public void GetListOfFaceTagIdsFromDataBase()
        {
            var justThePath = System.IO.Path.GetDirectoryName(_digiKamDataBaseFileNameWithFullPath);
            var pathtoRootsMagicToDigiKamDatabaseFile = justThePath + "\\rootsmagic-digikam.db";
            using (var conn = new SqliteConnection("URI=file:" + pathtoRootsMagicToDigiKamDatabaseFile)) {
                conn.Open();
                using (var cmd = conn.CreateCommand()) {
                    cmd.CommandText = "SELECT * FROM PersonDigiKamTag";
                    using (var reader = cmd.ExecuteReader()) {
                        while (reader.Read()) {
                            DigiKamFaceTag faceTag = new DigiKamFaceTag(reader.GetInt32(0), reader.GetInt32(1), reader.GetString(2));                                                       
                            faceTagList.Add(faceTag);
                        }
                    }
                }
            }
        }

        public bool AreAllDatabaseFilesPresent()
        {
            if (!System.IO.File.Exists(_rootsMagicDataBaseFileNameWithFullPath)) {
                Debug.Log($"RootsMagic database file not found. {_rootsMagicDataBaseFileNameWithFullPath}");
                return false;
            }
            if (!System.IO.File.Exists(_digiKamDataBaseFileNameWithFullPath)) {
                Debug.Log($"Base DigiKam database file not found. {_digiKamDataBaseFileNameWithFullPath}");
                return false;
            }
            if (!DoesThisDBFileExistInDigiKamFolder(DigiKam_RootsMagic_DataBaseFileNameOnly)) {
                Debug.Log($"RootsMagic To DigiKam database file not found. {DigiKam_RootsMagic_DataBaseFileNameOnly}");
                return false;
            }
            if (!DoesThisDBFileExistInDigiKamFolder(DigiKam_Thumbnails_DataBaseFileNameOnly)) {
                Debug.Log($"DigiKam Thumbnail database file not found. {DigiKam_Thumbnails_DataBaseFileNameOnly}");
                return false;
            }
            return true;    
        }

        private bool DoesThisDBFileExistInDigiKamFolder(string filename)
        {
            var folderOnly = System.IO.Path.GetDirectoryName(_digiKamDataBaseFileNameWithFullPath);
            var filenameToCheck = folderOnly + "\\" + filename;
            return System.IO.File.Exists(filenameToCheck);
        }

        public byte[] GetPrimaryThumbnailForPersonFromDataBase(int ownerId)
        {
            var imageToReturn = new byte[0];
            var connectionString = "URI=file:" + _rootsMagicToDigiKamDataBaseFileNameWithFullPath;  //PersonDigiKamTag
            using (IDbConnection dbConnection = new SqliteConnection(connectionString)) {
                dbConnection.Open();
                try {
                    IDbCommand dbcmd = dbConnection.CreateCommand();
                    // Step 1: Attach secondary database
                    var attachCommands = $"ATTACH DATABASE '{_digiKamThumbnailsDataBaseFileNameWithFullPath}' AS 'thumbnailsdigikam';";
                                    
                    attachCommands += $"ATTACH DATABASE '{_digiKamDataBaseFileNameWithFullPath}' AS 'digikam4';";

                    // Step 2: Query for the thumbnail
                    int limitListSizeTo = 1;
                    string sqlQuery =
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
                        "INNER JOIN thumbnailsdigikam.FilePaths paths ON fullPathToFileName = paths.path\n" +
                        "INNER JOIN thumbnailsdigikam.Thumbnails tnails ON paths.thumbId = tnails.id\n";
                    sqlQuery +=
                        $"WHERE r2d.PersonID = \"{ownerId}\" AND images.album IS NOT NULL;";

                    dbcmd.CommandText = attachCommands + sqlQuery;
                    var reader = dbcmd.ExecuteReader();
                    int currentArrayIndex = 0;
                    while (reader.Read() && currentArrayIndex < limitListSizeTo) {
                        string pathToFullResolutionImage = (string)reader["fullPathToFileName"];
                        
                        if (System.IO.File.Exists(pathToFullResolutionImage)) {
                            imageToReturn = System.IO.File.ReadAllBytes(pathToFullResolutionImage);
                        }
                        else {
                            imageToReturn = null;
                        }

                        currentArrayIndex++;
                    }
                }
                catch (Exception ex) {
                    Debug.Log("An error occurred: " + ex.Message);
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
