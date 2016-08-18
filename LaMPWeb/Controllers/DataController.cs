//------------------------------------------------------------------------------
//----- DataController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display master pages
//
//discussion:   
//
//     

#region Comments
// 02.17.15 - TR - Created 
#endregion


using System;
using System.Collections.Generic;
using System.Web.Mvc;

using LaMPWeb.Utilities;
using RestSharp;

using LaMPServices;
using LaMPWeb.Helpers;

namespace LaMPWeb.Controllers
{
    [Authorize]
    [RequireSSL]
    public class DataController : Controller
    {
        //GET: infobox containing data for a project
        public PartialViewResult DataInfoBox(int id)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "projects/{projectId}/dataHosts";
            request.RootElement = "ArrayOfDATA_HOST";
            request.AddParameter("projectId", id, ParameterType.UrlSegment);
            ViewData["data"] = serviceCaller.Execute<List<DATA_HOST>>(request);
            //pass the projectId back
            ViewData["projectId"] = id;

            return PartialView();
        }

        public ActionResult DataDetails(int id, int projId)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/datahosts/{entityId}";
            request.RootElement = "DATA_HOST";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            DATA_HOST thisData = serviceCaller.Execute<DATA_HOST>(request);

            //pass this project
            ViewData["project"] = GetThisProject(projId);

            return View(thisData);
        }
        
        //GET: Edit page for project data
        public ActionResult DataEdit(int id, int projId)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/datahosts/{entityId}";
            request.RootElement = "DATA_HOST";
            request.AddParameter("entityId", id, ParameterType.UrlSegment);
            DATA_HOST thisData = serviceCaller.Execute<DATA_HOST>(request);

            //pass this project
            ViewData["Project"] = GetThisProject(projId);

            return View(thisData);
        }


        //POST: post the edit
        [HttpPost]
        public ActionResult DataEdit(int id, DATA_HOST thisData, int projId)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/datahosts/{entityId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(thisData);
                DATA_HOST updatedData = serviceCaller.Execute<DATA_HOST>(request);
                return RedirectToAction("DataDetails", new { id = updatedData.DATA_HOST_ID, projId = projId });
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }


        //GET: /Datahosts/Delete/1
        public ActionResult Delete(int id, int projID)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "datahosts/{entityId}";
                request.AddParameter("entityId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<DATA_HOST>(request);

                return RedirectToAction("ProjectDetails", "Project", new { id = projID });
            }
            catch
            {
                return View();
            }
        }
        
        // GET: /Datahosts/
        public ActionResult DataCreate(int id, string From)
        {
            ViewData["project"] = GetThisProject(id);
            
            //get any Data for this project
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/projects/{projectId}/dataHosts";
            request.RootElement = "ArrayOfDATA_HOST";
            request.AddParameter("projectId", id, ParameterType.UrlSegment);
            List<DATA_HOST> projData = serviceCaller.Execute<List<DATA_HOST>>(request);
            
            if (projData.Count >= 1)
            {
                ViewData["Data"] = projData;
            }

            if (From == "Data")
            {
                ViewData["From"] = From;
            }

            return View();
        }

        [HttpPost]
        public ActionResult DataCreate(DATA_HOST dh, string Create)
        {
            try
            {
                decimal projId = dh.PROJECT_ID;
                string From = Request.Form["From"];

                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/projects/{projectId}/addDataHost";
                request.AddParameter("projectId", projId, ParameterType.UrlSegment);
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(dh);
                List<DATA_HOST> createdData = serviceCaller.Execute<List<DATA_HOST>>(request);

                if (Create == "Save & Add\r\n Another Data")
                {
                    return RedirectToAction("DataCreate", new { id = projId, From = From });
                }
                else if (Create == "Save & Go To\r\n Project Details")
                {
                    return RedirectToAction("ProjectDetails", "Project", new { id = projId });
                }
                else
                {
                    return RedirectToAction("ContactCreate", "Contact", new { id = projId, From = "Data" });
                }
            }
            catch (Exception e)
            {
                return View(e.ToString());
            }
        }

        // methods called from above////////

        //GET: get this particular project based on id
        public PROJECT GetThisProject(int id)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/projects/{projectId}";
            request.RootElement = "projects";
            request.AddParameter("projectId", id, ParameterType.UrlSegment);
            PROJECT thisProject = serviceCaller.Execute<PROJECT>(request);
            return thisProject;
        }


    }
}
