﻿//------------------------------------------------------------------------------
//----- SiteController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2014 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Populates Site resource for the view
//
//     

#region Comments
#endregion

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Linq;
using System.Web.Mvc;
using System.Web.Script.Serialization;

using RestSharp;
using LaMPServices;
using LaMPWeb.Models;
using LaMPWeb.Utilities;
using LaMPWeb.Helpers;

namespace LaMPWeb.Controllers
{
    [Authorize]
    [RequireSSL]
    public class SiteController : Controller
    {
        //
        // GET: /Site/

        //get list of sites to show in infobox
        public PartialViewResult SiteInfoBox(int id)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;

            List<SITE> SiteList = GetProjectSites(id);
            if (SiteList != null)
            {
                ViewData["SiteList"] = SiteList;
                ViewData["SiteCount"] = SiteList.Count();    
            }

            ViewData["project"] = GetThisProject(id);
            return PartialView();
        }
        
        //GET: popup partial view containing parameter checkboxes
        public ActionResult AddParameters(string paramS)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/parameters";
            request.RootElement = "ArrayOfPARAMETER_TYPE";
            List<PARAMETER_TYPE> Parameters = serviceCaller.Execute<List<PARAMETER_TYPE>>(request);
            ViewData["Chemical"] = Parameters.Where(p => p.PARAMETER_GROUP == "Chemical").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            ViewData["Physical"] = Parameters.Where(p => p.PARAMETER_GROUP == "Physical").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            ViewData["Biological"] = Parameters.Where(p => p.PARAMETER_GROUP == "Biological").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            ViewData["Microbiological"] = Parameters.Where(p => p.PARAMETER_GROUP == "Microbiological").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            ViewData["Toxicological"] = Parameters.Where(p => p.PARAMETER_GROUP == "Toxicological").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();

            //parse the params to a List and send to the popup so can pre-check boxes of those selected
            if (paramS != null)
            {
                string[] pars = Regex.Split(paramS, ",");
                List<int> siteParamsList = new List<int>();
                foreach (string p in pars)
                {
                    if (!string.IsNullOrWhiteSpace(p))
                    {
                        siteParamsList.Add(Convert.ToInt32(p));
                    }
                }
                ViewData["siteParams"] = siteParamsList;
            }
            return PartialView();
        }
        
        //POST: SiteParameter popup post
        [HttpPost]
        public JsonResult AddSiteParameters(FormCollection fc)
        {
            List<string> paramIDs = new List<string>();
            foreach (var key in fc.AllKeys)
            {
                if (key != "X-Requested-With")
                {
                    paramIDs.Add(key);
                }
            }

            return Json(JsonResponseFactory.SuccessResponse(paramIDs), JsonRequestBehavior.DenyGet);
        }

        //GET: SiteCreate page, manually 1 by 1 entry form
        public ActionResult SiteCreate1(int id, string From)
        {
            ViewData["project"] = GetThisProject(id);
            //get any sites that may already exist for this project
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();

            List<SITE> projSites = GetProjectSites(id);

            if (projSites != null && projSites.Count >= 1)
            {
                ViewData["sites"] = projSites;
            }

            ViewData["states"] = GetStates();
            ViewData["countries"] = GetCountries();
            ViewData["Lakes"] = GetLakes();
            ViewData["statusTypes"] = GetStatusTypes();
            ViewData["resourceTypes"] = GetResources();
            ViewData["mediaTypes"] = GetMedia();
            ViewData["frequencyTypes"] = GetSampleFreq();
            
            request = new RestRequest();
            request.Resource = "/parameters";
            request.RootElement = "ArrayOfPARAMETER_TYPE";
            List<PARAMETER_TYPE> Parameters = serviceCaller.Execute<List<PARAMETER_TYPE>>(request);
            ViewData["Chemical"] = Parameters.Where(p => p.PARAMETER_GROUP == "Chemical").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            ViewData["Physical"] = Parameters.Where(p => p.PARAMETER_GROUP == "Physical").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            ViewData["Biological"] = Parameters.Where(p => p.PARAMETER_GROUP == "Biological").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            ViewData["Microbiological"] = Parameters.Where(p => p.PARAMETER_GROUP == "Microbiological").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            ViewData["Toxicological"] = Parameters.Where(p => p.PARAMETER_GROUP == "Toxicological").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();

            if (From == "Publications")
            {
                ViewData["From"] = From;
            }
            return View();
        }

         //POST: Post the site
        [HttpPost]
        public ActionResult SiteCreate(SiteModel ModelSite, string Create)
        {
            try
            {
                string From = ModelSite.From;
                
                SITE aSite = ModelSite.aSite;
                //ensure longitude is negative
                decimal l;
                if (aSite.LONGITUDE > 0)
                {
                    l = decimal.Negate(Convert.ToDecimal(aSite.LONGITUDE));
                    aSite.LONGITUDE = l;
                }

                //POST the site
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/sites";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(aSite);
                SITE postedSite = serviceCaller.Execute<SITE>(request);

                //ResourceTYpes
                //parse, get the object, post it to the project
                if (!string.IsNullOrWhiteSpace(ModelSite.CreateResourceTypes))
                {
                    string[] res = Regex.Split(ModelSite.CreateResourceTypes, ",");
                    foreach (string r in res)
                    {
                        if (!string.IsNullOrWhiteSpace(r))
                        {
                            //get it
                            RESOURCE_TYPE thisResource = GetAResourceType(Convert.ToDecimal(r));

                            if (thisResource != null)
                            {
                                //now post it
                                request = new RestRequest(Method.POST);
                                request.Resource = "/sites/{siteId}/addResourcetype";
                                request.AddParameter("siteId", postedSite.SITE_ID, ParameterType.UrlSegment);
                                request.RequestFormat = DataFormat.Xml;
                                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                                request.AddBody(thisResource);
                                serviceCaller.Execute<List<RESOURCE_TYPE>>(request);
                            }
                        }
                    }
                }

                if (!string.IsNullOrWhiteSpace(ModelSite.CreateMediaTypes))
                {
                    string[] med = Regex.Split(ModelSite.CreateMediaTypes, ",");
                    foreach (string m in med)
                    {
                        if (!string.IsNullOrWhiteSpace(m))
                        {
                            //get it
                            MEDIA_TYPE thisMedia = GetAMedia(Convert.ToDecimal(m));

                            if (thisMedia != null)
                            {
                                //now post it   
                                request = new RestRequest(Method.POST);
                                request.Resource = "/sites/{siteId}/addMedium";
                                request.AddParameter("siteId", postedSite.SITE_ID, ParameterType.UrlSegment);
                                request.RequestFormat = DataFormat.Xml;
                                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                                request.AddBody(thisMedia);
                                serviceCaller.Execute<List<MEDIA_TYPE>>(request);
                            }
                        }
                    }
                }
                if (!string.IsNullOrWhiteSpace(ModelSite.CreateFrequencyTypes))
                {
                    string[] fre = Regex.Split(ModelSite.CreateFrequencyTypes, ",");
                    foreach (string f in fre)
                    {
                        if (!string.IsNullOrWhiteSpace(f))
                        {
                            //get it
                            FREQUENCY_TYPE thisFrequency = GetAFrequency(Convert.ToDecimal(f));

                            if (thisFrequency != null)
                            {
                                //now post it 
                                request = new RestRequest(Method.POST);
                                request.Resource = "/sites/{siteId}/addFrequency";
                                request.AddParameter("siteId", postedSite.SITE_ID, ParameterType.UrlSegment);
                                request.RequestFormat = DataFormat.Xml;
                                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                                request.AddBody(thisFrequency);
                                serviceCaller.Execute<List<FREQUENCY_TYPE>>(request);
                            }
                        }
                    }
                }

                string[] parameters = ModelSite.Params;
                
                foreach (string p in parameters)
                {
                    if (!string.IsNullOrWhiteSpace(p))
                    {
                        //get it
                       PARAMETER_TYPE thisParam = GetAParameter(Convert.ToDecimal(p));
                       if (thisParam != null)
                       {
                            //now post it      
                            request = new RestRequest(Method.POST);
                            request.Resource = "/sites/{siteId}/addParameter";
                            request.AddParameter("siteId", postedSite.SITE_ID, ParameterType.UrlSegment);
                            request.RequestFormat = DataFormat.Xml;
                            request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                            request.AddBody(thisParam);
                            serviceCaller.Execute<List<PARAMETER_TYPE>>(request);
                       }
                    }
                }
            
                if (Create == "Save & Add\r\n Another Site")
                {
                    return RedirectToAction("SiteCreate1", new { id = postedSite.PROJECT_ID, From = From });
                }
                else if (Create == "Save & Go To\r\n Project Details")
                {
                    return RedirectToAction("ProjectDetails", "Project", new { id = postedSite.PROJECT_ID });
                }
                else
                {
                    return RedirectToAction("Finish", "Home");
                }
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //GET: copy/paste data grid view opttion
        public ActionResult SiteCreate2(int id, string From)
        {
            ViewData["project"] = GetThisProject(id);
            ViewData["Lakes"] = GetLakes();
            ViewData["Resources"] = GetResources();
            ViewData["Media"] = GetMedia();
            ViewData["SampleFreq"] = GetSampleFreq();

            return View();
        }

       //POST: Post the site GridView
        //[HttpPost]
        //public string SiteCreate2(string input, string project)
        //{
        //    List<SiteGridModel> inputs = new List<SiteGridModel>();
        //    inputs = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SiteGridModel>>(input);
            
        //    //put all the pieces where they need to go
        //    foreach (SiteGridModel sgm in inputs)
        //    {
        //        if (sgm.NAME != null)
        //        {
        //            LOCATION aLocation = new LOCATION();
        //            aLocation.NAME = sgm.NAME;
        //            aLocation.DESCRIPTION = sgm.DESCRIPTION;
        //            aLocation.LATITUDE = sgm.LATITUDE;
        //            aLocation.LONGITUDE = sgm.LONGITUDE;
        //            aLocation.WATERBODY = sgm.WATERBODY;


        //            CATALOG_ aCatalog = new CATALOG_();

        //        }
        //    }
        //    return "ok";
        //}
        
        
        //GET: details page for a Site/Location (Catalog)
        
        public ActionResult SiteDetails(int id, int projId)
        {
            //get this project
            ViewData["Project"] = GetThisProject(projId);
            
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/sites/{siteId}";
            request.RootElement = "SITE";
            request.AddParameter("siteId", id, ParameterType.UrlSegment);
            SITE thisSite = serviceCaller.Execute<SITE>(request);
            
            if (thisSite != null)
            {
                //make sure longitude is negative (need to store it first because of decimal? format)
                decimal longi = Convert.ToDecimal(thisSite.LONGITUDE);
                if (longi > 0)
                {
                    longi = decimal.Negate(longi);
                    thisSite.LONGITUDE = longi;
                }

                if (thisSite.LAKE_TYPE_ID != null && thisSite.LAKE_TYPE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/lakes/{lakeId}";
                    request.RootElement = "LAKE_TYPE";
                    request.AddParameter("lakeId", thisSite.LAKE_TYPE_ID, ParameterType.UrlSegment);
                    ViewData["LakeType"] = serviceCaller.Execute<LAKE_TYPE>(request).LAKE;
                }
                if (thisSite.STATUS_TYPE_ID != null && thisSite.STATUS_TYPE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/status/{statusId}";
                    request.RootElement = "STATUS_TYPE";
                    request.AddParameter("statusId", thisSite.STATUS_TYPE_ID, ParameterType.UrlSegment);
                    ViewData["StatusType"] = serviceCaller.Execute<STATUS_TYPE>(request).STATUS;
                }
            }

            request = new RestRequest();
            request.Resource = "/sites/{siteId}/resourcetypes";
            request.RootElement = "ArrayOfResource_type";
            request.AddParameter("siteId", thisSite.SITE_ID, ParameterType.UrlSegment);
            ViewData["ResourceList"] = serviceCaller.Execute<List<RESOURCE_TYPE>>(request);

            request = new RestRequest();
            request.Resource = "/sites/{siteId}/media";
            request.RootElement = "ArrayOfMedia_type";
            request.AddParameter("siteId", thisSite.SITE_ID, ParameterType.UrlSegment);
            ViewData["MediaList"] = serviceCaller.Execute<List<MEDIA_TYPE>>(request);

            request = new RestRequest();
            request.Resource = "/sites/{siteId}/frequencies";
            request.RootElement = "ArrayOfFrequency_type";
            request.AddParameter("siteId", thisSite.SITE_ID, ParameterType.UrlSegment);
            ViewData["FrequencyList"] = serviceCaller.Execute<List<FREQUENCY_TYPE>>(request);

            request = new RestRequest();
            request.Resource = "/sites/{siteId}/parameters";
            request.RootElement = "ArrayOfParameter_type";
            request.AddParameter("siteId", thisSite.SITE_ID, ParameterType.UrlSegment);
            List<PARAMETER_TYPE> parameters = serviceCaller.Execute<List<PARAMETER_TYPE>>(request);

            if (parameters.Count >= 1)
            {
                ViewData["ChemicalP"] = parameters.Where(p => p.PARAMETER_GROUP == "Chemical").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
                ViewData["PhysicalP"] = parameters.Where(p => p.PARAMETER_GROUP == "Physical").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
                ViewData["BiologicalP"] = parameters.Where(p => p.PARAMETER_GROUP == "Biological").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
                ViewData["MicrobiolP"] = parameters.Where(p => p.PARAMETER_GROUP == "Microbiological").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
                ViewData["ToxicologicalP"] = parameters.Where(p => p.PARAMETER_GROUP == "Toxicological").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            }

            return View(thisSite);            
        }

        //a Details view for all Sites in a Project (in a grid View)
        public ActionResult SiteDetailsGrid(int projId)
        {
            //get this project
            ViewData["Project"] = GetThisProject(projId);

            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/projects/{projectId}/sites";
            request.RootElement = "SITE";
            request.AddParameter("projectId", projId, ParameterType.UrlSegment);
            List<SITE> SiteList = serviceCaller.Execute<List<SITE>>(request);

            List<SiteGridModel> SiteModel = SiteList.Select(x => new SiteGridModel
                    {
                        SiteID = x.SITE_ID,
                        NAME = x.NAME,
                        LATITUDE = x.LATITUDE,
                        LONGITUDE = x.LONGITUDE,
                        COUNTRY = x.COUNTRY,
                        STATE_PROVINCE = x.STATE_PROVINCE,
                        LAKE = x.LAKE_TYPE_ID.HasValue ? GetALake(x.LAKE_TYPE_ID) : "",
                        WATERBODY = x.WATERBODY,
                        WATERSHED_HUC8 = x.WATERSHED_HUC8,
                        DESCRIPTION = x.DESCRIPTION,
                        STATUS = x.STATUS_TYPE_ID.HasValue ? GetAStatus(x.STATUS_TYPE_ID) : "",
                        ResourceTypes = GetSiteResources(x.SITE_ID),
                        MediaTypes = GetSiteMedia(x.SITE_ID),
                        FrequencyTypes = GetSiteFrequencies(x.SITE_ID),
                        START_DATE = x.START_DATE.HasValue ? x.START_DATE.Value.ToShortDateString() : "",
                        END_DATE = x.END_DATE.HasValue ? x.END_DATE.Value.ToShortDateString() : "",
                        SAMPLE_PLATFORM = x.SAMPLE_PLATFORM,
                        ADDITIONAL_INFO = x.ADDITIONAL_INFO,
                        URL = x.URL,
                        Parameters = GetSiteParameters(x.SITE_ID)
                    }).ToList();

            ViewData["SiteList"] = SiteModel;

            return View("SiteDetails2");
        }

        public ActionResult AllSiteEdit(Int32 projId)
        {
            ViewData["Project"] = GetThisProject(projId);
            //get all the dropdowns to pass to the table
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            var States = serializer.Serialize(GetStates()); 
            ViewData["States"] = States;

            ViewData["Lakes"] = serializer.Serialize(GetLakes());
            ViewData["Status"] = GetStatusTypes();
            ViewData["Resources"] = GetResources();
            ViewData["Media"] = GetMedia();
            ViewData["Frequences"] = GetSampleFreq();

            return View("SiteEdit2");
        }

       
        public JsonResult GetSitesJson(Int32 id)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/projects/{projectId}/sites";
            request.RootElement = "SITE";
            request.AddParameter("projectId", id, ParameterType.UrlSegment);
            List<SITE> SiteList = serviceCaller.Execute<List<SITE>>(request);

            return Json(SiteList, JsonRequestBehavior.AllowGet);
        }

        //GET: details page for a Site
        public ActionResult SiteEdit1(int id, int projId)
        {
            SiteModel thisSite = new SiteModel();

            //pass this project
            thisSite.aProject = GetThisProject(projId);

            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/sites/{siteId}";
            request.RootElement = "SITE";
            request.AddParameter("siteId", id, ParameterType.UrlSegment);
            SITE aSite = serviceCaller.Execute<SITE>(request);

            if (aSite != null)
            {
                //make sure longitude is negative (need to store it first because of decimal? format)
                decimal longi = Convert.ToDecimal(aSite.LONGITUDE);
                if (longi > 0)
                {
                    longi = decimal.Negate(longi);
                    aSite.LONGITUDE = longi;
                }

                thisSite.aSite = aSite;

                if (aSite.LAKE_TYPE_ID != null && aSite.LAKE_TYPE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/lakes/{lakeId}";
                    request.RootElement = "LAKE_TYPE";
                    request.AddParameter("lakeId", aSite.LAKE_TYPE_ID, ParameterType.UrlSegment);
                    ViewData["LakeType"] = serviceCaller.Execute<LAKE_TYPE>(request).LAKE;
                }
                if (aSite.STATUS_TYPE_ID != null && aSite.STATUS_TYPE_ID != 0)
                {
                    request = new RestRequest();
                    request.Resource = "/status/{statusId}";
                    request.RootElement = "STATUS_TYPE";
                    request.AddParameter("statusId", aSite.STATUS_TYPE_ID, ParameterType.UrlSegment);
                    ViewData["StatusType"] = serviceCaller.Execute<STATUS_TYPE>(request).STATUS;
                }
            }

            //get all drop down values
            ViewData["states"] = GetStates();
            ViewData["countries"] = GetCountries();
            ViewData["Lakes"] = GetLakes();
            ViewData["statusTypes"] = GetStatusTypes();
            ViewData["resourceTypes"] = GetResources();
            ViewData["mediaTypes"] = GetMedia();
            ViewData["frequencyTypes"] = GetSampleFreq();

            //get the site's resource types
            request = new RestRequest();
            request.Resource = "/sites/{siteId}/resourcetypes";
            request.RootElement = "ArrayOfResource_type";
            request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
            List<RESOURCE_TYPE> SiteResources = serviceCaller.Execute<List<RESOURCE_TYPE>>(request);
            ViewData["SiteResources"] = SiteResources;
            
            //add to comma separated string to populate hidden input
            string siteRes = string.Empty;
            string trimmedResources = string.Empty;
            if (SiteResources.Count >= 1)
            {
                foreach (RESOURCE_TYPE rt in SiteResources)
                {
                    siteRes += rt.RESOURCE_TYPE_ID + ",";
                }
                trimmedResources = siteRes.TrimEnd(',', ' ');
            }
            thisSite.ResourceTypes = trimmedResources;

            //get this site's Media
            request = new RestRequest();
            request.Resource = "/sites/{siteId}/media";
            request.RootElement = "ArrayOfMedia_type";
            request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
            List<MEDIA_TYPE> SiteMedia = serviceCaller.Execute<List<MEDIA_TYPE>>(request);
            ViewData["MediaList"] = SiteMedia;

            //add to comma separated string to populate hidden input
            string siteMedia = string.Empty;
            string trimmedMedia = string.Empty;
            if (SiteMedia.Count >= 1)
            {
                foreach (MEDIA_TYPE mt in SiteMedia)
                {
                    siteMedia += mt.MEDIA_TYPE_ID + ",";
                }
                trimmedMedia = siteMedia.TrimEnd(',', ' ');
            }
            thisSite.MediaTypes = trimmedMedia;

            //get this site's Frequencies
            request = new RestRequest();
            request.Resource = "/sites/{siteId}/frequencies";
            request.RootElement = "ArrayOfFrequency_type";
            request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
            List<FREQUENCY_TYPE> SiteFrequency = serviceCaller.Execute<List<FREQUENCY_TYPE>>(request);
            ViewData["FrequencyList"] = SiteFrequency;

            //add to comma separated string to populate hidden input
            string siteFreq = string.Empty;
            string trimmedFreq = string.Empty;
            if (SiteFrequency.Count >= 1)
            {
                foreach (FREQUENCY_TYPE ft in SiteFrequency)
                {
                    siteFreq += ft.FREQUENCY_TYPE_ID + ",";
                }
                trimmedFreq = siteFreq.TrimEnd(',', ' ');
            }
            thisSite.FrequencyTypes = trimmedFreq;

            //get this site's parameters
            request = new RestRequest();
            request.Resource = "/sites/{siteId}/parameters";
            request.RootElement = "ArrayOfParameter_type";
            request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
            List<PARAMETER_TYPE> parameters = serviceCaller.Execute<List<PARAMETER_TYPE>>(request);
            thisSite.Params = new string[parameters.Count()];
            if (parameters.Count >= 1)
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    thisSite.Params[i] = parameters[i].PARAMETER_TYPE_ID.ToString();
                }    
            }

            request = new RestRequest();
            request.Resource = "/parameters";
            request.RootElement = "ArrayOfPARAMETER_TYPE";
            List<PARAMETER_TYPE> AllParameters = serviceCaller.Execute<List<PARAMETER_TYPE>>(request);

            ViewData["ChemicalP"] = AllParameters.Where(p => p.PARAMETER_GROUP == "Chemical").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            ViewData["PhysicalP"] = AllParameters.Where(p => p.PARAMETER_GROUP == "Physical").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            ViewData["BiologicalP"] = AllParameters.Where(p => p.PARAMETER_GROUP == "Biological").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            ViewData["MicrobiolP"] = AllParameters.Where(p => p.PARAMETER_GROUP == "Microbiological").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();
            ViewData["ToxicologicalP"] = AllParameters.Where(p => p.PARAMETER_GROUP == "Toxicological").OrderBy(po => po.PARAMETER_TYPE_ID).ToList();

            return View(thisSite);
        }

        //POST: Post the site
        [HttpPost]
        public ActionResult SiteEdit1(SiteModel editedSite, int id)
        {
            try
            {
                SITE aSite = editedSite.aSite;
                

                //ensure longitude is negative
                decimal l;
                if (aSite.LONGITUDE > 0)
                {
                    l = decimal.Negate(Convert.ToDecimal(aSite.LONGITUDE));
                    aSite.LONGITUDE = l;
                }

                //PUT aSite
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/sites/{siteId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(aSite);
                SITE UpdatedSite = serviceCaller.Execute<SITE>(request);

                //ResourceTYpes - add all those in "ResourceTypes"
                //get all the siteResources that were on this site.. remove them all, then go through and add the ones passed
                request = new RestRequest();
                request.Resource = "/sites/{siteId}/resourcetypes";
                request.RootElement = "ArrayOfRESOURCE_TYPE";
                request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                List<RESOURCE_TYPE> resources = serviceCaller.Execute<List<RESOURCE_TYPE>>(request);

                //remove first
                if (resources != null)
                {
                    foreach (RESOURCE_TYPE rm in resources)
                    {
                        request = new RestRequest(Method.POST);
                        request.Resource = "/sites/{siteId}/removeResourcetype";
                        request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                        request.AddHeader("X-HTTP-Method-Override", "DELETE");
                        request.AddHeader("Content-Type", "application/xml");
                        request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                        request.AddBody(rm);
                        serviceCaller.Execute<RESOURCE_TYPE>(request);
                    }
                }
                
                //now add those from site edit page
                List<RESOURCE_TYPE> siteResources = new List<RESOURCE_TYPE>();
                if (!string.IsNullOrWhiteSpace(editedSite.ResourceTypes))
                {
                    //parse
                    string[] res = Regex.Split(editedSite.ResourceTypes, ",").ToArray();
                    foreach (string r in res)
                    {
                        if (!string.IsNullOrWhiteSpace(r))
                        {
                            RESOURCE_TYPE thisResType = GetAResourceType(Convert.ToDecimal(r));

                            if (thisResType != null)
                            {
                                request = new RestRequest(Method.POST);
                                request.Resource = "/sites/{siteId}/addResourcetype";
                                request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                                request.RequestFormat = DataFormat.Xml;
                                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                                request.AddBody(thisResType);
                                siteResources = serviceCaller.Execute<List<RESOURCE_TYPE>>(request);
                            }
                        }
                    }
                }
                
                //Media
                //get all the siteMedia that were on this site.. remove them all, then go through and add the ones passed
                request = new RestRequest();
                request.Resource = "/sites/{siteId}/media";
                request.RootElement = "ArrayOfMEDIA_TYPE";
                request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                List<MEDIA_TYPE> medias = serviceCaller.Execute<List<MEDIA_TYPE>>(request);

                //remove first
                if (medias != null)
                {
                    foreach (MEDIA_TYPE m in medias)
                    {
                        request = new RestRequest(Method.POST);
                        request.Resource = "/sites/{siteId}/removeMedium";
                        request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                        request.AddHeader("X-HTTP-Method-Override", "DELETE");
                        request.AddHeader("Content-Type", "application/xml");
                        request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                        request.AddBody(m);
                        serviceCaller.Execute<MEDIA_TYPE>(request);
                    }
                }

                //now add those from site edit page
                List<MEDIA_TYPE> siteMedia = new List<MEDIA_TYPE>();
                if (!string.IsNullOrWhiteSpace(editedSite.MediaTypes))
                {
                    //parse
                    string[] med = Regex.Split(editedSite.MediaTypes, ",");
                    foreach (string m in med)
                    {
                        if (!string.IsNullOrWhiteSpace(m))
                        {
                            //get it
                            MEDIA_TYPE thisMedia = GetAMedia(Convert.ToDecimal(m)); 

                            if (thisMedia != null)
                            {
                                //now post it   
                                request = new RestRequest(Method.POST);
                                request.Resource = "/sites/{siteId}/addMedium";
                                request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                                request.RequestFormat = DataFormat.Xml;
                                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                                request.AddBody(thisMedia);
                                siteMedia = serviceCaller.Execute<List<MEDIA_TYPE>>(request);
                            }
                        }
                    }
                }

                //FREQUENCY_TYPE
                //get all the siteFreq that were on this site.. remove them all, then go through and add the ones passed
                request = new RestRequest();
                request.Resource = "/sites/{siteId}/frequencies";
                request.RootElement = "ArrayOfFREQUENCY_TYPE";
                request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                List<FREQUENCY_TYPE> freqs = serviceCaller.Execute<List<FREQUENCY_TYPE>>(request);

                //remove first
                if (freqs != null)
                {
                    foreach (FREQUENCY_TYPE fr in freqs)
                    {
                        request = new RestRequest(Method.POST);
                        request.Resource = "/sites/{siteId}/removeFrequency";
                        request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                        request.AddHeader("X-HTTP-Method-Override", "DELETE");
                        request.AddHeader("Content-Type", "application/xml");
                        request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                        request.AddBody(fr);
                        serviceCaller.Execute<FREQUENCY_TYPE>(request);
                    }
                }

                //now add those from site edit page
                List<FREQUENCY_TYPE> siteFreqs = new List<FREQUENCY_TYPE>();
                //parse
                if (!string.IsNullOrWhiteSpace(editedSite.FrequencyTypes))
                {
                    string[] fre = Regex.Split(editedSite.FrequencyTypes, ",");
                    foreach (string fr in fre)
                    {
                        if (!string.IsNullOrWhiteSpace(fr))
                        {
                            FREQUENCY_TYPE thisFreq = GetAFrequency(Convert.ToDecimal(fr));

                            //now post it
                            if (thisFreq != null)
                            {
                                request = new RestRequest(Method.POST);
                                request.Resource = "/sites/{siteId}/addFrequency";
                                request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                                request.RequestFormat = DataFormat.Xml;
                                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                                request.AddBody(thisFreq);
                                siteFreqs = serviceCaller.Execute<List<FREQUENCY_TYPE>>(request);
                            }
                        }
                    }
                }

                //get all the siteParameters that were on this site.. remove them all, then go through and add the ones passed
                request = new RestRequest();
                request.Resource = "/sites/{siteId}/parameters";
                request.RootElement = "ArrayOfParameter_type";
                request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                List<PARAMETER_TYPE> parameters = serviceCaller.Execute<List<PARAMETER_TYPE>>(request);

                if (parameters != null)
                {
                    foreach (PARAMETER_TYPE p in parameters)
                    {
                        //remove it
                        request = new RestRequest(Method.POST);
                        request.Resource = "/sites/{siteId}/removeParameter";
                        request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                        request.AddHeader("X-HTTP-Method-Override", "DELETE");
                        request.AddHeader("Content-Type", "application/xml");
                        request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                        request.AddBody(p);
                        serviceCaller.Execute<PARAMETER_TYPE>(request);
                    }
                }
                    
                //now post those 
                //string theseParams = editedSite.SiteParameters;
                //if (!string.IsNullOrWhiteSpace(theseParams))
                //{
                List<PARAMETER_TYPE> siteParameters = new List<PARAMETER_TYPE>();
                //    //parse and post
                //    string[] param = Regex.Split(theseParams, ",");
                string[] newParams = editedSite.Params;
                foreach (string p in newParams)
                {
                    if (!string.IsNullOrWhiteSpace(p))
                    {
                        //get it
                        PARAMETER_TYPE thisParam = GetAParameter(Convert.ToDecimal(p));

                        if (thisParam != null)
                        {
                            //now post it      
                            request = new RestRequest(Method.POST);
                            request.Resource = "/sites/{siteId}/addParameter";
                            request.AddParameter("siteId", aSite.SITE_ID, ParameterType.UrlSegment);
                            request.RequestFormat = DataFormat.Xml;
                            request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                            request.AddBody(thisParam);
                            siteParameters = serviceCaller.Execute<List<PARAMETER_TYPE>>(request);
                        }
                    }
                    
                }
                return RedirectToAction("SiteDetails", new { id = aSite.SITE_ID, projId = aSite.PROJECT_ID });
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        //GET: /Contacts/Delete/1
        public ActionResult Delete(int id, int projID)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "Sites/{siteId}";
                request.AddParameter("siteId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<SITE>(request);

                return RedirectToAction("ProjectDetails", "Project", new { id = projID });
            }
            catch
            {
                return View();
            }
        }

        //GET: get this particular project based on id
        private PROJECT GetThisProject(int id)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/projects/{projectId}";
            request.RootElement = "projects";
            request.AddParameter("projectId", id, ParameterType.UrlSegment);
            PROJECT thisProject = serviceCaller.Execute<PROJECT>(request);
            return thisProject;
        }
        
        private RESOURCE_TYPE GetAResourceType(decimal id)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/resourcetypes/{resourceTypeId}";
            request.RootElement = "RESOURCE_TYPE";
            request.AddParameter("resourceTypeId", id, ParameterType.UrlSegment);
            RESOURCE_TYPE thisResource = serviceCaller.Execute<RESOURCE_TYPE>(request);
            return thisResource;
        }

        private List<SITE> GetProjectSites(decimal id)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "projects/{projectId}/sites";
            request.RootElement = "ArrayOfSITE";
            request.AddParameter("projectId", id, ParameterType.UrlSegment);
            List<SITE> SiteList = serviceCaller.Execute<List<SITE>>(request);
            return SiteList;
        }

        private MEDIA_TYPE GetAMedia(decimal? id)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/media/{mediaId}";
            request.RootElement = "MEDIA_TYPE";
            request.AddParameter("mediaId", id, ParameterType.UrlSegment);
            MEDIA_TYPE thisMedia = serviceCaller.Execute<MEDIA_TYPE>(request);
            return thisMedia;
        }

        private FREQUENCY_TYPE GetAFrequency(decimal? id)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/frequencies/{frequencyId}";
            request.RootElement = "FREQUENCY_TYPE";
            request.AddParameter("frequencyId", id, ParameterType.UrlSegment);
            FREQUENCY_TYPE thisFreq = serviceCaller.Execute<FREQUENCY_TYPE>(request);
            return thisFreq;
        }

        private PARAMETER_TYPE GetAParameter(decimal? id)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/parameters/{parameterId}";
            request.RootElement = "PARAMETER_TYPE";
            request.AddParameter("parameterId", id, ParameterType.UrlSegment);
            PARAMETER_TYPE thisParam = serviceCaller.Execute<PARAMETER_TYPE>(request);
            return thisParam;
        }

        private List<string> GetStates()
        {
            List<string> States = new List<string>();
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/states";
            request.RootElement = "ArrayOfString";
            States = serviceCaller.Execute<List<string>>(request);

            return States;
        }

        private List<string> GetCountries()
        {
            List<string> Countries = new List<string>();
            Countries.Add("Canada");
            Countries.Add("United States Of America");
            return Countries;
        }

        private List<LAKE_TYPE> GetLakes()
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/lakes";
            request.RootElement = "ArrayOfLAKE_TYPE";
            List<LAKE_TYPE> Lakes = serviceCaller.Execute<List<LAKE_TYPE>>(request);
                      
            return Lakes;
        }

        private List<STATUS_TYPE> GetStatusTypes()
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/status";
            request.RootElement = "ArrayOfSTATUS_TYPE";
            List<STATUS_TYPE> status = serviceCaller.Execute<List<STATUS_TYPE>>(request);

            return status;
        
        }

        private List<RESOURCE_TYPE> GetResources()
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/resourcetypes";
            request.RootElement = "ArrayOfRESOURCE_TYPE";
            List<RESOURCE_TYPE> Resources = serviceCaller.Execute<List<RESOURCE_TYPE>>(request);

            return Resources;
        }

        private List<MEDIA_TYPE> GetMedia()
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/media";
            request.RootElement = "ArrayOfMEDIA_TYPE";
            List<MEDIA_TYPE> Media = serviceCaller.Execute<List<MEDIA_TYPE>>(request);

            return Media;
        }

        private List<FREQUENCY_TYPE> GetSampleFreq()
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/frequencies";
            request.RootElement = "ArrayOfFREQUENCY_TYPE";
            List<FREQUENCY_TYPE> frequency = serviceCaller.Execute<List<FREQUENCY_TYPE>>(request);

            return frequency;
        }

        private string GetAStatus(decimal? id)
        {
 	        LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/status/{statusId}";
            request.RootElement = "STATUS_TYPE";
            request.AddParameter("statusId", id, ParameterType.UrlSegment);
            STATUS_TYPE thisStatus = serviceCaller.Execute<STATUS_TYPE>(request);

            return string.IsNullOrEmpty(thisStatus.STATUS) ? "" : thisStatus.STATUS;
        } 
        
        private string GetALake(decimal? id)
        {
 	        LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "lakes/{lakeId}";
            request.RootElement = "LAKE_TYPE";
            request.AddParameter("lakeId", id, ParameterType.UrlSegment);
            LAKE_TYPE thisLake = serviceCaller.Execute<LAKE_TYPE>(request);

            return string.IsNullOrEmpty(thisLake.LAKE) ? "" : thisLake.LAKE;
        }

        private string GetSiteResources(decimal p)
        {
 	        LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/sites/{siteId}/resourcetypes";
            request.RootElement = "ArrayOfRESOURCE_TYPE";
            request.AddParameter("siteId", p, ParameterType.UrlSegment);
            List<RESOURCE_TYPE> ResourceList = serviceCaller.Execute<List<RESOURCE_TYPE>>(request);

            string resources = string.Join(",", ResourceList.Select(x => x.RESOURCE_NAME));

            return resources;
        }

        private string GetSiteParameters(decimal p)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/sites/{siteId}/parameters";
            request.RootElement = "ArrayOfPARAMETER_TYPE";
            request.AddParameter("siteId", p, ParameterType.UrlSegment);
            List<PARAMETER_TYPE> ParameterList = serviceCaller.Execute<List<PARAMETER_TYPE>>(request);
            string parameters = string.Empty;

            if (ParameterList.Count >= 1)
            {
                string ChemicalP = string.Join(", ", ParameterList.Where(x => x.PARAMETER_GROUP == "Chemical").OrderBy(po => po.PARAMETER_TYPE_ID).Select(y=>y.PARAMETER));
                string PhysicalP = string.Join(", ", ParameterList.Where(x => x.PARAMETER_GROUP == "Physical").OrderBy(po => po.PARAMETER_TYPE_ID).Select(y => y.PARAMETER));
                string BiologicalP = string.Join(", ", ParameterList.Where(x => x.PARAMETER_GROUP == "Biological").OrderBy(po => po.PARAMETER_TYPE_ID).Select(y => y.PARAMETER));
                string MicrobiolP = string.Join(", ", ParameterList.Where(x => x.PARAMETER_GROUP == "Microbiological").OrderBy(po => po.PARAMETER_TYPE_ID).Select(y => y.PARAMETER));
                string ToxicologicalP = string.Join(", ", ParameterList.Where(x => x.PARAMETER_GROUP == "Toxicological").OrderBy(po => po.PARAMETER_TYPE_ID).Select(y => y.PARAMETER));
                
                if (ChemicalP.Length > 0)
                    ChemicalP = "CHEMICAL: " + ChemicalP;

                if (PhysicalP.Length > 0)
                    PhysicalP = "PHYSICAL: " + PhysicalP;

                if (BiologicalP.Length > 0)
                    BiologicalP = "BIOLOGICAL: " + BiologicalP;

                if (MicrobiolP.Length > 0)
                    MicrobiolP = "MICROBIOLOGICAL: " + MicrobiolP;

                if (ToxicologicalP.Length > 0)
                    ToxicologicalP = "TOXICOLOGICAL: " + ToxicologicalP;

                List<string> allParams = new List<string>();
                if (ChemicalP.Length > 0)
                    allParams.Add(ChemicalP);

                if (PhysicalP.Length > 0)
                    allParams.Add(PhysicalP);
                if (BiologicalP.Length > 0)
                    allParams.Add(BiologicalP);
                if (MicrobiolP.Length > 0)
                    allParams.Add(MicrobiolP);
                if (ToxicologicalP.Length > 0)
                    allParams.Add(ToxicologicalP);

                parameters = string.Join(",", allParams);
            }
            return parameters;
        }

        private string GetSiteFrequencies(decimal p)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/sites/{siteId}/frequencies";
            request.RootElement = "ArrayOfFREQUENCY_TYPE";
            request.AddParameter("siteId", p, ParameterType.UrlSegment);
            List<FREQUENCY_TYPE> freqList = serviceCaller.Execute<List<FREQUENCY_TYPE>>(request);

            string frequencies = string.Join(",", freqList.Select(x => x.FREQUENCY));

            return frequencies;
        }

        private string GetSiteMedia(decimal p)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/sites/{siteId}/media";
            request.RootElement = "ArrayOfMEDIA_TYPE";
            request.AddParameter("siteId", p, ParameterType.UrlSegment);
            List<MEDIA_TYPE> MediaList = serviceCaller.Execute<List<MEDIA_TYPE>>(request);

            string Media = string.Join(",", MediaList.Select(x => x.MEDIA));

            return Media;
        }
    
    
    }
}
