﻿using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using GQ.Util;
using System.IO;
using System;
using GQ.Editor.Util;

namespace GQTests.Editor.Util {

	public class AssetsTest {

		private static string ASSETS_TEST_DIR = Files.CombinePath(GQAssert.TEST_DATA_BASE_DIR, "AssetsTest");
		private static string EMPTY_DIR = Files.CombinePath(ASSETS_TEST_DIR, "EmptyFolder");
		private static string GIVEN_ASSETS_DIR = Files.CombinePath(ASSETS_TEST_DIR, "GivenAssets");
		private static string GIVEN_RECURSIVE_ASSETS_DIR = Files.CombinePath(ASSETS_TEST_DIR, "GivenRecursiveAssets");


		[SetUp]
		public void SetUp () {
			Assets.ClearAssetFolder(EMPTY_DIR);
		}

		[TearDown]
		public void TearDown () {
			Assets.ClearAssetFolder(EMPTY_DIR);
		}



		[Test]
		public void AssetsRelAndAbsPaths () {
			// Arrange:

			// Act:
			string relPath = Files.CombinePath("Assets", "MySubdir", "FurtherSubDir");
			string absPath = Files.CombinePath(Application.dataPath, "MySubdir", "FurtherSubDir");

			// Assert:
			Assert.AreEqual(absPath, Assets.AbsolutePath(relPath));
		}

		[Test]
		public void CreateFolder () {
			// Arrange:
			string emptyDir = Files.CombinePath(ASSETS_TEST_DIR, "EmptyFolder");
			string newDir = Files.CombinePath(emptyDir, "NewDir");

			// Pre Assert:
			Assert.That(new System.IO.DirectoryInfo(emptyDir), Is.Empty);

			// Act:
			AssetDatabase.CreateFolder(emptyDir, "NewDir");
			AssetDatabase.SaveAssets();
			AssetDatabase.Refresh();

			// Assert:
			Assert.That(new System.IO.DirectoryInfo(emptyDir), Is.Not.Empty);
			Assert.That(Directory.Exists(newDir));
			Assert.That(Assets.Exists(newDir));
		}


		[Test]
		public void ExistsAsset () {
			// Arrange:
//			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
			string newAssetPath = Files.CombinePath(EMPTY_DIR, "Text.txt");

			// Pre Assert:
			Assert.That(!File.Exists(newAssetPath));
			Assert.That(!Assets.Exists(newAssetPath));

			// Act Create:
			using ( StreamWriter sw = File.CreateText(Assets.AbsolutePath(newAssetPath)) ) {
				sw.WriteLine("Some text content.");
				sw.Close();
			}

			AssetDatabase.Refresh();

			// Assert:
			Assert.That(File.Exists(newAssetPath));
			Assert.That(Assets.Exists(newAssetPath));

			// Act Delete:
			AssetDatabase.DeleteAsset(newAssetPath);

			// Post Assert:
			Assert.That(!File.Exists(newAssetPath));
			Assert.That(!Assets.Exists(newAssetPath));
		}

		[Test]
		public void IsAssetPath () {
			// Arrange relative asset paths:
			string relPathToExistingAssetsFile = 
				Files.CombinePath(GQAssert.TEST_DATA_BASE_DIR, "AssetsTest", "GivenAssets", "Image.png");
			string relPathToExistingAssetsDir = 
				Files.CombinePath(GQAssert.TEST_DATA_BASE_DIR, "AssetsTest", "EmptyFolder");
			string relPathToNonExistingAssetsFile = 
				Files.CombinePath(GQAssert.TEST_DATA_BASE_DIR, "AssetsTest", "GivenAssets", "DoesNotExi.st");

			// Assert:
			Assert.That(
				Assets.IsAssetPath(relPathToExistingAssetsFile), 
				"Existing asset file should be validated by relative path.");
			Assert.That(
				Assets.IsAssetPath(relPathToExistingAssetsDir), 
				"Existing asset dir should be validated by relative path.");
			Assert.That(
				!Assets.IsAssetPath(relPathToNonExistingAssetsFile), 
				"Non-Existing asset file should NOT be validated by relative path.");


			// Arrange absolute asset paths:
			string absPathToExistingAssetsFile = 
				Files.CombinePath(Application.dataPath, "Editor", "GQTestsData", "AssetsTest", "GivenAssets", "Image.png");
			string absPathToExistingAssetsDir = 
				Files.CombinePath(Application.dataPath, "Editor", "GQTestsData", "AssetsTest", "EmptyFolder");
			string absPathToNonExistingAssetsFile = 
				Files.CombinePath(Application.dataPath, "Editor", "GQTestsData", "AssetsTest", "GivenAssets", "DoesNotExi.st");
			
			// Assert:
			Assert.That(
				Assets.IsAssetPath(absPathToExistingAssetsFile), 
				"Existing asset file should be validated by absolute path.");
			Assert.That(
				Assets.IsAssetPath(absPathToExistingAssetsDir), 
				"Existing asset dir should be validated by absolute path.");
			Assert.That(
				!Assets.IsAssetPath(absPathToNonExistingAssetsFile), 
				"Non-Existing asset file should NOT be validated by absolute path.");
					
			// Arrange NON asset paths:
			string PROJECT_PATH = Application.dataPath.Substring(0, Application.dataPath.Length - "/Assets".Length);
			string pathToExistingNonAssetsFile = 
				Files.CombinePath(PROJECT_PATH, "TestsData", "NonEmptyDir", "Image.png");
			string pathToExistingNonAssetsDir = 
				Files.CombinePath(GQAssert.TEST_DATA_BASE_DIR, "TestsData", "EmptyDir");
			string pathToNonExistingNonAssetsFile = 
				Files.CombinePath(GQAssert.TEST_DATA_BASE_DIR, "TestsData", "EmptyDir", "DoesNotExi.st");

			// Assert:
			Assert.That(
				!Assets.IsAssetPath(pathToExistingNonAssetsFile), 
				"Existing non asset file should not be validated by path.");
			Assert.That(
				!Assets.IsAssetPath(pathToExistingNonAssetsDir), 
				"Existing non asset dir should not be validated by path.");
			Assert.That(
				!Assets.IsAssetPath(pathToNonExistingNonAssetsFile), 
				"Non-Existing non asset file should NOT be validated by path.");
		}


		[Test]
		public void CopyFlatAssetsFolder () {
			// Arrange:
			string newDir = Files.CombinePath(EMPTY_DIR, "newDir");
			DirectoryInfo newDirInfo = new DirectoryInfo(newDir);

			Directory.CreateDirectory(Assets.AbsolutePath(newDir));

			// Pre Assert:
			Assert.That(newDirInfo, Is.Empty);

			// Act:
			Assets.CopyAssetsDir(GIVEN_ASSETS_DIR, newDir);

			// Post Assert:
			Assert.That(newDirInfo, Is.Not.Empty);

			AssertThatAllGivenAssetsExistInDir(newDir);
		}

		[Test]
		public void CopyDeepAssetsFolder () {
			string newDir = Files.CombinePath(EMPTY_DIR, "newDir");
			DirectoryInfo newDirInfo = new DirectoryInfo(newDir);

			Directory.CreateDirectory(Assets.AbsolutePath(newDir));

			// Pre Assert:
			Assert.That(newDirInfo, Is.Empty);

			// Act:
			Assets.CopyAssetsDir(GIVEN_RECURSIVE_ASSETS_DIR, newDir);

			// Post Assert:
			Assert.That(newDirInfo, Is.Not.Empty);

			string pathToSubfolder = Files.CombinePath(newDirInfo.FullName, "Subfolder");
			Assert.That(Directory.Exists(pathToSubfolder), "subfolder should have been copied to " + pathToSubfolder);
			AssertThatAllGivenAssetsExistInDir(Assets.RelativeAssetPath(pathToSubfolder));

			string pathToDeepSubfolder = Files.CombinePath(newDirInfo.FullName, "Subfolder", "DeepSubfolder");
			Assert.That(Directory.Exists(pathToDeepSubfolder), "deep subfolder should have been copied to " + pathToDeepSubfolder);
			AssertThatAllGivenAssetsExistInDir(Assets.RelativeAssetPath(pathToDeepSubfolder));

			string pathToDeepSubfolder2 = Files.CombinePath(newDirInfo.FullName, "Subfolder", "DeepSubfolder2");
			Assert.That(Directory.Exists(pathToDeepSubfolder2), "deep subfolder should have been copied to " + pathToDeepSubfolder2);
			AssertThatAllGivenAssetsExistInDir(Assets.RelativeAssetPath(pathToDeepSubfolder2));
		}


		private void AssertThatAllGivenAssetsExistInDir (string dir) {

			DirectoryInfo givenAssetsDir = new DirectoryInfo(GIVEN_ASSETS_DIR);
			foreach ( FileInfo givenFile in givenAssetsDir.GetFiles() ) {
				if ( givenFile.Name.EndsWith(".meta") )
					continue;

				string targetFilePathRel = Files.CombinePath(dir, givenFile.Name);
				string targetFilePathAbs = Assets.AbsolutePath(targetFilePathRel);
				Assert.That(File.Exists(targetFilePathAbs), "File should have been copied to: " + targetFilePathAbs);
				Assert.That(Assets.Exists(targetFilePathRel), "Asset should have been copied to: " + targetFilePathRel);
			}
		}


		[Test]
		public void ClearAssetsFolder () {
			// Arrange: we place some iven assets into a new directory so that we can clear that directory and check that clearing works:
			string newDir = Files.CombinePath(EMPTY_DIR, "newDir");
			DirectoryInfo newDirInfo = new DirectoryInfo(newDir);

			Directory.CreateDirectory(Assets.AbsolutePath(newDir));
			Assets.CopyAssetsDir(GIVEN_ASSETS_DIR, newDir);

			// Act:
			Assets.ClearAssetFolder(newDir);

			// Assert:
			Assert.That(newDirInfo, Is.Empty);

			foreach ( FileInfo givenFile in newDirInfo.GetFiles() ) {
				if ( givenFile.Name.EndsWith(".meta") )
					continue;

				string targetFilePathRel = Files.CombinePath(newDir, givenFile.Name);
				string targetFilePathAbs = Assets.AbsolutePath(targetFilePathRel);
				Assert.That(!File.Exists(targetFilePathAbs), "File should have been deleted: " + targetFilePathAbs);
				Assert.That(!Assets.Exists(targetFilePathRel), "Asset should have been deleted: " + targetFilePathRel);
			}
		}

	}
}
