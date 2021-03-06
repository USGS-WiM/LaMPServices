﻿//------------------------------------------------------------------------------
//----- DataManagerController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2014 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display a master Data Manager page and link to individual data manager pages 
//
//discussion:   
//
//     

#region Comments 
//10.01.14 - TR - Created

#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using RestSharp;
using LaMPServices;
using MoreLinq;
using LaMPWeb.Utilities;
using LaMPWeb.Models;
using LaMPWeb.Helpers;


namespace LaMPWeb.Controllers
{
    [RequireSSL]
    [Authorize]
    public class DataManagerController : Controller
    {       
        //List of all Data managers
        public ActionResult Index()
        {
            try
            {
                //get the logged in member for authorization
                ViewData["Role"] = GetUserRole(User.Identity.Name);

                List<DataManListingModel> allDMModel = new List<DataManListingModel>();
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/dataManagers";
                request.RootElement = "ArrayOfDATA_MANAGER";
                List<DATA_MANAGER> DMList = serviceCaller.Execute<List<DATA_MANAGER>>(request);
                List<DATA_MANAGER> SortedDMs = DMList.OrderBy(x => x.LNAME).ToList();

                //request agencies
                request = new RestRequest();
                request.Resource = "/Organizations";
                request.RootElement = "ArrayOfORGANIZATION";
                List<ORGANIZATION> allOrgs = serviceCaller.Execute<List<ORGANIZATION>>(request);
                //List<ORGANIZATION> theOrgs = allOrgs.DistinctBy(a => a.NAME).ToList();

                //request roles
                request = new RestRequest();
                request.Resource = "/Roles";
                request.RootElement = "ArrayOfROLE";
                List<ROLE> theRoles = serviceCaller.Execute<List<ROLE>>(request);

                //loop through members to get each member and their agency and role
                foreach (DATA_MANAGER dem in SortedDMs)
                {
                    DataManListingModel aDM = new DataManListingModel();

                    aDM.DataManagerID = dem.DATA_MANAGER_ID;
                    aDM.DataManagerName = dem.LNAME + ", " + dem.FNAME;

                    //loop through agencies to get agencyname that matches mem.agencyid
                    if (dem.ORGANIZATION_ID != null)
                    {
                        ORGANIZATION thisOrg = allOrgs.FirstOrDefault(a => a.ORGANIZATION_ID == dem.ORGANIZATION_ID);
                        if (thisOrg != null)
                        {
                            aDM.OrganizationName = string.IsNullOrEmpty(thisOrg.NAME) ? "" : thisOrg.NAME;
                        }
                    }
                    //loop through roles to get role name that matches mem.roleid
                    if (dem.ROLE_ID != null)
                    { 
                        ROLE thisRole = theRoles.FirstOrDefault(r => r.ROLE_ID == dem.ROLE_ID);
                        if (thisRole != null)
                        {
                            aDM.RoleName = string.IsNullOrEmpty(thisRole.ROLE_NAME) ? "" : thisRole.ROLE_NAME; 
                        }
                    }
                    allDMModel.Add(aDM);
                }

                return View("../Settings/DataManagers/Index", allDMModel);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //create a data manager
        public ActionResult DataManagerCreate()
        {
            try
            {
                //get logged in user's role
                ViewData["Role"] = GetUserRole(User.Identity.Name);

                if (ViewData["Role"].ToString() == "MANAGER")
                {
                    return RedirectToAction("NotAuthorized", "Home", new { from = "Member" });
                }
                else
                {
                    //get the orgs and roles to populate dropdowns
                    LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                    var request = new RestRequest();
                    request.Resource = "/Organizations";
                    request.RootElement = "ArrayOfORGANIZATION";
                    List<ORGANIZATION> orgList = serviceCaller.Execute<List<ORGANIZATION>>(request);
                    orgList = orgList.OrderBy(x => x.NAME).ToList();
                    orgList = orgList.DistinctBy(a => a.NAME).ToList();
                    ViewData["DistinctOrgs"] = orgList;

                    request = new RestRequest();
                    request.Resource = "/Roles";
                    request.RootElement = "ArrayOfROLE";
                    ViewData["AllRoles"] = serviceCaller.Execute<List<ROLE>>(request);

                    return View("../Settings/DataManagers/DataManagerCreate");
                }
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        [HttpPost]
        public ActionResult DataManagerCreate(FormCollection fc)
        {
            try
            {
                DATA_MANAGER newDataManager = new DATA_MANAGER();
                newDataManager.USERNAME = fc["UserName"];
                newDataManager.FNAME = fc["FName"];
                newDataManager.LNAME = fc["LName"];

                int Section = Convert.ToInt32(fc["section"]);
                int Division = Convert.ToInt32(fc["division"]);
                if (Section != 0 && Section != null)
                { newDataManager.ORGANIZATION_ID = Section; }
                else if (Division != 0 && Division != null)
                { newDataManager.ORGANIZATION_ID = Division; }

                newDataManager.PHONE = fc["PHONE"];
                newDataManager.EMAIL = fc["EMAIL"];
                newDataManager.ROLE_ID = Convert.ToDecimal(fc["Role_ID"]);

                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "dataManagers/{pass}/addDataManager";
                request.AddParameter("pass", fc["Password"], ParameterType.UrlSegment);
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(newDataManager);
                DATA_MANAGER createdDM = serviceCaller.Execute<DATA_MANAGER>(request);

                return RedirectToAction("DataManagerDetails", new { id = createdDM.DATA_MANAGER_ID });
            }
            catch (Exception e)
            {
                return View("../../Shared/Error", e);
            }
        }

        //details page for data manager
        public ActionResult DataManagerDetails(int id)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/dataManagers/{dataManagerId}";
                request.RootElement = "DATA_MANAGER";
                request.AddParameter("dataManagerId", id, ParameterType.UrlSegment);
                DATA_MANAGER thisDM = serviceCaller.Execute<DATA_MANAGER>(request);

                //role of who's logged in
                ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);
                
                //get Org
                if (thisDM.ORGANIZATION_ID != null)
                {
                    request = new RestRequest();
                    request.Resource = "/Organizations/{organizationId}";
                    request.RootElement = "ORGANIZATION";
                    request.AddParameter("organizationId", thisDM.ORGANIZATION_ID, ParameterType.UrlSegment);
                    ORGANIZATION thisOrg = serviceCaller.Execute<ORGANIZATION>(request);

                    if (thisOrg != null)
                    {
                        if (!string.IsNullOrEmpty(thisOrg.SECTION))
                        {
                            ViewData["thisOrg"] = thisOrg.NAME + ", " + thisOrg.DIVISION + ", " + thisOrg.SECTION;
                        }
                        else if (!string.IsNullOrEmpty(thisOrg.DIVISION))
                        {
                            ViewData["thisOrg"] = thisOrg.NAME + ", " + thisOrg.DIVISION;
                        }
                        else
                        {
                            ViewData["thisOrg"] = thisOrg.NAME;
                        }
                    }

                }
                //get the role of user detail page is for
                if (thisDM.ROLE_ID != null)
                {
                    ViewData["thisRole"] = GetUserRole(thisDM.USERNAME);
                }

                return View("../Settings/DataManagers/DataManagerDetails", thisDM);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //username was clicked from top right, give them their details page
        public ActionResult DataManagerDetailsByName(string name)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/dataManagers?username={userName}";
                request.RootElement = "DATA_MANAGER";
                request.AddParameter("userName", name, ParameterType.UrlSegment);
                DATA_MANAGER thisDM = serviceCaller.Execute<DATA_MANAGER>(request);

                //role of who's logged in
                ViewData["LoggedInRole"] = GetUserRole(name);

                //get Org
                if (thisDM.ORGANIZATION_ID != null)
                {
                    request = new RestRequest();
                    request.Resource = "/Organizations/{organizationId}";
                    request.RootElement = "ORGANIZATION";
                    request.AddParameter("organizationId", thisDM.ORGANIZATION_ID, ParameterType.UrlSegment);
                    ORGANIZATION thisOrg = serviceCaller.Execute<ORGANIZATION>(request);

                    if (thisOrg != null)
                    {
                        if (!string.IsNullOrEmpty(thisOrg.SECTION))
                        {
                            ViewData["thisOrg"] = thisOrg.NAME + ", " + thisOrg.DIVISION + ", " + thisOrg.SECTION;
                        }
                        else if (!string.IsNullOrEmpty(thisOrg.DIVISION))
                        {
                            ViewData["thisOrg"] = thisOrg.NAME + ", " + thisOrg.DIVISION;
                        }
                        else
                        {
                            ViewData["thisOrg"] = thisOrg.NAME;
                        }
                    }

                }
                //get the role of user detail page is for
                if (thisDM.ROLE_ID != null)
                {
                    ViewData["thisRole"] = GetUserRole(thisDM.USERNAME);
                }

                return View("../Settings/DataManagers/DataManagerDetails", thisDM);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //edit page for data manager
        public ActionResult DataManagerEdit(int id)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest();
                request.Resource = "/dataManagers/{dataManagerId}";
                request.RootElement = "DATA_MANAGER";
                request.AddParameter("dataManagerId", id, ParameterType.UrlSegment);
                DATA_MANAGER thisDM = serviceCaller.Execute<DATA_MANAGER>(request);

                //get the logged in member for authorization
                ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);

                //get role of data manager being edited
                ViewData["thisRole"] = GetUserRole(thisDM.USERNAME);

                //get all orgs
                List<ORGANIZATION> allOrgs = GetAllOrganizations();
                List<ORGANIZATION> filteredOrgs = allOrgs.DistinctBy(d => d.NAME).OrderBy(x=>x.NAME).ToList();
                ViewData["Organizations"] = filteredOrgs;

                //get this DM's Org
                if (thisDM.ORGANIZATION_ID.HasValue)
                {
                    ViewData["DMOrg"] = GetAnOrganization(thisDM.ORGANIZATION_ID);
                }

                return View("../Settings/DataManagers/DataManagerEdit", thisDM);
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

        //PUT
        [HttpPost]
        public ActionResult DataManagerEdit(FormCollection fc, int id)
        {
             try
            {
                 LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                 var request = new RestRequest(Method.POST);

                 //build DM from fc.. need to see if Division or Section changed...
                 DATA_MANAGER editingDM = new DATA_MANAGER();
                 editingDM.DATA_MANAGER_ID = id;
                 editingDM.USERNAME = fc["USERNAME"];
                 editingDM.FNAME = fc["FNAME"];
                 editingDM.LNAME = fc["LNAME"];
                 editingDM.PHONE = fc["PHONE"];
                 editingDM.EMAIL = fc["EMAIL"];
                 var roleID = fc["ROLE_ID"];
                 editingDM.ROLE_ID = Convert.ToDecimal(roleID);

                 //existing org id
                 int orgId = Convert.ToInt32(fc["ORGANIZATION_ID"]);
                
                 //if they changed org, these will be populated and use these instead of existing
                 int newOrgId = Convert.ToInt32(fc["ProjOrg"]);
                 int newDivOrgId = Convert.ToInt32(fc["division"]);
                 int newSecOrgId = Convert.ToInt32(fc["section"]);
                
                 if (newSecOrgId > 0)
                 { editingDM.ORGANIZATION_ID = newSecOrgId; }
                 else if (newDivOrgId > 0)
                 { editingDM.ORGANIZATION_ID = newDivOrgId; }
                 else if (newOrgId > 0)
                 { editingDM.ORGANIZATION_ID = orgId; }
                 else
                 { editingDM.ORGANIZATION_ID = orgId; }

                 request.Resource = "/dataManagers/{dataManagerId}";
                 request.RequestFormat = DataFormat.Xml;
                 request.AddParameter("dataManagerId", id, ParameterType.UrlSegment);
                 request.AddHeader("X-HTTP-Method-Override", "PUT");
                 request.AddHeader("Content-Type", "application/xml");
                 request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                 request.AddBody(editingDM);
                 DATA_MANAGER updatedDM = serviceCaller.Execute<DATA_MANAGER>(request);

                 return RedirectToAction("DataManagerDetails", new { id = updatedDM.DATA_MANAGER_ID });
             }
             catch (Exception e)
             {
                 return View(e.ToString());
             }
        }

        //change password
        //GET: /Settings/Members/MemberEdit/1
        public PartialViewResult DataManagerPassword(int id)
        {
            try
            {
                //get the logged in member for authorization
                ViewData["LoggedInRole"] = GetUserRole(User.Identity.Name);

                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "DataManagers/{dataManaerId}";
                request.RootElement = "DATA_MANAGER";
                request.AddParameter("dataManaerId", id, ParameterType.UrlSegment);

                DATA_MANAGER aDataManager = serviceCaller.Execute<DATA_MANAGER>(request);

                return PartialView("../Settings/DataManagers/ChangeDMPassword", aDataManager);
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

        [HttpPost]
        public ActionResult DMPassword(FormCollection fc)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest();

                request.Resource = "/dataManagers?username={userName}&new={newPassword}";
                request.RootElement = "DATA_MANAGER";
                request.AddParameter("userName", fc["USERNAME"], ParameterType.UrlSegment);
                request.AddParameter("newPassword", fc["New_Password"], ParameterType.UrlSegment);
                DATA_MANAGER updatedDM = serviceCaller.Execute<DATA_MANAGER>(request);

                return RedirectToAction("DataManagerDetails", new { id = updatedDM.DATA_MANAGER_ID });
            }
            catch (Exception e)
            {
                return PartialView("../Shared/Error", e);
            }
        }

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

        private List<ORGANIZATION> GetAllOrganizations()
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "organizations";
            request.RootElement = "ArrayOfORGANIZATION";
            List<ORGANIZATION> organizationList = serviceCaller.Execute<List<ORGANIZATION>>(request);

            return organizationList;
        }

        private ORGANIZATION GetAnOrganization(decimal? id)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "organizations/{orgId}";
            request.RootElement = "organization";
            request.AddParameter("orgId", id, ParameterType.UrlSegment);
            ORGANIZATION thisOrg = serviceCaller.Execute<ORGANIZATION>(request);
            return thisOrg;
        }
        
    }
}
