using UnityEngine;
using System.Collections;

namespace Restifizer {
	public class RestifizerError: System.Exception {
		public int Status;
		public Hashtable ErrorRaw;
		public ArrayList ErrorListRaw;
        public string ErrorText;
		public string Tag;
		public string Url;
        public Hashtable Parameters;
        
        public enum SpecialStatus
        {
            Timeout = -1,
            BadJSON = -2,
            UnsupportedJSONType = -3,
            Count = 4
        };
        
		public RestifizerError(int status, object error, string tag, string url, Hashtable parameters) {
			this.Status = status;
			this.Tag = tag;
            this.Url = url;
            this.Parameters = parameters;

			if (error is ArrayList) {
				this.ErrorListRaw = (ArrayList)error;
			} else if (error is Hashtable) {
				this.ErrorRaw = (Hashtable)error;
			} else if (error is string) {
				this.ErrorText = (string)error;
			} else if (error != null) {
				Debug.LogWarning("Unsupported type in response: " + error.GetType());
			}

			parse();
		}
		
		virtual protected void parse() {
		}
		
		override public string ToString() {
            string statusString = "";
            if ( Status == (int) SpecialStatus.Timeout )
            {
                statusString = " (Probably timeout) ";
            }
            else if ( Status == (int) SpecialStatus.BadJSON )
            {
                statusString = " (Bad JSON) ";
            }
            else if ( Status == (int) SpecialStatus.UnsupportedJSONType )
            {
                statusString = " (unsupported JSON data type) ";
            }
            
            string paramsString = "";
            if ( Parameters != null )
            {
                paramsString = "\nParameters: " + JSON.Stringify( Parameters );
            }
            
			string result = "URL: " + Url + "\nStatus: " + Status + statusString + "\nTag: " + Tag + paramsString + "\nRaw: ";
			if (ErrorRaw != null) {
				result += JSON.Stringify(ErrorRaw);
			} else if (ErrorListRaw != null) {
				result += JSON.Stringify(ErrorListRaw);
            } else if (ErrorText != null && !ErrorText.Equals(string.Empty)) {
                result += ErrorText;
			} else {
				result += "<EMPTY>";
			}
			
			return result;
		}
	}

	public class BadRequestError: RestifizerError {
		public BadRequestError(int status, object error, string tag, string url, Hashtable parameters): base(status, error, tag, url, parameters) {
		}

		protected override void parse() {
			// TODO: Implement
		}
	}
	
	public class UnauthorizedError: RestifizerError {
		public new string Message;

		public UnauthorizedError(int status, object error, string tag, string url, Hashtable parameters): base(status, error, tag, url, parameters) {
		}
		
		protected override void parse() {
			Message = ErrorRaw["message"] as string;
		}
	}

	public class ForbiddenError: RestifizerError {
		public ForbiddenError(int status, object error, string tag, string url, Hashtable parameters): base(status, error, tag, url, parameters) {
		}
		
		protected override void parse() {
			// TODO: Implement
		}
	}

	public class NotFoundError: RestifizerError {
		public NotFoundError(int status, object error, string tag, string url, Hashtable parameters): base(status, error, tag, url, parameters) {
		}
		
		protected override void parse() {
			// TODO: Implement
		}
	}

	public class ServerNotAvailableError: RestifizerError {
		public ServerNotAvailableError(string tag, string url, Hashtable parameters): base(-1, null, tag, url, parameters) {
		}
	}
	
	public class WrongResponseFormatError: RestifizerError {
		public string UnparsedResponse;

		public WrongResponseFormatError(object error, string tag, string url, Hashtable parameters): base(-2, null, tag, url, parameters) {
			UnparsedResponse = error as string;
		}
	}
}
