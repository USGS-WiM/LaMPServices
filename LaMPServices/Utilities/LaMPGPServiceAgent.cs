﻿//------------------------------------------------------------------------------
//----- LaMPGPServiceAgent -------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   The service agent is responsible for initiating the service call, 
//              to Lamps' GP services to update site data
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

using LaMPServices.Resources;
using WiM.Utilities.ServiceAgent;

namespace LaMPServices.Utilities
{
    public class LaMPGPServiceAgent:ServiceAgentBase
    {
        #region "Properties"
        #endregion
        #region "Collections & Dictionaries"
        #endregion
        #region "Constructor and IDisposable Support"
        #region Constructors
        public LaMPGPServiceAgent()
            : base(ConfigurationManager.AppSettings["AGSLaMPServer"])
        {            
        }
        #endregion
        #endregion
        #region "Methods"
        public Boolean POSTSite (string postURL, List<FullSite> parameterBody)
        {
            try
            {
               
                if (parameterBody.Count < 1) throw new Exception("Site id is invalid");

                var result = Execute(getRestRequest(postURL, "Feature=", parameterBody));

                return result != null;

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        public Boolean DELETESite(string postURL, List<FullSite> parameterBody)
        {
            try
            {
                if (parameterBody.Count < 1) throw new Exception("Site id is invalid");

                var result = Execute(getRestRequest(postURL, "Feature=", parameterBody, true));

                return result != null;

            }
            catch (Exception e)
            {
                throw e;
            }
        }
        #endregion
        #region "Helper Methods"
        protected RestRequest getRestRequest(string URI, string bodyName, object Body, Boolean remove = false)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.TypeNameHandling = TypeNameHandling.None;
            settings.TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;

            //http://54.84.124.192:6080/arcgis/rest/services/SIGL/SIGLUpdate/GPServer/SIGLUpdate/execute
            RestRequest request = new RestRequest(URI);
            request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
            request.AddParameter("application/x-www-form-urlencoded", bodyName + JsonConvert.SerializeObject(Body, settings) + "&removeFeatures=" + remove.ToString() + "&f=pjson", ParameterType.RequestBody);
            request.Method = Method.POST;

            return request;
        }//end BuildRestRequest
        #endregion

    }//end class
}