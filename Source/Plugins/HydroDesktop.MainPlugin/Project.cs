﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DotSpatial.Data;
using DotSpatial.Controls;
using DotSpatial.Projections;
using HydroDesktop.Configuration;
using HydroDesktop.Interfaces;
using HydroDesktop.Database;
using HydroDesktop.Interfaces.ObjectModel;
using System.Reflection;
using System.Xml;
//using NDepend.Helpers.FileDirectoryPath;
using DotSpatial.Topology;
using DotSpatial.Symbology;
using System.Diagnostics;
using System.Security.AccessControl;
using HydroDesktop.Controls;
using HydroDesktop.Controls.Themes;

namespace HydroDesktop.Main
{
    public class Project
    {
        public static ProjectionInfo DefaultProjection { get { return KnownCoordinateSystems.Projected.World.WebMercator; } }

        //sets the map extent to continental U.S
        private static void SetDefaultMapExtents(Map mainMap)
        {
            mainMap.ViewExtents = DefaultMapExtents().ToExtent();
        }

        public static Envelope DefaultMapExtents()
        {
            Envelope _defaultMapExtent = new Envelope(-130, -60, 10, 55);


            double[] xy = new double[4];
            xy[0] = _defaultMapExtent.Minimum.X;
            xy[1] = _defaultMapExtent.Minimum.Y;
            xy[2] = _defaultMapExtent.Maximum.X;
            xy[3] = _defaultMapExtent.Maximum.Y;
            double[] z = new double[] { 0, 0 };
            ProjectionInfo wgs84 = KnownCoordinateSystems.Geographic.World.WGS1984;
            Reproject.ReprojectPoints(xy, z, wgs84, DefaultProjection, 0, 2);

            return new Envelope(xy[0], xy[2], xy[1], xy[3]);
        }

        private static void CopyStream(Stream input, Stream output)
        {
            byte[] buffer = new byte[8192];

            int bytesRead;
            while ((bytesRead = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                output.Write(buffer, 0, bytesRead);
            }
        }

        /// <summary>
        /// Check if the path is a valid SQLite database
        /// This function returns false, if the SQLite db
        /// file doesn't exist or if the file size is 0 Bytes
        /// </summary>
        public static bool DatabaseExists(string dbPath)
        {
            return SQLiteHelper.DatabaseExists(dbPath);
        }

        /// <summary>
        /// To get the SQLite database path given the SQLite connection string
        /// </summary>
        public static string GetSQLiteFileName(string sqliteConnString)
        {
            return SQLiteHelper.GetSQLiteFileName(sqliteConnString);
        }
        /// <summary>
        /// To get the full SQLite connection string given the SQLite database path
        /// </summary>
        public static string GetSQLiteConnectionString(string dbFileName)
        {
            return SQLiteHelper.GetSQLiteConnectionString(dbFileName);
        }

        /// <summary>
        /// Create the default .SQLITE database in the user-specified path
        /// </summary>
        /// <returns>true if database was created, false otherwise</returns>
        public static Boolean CreateNewDatabase(string dbPath)
        {
            //to create the default.sqlite database file using the SQLiteHelper method
            return SQLiteHelper.CreateSQLiteDatabase(dbPath);
        }

        /// <summary>
        /// Checks if the two paths are on the same drive.
        /// </summary>
        /// <param name="path1">the first path</param>
        /// <param name="path2">the second path</param>
        /// <returns>true if the two paths are on same drive</returns>
        private static Boolean IsSameDrive(string path1, string path2)
        {
            if (Path.IsPathRooted(path1) && Path.IsPathRooted(path2) && !path1.StartsWith("\\\\") && !path2.StartsWith("\\\\"))
            {
                if (Path.GetPathRoot(path1) == Path.GetPathRoot(path2))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Opens a project and updates the maps
        /// </summary>
        public static void OpenProject(string projectFileName, AppManager applicationManager)
        {
            applicationManager.ProgressHandler.Progress("Opening Project", 0, "Opening Project");
            applicationManager.SerializationManager.OpenProject(projectFileName);
            applicationManager.ProgressHandler.Progress("Project opened", 0, "");
        }

        /// <summary>
        /// Creates a new project, using a predefined project template to load the base maps.
        /// </summary>
        /// <param name="templateName">The project template name</param>
        public static void CreateNewProject(string templateName, AppManager appManager, Map map)
        {
            switch (templateName)
            {
                case "North America":
                    NorthAmericaProjectTemplate.LoadBaseMaps(appManager, map);
                    break;
                case "World":
                    WorldProjectTemplate.LoadBaseMaps(appManager, map);
                    break;
                default:
                    NorthAmericaProjectTemplate.LoadBaseMaps(appManager, map);
                    break;
            }
        }

        /// <summary>
        /// Creates a new 'empty' project
        /// </summary>
        public static void CreateEmptyProject(IMap map)
        {
            map.Layers.Clear();
        }

        //saves the current HydroDesktop project file to the user specified location
        public static void SaveProjectAs(string projectFileName, AppManager appManager)
        {
            Settings.Instance.AddFileToRecentFiles(projectFileName);

            string newProjectDirectory = Path.GetDirectoryName(projectFileName);

            appManager.ProgressHandler.Progress("Saving Project " + projectFileName, 0, "");
            Application.DoEvents();

            //also create a copy of the .sqlite database
            string newDbPath = Path.ChangeExtension(projectFileName, ".sqlite");

            //current database path
            string currentDbPath = SQLiteHelper.GetSQLiteFileName(Settings.Instance.DataRepositoryConnectionString);
            //copy db to new path. If no db exists, create new db in the new location
            if (SQLiteHelper.DatabaseExists(currentDbPath))
            {
                File.Copy(currentDbPath, newDbPath, true);
            }
            else
            {
                Project.CreateNewDatabase(newDbPath);
            }
            //create a copy of the metadata cache (_cache.sqlite) database
            string newCachePath = projectFileName.Replace(".dspx", "_cache.sqlite");

            //current database path
            string currentCachePath = SQLiteHelper.GetSQLiteFileName(Settings.Instance.MetadataCacheConnectionString);
            //copy db to new path. If no db exists, create new db in the new location
            if (SQLiteHelper.DatabaseExists(currentCachePath))
            {
                File.Copy(currentCachePath, newCachePath, true);
            }
            else
            {
                SQLiteHelper.CreateMetadataCacheDb(newCachePath);
            }


            //save the current project to the new db path
            appManager.SerializationManager.SaveProject(projectFileName);

            //TODO: need to trigger a DatabaseChanged event (Settings.Instance.DatabaseChanged..)

            //update application level database configuration settings
            Settings.Instance.DataRepositoryConnectionString = SQLiteHelper.GetSQLiteConnectionString(newDbPath);
            Settings.Instance.MetadataCacheConnectionString = SQLiteHelper.GetSQLiteConnectionString(newCachePath);
            Settings.Instance.CurrentProjectFile = appManager.SerializationManager.CurrentProjectFile;
        }

        /// <summary>
        /// Finds the map group with the specific name
        /// </summary>
        /// <param name="map">the map to search</param>
        /// <param name="groupLegendText">the legend text of the map  group</param>
        /// <returns></returns>
        private static IMapGroup FindGroupByName(Map map, string groupLegendText)
        {
            foreach (LegendItem item in map.Layers)
            {
                if (item is IMapGroup && item.LegendText.ToLower() == groupLegendText.ToLower())
                {
                    return (IMapGroup)item;
                }
            }
            return null;
        }

        public static string GetMapDirectory()
        {
            string binariesDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (binariesDirectory.EndsWith(@"\")) binariesDirectory =
                binariesDirectory.Substring(0, binariesDirectory.Length - 1);
            DirectoryInfo baseDirInfo = Directory.GetParent(binariesDirectory);
            string baseDirectory = baseDirInfo.FullName;

            string baseMapFolder1 = baseDirectory + Path.DirectorySeparatorChar +
                @"Maps\BaseData-MercatorSphere";
            string baseMapFolder2 = Path.Combine(binariesDirectory, @"Maps\BaseData-MercatorSphere");

            if (Directory.Exists(baseMapFolder1))
            {
                return baseMapFolder1;
            }
            else if (Directory.Exists(baseMapFolder2))
            {
                return baseMapFolder2;
            }
            else
            {
                MessageBox.Show("error loading base map data. The directory " +
                    baseMapFolder2 + " does not exist.");
                return "";
            }
        }
    }
}