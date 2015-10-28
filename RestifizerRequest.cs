//#define VERBOSE_LOGGING
#define ERROR_LOGGING

using UnityEngine;
using System;
using System.Collections;

namespace Restifizer {
	public class RestifizerRequest {
		enum AuthType {
			None,
			Client,
			Bearer
		};
		public string Path;
		public string Method;
		public bool FetchList = true;
		public string Tag;
		public int PageNumber = -1;
		public int PageSize = -1;


		private Hashtable filterParams;

		private Hashtable extraQuery;

		private AuthType authType = AuthType.None;
		
		private RestifizerParams restifizerParams;
		private IErrorHandler errorHandler;
		
		public RestifizerRequest(RestifizerParams restifizerParams, IErrorHandler errorHandler) {
			this.restifizerParams = restifizerParams;
			this.errorHandler = errorHandler;
			
			this.Path = "";
#if VERBOSE_LOGGING
            HTTP.Request.verboseStatusLogging = true;
#endif
		}
		
		public RestifizerRequest WithClientAuth() {
			this.authType = AuthType.Client;
			
			return this;
		}
		
		public RestifizerRequest WithBearerAuth() {
			this.authType = AuthType.Bearer;
			
			return this;
		}

		public RestifizerRequest WithTag(string tag) {
			this.Tag = tag;
			
			return this;
		}

		public RestifizerRequest Filter(String key, object value) {
			if (filterParams == null) {
				filterParams = new Hashtable();
			}
			filterParams[key] = value;
			
			return this;
		}

		public RestifizerRequest Query(String key, object value) {
			if (extraQuery == null) {
				extraQuery = new Hashtable();
			}
			extraQuery[key] = value;
			
			return this;
		}
		
		public RestifizerRequest Page(int pageNumber, int pageSize) {
			this.PageNumber = pageNumber;
			this.PageSize = pageSize;
			
			return this;
		}
		
		public RestifizerRequest Page(int pageNumber) {
			this.PageNumber = pageNumber;

			return this;
		}

		public RestifizerRequest SetPageSize(int pageSize) {
			this.PageSize = pageSize;
			
			return this;
		}
		
		public RestifizerRequest One(String id) {
			this.Path += "/" + id;
			this.FetchList = false;
			
			return this;
		}
		
		public void List(String path) {
			this.Path += "/" + path;
			this.FetchList = true;
		}
		
        public void Get(Action<RestifizerResponse> callback = null) {
            Get(null, callback);
        }
		public void Get(Hashtable parameters = null, Action<RestifizerResponse> callback = null) {
            if (this.authType == AuthType.Client)
            {
                this.Path = GetAuthUrl( this.Path );
            }
            this.Path = GetUrlWithParams( this.Path, parameters );
			performRequest("get", parameters, callback);
		}
		
		public void Post(Hashtable parameters = null, Action<RestifizerResponse> callback = null) {
			performRequest("post", parameters, callback);
		}
		
		public void Put(Hashtable parameters = null, Action<RestifizerResponse> callback = null) {
			performRequest("put", parameters, callback);
		}
		
		public void Patch(Hashtable parameters = null, Action<RestifizerResponse> callback = null) {
			performRequest("patch", parameters, callback);
		}
		
		public RestifizerRequest Copy() {
			RestifizerRequest restifizerRequest = new RestifizerRequest(restifizerParams, errorHandler);
			restifizerRequest.Path = Path;
			restifizerRequest.Method = Method;
			restifizerRequest.Tag = Tag;
			restifizerRequest.PageNumber = PageNumber;
			restifizerRequest.PageSize = PageSize;
			restifizerRequest.FetchList = FetchList;
			if (filterParams != null) {
				restifizerRequest.filterParams = filterParams.Clone() as Hashtable;
			}
			if (extraQuery != null) {
				restifizerRequest.extraQuery = extraQuery.Clone() as Hashtable;
			}
			restifizerRequest.authType = authType;
			return restifizerRequest;
		}
		
		private void performRequest(string method, Hashtable parameters = null, Action<RestifizerResponse> callback = null) {
			
			HTTP.Request someRequest;
			
			string url = Path;
			string queryStr = "";

			// paging
			if (PageNumber != -1) {
				if (queryStr.Length > 0) {
					queryStr += "&";
				}
				queryStr += "per_page=" + PageNumber;
			}
			if (PageSize != -1) {
				if (queryStr.Length > 0) {
					queryStr += "&";
				}
				queryStr += "page=" + PageSize;
			}

			// filtering
			if (filterParams != null && filterParams.Count > 0) {
				if (queryStr.Length > 0) {
					queryStr += "&";
				}
				string filterValue = JSON.JsonEncode(filterParams);
				queryStr += "filter=" + filterValue;
			}

			// extra params
			if (extraQuery != null && extraQuery.Count > 0) {
				foreach (string key in extraQuery.Keys) {
					if (queryStr.Length > 0) {
						queryStr += "&";
					}
					queryStr += key + "=" + extraQuery[key];
				}
			}

			if (queryStr.Length > 0) {
				url += "?" + queryStr;
			}
			
            if (parameters != null && restifizerParams.UseDataRootInParameters())
            {
                Hashtable newParams = new Hashtable();
                newParams["data"] = parameters;
                parameters = newParams;
            }
			
			// Handle authentication
			if (this.authType == AuthType.Client && !method.Equals("get")) {
				if (parameters == null) {
					parameters = new Hashtable();
				}
                
                Hashtable insertObject = parameters;
                if ( restifizerParams.UseDataRootInParameters() )
                {
                    if ( parameters["data"] == null )
                    {
                        parameters["data"] = new Hashtable();
                        
                    }
                    insertObject = parameters["data"] as Hashtable;
                }
                
				insertObject.Add( restifizerParams.GetClientIdKey(), restifizerParams.GetClientId() );
				insertObject.Add( restifizerParams.GetClientSecretKey(), restifizerParams.GetClientSecret() );
				
				someRequest = new HTTP.Request(method, url, parameters);
			} else if (this.authType == AuthType.Bearer) {
				if (parameters == null) {
					someRequest = new HTTP.Request(method, url);
				} else {
					someRequest = new HTTP.Request(method, url, parameters);
				}
				someRequest.SetHeader("Authorization", "Bearer " + restifizerParams.GetAccessToken());
			} else {
				if (parameters == null) {
					someRequest = new HTTP.Request(method, url);
				} else {
					someRequest = new HTTP.Request(method, url, parameters);
				}
			}

#if VERBOSE_LOGGING
            Debug.Log( "RestifizerRequest: " + method + " " + url + "\nparams: " + JSON.Stringify(parameters));
#endif

			string tag = this.Tag;
			// Perform request
			someRequest.Send( ( request ) => {
				if (request.response == null) {
#if !VERBOSE_LOGGING && ERROR_LOGGING
                    Debug.LogError( "RestifizerRequest failed: " + method + " " + url + "\nparams: " + JSON.Stringify(parameters));
#endif
					RestifizerError error = RestifizerErrorFactory.Create(-1, null, tag, url, parameters);
					if (errorHandler != null) {
						bool propagateResult = !errorHandler.onRestifizerError(error);
						if (propagateResult) {
							callback(new RestifizerResponse(request, error, tag));
						}
					} else {
						callback(new RestifizerResponse(request, error, tag));
					}
					return;
				}
				bool result = false;
				object responseResult = JSON.JsonDecode(request.response.Text, ref result);
				if (!result) {
#if !VERBOSE_LOGGING && ERROR_LOGGING
                    Debug.LogError( "RestifizerRequest failed: " + method + " " + url + "\nparams: " + JSON.Stringify(parameters));
#endif
					RestifizerError error = RestifizerErrorFactory.Create(request.response.status, request.response.Text, tag, url, parameters);
					if (errorHandler != null) {
						bool propagateResult = !errorHandler.onRestifizerError(error);
						if (propagateResult) {
							callback(new RestifizerResponse(request, error, tag));
						}
					} else {
						callback(new RestifizerResponse(request, error, tag));
					}
					return;
				}

				bool hasError = request.response.status >= 300;
				if (hasError) {
#if !VERBOSE_LOGGING && ERROR_LOGGING
                    Debug.LogError( "RestifizerRequest failed: " + method + " " + url + "\nparams: " + JSON.Stringify(parameters));
#endif
					RestifizerError error = RestifizerErrorFactory.Create(request.response.status, responseResult, tag, url, parameters);
					if (errorHandler != null) {
						bool propagateResult = !errorHandler.onRestifizerError(error);
						if (propagateResult) {
							callback(new RestifizerResponse(request, error, tag));
						}
					} else {
						callback(new RestifizerResponse(request, error, tag));
					}
				} else if (responseResult is ArrayList) {
					callback(new RestifizerResponse(request, (ArrayList)responseResult, tag));
				} else if (responseResult is Hashtable) {
					callback(new RestifizerResponse(request, (Hashtable)responseResult, tag));
				} else {
					Debug.LogWarning("Unsupported type in response: " + responseResult.GetType());
					callback(new RestifizerResponse(request, RestifizerErrorFactory.Create(-3, responseResult, tag, url, parameters), tag));
				}
			});
		}
        
        //adds auth data to the passed URL. You MUST use this for all GET calls that require auth.
        protected string GetAuthUrl( string baseUrl )
        {
            string url = baseUrl;
            if ( url.Contains( "?" ) )
            {
                url += "&";
            }
            else
            {
                url += "?";
            }
            url += "data[" + restifizerParams.GetClientIdKey()     + "]=" + WWW.EscapeURL( restifizerParams.GetClientId() ) + "&";
            url += "data[" + restifizerParams.GetClientSecretKey() + "]=" + WWW.EscapeURL( restifizerParams.GetClientSecret() );
            return url;
        }
        
        protected string GetUrlWithParams( string baseUrl, Hashtable parameters )
        {
            if ( parameters == null || parameters.Count <= 0 )
            {
                return baseUrl;
            }
            
            string url = baseUrl;
            if ( url.Contains( "?" ) )
            {
                url += "&";
            }
            else
            {
                url += "?";
            }
            
            url += GetObjectAsUrlString( parameters );
            
            return url;
        }
        
        //you CANNOT have nested ArrayLists or Hashtables. This tries to deal with nested Hashtables
        //by inserting them into the root, but stuff could be broke. Nested ArrayLists are just ignored.
        protected string GetObjectAsUrlString( Hashtable obj )
        {
            string url = "";
            
            bool useAmp = false;
            foreach ( string key in obj.Keys )
            {
                if ( useAmp )
                {
                    url += "&";
                }
                
                string urlKey = "data[" + key + "]";
                
                if ( obj[ key ] is Hashtable )
                {
                    Debug.LogWarning( "RestifizerRequest: You shouldn't use nested Hashtables in GET requests!" );
                    url += GetObjectAsUrlString( obj[ key ] as Hashtable );
                }
                else if ( obj[ key ] is ArrayList )
                {
                    ArrayList list = obj[ key ] as ArrayList;
                    urlKey = urlKey + "[]";
                    for ( int index = 0; index < list.Count; index++ )
                    {
                        if ( list[ index ] is ArrayList )
                        {
                            Debug.LogWarning( "RestifizerRequest: You can't use nested ArrayLists in GET requests!" );
                        }
                        else if ( list[ index ] is Hashtable )
                        {
                            Debug.LogWarning( "RestifizerRequest: You can't use Hashtables inside ArrayLists in GET requests!" );
                        }
                        else
                        {
                            url += urlKey + "=" + WWW.EscapeURL( list[ index ].ToString() );
                        }
                    }
                }
                else
                {
                    url += urlKey + "=" + WWW.EscapeURL( obj[ key ].ToString() );
                }
                
                useAmp = true;
            }
            
            return url;
        }
	}
}
