using UnityEngine;
using System.Collections;

namespace Restifizer {
	public class RestifizerErrorFactory {
		public static RestifizerError Create(int status, object error, string tag, string url, Hashtable parameters) {
			switch (status) {
			case -1: 
				return new ServerNotAvailableError(tag, url, parameters);
			case -2: 
				return new WrongResponseFormatError(error, tag, url, parameters);
			case 400: 
				return new BadRequestError(status, error, tag, url, parameters);
			case 401: 
				return new UnauthorizedError(status, error, tag, url, parameters);
			case 403: 
				return new ForbiddenError(status, error, tag, url, parameters);
			case 404: 
				return new NotFoundError(status, error, tag, url, parameters);
			default: 
				return new RestifizerError(status, error, tag, url, parameters);
			}
		}
	}
}
