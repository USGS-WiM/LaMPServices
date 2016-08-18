﻿#region Comments
// 03.05.13 - TR - Created 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using System.Web.Mvc;

using LaMPWeb.Utilities;
using RestSharp;

using LaMPServices;
using LaMPWeb.Helpers;

namespace LaMPWeb.Controllers
{
    [Authorize]
    [RequireSSL]
    public class LookupsController : Controller
    {
        //
        // GET: /Lookups/

        public ActionResult Index()
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest();

                //Get lists and add to ViewData
                ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);

                //get all Frequency Types
                #region FreqTypes
                request.Resource = "frequencies";
                request.RootElement = "ArrayOfFREQUENCY_TYPE";
                List<FREQUENCY_TYPE> allFreqList = serviceCaller.Execute<List<FREQUENCY_TYPE>>(request);
                ViewData["FreqTypes"] = allFreqList.OrderBy(x=>x.FREQUENCY).ToList();
                #endregion FreqTypes

                //get all Lake Types
                #region LakeTypes
                request.Resource = "lakes";
                request.RootElement = "ArrayOfLAKE_TYPE";
                List<LAKE_TYPE> allLakeList = serviceCaller.Execute<List<LAKE_TYPE>>(request);
                ViewData["LakeTypes"] = allLakeList.OrderBy(x => x.LAKE).ToList();

                #endregion LakeTypes
                                
                #region MediaTypes
                //get all Media Types
                request.Resource = "media";
                request.RootElement = "ArrayOfMEDIA_TYPE";
                List<MEDIA_TYPE> allMedList = serviceCaller.Execute<List<MEDIA_TYPE>>(request);
                ViewData["MediaType"] = allMedList.OrderBy(x => x.MEDIA).ToList();
                #endregion MediaTypes

                //get all Objective Types
                #region ObjTypes
                request.Resource = "objectives";
                request.RootElement = "ArrayOfOBJECTIVE_TYPE";
                List<OBJECTIVE_TYPE> allObjList = serviceCaller.Execute<List<OBJECTIVE_TYPE>>(request);
                ViewData["ObjectiveType"] = allObjList.OrderBy(x => x.OBJECTIVE).ToList();
                #endregion ObjTypes

                //get all Organizations
                #region Organizations
                request.Resource = "organizations";
                request.RootElement = "ArrayOfORGANIZATION";
                List<ORGANIZATION> allOrgList = serviceCaller.Execute<List<ORGANIZATION>>(request);
                
                ViewData["Organization"] = allOrgList.OrderBy(x => x.NAME).ToList();
                #endregion Organizations

                //get all Parameter types
                #region ParamTypes
                request.Resource = "parameters";
                request.RootElement = "ArrayOfPARAMETER_TYPE";
                List<PARAMETER_TYPE> allParamsList = serviceCaller.Execute<List<PARAMETER_TYPE>>(request);
                ViewData["ParameterTypes"] = allParamsList.OrderBy(x => x.PARAMETER).ToList();
                #endregion ParamTypes

                //get all ProjDuration
                #region ProjDuration
                request.Resource = "ProjectDuration";
                request.RootElement = "ArrayOfPROJ_DURATION";
                List<PROJ_DURATION> allDurationList = serviceCaller.Execute<List<PROJ_DURATION>>(request);
                ViewData["ProjDurations"] = allDurationList;
                #endregion ProjDuration

                //get all ProjStatus
                #region ProjStatus
                request.Resource = "ProjectStatus";
                request.RootElement = "ArrayOfPROJ_STATUS";
                List<PROJ_STATUS> allStatusList = serviceCaller.Execute<List<PROJ_STATUS>>(request);
                ViewData["ProjStatus"] = allStatusList;
                #endregion ProjStatus

                //get all Resource Types
                #region ResTypes
                request.Resource = "resourcetypes";
                request.RootElement = "ArrayOfRESOURCE_TYPE";
                List<RESOURCE_TYPE> allResList = serviceCaller.Execute<List<RESOURCE_TYPE>>(request);
                ViewData["ResourceTypes"] = allResList.OrderBy(x => x.RESOURCE_NAME).ToList();
                #endregion ResTypes

                //get all Site Status Types
                #region StatTypes
                request.Resource = "status";
                request.RootElement = "ArrayOfSTATUS_TYPE";
                List<STATUS_TYPE> allSiteStatList = serviceCaller.Execute<List<STATUS_TYPE>>(request);
                ViewData["SiteStatusTypes"] = allSiteStatList.OrderBy(x => x.STATUS).ToList();
                #endregion StatTypes

                return View("../Settings/Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
            
        }

        #region Frequency_Type
       //need to redirect to get index if error and want to refresh FreqType
       public ActionResult FreqType()
       {
           return RedirectToAction("Index");
       }

       //a edit or add new button was clicked in the FreqType infoxbox -- get FreqTypeEdit page or redirect to FreqTypeCreate
       //GET: /Settings/Lookups/LookupEdits/FreqTypeEdit/
       [HttpPost]
       public ActionResult FreqType(FormCollection fc, string Create)
       {
           try
           {
               if (Create == "Add New")
               { return RedirectToAction("FreqTypeCreate"); }
               else
               {//edit
                   int FreqTypeId = Convert.ToInt32(fc["FREQUENCY_TYPE_ID"]);
                   if (FreqTypeId == 0) //they didn't choose one before hitting edit
                   { return RedirectToAction("Index"); }
                   else
                   {
                       //send them to edit page with the one to edit
                       return RedirectToAction("FreqTypeEdit", new { id = FreqTypeId });
                   }
               }
           }
           catch (Exception e)
           {
               return View("../Shared/Error", e);
           }
       }

       //FreqType edit page
       public ActionResult FreqTypeEdit(int id)
       {
           try
           {
               //ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);
               ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);
               if (ViewData["LoggedInRole"].ToString() == "Admin")
               {
                   LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                   var request = new RestRequest();
                   request.Resource = "/frequencies/{frequencyId}";
                   request.RootElement = "FREQUENCY_TYPE";
                   request.AddParameter("frequencyId", id, ParameterType.UrlSegment);
                   FREQUENCY_TYPE frType = serviceCaller.Execute<FREQUENCY_TYPE>(request);

                   return View("../Settings/Lookups/LookupEdits/FreqTypeEdit", frType);
               }
               else
               {
                   return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
               }
           }
           catch (Exception e)
           {
               return View("../Shared/Error", e);
           }
       }

       //change was made to FreqType, put it, then send back to index
       //POST: /Settings/Lookups/LookupEdits/FreqTypeEdit/
       [HttpPost]
       public ActionResult FreqTypeEdit(int id, FREQUENCY_TYPE aFreqType)
       {
           try
           {
               LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
               var request = new RestRequest(Method.POST);

               request.Resource = "/frequencies/{frequencyId}";
               request.RequestFormat = DataFormat.Xml;
               request.AddParameter("frequencyId", id, ParameterType.UrlSegment);
               request.AddHeader("X-HTTP-Method-Override", "PUT");
               request.AddHeader("Content-Type", "application/xml");
               request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
               request.AddBody(aFreqType);
               serviceCaller.Execute<FREQUENCY_TYPE>(request);

               return RedirectToAction("Index");
           }
           catch (Exception e)
           {
               return View("../Shared/Error", e);
           }
       }

       //redirected here from FreqType Add New button was clicked
       //GET: /Settings/Lookups/LookupCreates/FreqTypeCreate/
       public ActionResult FreqTypeCreate()
       {
           try
           {
               ViewData["Role"] = GetUserRole(User.Identity.Name);
               if (ViewData["Role"].ToString() == "Admin")
               {
                   LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                   var request = new RestRequest();

                   request.Resource = "/frequencies/";
                   request.RootElement = "ArrayOfFREQUENCY_TYPE";
                   List<FREQUENCY_TYPE> frType = serviceCaller.Execute<List<FREQUENCY_TYPE>>(request);
                   frType = frType.OrderBy(x => x.FREQUENCY).ToList();
                   return View("../Settings/Lookups/LookupCreates/FreqTypeCreate", frType);
               }
               else
               {
                   return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
               }
           }
           catch (Exception e)
           {
               return View("../Shared/Error", e);
           }
       }

       // new FreqType was created, post it
       //POST: /Settings/Lookups/LookupCreates/FreqTypeCreate
       [HttpPost]
       public ActionResult FreqTypeCreate(FREQUENCY_TYPE newFreqType)
       {
           try
           {
               LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
               var request = new RestRequest(Method.POST);

               request.Resource = "/frequencies";
               request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
               request.AddBody(newFreqType);
               FREQUENCY_TYPE newFreq = serviceCaller.Execute<FREQUENCY_TYPE>(request);

               return RedirectToAction("../Lookups/Index");
           }
           catch (Exception e)
           {
               return View("../Shared/Error", e);
           }
       }

       //
       //GET: /Settings/Lookups/FreqTypeDelete/1
       public Boolean FreqTypeDelete(int id)
       {
           try
           {
               LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
               var request = new RestRequest(Method.POST);

               request.Resource = "/frequencies/{frequencyId}";
               request.AddParameter("frequencyId", id, ParameterType.UrlSegment);
               request.AddHeader("X-HTTP-Method-Override", "DELETE");
               request.AddHeader("Content-Type", "application/xml");
               serviceCaller.Execute<FREQUENCY_TYPE>(request);

               return true;
           }
           catch
           {
               return false;
           }
       }

       #endregion Frequency_Type

        #region Lake_Type
       //need to redirect to get index if error and want to refresh LakeType
       public ActionResult LakeType()
       {
           return RedirectToAction("Index");
       }

       //a edit or add new button was clicked in the LakeType infoxbox -- get LakeTypeEdit page or redirect to LakeTypeCreate
       //GET: /Settings/Lookups/LookupEdits/LakeTypeEdit/
       [HttpPost]
       public ActionResult LakeType(FormCollection fc, string Create)
       {
           try
           {
               if (Create == "Add New")
               { return RedirectToAction("LakeTypeCreate"); }
               else
               {//edit
                   int LakeTypeId = Convert.ToInt32(fc["LAKE_TYPE_ID"]);
                   if (LakeTypeId == 0) //they didn't choose one before hitting edit
                   { return RedirectToAction("Index"); }
                   else
                   {
                       //send them to edit page with the one to edit
                       return RedirectToAction("LakeTypeEdit", new { id = LakeTypeId });
                   }
               }
           }
           catch (Exception e)
           {
               return View("../Shared/Error", e);
           }
       }

       //LakeType edit page
       public ActionResult LakeTypeEdit(int id)
       {
           try
           {
               ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);
               if (ViewData["LoggedInRole"].ToString() == "Admin")
               {
                   LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                   var request = new RestRequest();
                   request.Resource = "/lakes/{lakeId}";
                   request.RootElement = "LAKE_TYPE";
                   request.AddParameter("lakeId", id, ParameterType.UrlSegment);
                   LAKE_TYPE lakeType = serviceCaller.Execute<LAKE_TYPE>(request);

                   return View("../Settings/Lookups/LookupEdits/LakeTypeEdit", lakeType);
               }
               else
               {
                   return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
               }
           }
           catch (Exception e)
           {
               return View("../Shared/Error", e);
           }
       }

       //change was made to LakeType, put it, then send back to index
       //POST: /Settings/Lookups/LookupEdits/LakeTypeEdit/
       [HttpPost]
       public ActionResult LakeTypeEdit(int id, LAKE_TYPE aLakeType)
       {
           try
           {
               LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
               var request = new RestRequest(Method.POST);

               request.Resource = "/lakes/{lakeId}";
               request.RequestFormat = DataFormat.Xml;
               request.AddParameter("lakeId", id, ParameterType.UrlSegment);
               request.AddHeader("X-HTTP-Method-Override", "PUT");
               request.AddHeader("Content-Type", "application/xml");
               request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
               request.AddBody(aLakeType);
               serviceCaller.Execute<LAKE_TYPE>(request);

               return RedirectToAction("Index");
           }
           catch (Exception e)
           {
               return View("../Shared/Error", e);
           }
       }

       //redirected here from LakeType Add New button was clicked
       //GET: /Settings/Lookups/LookupCreates/LakeTypeCreate/
       public ActionResult LakeTypeCreate()
       {
           try
           {
               ViewData["Role"] = GetUserRole(User.Identity.Name);
               if (ViewData["Role"].ToString() == "Admin")
               {
                   LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                   var request = new RestRequest();

                   request.Resource = "/lakes/";
                   request.RootElement = "ArrayOfLAKE_TYPE";
                   List<LAKE_TYPE> lakeType = serviceCaller.Execute<List<LAKE_TYPE>>(request);
                   lakeType = lakeType.OrderBy(x => x.LAKE).ToList();
                   return View("../Settings/Lookups/LookupCreates/LakeTypeCreate", lakeType);
               }
               else
               {
                   return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
               }
           }
           catch (Exception e)
           {
               return View("../Shared/Error", e);
           }
       }

       // new LakeType was created, post it
       //POST: /Settings/Lookups/LookupCreates/LakeTypeCreate
       [HttpPost]
       public ActionResult LakeTypeCreate(LAKE_TYPE newLakeType)
       {
           try
           {
               LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
               var request = new RestRequest(Method.POST);

               request.Resource = "/lakes";
               request.RequestFormat = DataFormat.Xml;
               request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
               request.AddBody(newLakeType);
               LAKE_TYPE newlake = serviceCaller.Execute<LAKE_TYPE>(request);

               return RedirectToAction("../Lookups/Index");
           }
           catch (Exception e)
           {
               return View("../Shared/Error", e);
           }
       }

        //
        //GET: /Settings/Lookups/LakeTypeDelete/1
        public Boolean LakeTypeDelete(int id)
        {
           try
           {
               LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
               var request = new RestRequest(Method.POST);

               request.Resource = "/lakes/{lakeId}";
               request.AddParameter("lakeId", id, ParameterType.UrlSegment);
               request.AddHeader("X-HTTP-Method-Override", "DELETE");
               request.AddHeader("Content-Type", "application/xml");
               serviceCaller.Execute<LAKE_TYPE>(request);

               return true;
           }
           catch
           {
               return false;
           }
        }

        #endregion Lake_Type

        #region Media_Type
        //need to redirect to get index if error and want to refresh MediaType
        public ActionResult MediaType()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the MediaType infoxbox -- get MediaTypeEdit page or redirect to MediaTypeCreate
        //GET: /Settings/Lookups/LookupEdits/MediaTypeEdit/
        [HttpPost]
        public ActionResult MediaType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("MediaTypeCreate"); }
                else
                {//edit
                    int MediaTypeId = Convert.ToInt32(fc["MEDIA_TYPE_ID"]);
                    if (MediaTypeId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("MediaTypeEdit", new { id = MediaTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //MediaType edit page
        public ActionResult MediaTypeEdit(int id)
        {
            try
            {
                ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);
                if (ViewData["LoggedInRole"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/media/{mediaTypeId}";
                    request.RootElement = "MEDIA_TYPE";
                    request.AddParameter("mediaTypeId", id, ParameterType.UrlSegment);
                    MEDIA_TYPE mediaType = serviceCaller.Execute<MEDIA_TYPE>(request);

                    return View("../Settings/Lookups/LookupEdits/MediaTypeEdit", mediaType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to MediaType, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/MediaTypeEdit/
        [HttpPost]
        public ActionResult MediaTypeEdit(int id, MEDIA_TYPE aMediaType)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/media/{mediaTypeId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("mediaTypeId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(aMediaType);
                serviceCaller.Execute<MEDIA_TYPE>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from MediaType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/MediaTypeCreate/
        public ActionResult MediaTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetUserRole(User.Identity.Name);
                if (ViewData["Role"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/media/";
                    request.RootElement = "ArrayOfMEDIA_TYPE";
                    List<MEDIA_TYPE> mediaType = serviceCaller.Execute<List<MEDIA_TYPE>>(request);
                    mediaType = mediaType.OrderBy(x => x.MEDIA).ToList();

                    return View("../Settings/Lookups/LookupCreates/MediaTypeCreate", mediaType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new MediaType was created, post it
        //POST: /Settings/Lookups/LookupCreates/MediaTypeCreate
        [HttpPost]
        public ActionResult MediaTypeCreate(MEDIA_TYPE newMediaType)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/media";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(newMediaType);
                MEDIA_TYPE newmedia = serviceCaller.Execute<MEDIA_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/FreqTypeDelete/1
        public Boolean MediaTypeDelete(int id)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/media/{mediaTypeId}";
                request.AddParameter("mediaTypeId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<MEDIA_TYPE>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Media_Type

        #region Objective_Type
        //need to redirect to get index if error and want to refresh ObjectiveType
        public ActionResult ObjectiveType()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the ObjectiveType infoxbox -- get ObjectiveTypeEdit page or redirect to ObjectiveTypeCreate
        //GET: /Settings/Lookups/LookupEdits/ObjectiveTypeEdit/
        [HttpPost]
        public ActionResult ObjectiveType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("ObjectiveTypeCreate"); }
                else
                {//edit
                    int ObjectiveTypeId = Convert.ToInt32(fc["OBJECTIVE_TYPE_ID"]);
                    if (ObjectiveTypeId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("ObjectiveTypeEdit", new { id = ObjectiveTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //ObjectiveType edit page
        public ActionResult ObjectiveTypeEdit(int id)
        {
            try
            {
                ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);
                if (ViewData["LoggedInRole"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/objectives/{objectiveId}";
                    request.RootElement = "OBJECTIVE_TYPE";
                    request.AddParameter("objectiveId", id, ParameterType.UrlSegment);
                    OBJECTIVE_TYPE objType = serviceCaller.Execute<OBJECTIVE_TYPE>(request);

                    return View("../Settings/Lookups/LookupEdits/ObjectiveTypeEdit", objType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to ObjectiveType, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/ObjectiveTypeEdit/
        [HttpPost]
        public ActionResult ObjectiveTypeEdit(int id, OBJECTIVE_TYPE anObjectiveType)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/objectives/{objectiveId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("objectiveId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(anObjectiveType);
                serviceCaller.Execute<OBJECTIVE_TYPE>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from ObjectiveType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/ObjectiveTypeCreate/
        public ActionResult ObjectiveTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetUserRole(User.Identity.Name);
                if (ViewData["Role"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/objectives/";
                    request.RootElement = "ArrayOfOBJECTIVE_TYPE";
                    List<OBJECTIVE_TYPE> objType = serviceCaller.Execute<List<OBJECTIVE_TYPE>>(request);
                    objType = objType.OrderBy(x => x.OBJECTIVE).ToList();

                    return View("../Settings/Lookups/LookupCreates/ObjectiveTypeCreate", objType);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new ObjectiveType was created, post it
        //POST: /Settings/Lookups/LookupCreates/ObjectiveTypeCreate
        [HttpPost]
        public ActionResult ObjectiveTypeCreate(OBJECTIVE_TYPE newObjectiveType)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/objectives";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(newObjectiveType);
                OBJECTIVE_TYPE newobj = serviceCaller.Execute<OBJECTIVE_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/ObjectiveTypeDelete/1
        public Boolean ObjectiveTypeDelete(int id)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/objectives/{objectiveId}";
                request.AddParameter("objectiveId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<OBJECTIVE_TYPE>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Objective_Type

        #region Organization
        //need to redirect to get index if error and want to refresh Organization
        public ActionResult Organization()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the Organization infoxbox -- get OrganizationEdit page or redirect to OrganizationCreate
        //GET: /Settings/Lookups/LookupEdits/OrganizationEdit/
        [HttpPost]
        public ActionResult Organization(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("OrganizationCreate"); }
                else
                {//edit
                    int OrganizationId = Convert.ToInt32(fc["ORGANIZATION_ID"]);
                    if (OrganizationId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("OrganizationEdit", new { id = OrganizationId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //Organization edit page
        public ActionResult OrganizationEdit(int id)
        {
            try
            {
                ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);
                if (ViewData["LoggedInRole"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/organizations/{organizationId}";
                    request.RootElement = "ORGANIZATION";
                    request.AddParameter("organizationId", id, ParameterType.UrlSegment);
                    ORGANIZATION org = serviceCaller.Execute<ORGANIZATION>(request);

                    return View("../Settings/Lookups/LookupEdits/OrganizationEdit", org);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to Organization, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/OrganizationEdit/
        [HttpPost]
        public ActionResult OrganizationEdit(int id, ORGANIZATION anOrganization)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/organizations/{organizationId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("organizationId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(anOrganization);
                serviceCaller.Execute<ORGANIZATION>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from Organization Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/OrganizationCreate/
        public ActionResult OrganizationCreate()
        {
            try
            {
                ViewData["Role"] = GetUserRole(User.Identity.Name);
                if (ViewData["Role"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/organizations/";
                    request.RootElement = "ArrayOfORGANIZATION";
                    List<ORGANIZATION> org = serviceCaller.Execute<List<ORGANIZATION>>(request);
                    org = org.OrderBy(x => x.NAME).ToList();

                    return View("../Settings/Lookups/LookupCreates/OrganizationCreate", org);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new Organization was created, post it
        //POST: /Settings/Lookups/LookupCreates/OrganizationCreate
        [HttpPost]
        public ActionResult OrganizationCreate(ORGANIZATION newOrganization)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);


                request.Resource = "/organizations/";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(newOrganization);
                ORGANIZATION neworg = serviceCaller.Execute<ORGANIZATION>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/OrganizationDelete/1
        public Boolean OrganizationDelete(int id)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/organizations/{organizationId}";
                request.AddParameter("organizationId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<ORGANIZATION>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Organization

        #region Parameter_Type
        //need to redirect to get index if error and want to refresh ParamType
        public ActionResult ParamType()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the ParamType infoxbox -- get ParamTypeEdit page or redirect to ParamTypeCreate
        //GET: /Settings/Lookups/LookupEdits/ParamTypeEdit/
        [HttpPost]
        public ActionResult ParamType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("ParamTypeCreate"); }
                else
                {//edit
                    int ParamTypeId = Convert.ToInt32(fc["PARAMETER_TYPE_ID"]);
                    if (ParamTypeId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("ParamTypeEdit", new { id = ParamTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //ParamType edit page
        public ActionResult ParamTypeEdit(int id)
        {
            try
            {
                ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);
                if (ViewData["LoggedInRole"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/parameters/{parameterId}";
                    request.RootElement = "PARAMETER_TYPE";
                    request.AddParameter("parameterId", id, ParameterType.UrlSegment);
                    PARAMETER_TYPE paramT = serviceCaller.Execute<PARAMETER_TYPE>(request);

                    request = new RestRequest();
                    request.Resource = "/parameters";
                    request.RootElement = "ArrayOfPARAMETER_TYPE";
                    List<PARAMETER_TYPE> paramGrps = serviceCaller.Execute<List<PARAMETER_TYPE>>(request);

                    ViewData["ParamGroups"] = paramGrps.DistinctBy(x => x.PARAMETER_GROUP).OrderBy(y => y.PARAMETER_GROUP).ToList();

                    
                    return View("../Settings/Lookups/LookupEdits/ParameterTypeEdit", paramT);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to ParamType, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/ParamTypeEdit/
        [HttpPost]
        public ActionResult ParamTypeEdit(int id, PARAMETER_TYPE aParamType)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/parameters/{parameterId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("parameterId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(aParamType);
                serviceCaller.Execute<PARAMETER_TYPE>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from ParamType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/ParamTypeCreate/
        public ActionResult ParamTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetUserRole(User.Identity.Name);
                if (ViewData["Role"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/parameters";
                    request.RootElement = "ArrayOfPARAMETER_TYPE";
                    List<PARAMETER_TYPE> paramT = serviceCaller.Execute<List<PARAMETER_TYPE>>(request);
                    
                    ViewData["ParamGroups"] = paramT.DistinctBy(x => x.PARAMETER_GROUP).OrderBy(y => y.PARAMETER_GROUP).ToList();
                    paramT = paramT.OrderBy(x => x.PARAMETER).ToList();

                    return View("../Settings/Lookups/LookupCreates/ParameterTypeCreate", paramT);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new ParamType was created, post it
        //POST: /Settings/Lookups/LookupCreates/ParamTypeCreate
        [HttpPost]
        public ActionResult ParamTypeCreate(PARAMETER_TYPE newParamType)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/parameters";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(newParamType);
                PARAMETER_TYPE newparam = serviceCaller.Execute<PARAMETER_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/ParamTypeDelete/1
        public Boolean ParamTypeDelete(int id)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/parameters/{parameterId}";
                request.AddParameter("parameterId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<PARAMETER_TYPE>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Parameter_Type

        #region PROJ_DURATION
        //need to redirect to get index if error and want to refresh ProjDuration
        public ActionResult ProjDuration()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the ProjDuration infoxbox -- get ProjDurationEdit page or redirect to ProjDurationCreate
        //GET: /Settings/Lookups/LookupEdits/ProjDurationEdit/
        [HttpPost]
        public ActionResult ProjDuration(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("ProjDurationCreate"); }
                else
                {//edit
                    int ProjDurationId = Convert.ToInt32(fc["PROJ_DURATION_ID"]);
                    if (ProjDurationId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("ProjDurationEdit", new { id = ProjDurationId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //ProjDuration edit page
        public ActionResult ProjDurationEdit(int id)
        {
            try
            {
                ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);
                if (ViewData["LoggedInRole"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/ProjectDuration/{entityId}";
                    request.RootElement = "PROJ_DURATION";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    PROJ_DURATION dur = serviceCaller.Execute<PROJ_DURATION>(request);

                    return View("../Settings/Lookups/LookupEdits/ProjDurationEdit", dur);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to FreqType, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/FreqTypeEdit/
        [HttpPost]
        public ActionResult ProjDurationEdit(int id, PROJ_DURATION aDuration)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ProjectDuration/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(aDuration);
                serviceCaller.Execute<PROJ_DURATION>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from ProjDuration Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/ProjDurationCreate/
        public ActionResult ProjDurationCreate()
        {
            try
            {
                ViewData["Role"] = GetUserRole(User.Identity.Name);
                if (ViewData["Role"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/ProjectDuration/";
                    request.RootElement = "ArrayOfPROJ_DURATION";
                    List<PROJ_DURATION> dur = serviceCaller.Execute<List<PROJ_DURATION>>(request);
                    
                    return View("../Settings/Lookups/LookupCreates/ProjDurationCreate", dur);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new ProjDuration was created, post it
        //POST: /Settings/Lookups/LookupCreates/ProjDurationCreate
        [HttpPost]
        public ActionResult ProjDurationCreate(PROJ_DURATION newDuration)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ProjectDuration";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(newDuration);
                PROJ_DURATION newDur = serviceCaller.Execute<PROJ_DURATION>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/ProjDurationDelete/1
        public Boolean ProjDurationDelete(int id)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ProjectDuration/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<PROJ_DURATION>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion PROJ_DURATION

        #region PROJ_STATUS
        //need to redirect to get index if error and want to refresh ProjStatus
        public ActionResult ProjStatus()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the ProjStatus infoxbox -- get ProjStatusEdit page or redirect to ProjStatusCreate
        //GET: /Settings/Lookups/LookupEdits/ProjStatusEdit/
        [HttpPost]
        public ActionResult ProjStatus(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("ProjStatusCreate"); }
                else
                {//edit
                    int ProjStatusId = Convert.ToInt32(fc["PROJ_STATUS_ID"]);
                    if (ProjStatusId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("ProjStatusEdit", new { id = ProjStatusId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //ProjStatus edit page
        public ActionResult ProjStatusEdit(int id)
        {
            try
            {
                ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);
                if (ViewData["LoggedInRole"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/ProjectStatus/{entityId}";
                    request.RootElement = "PROJ_STATUS";
                    request.AddParameter("entityId", id, ParameterType.UrlSegment);
                    PROJ_STATUS stat = serviceCaller.Execute<PROJ_STATUS>(request);

                    return View("../Settings/Lookups/LookupEdits/ProjStatusEdit", stat);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to ProjStatus, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/ProjStatusEdit/
        [HttpPost]
        public ActionResult ProjStatusEdit(int id, PROJ_STATUS aStat)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ProjectStatus/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(aStat);
                serviceCaller.Execute<PROJ_STATUS>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from ProjStatus Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/ProjStatusCreate/
        public ActionResult ProjStatusCreate()
        {
            try
            {
                ViewData["Role"] = GetUserRole(User.Identity.Name);
                if (ViewData["Role"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/ProjectStatus/";
                    request.RootElement = "ArrayOfPROJ_STATUS";
                    List<PROJ_STATUS> stat = serviceCaller.Execute<List<PROJ_STATUS>>(request);

                    return View("../Settings/Lookups/LookupCreates/ProjStatusCreate", stat);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new ProjStatus was created, post it
        //POST: /Settings/Lookups/LookupCreates/ProjStatusCreate
        [HttpPost]
        public ActionResult ProjStatusCreate(PROJ_STATUS newStatus)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ProjectStatus";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(newStatus);
                PROJ_STATUS newStat = serviceCaller.Execute<PROJ_STATUS>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/ProjStatusDelete/1
        public Boolean ProjStatusDelete(int id)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/ProjectStatus/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<PROJ_STATUS>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion PROJ_STATUS

        #region Resource_Type
        //need to redirect to get index if error and want to refresh ResourceType
        public ActionResult ResourceType()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the ResourceType infoxbox -- get ResourceTypeEdit page or redirect to ResourceTypeCreate
        //GET: /Settings/Lookups/LookupEdits/ResourceTypeEdit/
        [HttpPost]
        public ActionResult ResourceType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("ResourceTypeCreate"); }
                else
                {//edit
                    int ResourceTypeId = Convert.ToInt32(fc["RESOURCE_TYPE_ID"]);
                    if (ResourceTypeId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("ResourceTypeEdit", new { id = ResourceTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //ResourceType edit page
        public ActionResult ResourceTypeEdit(int id)
        {
            try
            {
                ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);
                if (ViewData["LoggedInRole"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/resourcetypes/{resourceTypeId}";
                    request.RootElement = "RESOURCE_TYPE";
                    request.AddParameter("resourceTypeId", id, ParameterType.UrlSegment);
                    RESOURCE_TYPE resT = serviceCaller.Execute<RESOURCE_TYPE>(request);

                    return View("../Settings/Lookups/LookupEdits/ResourceTypeEdit", resT);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to ResourceType, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/ResourceTypeEdit/
        [HttpPost]
        public ActionResult ResourceTypeEdit(int id, RESOURCE_TYPE aResourceType)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/resourcetypes/{resourceTypeId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("resourceTypeId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(aResourceType);
                serviceCaller.Execute<RESOURCE_TYPE>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from ResourceType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/ResourceTypeCreate/
        public ActionResult ResourceTypeCreate()
        {
            try
            {
                ViewData["Role"] = GetUserRole(User.Identity.Name);
                if (ViewData["Role"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/resourcetypes";
                    request.RootElement = "ArrayOfRESOURCE_TYPE";
                    List<RESOURCE_TYPE> resT = serviceCaller.Execute<List<RESOURCE_TYPE>>(request);
                    resT = resT.OrderBy(x => x.RESOURCE_NAME).ToList();

                    return View("../Settings/Lookups/LookupCreates/ResourceTypeCreate", resT);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new ResourceType was created, post it
        //POST: /Settings/Lookups/LookupCreates/ResourceTypeCreate
        [HttpPost]
        public ActionResult ResourceTypeCreate(RESOURCE_TYPE newResourceType)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/resourcetypes";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(newResourceType);
                RESOURCE_TYPE newres = serviceCaller.Execute<RESOURCE_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/ResourceTypeDelete/1
        public Boolean ResourceTypeDelete(int id)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/resourcetypes/{resourceTypeId}";
                request.AddParameter("resourceTypeId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<RESOURCE_TYPE>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Resource_Type

        #region Status_Type
        //need to redirect to get index if error and want to refresh StatusType
        public ActionResult StatusType()
        {
            return RedirectToAction("Index");
        }

        //a edit or add new button was clicked in the StatusType infoxbox -- get StatusTypeEdit page or redirect to StatusTypeCreate
        //GET: /Settings/Lookups/LookupEdits/StatusTypeEdit/
        [HttpPost]
        public ActionResult StatusType(FormCollection fc, string Create)
        {
            try
            {
                if (Create == "Add New")
                { return RedirectToAction("StatusTypeCreate"); }
                else
                {//edit
                    int StatusTypeId = Convert.ToInt32(fc["STATUS_ID"]);
                    if (StatusTypeId == 0) //they didn't choose one before hitting edit
                    { return RedirectToAction("Index"); }
                    else
                    {
                        //send them to edit page with the one to edit
                        return RedirectToAction("StatusTypeEdit", new { id = StatusTypeId });
                    }
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //StatusType edit page
        public ActionResult StatusTypeEdit(int id)
        {
            try
            {
                ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);
                if (ViewData["LoggedInRole"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/status/{statusId}";
                    request.RootElement = "STATUS_TYPE";
                    request.AddParameter("statusId", id, ParameterType.UrlSegment);
                    STATUS_TYPE statT = serviceCaller.Execute<STATUS_TYPE>(request);

                    return View("../Settings/Lookups/LookupEdits/StatusTypeEdit", statT);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //change was made to StatusType, put it, then send back to index
        //POST: /Settings/Lookups/LookupEdits/StatusTypeEdit/
        [HttpPost]
        public ActionResult StatusTypeEdit(int id, STATUS_TYPE aStatusType)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/status/{statusId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("statusId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(aStatusType);
                serviceCaller.Execute<STATUS_TYPE>(request);

                return RedirectToAction("Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //redirected here from StatusType Add New button was clicked
        //GET: /Settings/Lookups/LookupCreates/StatusTypeCreate/
        public ActionResult StatusTypeCreate()
        {
            try
            {
//                ViewData["Role"] = GetUserRole(User.Identity.Name);
                ViewData["Role"] = GetUserRole(Session["Username"].ToString());
                if (ViewData["Role"].ToString() == "Admin")
                {
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();

                    request.Resource = "/status";
                    request.RootElement = "ArrayOfSTATUS_TYPE";
                    List<STATUS_TYPE> statT = serviceCaller.Execute<List<STATUS_TYPE>>(request);
                    statT = statT.OrderBy(x => x.STATUS).ToList();

                    return View("../Settings/Lookups/LookupCreates/StatusTypeCreate", statT);
                }
                else
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Lookups" });
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        // new StatusType was created, post it
        //POST: /Settings/Lookups/LookupCreates/StatusTypeCreate
        [HttpPost]
        public ActionResult StatusTypeCreate(STATUS_TYPE newStatusType)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/status";
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(newStatusType);
                STATUS_TYPE newstat = serviceCaller.Execute<STATUS_TYPE>(request);

                return RedirectToAction("../Lookups/Index");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //
        //GET: /Settings/Lookups/ResourceTypeDelete/1
        public Boolean StatusTypeDelete(int id)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/status/{statusId}";
                request.AddParameter("statusId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<STATUS_TYPE>(request);

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion Status_Type

        //internal function calls
        private string GetUserRole(string username)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/dataManagers?username={userName}";
            request.RootElement = "DATA_MANAGER";
            request.AddParameter("userName", username, ParameterType.UrlSegment);
            DATA_MANAGER thisDM = serviceCaller.Execute<DATA_MANAGER>(request);
            int loggedInMember = Convert.ToInt32(thisDM.ROLE_ID);
            string Role = string.Empty;
            switch (loggedInMember)
            {
                case 1: Role = "Admin"; break;
                case 2: Role = "Manager"; break;
                case 3: Role = "Public"; break;
                default: Role = "error"; break;
            }

            return Role;
        }
    }
}
