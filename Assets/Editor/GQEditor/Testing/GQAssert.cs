﻿using System.IO;
using NUnit.Framework;
using System;
using System.Security;
using GQ.Client.Util;
using UnityEngine;
using GQ.Editor.Util;

namespace GQTests
{

	public class GQAssert
	{

		// TODO move these paths to Files class:

		static private string _TEST_DATA_BASE_DIR = "Assets/Editor/GQTestsData/";

		static public string TEST_DATA_BASE_DIR {
			get {
				return _TEST_DATA_BASE_DIR;
			}
		}

		//		static private string _PROJECT_PATH = Application.dataPath.Substring (0, Application.dataPath.Length - "/Assets".Length);
//		static private string _PROJECT_PATH = "/Users/muegge/projects/qv-geoquest/GQUnityClient"; // On MacBookPro
		static private string _PROJECT_PATH = "/Users/qeeveedeveloper/GeoQuest/GQUnityClient"; // On MacAir

		public static string PROJECT_PATH {
			get {
				return _PROJECT_PATH;
			}
		}

		public static string UniqueNameInDir (string nameGiven, string dirPath)
		{
			string curNameChecking = nameGiven;

			bool nameAlreadyTaken = checkIfNameExistsInDir (curNameChecking, dirPath);

			string strippedName = Files.StripExtension (nameGiven);
			string extension = Files.Extension (nameGiven);
			int counter = 1;

			while (nameAlreadyTaken) {
				curNameChecking = strippedName + counter;
				if (extension.Length > 0)
					curNameChecking += '.' + extension;
				
				nameAlreadyTaken = checkIfNameExistsInDir (curNameChecking, dirPath);
				counter++;
			}

			return curNameChecking;
		}

		private static bool checkIfNameExistsInDir (string nameToCheck, string dir)
		{
			FileInfo fileWithSameName = new FileInfo (Files.CombinePath (dir, nameToCheck));
			if (fileWithSameName.Exists)
				return true;

			DirectoryInfo dirWithSameName = new DirectoryInfo (Files.CombinePath (dir, nameToCheck));
			return dirWithSameName.Exists;
		}

	}

}
