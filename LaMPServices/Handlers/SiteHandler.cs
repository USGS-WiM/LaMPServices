﻿//------------------------------------------------------------------------------
//----- SiteHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Site resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     
#region Comments
// 04.01.14 - TR - Adding Site Model for mapper Site point click
// 03.07.14 - TR - Adding endpoint for mapper filter
// 08.22.13 - TR - Changed this to SiteHandler and removed Location from schema
// 04.02.13 - jkn - Created public user for get requests
// 03.22.13 - jkn - Edited DB schema to have a many2many relation with frequency,media, resources
// 04.26.12 - jkn - Created
#endregion                          


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using LaMPServices.Authentication;
using LaMPServices.Resources;
using OpenRasta.Web;
using OpenRasta.Security;
using System.Configuration;

using LaMPServices.Utilities;

namespace LaMPServices.Handlers
{
    public class SiteHandler:HandlerBase
    {
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 siteId)
        {
            SITE aSite;

            //return BadRequest if ther is no ID
            if (siteId <= 0)
            {return new OperationResult.BadRequest();}

            using (LaMPDSEntities aLaMPRDS = GetRDS())
                {
                    aSite = aLaMPRDS.SITE.SingleOrDefault(c => c.SITE_ID == siteId);

                    if (aSite != null)
                        aSite.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                }//end using


            return new OperationResult.OK { ResponseResource = aSite };
        }//end httpMethod get


        [HttpOperation(HttpMethod.GET, ForUriName = "GetSitesView")]
        public OperationResult GetSiteView()
        {
            List<SITE_LIST_VIEW> aSiteList = null;
            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                {
                    aSiteList = aLaMPRDS.SITE_LIST_VIEW.ToList();
                }


            }

            return new OperationResult.OK { ResponseResource = aSiteList };
        }//end httpMethod get


        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<SITE> aSiteList = null;

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    if (IsAuthorized(ManagerRole))
                    {
                        aSiteList = aLaMPRDS.SITE.Where(c => c.PROJECT.DATA_MANAGER.USERNAME == username).OrderBy(c => c.SITE_ID).ToList();
                    }
                    else//AdminRole
                    {
                        aSiteList = aLaMPRDS.SITE.OrderBy(c => c.SITE_ID).ToList();
                    }


                    if (aSiteList != null)
                        aSiteList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = aSiteList };
        }//end httpMethod get


        //merge these two
        [HttpOperation(HttpMethod.POST, ForUriName = "GetFilteredSites")]
        public OperationResult GetFilteredSites([Optional] string parameters, [Optional] string fromDate, [Optional] string toDate, [Optional] string durationIDs, [Optional] string statusIDs, [Optional] string resComps, [Optional] string media, [Optional] string lakes, [Optional] string states)
        {
            try
            {
                IQueryable<SITE> query;

                List<SITE> sites = new List<SITE>();
                List<Decimal> _parameters = new List<Decimal>();
                DateTime fDate;
                DateTime tDate;
                List<Decimal> _durations = new List<Decimal>();
                List<Decimal> _status = new List<Decimal>();
                List<Decimal> _resources = new List<Decimal>();
                List<Decimal> _media = new List<Decimal>();
                List<Decimal> _lakes = new List<Decimal>();
                List<string> _states = new List<string>();

                char[] delimiter = { ',' };

                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {
                    query = aLaMPRDS.SITE;

                    //is this a parameter filter or parameter type filter request?
                    if (!string.IsNullOrEmpty(parameters))
                    {
                        List<string> paramStrings = parameters.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();

                        paramStrings.ForEach(p => _parameters.Add(Convert.ToDecimal(p)));
                        
                        //go through site_parameters table and give me all sites with matching parameter_type_id as is in the list
                        query = query.Where(s=> s.SITE_PARAMETERS.Any(a => _parameters.Contains(a.PARAMETER_TYPE_ID.Value)));    
                    }

                    //is there a date range specified
                    if (!string.IsNullOrEmpty(fromDate))
                    {
                        fDate = Convert.ToDateTime(fromDate);

                        query = query.Where(s => s.START_DATE >= fDate);
                    }

                    if (!string.IsNullOrEmpty(toDate))
                    {
                        tDate = Convert.ToDateTime(toDate);
                        query = query.Where(s => s.END_DATE <= tDate);
                    }

                    //get projects with these durations sites that match
                    if (!string.IsNullOrEmpty(durationIDs))
                    {
                        List<string> durations = durationIDs.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                        durations.ForEach(d => _durations.Add(Convert.ToDecimal(d)));

                        query = query.Where(s => _durations.Any(d => d == s.PROJECT.PROJ_DURATION_ID));
                    }

                    //get projects with these status sites that match
                    if (!string.IsNullOrEmpty(statusIDs))
                    {
                        List<string> stats = statusIDs.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                        stats.ForEach(d => _status.Add(Convert.ToDecimal(d)));

                        query = query.Where(s => _status.Any(d => d == s.PROJECT.PROJ_STATUS_ID));
                    }

                    //get resources
                    if (!string.IsNullOrEmpty(resComps))
                    {
                        List<string> resStrings = resComps.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                        
                        resStrings.ForEach(rs =>  _resources.Add(Convert.ToDecimal(rs)));
                        
                        //go through site_resources table and give me all sites with matching resource_type_id as is in the list
                        query = query.Where(s => s.SITE_RESOURCE.Any(a => _resources.Contains(a.RESOURCE_TYPE_ID)));
                          
                    }

                    //Media
                    if (!string.IsNullOrEmpty(media))
                    {
                        List<string> medStrings = media.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                        
                        medStrings.ForEach(ms => _media.Add(Convert.ToDecimal(ms)));
                        
                        //go through site_media table and give me all sites with matching media_type_id as is in the list
                        query = query.Where(s => s.SITE_MEDIA.Any(a => _media.Contains(a.MEDIA_TYPE_ID)));

                    }
                    
                    //Lakes
                    if (!string.IsNullOrEmpty(lakes))
                    {
                        List<string> lakeStrings = lakes.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                        
                        lakeStrings.ForEach(ls => _lakes.Add(Convert.ToDecimal(ls)));
                        
                        //go through site table and give me all sites with matching lake_type_id as is in the list
                        query = query.Where(x => _lakes.Any(l => l == x.LAKE_TYPE_ID));
                    }

                    //States
                    if (!string.IsNullOrEmpty(states))
                    {
                        List<string> stateStrings = states.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                        
                        stateStrings.ForEach(ss => _states.Add(ss));
                        
                        //go through site table and give me all sites with matching state name as is in the list
                        query = query.Where(x => _states.Any(s => s == x.STATE_PROVINCE)); 
                    }                    

                    sites = query.ToList();
                }//end using

                return new OperationResult.OK { ResponseResource = sites };

            }
            catch (Exception e)
            {
                return new OperationResult.BadRequest();
            }
        }

        [HttpOperation(HttpMethod.POST, ForUriName = "GetFilteredProjectSites")]
        public OperationResult GetFilteredProjectSites([Optional] string orgID, [Optional] string objIDs, [Optional] string durationIDs, [Optional] string statusIDs, [Optional] string fromDate, [Optional] string toDate, [Optional] string lakes, [Optional] string states)
        {
            try
            {
                IQueryable<SITE> query;

                List<SITE> sites = new List<SITE>();
                Decimal _orgID = string.IsNullOrEmpty(orgID) ? 0 : Convert.ToDecimal(orgID);
                List<Decimal> _orgs = new List<Decimal>();
                List<Decimal> _objectiveTypes = new List<Decimal>();
                List<Decimal> _durations = new List<Decimal>();
                List<Decimal> _status = new List<Decimal>();
                DateTime fDate;
                DateTime tDate;
                List<Decimal> _lakes = new List<Decimal>();
                List<string> _states = new List<string>();

                char[] delimiter = { ',' };

                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {
                    //query = aLaMPRDS.SITE.Where(s => s.PROJECT.READY_FLAG == 1);
                    query = aLaMPRDS.SITE;
                    //did they choose to find sites that are a part of a project with this orgID
                    if (_orgID >= 1)
                    {
                        //sites where project_cooperators have the org NAME that is this OrgID passed
                        //get this org to grab the name
                     //   string thisOrgName = aLaMPRDS.ORGANIZATION_SYSTEM.FirstOrDefault(a=>a.ORGANIZATION_SYSTEM_ID == _orgID).ORGANIZATION.ORGANIZATION_NAME;
                        //get all orgs that have this same name
                        List<ORGANIZATION_SYSTEM> orgSystemsWithThisOrg = aLaMPRDS.ORGANIZATION_SYSTEM.Where(b => b.ORG_ID == _orgID).ToList();
                        //add all the ids to the list of dec
                        orgSystemsWithThisOrg.ForEach(no => _orgs.Add(Convert.ToDecimal(no.ORGANIZATION_SYSTEM_ID)));
                        query = query.Where(s => s.PROJECT.COOPERATORS.Any(a => _orgs.Contains(a.ORGANIZATION_SYSTEM_ID.Value)));
                      
                    }
                    
                    //get project object sites that match
                    if (!string.IsNullOrEmpty(objIDs))
                    {
                        List<string> objectiveTypes = objIDs.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();

                        objectiveTypes.ForEach(ot => _objectiveTypes.Add(Convert.ToDecimal(ot)));

                        //go through project objectives table and give me all sites with matching object_type_id as is in the list
                        query = query.Where(s => s.PROJECT.PROJECT_OBJECTIVES.Any(a => _objectiveTypes.Contains(a.OBJECTIVE_ID)));
                    }

                    //get projects with these durations sites that match
                    if (!string.IsNullOrEmpty(durationIDs))
                    {
                        List<string> durations = durationIDs.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                        durations.ForEach(d => _durations.Add(Convert.ToDecimal(d)));
                        
                        query = query.Where(s => _durations.Any(d => d == s.PROJECT.PROJ_DURATION_ID));
                    }

                    //get projects with these status sites that match
                    if (!string.IsNullOrEmpty(statusIDs))
                    {
                        List<string> stats = statusIDs.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();
                        stats.ForEach(d => _status.Add(Convert.ToDecimal(d)));

                        query = query.Where(s => _status.Any(d => d == s.PROJECT.PROJ_STATUS_ID));
                    }

                    //is there a date range specified
                    if (!string.IsNullOrEmpty(fromDate))
                    {
                        fDate = Convert.ToDateTime(fromDate);

                        query = query.Where(s => s.START_DATE >= fDate);
                    }

                    if (!string.IsNullOrEmpty(toDate))
                    {
                        tDate = Convert.ToDateTime(toDate);
                        query = query.Where(s => s.END_DATE <= tDate);
                    }

                    //Lakes
                    if (!string.IsNullOrEmpty(lakes))
                    {
                        List<string> lakeStrings = lakes.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();

                        lakeStrings.ForEach(ls => _lakes.Add(Convert.ToDecimal(ls)));

                        //go through site table and give me all sites with matching lake_type_id as is in the list
                        query = query.Where(x => _lakes.Any(l => l == x.LAKE_TYPE_ID));
                    }
                    
                    //States
                    if (!string.IsNullOrEmpty(states))
                    {
                        List<string> stateStrings = states.Split(delimiter, StringSplitOptions.RemoveEmptyEntries).ToList();

                        stateStrings.ForEach(ss => _states.Add(ss));

                        //go through site table and give me all sites with matching state name as is in the list
                        query = query.Where(x => _states.Any(s => s == x.STATE_PROVINCE)); 

                    }

                    sites = query.ToList();
                }//end using

                return new OperationResult.OK { ResponseResource = sites };

            }
            catch (Exception e)
            {
                return new OperationResult.BadRequest();
            }
        }
        
        [HttpOperation(HttpMethod.GET, ForUriName = "GetProjectSites")]
        public OperationResult GetProjectSites(Int32 projectId)
        {
            List<SITE> siteList = null;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                siteList = aLaMPRDS.PROJECTS.SingleOrDefault(p => p.PROJECT_ID == projectId).SITE.OrderBy(c => c.SITE_ID).ToList();

                if (siteList != null)
                    siteList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using

            return new OperationResult.OK { ResponseResource = siteList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetFrequencySites")]
        public OperationResult GetFrequencySites(Int32 frequencyId)
        {
            List<SITE> siteList = null;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                siteList = aLaMPRDS.SITE_FREQUENCY.Where(cf => cf.FREQUENCY_TYPE_ID == frequencyId)
                                                    .Select(cf => cf.SITE).OrderBy(c => c.SITE_ID).ToList();

                if (siteList != null)
                    siteList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using

            return new OperationResult.OK { ResponseResource = siteList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetLakeSites")] //from SitesHandler
        public OperationResult GetLakeSites(Int32 lakeId)
        {
            List<SITE> siteList;

            //return BadRequest if ther is no ID
            if (lakeId <= 0)
            { return new OperationResult.BadRequest(); }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                //siteList = aLaMPRDS.LAKE_TYPE.SingleOrDefault(lt => lt.LAKE_TYPE_ID == lakeId).LOCATIONs.ToList();
                siteList = aLaMPRDS.LAKE_TYPE.SingleOrDefault(lt => lt.LAKE_TYPE_ID == lakeId).SITE.ToList();

            }//end using
            if (siteList != null)
                siteList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            return new OperationResult.OK { ResponseResource = siteList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetStatusSites")] //from siteHandler
        public OperationResult GetStatusSites(Int32 statusId)
        {
            List<SITE> siteList;

            //return BadRequest if ther is no ID
            if (statusId <= 0)
            { return new OperationResult.BadRequest(); }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                //siteList = aLaMPRDS.STATUS_TYPE.SingleOrDefault(s => s.STATUS_ID == statusId).LOCATIONs.ToList();
                siteList = aLaMPRDS.STATUS_TYPE.SingleOrDefault(s => s.STATUS_ID == statusId).SITE.ToList();

            }//end using

            if (siteList != null)
                siteList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            return new OperationResult.OK { ResponseResource = siteList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetResourceSites")]
        public OperationResult GetResourceSites(Int32 resourceId)
        {
            List<SITE> siteList = null;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                siteList = aLaMPRDS.SITE_RESOURCE.Where(cr => cr.RESOURCE_TYPE_ID == resourceId)
                                                    .Select(cm => cm.SITE).OrderBy(c => c.SITE_ID).ToList();

                if (siteList != null)
                    siteList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using

            return new OperationResult.OK { ResponseResource = siteList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetMediaSites")]
        public OperationResult GetMediaSites(Int32 mediaId)
        {
            List<SITE> siteList = null;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                siteList = aLaMPRDS.SITE_MEDIA.Where(cm => cm.MEDIA_TYPE_ID == mediaId)
                                                    .Select(cm => cm.SITE).OrderBy(c => c.SITE_ID).ToList();

                if (siteList != null)
                    siteList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
            }//end using

            return new OperationResult.OK { ResponseResource = siteList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetParameterSites")]
        public OperationResult GetParameterSites(Int32 parameterId)
        {
            List<SITE> siteList = null;

                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {

                    siteList = aLaMPRDS.SITE_PARAMETERS.Where(p => p.PARAMETER_TYPE_ID == parameterId)
                                                                            .Select(cp => cp.SITE).OrderBy(c => c.SITE_ID).ToList();

                    if (siteList != null)
                        siteList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                }//end using

                return new OperationResult.OK { ResponseResource = siteList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteInformation")]
        public OperationResult SiteInformation(Int32 siteId)
        {
            SiteMapModel aSiteModel = null;

            //return BadRequest if ther is no ID
            if (siteId <= 0)
            { return new OperationResult.BadRequest(); }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {                
                //SITE aSite = aLaMPRDS.SITE.SingleOrDefault(c => c.SITE_ID == siteId && c.PROJECT.READY_FLAG == 1);
                SITE aSite = aLaMPRDS.SITE.SingleOrDefault(c => c.SITE_ID == siteId);
                if (aSite != null)
                {
                    aSiteModel = new SiteMapModel()
                    {
                        SiteId = aSite.SITE_ID,
                        Name = aSite.NAME,
                        latitude = aSite.LATITUDE,
                        longitude = aSite.LONGITUDE,
                        StartDate = ((DateTime?)aSite.START_DATE).HasValue ? ((DateTime?)aSite.START_DATE).Value.ToString("d") : null,
                        EndDate = ((DateTime?)aSite.END_DATE).HasValue ? ((DateTime?)aSite.END_DATE).Value.ToString("d") : null,
                        SamplePlatform = aSite.SAMPLE_PLATFORM,
                        AdditionalInfo = aSite.ADDITIONAL_INFO,
                        Description = aSite.DESCRIPTION,
                        Waterbody = aSite.WATERBODY,                        
                        GreatLake = aLaMPRDS.LAKE_TYPE.SingleOrDefault(l => l.LAKE_TYPE_ID == aSite.LAKE_TYPE_ID).LAKE,
                        Status = aSite.STATUS_TYPE_ID != null ? aLaMPRDS.STATUS_TYPE.SingleOrDefault(s => s.STATUS_ID == aSite.STATUS_TYPE_ID).STATUS : aLaMPRDS.STATUS_TYPE.SingleOrDefault(s => s.STATUS_ID == 2).STATUS,
                        Resources = GetSiteResources(aSite.SITE_ID, aLaMPRDS.SITE_RESOURCE),
                        Country = aSite.COUNTRY,
                        State = aSite.STATE_PROVINCE,
                        WatershedHUC8 = aSite.WATERSHED_HUC8,
                        URL = aSite.URL,
                        
                        Frequency = GetSiteFrequency(aSite.SITE_ID, aLaMPRDS.SITE_FREQUENCY),
                        Media = GetSiteMedia(aSite.SITE_ID, aLaMPRDS.SITE_MEDIA),
                        Parameters = GetParameters(aSite.SITE_ID, aLaMPRDS.SITE_PARAMETERS),
                        aProject = aLaMPRDS.PROJECTS.Where(a => a.PROJECT_ID == aSite.PROJECT_ID).SingleOrDefault(),
                        ProjOrgs = aLaMPRDS.ORGANIZATION_SYSTEM.Where(o => o.PROJECT.Any(p => p.PROJECT_ID == aSite.PROJECT_ID)).Select(or => new OrganizationResource {
                             OrganizationSystemID = or.ORGANIZATION_SYSTEM_ID,
                             OrganizationID = or.ORG_ID,
                             OrganizationName = or.ORGANIZATION.ORGANIZATION_NAME,
                             DivisionID = or.DIV_ID,
                             DivisionName = or.DIVISION.DIVISION_NAME,
                             SectionID = or.SEC_ID,
                             SectionName = or.SECTION.SECTION_NAME,
                             ScienceBaseID = or.SCIENCE_BASE_ID }).ToList(),
                        ProjDuration = aSite.PROJECT.PROJ_DURATION_ID != null ? GetProjectDuration(aSite.PROJECT.PROJ_DURATION_ID, aLaMPRDS) : "",
                        ProjectObjectives = GetProjObjectives(aSite.PROJECT_ID, aLaMPRDS.PROJECT_OBJECTIVES),
                        
                        ProjectKeywords = GetProjectKeywords(aSite.PROJECT_ID, aLaMPRDS.PROJECT_KEYWORDS),
                        ProjectData = aLaMPRDS.DATA_HOST.Where(dh => dh.PROJECT_ID == aSite.PROJECT_ID).ToList(),
                        ProjectPubs = aLaMPRDS.PUBLICATIONS.Where(p => p.PROJECT.Any(pp => pp.PROJECT_ID == aSite.PROJECT_ID)).ToList(),
                        ProjectContacts = GetProjectContacts(Convert.ToInt32(aSite.PROJECT_ID), aLaMPRDS)
                    };
                }
            }//end using

            return new OperationResult.OK { ResponseResource = aSiteModel };
        }

        //Used in "Search by Project Name: " -- unknown if there are sites or not, and used to populate Sites Side bar in Popup window on mapper on site point click
        [HttpOperation(HttpMethod.GET, ForUriName = "GetProjectSitesInfo")]
        public OperationResult ProjectSitesInfo(Int32 projectId)
        {
            List<SITE> SiteList = new List<SITE>();
            List<SiteMapModel> SiteModelList = null;
            SiteMapModel ProjectInfo = null;
            //return BadRequest if ther is no ID
            if (projectId <= 0)
            { return new OperationResult.BadRequest(); }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {//clicked on point in map get all sites for this project for popup,
                SiteList = aLaMPRDS.PROJECTS.Where(c => c.PROJECT_ID == projectId).FirstOrDefault().SITE.ToList();
                if (SiteList.Count > 0)
                {
                    SiteModelList = SiteList.Select(x => new SiteMapModel
                    {
                        SiteId = x.SITE_ID,
                        Name = x.NAME            
                    }).ToList<SiteMapModel>();
                }
                else {
                // "Search by Project Name: " --No sites
                    ProjectInfo = new SiteMapModel();
                    ProjectInfo.aProject = aLaMPRDS.PROJECTS.AsEnumerable().SingleOrDefault(p => p.PROJECT_ID == projectId);
                    ProjectInfo.ProjDuration = ProjectInfo.aProject.PROJ_DURATION_ID != null ? GetProjectDuration(ProjectInfo.aProject.PROJ_DURATION_ID, aLaMPRDS) : "";
                    ProjectInfo.ProjStatus = ProjectInfo.aProject.PROJ_STATUS_ID != null ? GetProjectStatus(ProjectInfo.aProject.PROJ_STATUS_ID, aLaMPRDS) : "";
                    ProjectInfo.ProjOrgs = aLaMPRDS.ORGANIZATION_SYSTEM.Where(o => o.PROJECT.Any(p => p.PROJECT_ID == projectId)).Select(org => new OrganizationResource {
                        OrganizationSystemID = org.ORGANIZATION_SYSTEM_ID,
                        OrganizationID = org.ORG_ID,
                        OrganizationName = org.ORGANIZATION.ORGANIZATION_NAME,
                        DivisionID = org.DIV_ID,
                        DivisionName = org.DIVISION.DIVISION_NAME,
                        SectionID = org.SEC_ID,
                        SectionName = org.SECTION.SECTION_NAME,
                        ScienceBaseID = org.SCIENCE_BASE_ID
                    }).ToList();
                    ProjectInfo.ProjectObjectives = GetProjObjectives(projectId, aLaMPRDS.PROJECT_OBJECTIVES);
                    ProjectInfo.ProjectKeywords = GetProjectKeywords(projectId, aLaMPRDS.PROJECT_KEYWORDS);
                    ProjectInfo.ProjectData = aLaMPRDS.DATA_HOST.Where(dh => dh.PROJECT_ID == projectId).ToList();
                    ProjectInfo.ProjectPubs = aLaMPRDS.PUBLICATIONS.Where(p => p.PROJECT.Any(pp => pp.PROJECT_ID == projectId)).ToList();
                    ProjectInfo.ProjectContacts = GetProjectContacts(projectId, aLaMPRDS);
                }
            }//end using
            if (SiteList.Count > 0)
            {
                return new OperationResult.OK { ResponseResource = SiteModelList };
            }
            else 
            {
                return new OperationResult.OK { ResponseResource = ProjectInfo };
            }
        }


        [HttpOperation(HttpMethod.GET, ForUriName = "GetFullSitesInfo")]
        public OperationResult FullSiteInfo(Int32 projectId)
        {
           // List<SITE> SiteList = new List<SITE>();
            IQueryable<SITE_LIST_VIEW> SiteList;
            List<SiteMapModel> SiteModelList = null;
            
            //return BadRequest if ther is no ID
            if (projectId <= 0)
            { return new OperationResult.BadRequest(); }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            { // .include or .join 
                //SiteList = aLaMPRDS.PROJECTS.Where(c => c.PROJECT_ID == projectId).FirstOrDefault().SITE.ToList();
                SiteList = aLaMPRDS.SITE_LIST_VIEW.Where(c => c.PROJECT_ID == projectId);
                //if (SiteList.Count > 0)
                //{
                    SiteModelList = SiteList.AsEnumerable<SITE_LIST_VIEW>().Select(x => new SiteMapModel
                    {
                        SiteId = x.SITE_ID,
                        Name = x.NAME,
                        latitude = x.LATITUDE,
                        longitude = x.LONGITUDE,
                        StartDate = ((DateTime?)x.START_DATE).HasValue ? ((DateTime?)x.START_DATE).Value.ToString("d") : "",
                        EndDate = ((DateTime?)x.END_DATE).HasValue ? ((DateTime?)x.END_DATE).Value.ToString("d") : "",
                        SamplePlatform = x.SAMPLE_PLATFORM,
                        AdditionalInfo = x.ADDITIONAL_INFO,
                        Description = x.DESCRIPTION,
                        Waterbody = x.WATERBODY,
//                        GreatLake = aLaMPRDS.LAKE_TYPE.SingleOrDefault(l => l.LAKE_TYPE_ID == x.LAKE_TYPE_ID).LAKE,
                        GreatLake = x.LAKE,
//                        Status = x.STATUS_TYPE_ID > 0 ? aLaMPRDS.STATUS_TYPE.SingleOrDefault(s => s.STATUS_ID == x.STATUS_TYPE_ID).STATUS : aLaMPRDS.STATUS_TYPE.SingleOrDefault(s => s.STATUS_ID == 2).STATUS,
                        Status = x.STATUS != null ? x.STATUS : "",
                        Resources = GetSiteResources(x.SITE_ID, aLaMPRDS.SITE_RESOURCE),
                        Country = x.COUNTRY,
                        State = x.STATE_PROVINCE,
                        WatershedHUC8 = x.WATERSHED_HUC8,
                        URL = x.URL,
                        Frequency = GetSiteFrequency(x.SITE_ID, aLaMPRDS.SITE_FREQUENCY),
                        Media = GetSiteMedia(x.SITE_ID, aLaMPRDS.SITE_MEDIA),                        
                        ParameterStrings = GetParameterStrings(x.SITE_ID, aLaMPRDS.SITE_PARAMETERS)
                    }).ToList<SiteMapModel>();
                //}
                
            }//end using
            
            return new OperationResult.OK { ResponseResource = SiteModelList };
           
            
        }
       
        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(SITE aSite)
        {
            try
            {   
                //Return BadRequest if missing required fields             
                if (!aSite.PROJECT_ID.HasValue || string.IsNullOrEmpty(aSite.NAME) || !aSite.LATITUDE.HasValue || !aSite.LONGITUDE.HasValue)
                { return new OperationResult.BadRequest(); }
                
                LaMPGPServiceAgent gpAgent = null;               

                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                    {
                        if (!IsAuthorizedToEdit(aLaMPRDS.PROJECTS.First(p => p.PROJECT_ID == aSite.PROJECT_ID).DATA_MANAGER.USERNAME))
                            return new OperationResult.Forbidden { Description = "Not Authorized" };

                        // should be done before test exists
                        //aSite.COUNTRY = GetCountry(GetCountryByName(aSite.COUNTRY));
                        //aSite.STATE_PROVINCE = (GetCountryByName(aSite.COUNTRY) == Country.USA) ?
                        //GetState(GetStateByName(aSite.STATE_PROVINCE)) : GetProvince(GetProvinceByName(aSite.STATE_PROVINCE));

                        //Check for existing project
                        if (!Exists(aLaMPRDS.SITE, ref aSite))
                        {
                            aLaMPRDS.SITE.AddObject(aSite);
                            aLaMPRDS.SaveChanges();

                            if (aSite.PROJECT.READY_FLAG != null)
                            {
                                if (aSite.PROJECT.READY_FLAG == 1)
                                {
                                    //only if it's ready to see on the map
                                    gpAgent = new LaMPGPServiceAgent();
                                    gpAgent.POSTSite(ConfigurationManager.AppSettings["AGSSiglUpdate"], new List<FullSite>() { new FullSite(aSite) });
                                }
                            }
                        }
                        if (aSite != null)
                            aSite.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                    }// end using
                } //end using

                //Return object to verify persisitance
                return new OperationResult.OK { ResponseResource = aSite };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }
        #endregion

        #region PutMethods
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult PUT(Int32 siteId, SITE instance)
        {
            //Return BadRequest if missing required fields
            if ((siteId <= 0))
            {
                return new OperationResult.BadRequest();
            }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    LaMPGPServiceAgent gpAgent = null;
                    //fetch the object to be updated (assuming that it exists)
                    SITE ObjectToBeUpdated = aLaMPRDS.SITE.SingleOrDefault(c => c.SITE_ID == siteId);

                    if (!IsAuthorizedToEdit(ObjectToBeUpdated.PROJECT.DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" };

                     //Start date
                    ObjectToBeUpdated.START_DATE = (DateTime.Equals(instance.START_DATE, ObjectToBeUpdated.START_DATE) ?
                         ObjectToBeUpdated.START_DATE : instance.START_DATE);

                    //End date
                    ObjectToBeUpdated.END_DATE = (DateTime.Equals(instance.END_DATE, ObjectToBeUpdated.END_DATE) ?
                         ObjectToBeUpdated.END_DATE : instance.END_DATE);
               
                    //PROJECT_ID
                    ObjectToBeUpdated.PROJECT_ID = (Decimal.Equals(instance.PROJECT_ID, ObjectToBeUpdated.PROJECT_ID) ?
                         ObjectToBeUpdated.PROJECT_ID : instance.PROJECT_ID);

                    //SAMPLE_PLATFORM
                    ObjectToBeUpdated.SAMPLE_PLATFORM = instance.SAMPLE_PLATFORM;

                    //Comments
                    ObjectToBeUpdated.ADDITIONAL_INFO = instance.ADDITIONAL_INFO;

                    //Name
                    ObjectToBeUpdated.NAME = (string.IsNullOrEmpty(instance.NAME) ?
                         ObjectToBeUpdated.NAME : instance.NAME);

                    //Description
                    ObjectToBeUpdated.DESCRIPTION = (string.IsNullOrEmpty(instance.DESCRIPTION) ?
                         ObjectToBeUpdated.DESCRIPTION : instance.DESCRIPTION);

                    //Latitude
                    ObjectToBeUpdated.LATITUDE = (Decimal.Equals(instance.LATITUDE, ObjectToBeUpdated.LATITUDE) ?
                         ObjectToBeUpdated.LATITUDE : instance.LATITUDE);

                    //Longitude
                    ObjectToBeUpdated.LONGITUDE = (Decimal.Equals(instance.LONGITUDE, ObjectToBeUpdated.LONGITUDE) ?
                         ObjectToBeUpdated.LONGITUDE : instance.LONGITUDE);

                    //Waterbody
                    ObjectToBeUpdated.WATERBODY = (string.IsNullOrEmpty(instance.WATERBODY) ?
                        ObjectToBeUpdated.WATERBODY : instance.WATERBODY);

                    //Status Type
                    ObjectToBeUpdated.STATUS_TYPE_ID = (Decimal.Equals(instance.STATUS_TYPE_ID, ObjectToBeUpdated.STATUS_TYPE_ID) ?
                         ObjectToBeUpdated.STATUS_TYPE_ID : instance.STATUS_TYPE_ID);

                    //Lake Type
                    ObjectToBeUpdated.LAKE_TYPE_ID = (Decimal.Equals(instance.LAKE_TYPE_ID, ObjectToBeUpdated.LAKE_TYPE_ID) ?
                         ObjectToBeUpdated.LAKE_TYPE_ID : instance.LAKE_TYPE_ID);

                    //Country
                    ObjectToBeUpdated.COUNTRY = (string.IsNullOrEmpty(instance.COUNTRY) ?
                        ObjectToBeUpdated.COUNTRY : instance.COUNTRY);

                    //State_Province
                    ObjectToBeUpdated.STATE_PROVINCE = (string.IsNullOrEmpty(instance.STATE_PROVINCE) ?
                        ObjectToBeUpdated.STATE_PROVINCE : instance.STATE_PROVINCE);

                    //Watershed_Huc8
                    ObjectToBeUpdated.WATERSHED_HUC8 = instance.WATERSHED_HUC8;

                    //Url
                    ObjectToBeUpdated.URL = instance.URL;
                    
                    aLaMPRDS.SaveChanges();
                    if (ObjectToBeUpdated.PROJECT.READY_FLAG != null)
                    {
                        if (ObjectToBeUpdated.PROJECT.READY_FLAG == 1)
                        {
                          //only if it's ready to see on the map                   
                            gpAgent = new LaMPGPServiceAgent();
                         // remove then readd
                            gpAgent.DELETESite(ConfigurationManager.AppSettings["AGSSiglUpdate"], new List<FullSite>() { new FullSite(ObjectToBeUpdated) });
                            gpAgent.POSTSite(ConfigurationManager.AppSettings["AGSSiglUpdate"], new List<FullSite>() { new FullSite(ObjectToBeUpdated) });
                        }
                    }
                }// end using
            }// end using

          if (instance != null)
              instance.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = instance };
        }

        #endregion

        #region DeleteMethods
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 siteId)
        {
            LaMPGPServiceAgent gpAgent = null;
            //Return BadRequest if missing required fields
            if (siteId <= 0)
            {return new OperationResult.BadRequest();}

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                    {
                        //fetch the object to be updated (assuming that it exists)
                        SITE ObjectToBeDeleted = aLaMPRDS.SITE.SingleOrDefault(c => c.SITE_ID == siteId);

                        if (ObjectToBeDeleted != null)
                        {
                            if (!IsAuthorizedToEdit(ObjectToBeDeleted.PROJECT.DATA_MANAGER.USERNAME))
                                return new OperationResult.Forbidden { Description = "Not Authorized" };


                            //delete it
                            aLaMPRDS.SITE.DeleteObject(ObjectToBeDeleted);

                            aLaMPRDS.SaveChanges();
                            //Delete to gpservices
                            gpAgent = new LaMPGPServiceAgent();
                            gpAgent.DELETESite(ConfigurationManager.AppSettings["AGSSiglUpdate"], new List<FullSite>() { new FullSite(ObjectToBeDeleted) });
                        }

                    }// end using

                }// end using

                //Return object to verify persisitance
                return new OperationResult.OK { };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }

        #endregion

        #endregion
        #region "Helper Methods"
        private Boolean Exists(System.Data.Objects.IObjectSet<SITE> sites, ref SITE aSite)
        {
            SITE existingSite;
            SITE thisSite = aSite;
            //check if it exists
            try
            {
                existingSite = sites.FirstOrDefault(c => c.PROJECT_ID == thisSite.PROJECT_ID &&
                                                                                     (string.IsNullOrEmpty(thisSite.NAME) || String.Equals(c.NAME.ToUpper(), thisSite.NAME.ToUpper())) &&
                                                                                     (c.LATITUDE.Value == thisSite.LATITUDE.Value || !thisSite.LATITUDE.HasValue) &&
                                                                                     (c.LONGITUDE.Value == thisSite.LONGITUDE.Value || !thisSite.LONGITUDE.HasValue) &&
                                                                                     (string.IsNullOrEmpty(thisSite.STATE_PROVINCE) || String.Equals(c.STATE_PROVINCE.ToUpper(), thisSite.STATE_PROVINCE.ToUpper())) &&
                                                                                     (c.LAKE_TYPE_ID.Value == thisSite.LAKE_TYPE_ID.Value || !thisSite.LAKE_TYPE_ID.HasValue) &&
                                                                                     (c.START_DATE.Value == thisSite.START_DATE.Value || !thisSite.START_DATE.HasValue) &&
                                                                                     (c.END_DATE.Value == thisSite.END_DATE.Value || !thisSite.END_DATE.HasValue) &&
                                                                                     (string.IsNullOrEmpty(thisSite.SAMPLE_PLATFORM) || String.Equals(c.SAMPLE_PLATFORM.ToUpper(), thisSite.SAMPLE_PLATFORM.ToUpper())) &&
                                                                                     (string.IsNullOrEmpty(thisSite.ADDITIONAL_INFO) || String.Equals(c.ADDITIONAL_INFO.ToUpper(), thisSite.ADDITIONAL_INFO.ToUpper())) &&
                                                                                     (string.IsNullOrEmpty(thisSite.WATERBODY) || String.Equals(c.WATERBODY.ToUpper(), thisSite.WATERBODY.ToUpper())));


                if (existingSite == null)
                    return false;

                //if exists then update ref contact
                aSite = existingSite;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GetProjectStatus(decimal? statId, LaMPDSEntities aLaMPRDS)
        {
            string stat = aLaMPRDS.PROJ_STATUS.Where(x => x.PROJ_STATUS_ID == statId).FirstOrDefault().STATUS_VALUE;
            return stat;
        }
        
        private string GetProjectDuration(decimal? durId, LaMPDSEntities aLaMPRDS)
        {
            string dur = aLaMPRDS.PROJ_DURATION.Where(x => x.PROJ_DURATION_ID == durId).FirstOrDefault().DURATION_VALUE;
            return dur;
        }
        
        private List<ContactModel> GetProjectContacts(int projectId, LaMPDSEntities aLaMPRDS)
        {
            List<CONTACT> projContacts = aLaMPRDS.CONTACTS.Where(c => c.PROJECT.Any(pc => pc.PROJECT_ID == projectId)).ToList();
            List<ContactModel> allContacts = new List<ContactModel>();
            foreach (CONTACT c in projContacts)
            {
                ContactModel cm = new ContactModel();
                cm.ContactName = c.NAME;
                cm.ContactOrgName = OrganizationSystemHandler.GetOrganizationFullName(c.ORGANIZATION_SYSTEM);
                cm.ContactEmail = c.EMAIL;
                cm.ContactPhone = c.PHONE;
                allContacts.Add(cm);
            }
            return allContacts;
        }

        private string GetProjectKeywords(decimal? p, System.Data.Objects.IObjectSet<PROJECT_KEYWORDS> objectSet)
        {
            //Project Keywords
            List<string> something = new List<string>();
            objectSet.Where(sr => sr.PROJECT_ID == p).ToList().ForEach(pk => something.Add(pk.KEYWORD.TERM));
            return string.Join(", ", something);
        }

        private string GetProjObjectives(decimal? p, System.Data.Objects.IObjectSet<PROJECT_OBJECTIVES> objectSet)
        {
            List<string> something = new List<string>();
            objectSet.Where(sr => sr.PROJECT_ID == p).ToList().ForEach(po => something.Add(po.OBJECTIVE_TYPE.OBJECTIVE));
            return string.Join(", ", something);
        }

        private ParameterGroups GetParameters(decimal p, System.Data.Objects.IObjectSet<SITE_PARAMETERS> objectSet)
        {
            //site Parameters
            List<PARAMETER_TYPE> siteParams = objectSet.Where(c => c.SITE_ID == p)
                                                                        .Select(cp => cp.PARAMETER_TYPE).ToList();
            ParameterGroups parameterGroups = new ParameterGroups();

            //Physical
            parameterGroups.Physical = siteParams.Where(a => a.PARAMETER_GROUP.Equals("Physical", StringComparison.OrdinalIgnoreCase)).ToList();

            //Chemical
            parameterGroups.Chemical = siteParams.Where(a => a.PARAMETER_GROUP.Equals("Chemical", StringComparison.OrdinalIgnoreCase)).ToList();

            //Biological
            parameterGroups.Biological = siteParams.Where(a => a.PARAMETER_GROUP.Equals("Biological", StringComparison.OrdinalIgnoreCase)).ToList();

            //Microbiological
            parameterGroups.Microbiological = siteParams.Where(a => a.PARAMETER_GROUP.Equals("Microbiological", StringComparison.OrdinalIgnoreCase)).ToList();

            //Toxicological
            parameterGroups.Toxicological = siteParams.Where(a => a.PARAMETER_GROUP.Equals("Toxicological", StringComparison.OrdinalIgnoreCase)).ToList();

            return parameterGroups;

        }

        private ParameterGroupsStrings GetParameterStrings(decimal p, System.Data.Objects.IObjectSet<SITE_PARAMETERS> objectSet)
        {
            //site Parameters
            List<PARAMETER_TYPE> siteParams = objectSet.Where(c => c.SITE_ID == p)
                                                                        .Select(cp => cp.PARAMETER_TYPE).ToList();
            ParameterGroupsStrings parameterGroups = new ParameterGroupsStrings();
            List<string> phyStrings = new List<string>();
            List<string> chemStrings = new List<string>();
            List<string> bioStrings = new List<string>();
            List<string> microBiStrings = new List<string>();
            List<string> toxStrings = new List<string>();

            //Physical
            //parameterGroups.Physical = 
            siteParams.Where(a => a.PARAMETER_GROUP.Equals("Physical", StringComparison.OrdinalIgnoreCase)).ToList().ForEach(ps => phyStrings.Add(ps.PARAMETER));
            parameterGroups.Physical = string.Join("; ", phyStrings);

            //Chemical
            //parameterGroups.Chemical = 
            siteParams.Where(a => a.PARAMETER_GROUP.Equals("Chemical", StringComparison.OrdinalIgnoreCase)).ToList().ForEach(ps => chemStrings.Add(ps.PARAMETER));
            parameterGroups.Chemical = string.Join("; ", chemStrings);

            //Biological
            //parameterGroups.Biological = 
            siteParams.Where(a => a.PARAMETER_GROUP.Equals("Biological", StringComparison.OrdinalIgnoreCase)).ToList().ForEach(ps => bioStrings.Add(ps.PARAMETER));
            parameterGroups.Biological = string.Join("; ", bioStrings);

            //Microbiological
            //parameterGroups.Microbiological = 
            siteParams.Where(a => a.PARAMETER_GROUP.Equals("Microbiological", StringComparison.OrdinalIgnoreCase)).ToList().ForEach(ps => microBiStrings.Add(ps.PARAMETER));
            parameterGroups.Microbiological = string.Join("; ", microBiStrings);

            //Toxicological
            //parameterGroups.Toxicological = 
            siteParams.Where(a => a.PARAMETER_GROUP.Equals("Toxicological", StringComparison.OrdinalIgnoreCase)).ToList().ForEach(ps => toxStrings.Add(ps.PARAMETER));
            parameterGroups.Toxicological = string.Join("; ", toxStrings);

            return parameterGroups;

        }
        private string GetSiteMedia(decimal p, System.Data.Objects.IObjectSet<SITE_MEDIA> objectSet)
        {
            List<string> something = new List<string>();

            objectSet.Where(sr => sr.SITE_ID == p).ToList().ForEach(rs => something.Add(rs.MEDIA_TYPE.MEDIA));

            return string.Join(", ", something);
        }

        private string GetSiteFrequency(decimal p, System.Data.Objects.ObjectSet<SITE_FREQUENCY> objectSet)
        {
            List<string> something = new List<string>();

            objectSet.Where(sr => sr.SITE_ID == p).ToList().ForEach(rs => something.Add(rs.FREQUENCY_TYPE.FREQUENCY));

            return string.Join(", ", something);
        }
        
        private string GetSiteResources(decimal p, System.Data.Objects.IObjectSet<SITE_RESOURCE> objectSet)
        {
            List<string> something = new List<string>();

            objectSet.Where(sr => sr.SITE_ID == p).ToList().ForEach(rs => something.Add(rs.RESOURCE_TYPE.RESOURCE_NAME));

            return string.Join(", ", something);
        }
        #endregion
    }//end SiteHandler
}//end namespace