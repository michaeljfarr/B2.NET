using System;
using System.Text;

namespace B2Net.Models {
	public class B2Config {
		public B2Config() {
		}

		public B2Config(string keyId, string applicationKey, int requestTimeout) {
			KeyId = keyId;
			ApplicationKey = applicationKey;
			RequestTimeout = requestTimeout;
		}

		public string KeyId { get; init; }
		public string ApplicationKey { get; init; }
		public string KeyName { get; init; }
		public int RequestTimeout { get; init; }
		public string BucketId { get; init; }
	}
}
