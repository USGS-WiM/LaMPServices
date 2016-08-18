//------------------------------------------------------------------------------
//----- ReportingController.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2015 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Display a master Reporting page and link to individual data manager pages 
//
//discussion:   
//
//     

#region Comments
//02.20.15 - TR - Created

#endregion


using System;
using System.Web.Mvc;

using LaMPWeb.Helpers;


namespace LaMPWeb.Controllers
{
    [RequireSSL]
    [Authorize]
    public class ReportingController : Controller
    {
        //Report page
        public ActionResult Index()
        {
            try
            {
                
                return View("../Settings/Reporting/Report");
            }
            catch (Exception e)
            {
                return View("../Shared/Error", e);
            }
        }

    }
}
