﻿using B2Net.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using B2Net.Http;

namespace B2Net.Tests {
	[TestClass]
	public class PublicFileTests : BaseTest {
		private B2Bucket TestBucket = new B2Bucket();
		private B2Client Client = null;
		private List<B2File> FilesToDelete = new List<B2File>();
		private string BucketName = "B2NETTestingBucketPublic";

#if NETFULL
		private string FilePath => Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "../../../");
#else
		private string FilePath => Path.Combine(System.AppContext.BaseDirectory, "../../../");
#endif

		[TestInitialize]
		public void Initialize() {
			Client = CreateB2ClientWithNormalKey();
			Client.Authorize().Wait();

			var buckets = Client.Buckets.GetList().Result;
			B2Bucket existingBucket = null;
			foreach (B2Bucket b2Bucket in buckets) {
				if (b2Bucket.BucketName == BucketName) {
					existingBucket = b2Bucket;
				}
			}

			if (existingBucket != null) {
				TestBucket = existingBucket;
			}
			else {
				TestBucket = Client.Buckets.Create(BucketName, BucketTypes.allPublic).Result;
			}
		}

		[TestMethod]
		public async Task FileGetFriendlyUrlTest() {
			var fileName = "B2Test.txt";
			var fileData = File.ReadAllBytes(Path.Combine(FilePath, fileName));
			string hash = Utilities.GetSHA1Hash(fileData);
			var file = Client.Files.Upload(fileData, fileName, TestBucket.BucketId).Result;
			// Clean up.
			FilesToDelete.Add(file);

			Assert.AreEqual(hash, file.ContentSHA1, "File hashes did not match.");

			// Get url
			var friendlyUrl = await Client.Files.GetFriendlyDownloadUrl(fileName, TestBucket.BucketName);

			// Test download
			var client = new HttpClient();
			var friendFile = client.GetAsync(friendlyUrl).Result;
			var ffileData = friendFile.Content.ReadAsByteArrayAsync().Result;
			var downloadHash = Utilities.GetSHA1Hash(ffileData);

			Assert.AreEqual(hash, downloadHash);
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
