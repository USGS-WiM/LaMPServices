
/*Comments:
 03.10.13 - TR - Changed index to list of projects for manager signed in
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using LaMPWeb.Utilities;
using RestSharp;

using LaMPServices;
using LaMPServices.Resources;
using LaMPWeb.Helpers;

namespace LaMPWeb.Controllers
{
    [Authorize]
    [RequireSSL]
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.CurrentPage = "HOME";

            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/projects/IndexProjects";
            request.RootElement = "ArrayOfPRODUCT";
            List<ProjectRes> ProjectList = serviceCaller.Execute<List<ProjectRes>>(request);            
            
            if (ProjectList != null && ProjectList.Count > 0)
            {
                ViewData["ProjectList"] = ProjectList.OrderBy(x => x.Name).ToList();
                Session["TotProj"] = String.Format("{0:n0}", ProjectList.Count());
                Session["TotSites"] = String.Format("{0:n0}", ProjectList.AsEnumerable().Sum(x => x.SiteCount));
            }

            ViewData["Role"] = GetLoggedInRole();

            return View();
        }

        public ActionResult About()
        {
            return View();
        }

        //call for who the member logged in is 
        public string GetLoggedInRole()
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/dataManagers?username={userName}";
            request.RootElement = "DATA_MANAGER";
            request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
            DATA_MANAGER thisUser = serviceCaller.Execute<DATA_MANAGER>(request);
            int loggedInMember = Convert.ToInt32(thisUser.ROLE_ID);
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

        //call for who the member logged in is 
        public string GetLoggedInMember()
        {
            LaMPServiceCaller serviceCaller = LaMPServiceCaller.Instance;
            var request = new RestRequest();
            request.Resource = "/dataManagers?username={userName}";
            request.RootElement = "DATA_MANAGER";
            request.AddParameter("userName", User.Identity.Name, ParameterType.UrlSegment);
            DATA_MANAGER thisUser = serviceCaller.Execute<DATA_MANAGER>(request);
            
            return thisUser.USERNAME;
        }

        public ActionResult Mapper()
        {
            return View();
        }

        public ActionResult Finish()
        {
            return View();
        }
    }
}
