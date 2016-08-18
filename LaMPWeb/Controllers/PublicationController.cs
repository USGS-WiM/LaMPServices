//------------------------------------------------------------------------------
//----- PublicationController.cs-----------------------------------------------------
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
// 07.17.13 - TR - Created 
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
    public class PublicationController : Controller
    {
        //GET: infobox containing publications for a project
        public PartialViewResult PubInfoBox(int id)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "projects/{projectId}/publications";
            request.RootElement = "ArrayOfPublications";
            request.AddParameter("projectId", id, ParameterType.UrlSegment);
            ViewData["publications"] = serviceCaller.Execute<List<PUBLICATION>>(request);
            //pass the projectId back
            ViewData["projectId"] = id;

            return PartialView();
        }

        public ActionResult PublicationDetails(int id, int projId)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/publications/{publicationId}";
            request.RootElement = "PUBLICATION";
            request.AddParameter("publicationId", id, ParameterType.UrlSegment);
            PUBLICATION thisPub = serviceCaller.Execute<PUBLICATION>(request);

            //pass this project
            ViewData["project"] = GetThisProject(projId);

            return View(thisPub);
        }
        
        //GET: Edit page for publication
        public ActionResult PublicationEdit(int id, int projId)
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/publications/{publicationId}";
            request.RootElement = "PUBLICATION";
            request.AddParameter("publicationId", id, ParameterType.UrlSegment);
            PUBLICATION thisPub = serviceCaller.Execute<PUBLICATION>(request);

            //pass this project
            ViewData["Project"] = GetThisProject(projId);

            return View(thisPub);
        }


        //POST: post the edit
        [HttpPost]
        public ActionResult PublicationEdit(int id, PUBLICATION thisPub, int projId)
        {
            try
            {
                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);

                request.Resource = "/publications/{publicationId}";
                request.RequestFormat = DataFormat.Xml;
                request.AddParameter("publicationId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "PUT");
                request.AddHeader("Content-Type", "application/xml");
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(thisPub);
                PUBLICATION updatedPub = serviceCaller.Execute<PUBLICATION>(request);
                return RedirectToAction("PublicationDetails", new { id = updatedPub.PUBLICATION_ID, projId = projId });
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

                request.Resource = "Publications/{publicationId}";
                request.AddParameter("publicationId", id, ParameterType.UrlSegment);
                request.AddHeader("X-HTTP-Method-Override", "DELETE");
                request.AddHeader("Content-Type", "application/xml");
                serviceCaller.Execute<PUBLICATION>(request);

                return RedirectToAction("ProjectDetails", "Project", new { id = projID });
            }
            catch
            {
                return View();
            }
        }
        
        // GET: /Publication/
        public ActionResult PublicationCreate(int id, string From)
        {
            ViewData["project"] = GetThisProject(id);
            //get any publications for this project
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/projects/{projectId}/publications";
            request.RootElement = "ArrayOfPUBLICATION";
            request.AddParameter("projectId", id, ParameterType.UrlSegment);
            List<PUBLICATION> projPubs = serviceCaller.Execute<List<PUBLICATION>>(request);
            if (projPubs.Count >= 1)
            {
                ViewData["publications"] = projPubs;
            }

            if (From == "Contacts")
            {
                ViewData["From"] = From;
            }

            return View();
        }

        [HttpPost]
        public ActionResult PublicationCreate(FormCollection fc, string Create)
        {
            try
            {
                decimal projId = Convert.ToDecimal(fc["projId"]);
                string From = fc["From"];

                PUBLICATION aPub = new PUBLICATION();
                aPub.TITLE = fc["Publication.TITLE"];
                aPub.DESCRIPTION = fc["Publication.DESCRIPTION"];
                aPub.URL = fc["Publication.URL"];

                LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
                var request = new RestRequest(Method.POST);
                request.Resource = "/projects/{projectId}/addPublication";
                request.AddParameter("projectId", projId, ParameterType.UrlSegment);
                request.RequestFormat = DataFormat.Xml;
                request.XmlSerializer = new RestSharp.Serializers.DotNetXmlSerializer();
                request.AddBody(aPub);
                List<PUBLICATION> createdPublication = serviceCaller.Execute<List<PUBLICATION>>(request);

                if (Create == "Save & Add\r\n Another Publication")
                {
                    return RedirectToAction("PublicationCreate", new { id = projId, From = From });
                }
                else if (Create == "Save & Go To\r\n Project Details")
                {
                    return RedirectToAction("ProjectDetails", "Project", new { id = projId });
                }
                else
                {
                    return RedirectToAction("SiteCreate1", "Site", new { id = projId, From = "Publications" });
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
