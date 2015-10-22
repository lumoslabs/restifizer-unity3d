using UnityEngine;
using System.Collections;

namespace Restifizer {
	public class RestifizerError: System.Exception {
		public int Status;
		public Hashtable ErrorRaw;
		public ArrayList ErrorListRaw;
		public string Tag;
		
        public enum SpecialStatus
        {
            Timeout = -1,
            BadJSON = -2,
            UnsupportedJSONType = -3,
            Count = 4
        };
        
		public RestifizerError(int status, object error, string tag) {
			this.Status = status;
			this.Tag = tag;

			if (error is ArrayList) {
				this.ErrorListRaw = (ArrayList)error;
			} else if (error is Hashtable) {
				this.ErrorRaw = (Hashtable)error;
			} else if (error != null) {
				Debug.LogWarning("Unsupported type in response: " + error.GetType());
			}

			parse();
		}
		
		virtual protected void parse() {
		}
		
		override public string ToString() {
            string statusString = "";
            if ( Status == -1 )
            {
                statusString = " (Probably timeout) ";
            }
            else if ( Status == -2 )
            {
                statusString = " (Bad JSON) ";
            }
            else if ( Status == -3 )
            {
                statusString = " (unsupported JSON data type) ";
            }
            
			string result = "tag: " + Tag + ", status: " + Status + statusString + ", raw: ";
			if (ErrorRaw != null) {
				result += JSON.Stringify(ErrorRaw);
			} else if (ErrorListRaw != null) {
				result += JSON.Stringify(ErrorListRaw);
			} else {
				result += "<EMPTY>";
			}
			
			return result;
		}
	}

	public class BadRequestError: RestifizerError {
		public BadRequestError(int status, object error, string tag): base(status, error, tag) {
		}

		protected override void parse() {
			// TODO: Implement
		}
	}
	
	public class UnauthorizedError: RestifizerError {
		public new string Message;

		public UnauthorizedError(int status, object error, string tag): base(status, error, tag) {
		}
		
		protected override void parse() {
			Message = ErrorRaw["message"] as string;
		}
	}

	public class ForbiddenError: RestifizerError {
		public ForbiddenError(int status, object error, string tag): base(status, error, tag) {
		}
		
		protected override void parse() {
			// TODO: Implement
		}
	}

	public class NotFoundError: RestifizerError {
		public NotFoundError(int status, object error, string tag): base(status, error, tag) {
		}
		
		protected override void parse() {
			// TODO: Implement
		}
	}

	public class ServerNotAvailableError: RestifizerError {
		public ServerNotAvailableError(string tag): base(-1, null, tag) {
		}
	}
	
	public class WrongResponseFormatError: RestifizerError {
		public string UnparsedResponse;

		public WrongResponseFormatError(object error, string tag): base(-2, null, tag) {
			UnparsedResponse = error as string;
		}
	}
}
