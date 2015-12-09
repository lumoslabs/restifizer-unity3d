using UnityEngine;
using System.Collections;

namespace Restifizer {
	public class RestifizerManager: MonoBehaviour, RestifizerParams {
		
		public string baseUrl;
        public string baseStagingUrl;
        public bool useStagingUrl = false;
        public bool useDataRootInParameters = false;
        public string clientIdKey = "client_id";
        public string clientSecretKey = "client_secret";
		public MonoBehaviour errorHandler;
		public static RestifizerManager Instance;
		
		private string clientId;
		private string clientSecret;
		private string accessToken;

		virtual public void Awake() {
			if (errorHandler != null && !(errorHandler is IErrorHandler)) {
				Debug.LogError("Wrong ErrorHandler, it should implement IErrorHandler");
			}
			Instance = this;
		}

		public RestifizerManager ConfigClientAuth(string clientId, string clientSecret) {
			this.clientId = clientId;
			this.clientSecret = clientSecret;
			return this;
		}
		
		public RestifizerManager ConfigBearerAuth(string accessToken) {
			this.accessToken = accessToken;
			return this;
		}
		
        public RestifizerRequest ResourceAt(string resourceName) {
            return ResourceAt(resourceName, useStagingUrl ? baseStagingUrl : baseUrl);
        }
        
		public RestifizerRequest ResourceAt(string resourceName, string thisBaseUrl) {
			RestifizerRequest restifizerRequest = new RestifizerRequest(this, (IErrorHandler)errorHandler);
			restifizerRequest.FetchList = true;
			restifizerRequest.Path += thisBaseUrl + "/" + resourceName;

			return restifizerRequest;
		}
		
		
		/* RestifizerParams */
		public string GetClientId() {
			return clientId;
		}
		
		public string GetClientSecret() {
			return clientSecret;
		}
		
		public string GetAccessToken() {
			return accessToken;
		}
        
        public bool UseDataRootInParameters() {
            return useDataRootInParameters;
        }
        
        public string GetClientIdKey() {
            return clientIdKey;
        }
        
        public string GetClientSecretKey() {
            return clientSecretKey;
        }
	}
}
