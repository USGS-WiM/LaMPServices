//------------------------------------------------------------------------------
//----- ServiceAgent -------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   The service agent is responsible for initiating the service call, 
//              capturing the data that's returned and forwarding the data back to 
//              the requestor.
//
//discussion:   delegated hunting and gathering responsibilities.   
//
//    

using System;
using System.Configuration;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Net;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Threading;

using Newtonsoft;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RestSharp.Serializers;
using RestSharp;


namespace WiM.Utilities.ServiceAgent
{    
    public abstract class ServiceAgentBase
    {
        #region "Events"

        #endregion

        #region Properties & Fields

        readonly string _accountSid;
        readonly string _secretKey;

        private RestClient client = new RestClient();
        #endregion

        #region Constructors
        public ServiceAgentBase(string BaseUrl)
        {
            client.BaseUrl = BaseUrl;
        }

        public ServiceAgentBase(string accountSid, string secretKey, string baseUrl)
            : this(baseUrl)
        {
            _accountSid = accountSid;
            _secretKey = secretKey;
        }
        #endregion

        #region Methods
        public void ExecuteAsync<T>(RestRequest request, Action<T> CallBackOnSuccess, Action<string> CallBackOnFail) where T : new()
        {
            // request.AddParameter("AccountSid", _accountSid, ParameterType.UrlSegment); // used on every request

            client.ExecuteAsync<T>(request, (response) =>
            {
                if (response.ResponseStatus == ResponseStatus.Error)
                {
                    CallBackOnFail(response.ErrorMessage);
                }
                else
                {
                    CallBackOnSuccess(response.Data);
                }
            });


        }//end ExecuteAsync<T>

        public IRestResponse<T> Execute<T>(IRestRequest request) where T : new()
        {
            IRestResponse<T> result = null;
            if (request == null) throw new ArgumentNullException("request");

            AutoResetEvent waitHandle = new AutoResetEvent(false);
            Exception exception = null;

            client.ExecuteAsync<T>(request, (response) =>
            {
                if (response.ResponseStatus == ResponseStatus.Error)
                {
                    exception = new Exception(response.ResponseStatus.ToString());
                    //release the Event
                    waitHandle.Set();
                }
                else
                {
                    result = response;
                    //release the Event
                    waitHandle.Set();
                }
            });

            //wait until the thread returns
            waitHandle.WaitOne();

            return result;
        }//end Execute<T>

        public Object Execute(IRestRequest request)
        {
            IRestResponse response = null;
            if (request == null) throw new ArgumentNullException("request");

            response = client.Execute(request) as IRestResponse;
            if (response.StatusCode == HttpStatusCode.OK)
            {
                switch(response.ContentType)
                {
                    case "text/plain":
                        return response.Content;
                    default:
                        return JsonConvert.DeserializeObject(response.Content);
                }
                
            }//else
            else
            {
                throw new Exception(response.ErrorMessage);
            }
        }//endExecute

        protected RestRequest getRestRequest(string URI, object Body)
        {
            RestRequest request = new RestRequest(URI);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", Body, ParameterType.RequestBody);
            request.Method = Method.POST;
            request.Timeout = 600000;

            return request;
        }//end BuildRestRequest

        #endregion

    }//end class ServiceAgentBase
}//end namespace