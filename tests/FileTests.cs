﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Configuration;
using System.Text;
using System.Threading.Tasks;
using B2Net.Http;
using B2Net.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace B2Net.Tests {
	[TestClass]
	public class FileTests : BaseTest {
		private B2Bucket TestBucket = new B2Bucket();
		private B2Client Client = null;
		private List<B2File> FilesToDelete = new List<B2File>();

		[TestInitialize]
		public void Initialize() {
			Client = new B2Client(Options);
			Options = Client.Authorize().Result;
			TestBucket = Client.Buckets.Create("B2NETTestingBucket", BucketTypes.allPrivate).Result;
		}

		[TestMethod]
		public void GetListTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			var list = Client.Files.GetList(bucketId: TestBucket.BucketId).Result.Files;

			Assert.AreEqual(1, list.Count, list.Count + " files found.");
		}

		[TestMethod]
		public void HideFileTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
			
			var hiddenFile = Client.Files.Hide(file.FileName, TestBucket.BucketId).Result;

			Assert.IsTrue(hiddenFile.Action == "hide");
		}

		[TestMethod]
		public void FileUploadTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

			// Clean up.
			FilesToDelete.Add(file);
		}

		[TestMethod]
		public void FileDownloadNameTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

			// Test download
			var download = Client.Files.DownloadByName(file.FileName, TestBucket.BucketName).Result;
			var downloadHash = Utilities.GetSHA1Hash(download.FileData);

			Assert.AreEqual(hash, downloadHash);
		}

		[TestMethod]
		public void FileDownloadIdTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

			// Test download
			var download = Client.Files.DownloadById(file.FileId).Result;
			var downloadHash = Utilities.GetSHA1Hash(download.FileData);

			Assert.AreEqual(hash, downloadHash);
		}

		[TestMethod]
		public void FileDeleteTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

			// Clean up. We have to delete the file before we can delete the bucket
			var deletedFile = Client.Files.Delete(file.FileId, file.FileName).Result;

			Assert.AreEqual(file.FileId, deletedFile.FileId, "The deleted file id did not match.");
		}

		[TestMethod]
		public void ListVersionsTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");
			
			var versions = Client.Files.GetVersions(file.FileName, file.FileId, bucketId: TestBucket.BucketId).Result;

			Assert.AreEqual(1, versions.Files.Count);
		}

		[TestMethod]
		public void GetInfoTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

			var info = Client.Files.GetInfo(file.FileId).Result;

			Assert.AreEqual(file.UploadTimestamp, info.UploadTimestamp);
		}

		[TestCleanup]
		public void Cleanup() {
			foreach (B2File b2File in FilesToDelete) {
				var deletedFile = Client.Files.Delete(b2File.FileId, b2File.FileName).Result;
			}
			var deletedBucket = Client.Buckets.Delete(TestBucket.BucketId).Result;
		}
	}
}
