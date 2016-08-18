//------------------------------------------------------------------------------
//----- ProjectHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Project resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//               https://github.com/openrasta/openrasta/wiki/Handlers
//
//     
#region Comments
// 04.02.13 - jkn - Created public user for get requests
// 04.25.12 - jkn - Created
#endregion                          


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.InteropServices;
using System.Configuration;

using LaMPServices.Authentication;
using LaMPServices.Resources;
using LaMPServices.Utilities;


using OpenRasta.Web;
using OpenRasta.Security;

namespace LaMPServices.Handlers
{
    public class ProjectHandler:HandlerBase
    {
      
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET, ForUriName = "GetAllProjects")]
        public OperationResult Get()
        {
            List<PROJECT> projectList;

            //Get basic authentication password
           
            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                projectList = aLaMPRDS.PROJECTS.OrderBy(p => p.NAME).ToList();

                if (projectList != null)
                    projectList.ForEach(p=>p.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
 
            }//end using
        

            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get
       
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 projectId)
        {
            PROJECT aProject;

            //return BadRequest if ther is no ID
            if (projectId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                aProject = aLaMPRDS.PROJECTS.AsEnumerable().SingleOrDefault(p => p.PROJECT_ID == projectId);
                
                if (aProject != null)
                    aProject.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            }//end using


            return new OperationResult.OK { ResponseResource = aProject };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName="GetSiteProject")]
        public OperationResult GetSiteProject(Int32 siteId)
        {
            PROJECT aProject;

            //return BadRequest if ther is no ID
            if (siteId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                aProject = aLaMPRDS.SITE.SingleOrDefault(c => c.SITE_ID == siteId).PROJECT;

                if (aProject != null)
                    aProject.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            }//end using


            return new OperationResult.OK { ResponseResource = aProject };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataHostProject")]
        public OperationResult GetDataHostProject(Int32 dataHostId)
        {
            PROJECT aProject;

            //return BadRequest if ther is no ID
            if (dataHostId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                aProject = aLaMPRDS.DATA_HOST.SingleOrDefault(dh => dh.DATA_HOST_ID == dataHostId).PROJECTs;

                if (aProject != null)
                    aProject.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            }//end using


            return new OperationResult.OK { ResponseResource = aProject };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET,ForUriName ="GetContactProjects")]
        public OperationResult GetContactProjects(Int32 contactId)
        {
            List<PROJECT> projectList;
            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                projectList = aLaMPRDS.PROJECTS.Where(p => p.CONTACTS.Any(c=>c.CONTACT_ID == contactId)).OrderBy(p => p.PROJECT_ID).ToList();

                if (projectList != null)
                    projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using

            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get
        
        [HttpOperation(HttpMethod.GET, ForUriName = "GetKeyWordProjects")]
        public OperationResult GetKeyWordProjects(Int32 keywordId)
        {
            List<PROJECT> projectList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                projectList = aLaMPRDS.PROJECTS.Where(p => p.KEYWORDS.Any(k => k.KEYWORD_ID == keywordId)).OrderBy(p => p.PROJECT_ID).ToList();

                if (projectList != null)
                    projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
            }//end using


            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get

        //only used once for request of Projects that have sites with certain frequency
        [HttpOperation(HttpMethod.GET, ForUriName = "GetFreqSiteProjects")]
        public OperationResult GetFreqSiteProjects(Int32 frequencyId)
        {
            List<PROJECT> projectList = new List<PROJECT>();

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                
                List<SITE> freqSites = aLaMPRDS.SITE.Where(x => x.SITE_FREQUENCY.Any(c => c.FREQUENCY_TYPE_ID == frequencyId)).ToList();
                foreach (SITE s in freqSites)
                {
                    projectList.Add(s.PROJECT);
                }
                projectList = projectList.Distinct().ToList();
                //if (projectList != null)
                   // projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
            }//end using


            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get

        //only used once for request of Projects that have sites with certain media
        [HttpOperation(HttpMethod.GET, ForUriName = "GetMediaSiteProjects")]
        public OperationResult GetMediaSiteProjects(Int32 mediaId)
        {
            List<PROJECT> projectList = new List<PROJECT>();

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                List<SITE> mediaSites = aLaMPRDS.SITE.Where(x => x.SITE_MEDIA.Any(c => c.MEDIA_TYPE_ID == mediaId)).ToList();
                foreach (SITE s in mediaSites)
                {
                    projectList.Add(s.PROJECT);
                }
                projectList = projectList.Distinct().ToList();
                //if (projectList != null)
                // projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
            }//end using


            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get
        
        //only used once for request of Projects that have sites with certain resource
        [HttpOperation(HttpMethod.GET, ForUriName = "GetResourceSiteProjects")]
        public OperationResult GetResourceSiteProjects(Int32 resourceId)
        {
            List<PROJECT> projectList = new List<PROJECT>();

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                List<SITE> resourceSites = aLaMPRDS.SITE.Where(x => x.SITE_RESOURCE.Any(c => c.RESOURCE_TYPE_ID == resourceId)).ToList();
                foreach (SITE s in resourceSites)
                {
                    projectList.Add(s.PROJECT);
                }
                projectList = projectList.Distinct().ToList();
                //if (projectList != null)
                // projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
            }//end using


            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get
        
        //only used once for request of Projects that have sites with certain media
        [HttpOperation(HttpMethod.GET, ForUriName = "GetParameterSiteProjects")]
        public OperationResult GetParameterSiteProjects(Int32 parameterId)
        {
            List<PROJECT> projectList = new List<PROJECT>();

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                List<SITE> paramSites = aLaMPRDS.SITE.Where(x => x.SITE_PARAMETERS.Any(c => c.PARAMETER_TYPE_ID == parameterId)).ToList();
                foreach (SITE s in paramSites)
                {
                    projectList.Add(s.PROJECT);
                }
                projectList = projectList.Distinct().ToList();
                //if (projectList != null)
                // projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
            }//end using


            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get

        //only used once for request of Projects that have sites with certain lake
        [HttpOperation(HttpMethod.GET, ForUriName = "GetLakeSiteProjects")]
        public OperationResult GetLakeSiteProjects(Int32 lakeId)
        {
            List<PROJECT> projectList = new List<PROJECT>();

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                List<SITE> lakeSites = aLaMPRDS.SITE.Where(x => x.LAKE_TYPE_ID == lakeId).ToList();
                foreach (SITE s in lakeSites)
                {
                    projectList.Add(s.PROJECT);
                }
                projectList = projectList.Distinct().ToList();
                //if (projectList != null)
                // projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
            }//end using


            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get

        //only used once for request of Projects that have sites with certain site status
        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteStatusProjects")]
        public OperationResult GetSiteStatusProjects(Int32 statusId)
        {
            List<PROJECT> projectList = new List<PROJECT>();

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                List<SITE> statSites = aLaMPRDS.SITE.Where(x => x.STATUS_TYPE_ID == statusId).ToList();
                foreach (SITE s in statSites)
                {
                    projectList.Add(s.PROJECT);
                }
                projectList = projectList.Distinct().ToList();
                //if (projectList != null)
                // projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
            }//end using


            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetPublicationProjects")]
        public OperationResult GetPublicationProjects(Int32 publicationId)
        {
            List<PROJECT> projectList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                projectList = aLaMPRDS.PROJECTS.Where(p => p.PUBLICATIONS.Any(pp => pp.PUBLICATION_ID == publicationId))
                                                .OrderBy(p => p.PROJECT_ID).ToList();
                if (projectList != null)
                    projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using


            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetObjectiveProjects")]
        public OperationResult GetObjectiveProjects(Int32 objectiveId)
        {
            List<PROJECT> projectList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                projectList = aLaMPRDS.PROJECTS.Where(p => p.PROJECT_OBJECTIVES.Any(po => po.OBJECTIVE_ID == objectiveId))
                                                .OrderBy(p => p.PROJECT_ID).ToList();
                if (projectList != null)
                    projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using

            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetOrgSysProject")]
        public OperationResult GetOrgSysProject(Int32 orgSystemId)
        {
            List<PROJECT> projectList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                projectList = aLaMPRDS.PROJECTS.Where(p => p.COOPERATORS.Any(c => c.ORGANIZATION_SYSTEM_ID == orgSystemId)).ToList();

                if (projectList != null)
                    projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using

            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetDataManagersProjects")]
        public OperationResult GetDataManagersProjects(Int32 dataManagerId)
        {
            List<PROJECT> projectList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                projectList = aLaMPRDS.PROJECTS.Where(p => p.DATA_MANAGER_ID == dataManagerId).OrderBy(p => p.PROJECT_ID).ToList();

                if (projectList != null)
                    projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using


            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetProjectDurationProjects")]
        public OperationResult GetProjDurationProjects(Int32 durationId)
        {
            List<PROJECT> projectList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                projectList = aLaMPRDS.PROJECTS.Where(p => p.PROJ_DURATION_ID == durationId).ToList();

                if (projectList != null)
                    projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using


            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetProjectStatusProjects")]
        public OperationResult GetProjStatusProjects(Int32 statusId)
        {
            List<PROJECT> projectList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                projectList = aLaMPRDS.PROJECTS.Where(p => p.PROJ_STATUS_ID == statusId).ToList();

                if (projectList != null)
                    projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using


            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get
    
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetFlaggedProjects")]
        public OperationResult GetFlaggedProjects(int flag)
        {
            List<PROJECT> projectList;

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //projectList = aLaMPRDS.PROJECTS.Where(p => p.READY_FLAG == flag).ToList();
                    projectList = aLaMPRDS.PROJECTS.ToList();
                    
                    if (projectList != null)
                        projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get

   
        //returns ProjectRes
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetIndexProjects")]
        public OperationResult IndexProjects([Optional] decimal dataManagerID)
        {
            //List<PROJECT> projectList;
            List<ProjectRes> Projects;

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    List<PROJECT_SITECOUNT_VIEW> viewTable = aLaMPRDS.PROJECT_SITECOUNT_VIEW.ToList();
                    
                    if (IsAuthorized(AdminRole))
                    {
                        //for admin - if there's a dm ID, want all projects for this dm in this model format (SiGLDMS - Account page, user's project list)
                        if (dataManagerID > 0)
                        {
                            Projects = viewTable.Where(dp => dp.MANAGER == dataManagerID).Select(p => new ProjectRes()
                            {
                                Name = p.NAME,
                                ProjId = p.PROJID,
                                //StartDate = p.START_DATE,
                                SiteCount = p.SITECOUNT,
                                DataManagerID = dataManagerID,
                                LName = aLaMPRDS.DATA_MANAGER.FirstOrDefault(x => x.DATA_MANAGER_ID == p.MANAGER).LNAME,
                                FName = aLaMPRDS.DATA_MANAGER.FirstOrDefault(x => x.DATA_MANAGER_ID == p.MANAGER).FNAME,
                                ManagerOrg = GetManagerOrg(aLaMPRDS, aLaMPRDS.DATA_MANAGER.FirstOrDefault(x => x.DATA_MANAGER_ID == p.MANAGER))

                            }).ToList();
                        }
                        else
                        { //else just give me all the projects in this format (SiGLDMS - project list)
                            Projects = viewTable.Select(p => new ProjectRes()
                            {
                                Name = p.NAME,
                                ProjId = p.PROJID,
                                //StartDate = p.START_DATE,
                                SiteCount = p.SITECOUNT,
                                DataManagerID = p.MANAGER,
                                LName = aLaMPRDS.DATA_MANAGER.FirstOrDefault(x => x.DATA_MANAGER_ID == p.MANAGER).LNAME,
                                FName = aLaMPRDS.DATA_MANAGER.FirstOrDefault(x => x.DATA_MANAGER_ID == p.MANAGER).FNAME,
                                ManagerOrg = GetManagerOrg(aLaMPRDS, aLaMPRDS.DATA_MANAGER.FirstOrDefault(x => x.DATA_MANAGER_ID == p.MANAGER))
                            }).ToList();
                        }
                        
                    }
                    else
                    {//not authorized to see all projects, just give me my own in this format
                        DATA_MANAGER dm = aLaMPRDS.DATA_MANAGER.FirstOrDefault(y => y.USERNAME == Context.User.Identity.Name);

                        Projects = viewTable.Where(p => p.MANAGER == dm.DATA_MANAGER_ID).Select(p => new ProjectRes()
                        {
                            Name = p.NAME,
                            ProjId = p.PROJID,
                            //StartDate = p.START_DATE,
                            SiteCount = p.SITECOUNT,
                            DataManagerID = p.MANAGER,
                            LName = aLaMPRDS.DATA_MANAGER.FirstOrDefault(x => x.DATA_MANAGER_ID == p.MANAGER).LNAME,
                            FName = aLaMPRDS.DATA_MANAGER.FirstOrDefault(x => x.DATA_MANAGER_ID == p.MANAGER).FNAME,
                            ManagerOrg = GetManagerOrg(aLaMPRDS, aLaMPRDS.DATA_MANAGER.FirstOrDefault(x => x.DATA_MANAGER_ID == p.MANAGER))
                        }).OrderBy(a => a.Name).ToList();              
                        
                    }
                }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = Projects };
        }

        //returns FullProject
        [HttpOperation(HttpMethod.GET, ForUriName="GetFullProject")]
        public OperationResult GetFullProject(string scienceBaseId)
        {
            PROJECT aProject;
            FullProject aFullProject = null;
            //return BadRequest if ther is no ID
            if (string.IsNullOrEmpty(scienceBaseId))
            {
                return new OperationResult.BadRequest();
            }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                aProject = aLaMPRDS.PROJECTS.AsEnumerable().SingleOrDefault(p => p.SCIENCE_BASE_ID == scienceBaseId);
                aFullProject = new FullProject()
                {
                    ProjectId = aProject.PROJECT_ID,
                    ScienceBaseId = aProject.SCIENCE_BASE_ID,
                    Name = aProject.NAME,
                    StartDate = ((DateTime?)aProject.START_DATE).HasValue ? ((DateTime?)aProject.START_DATE).Value.ToString("d") : null,
                    EndDate = ((DateTime?)aProject.END_DATE).HasValue ? ((DateTime?)aProject.END_DATE).Value.ToString("d") : null,
                    DataManagerId = aProject.DATA_MANAGER_ID,
                    Description = aProject.DESCRIPTION,
                    Objectives = aProject.PROJECT_OBJECTIVES.ToList().Select(ot => ot.OBJECTIVE_TYPE).ToList(),
                    Keywords = aProject.KEYWORDS.ToList().Select(ot => ot.KEYWORD).ToList(),
                    ProjectWebsite = aProject.URL,
                    DataHost = aProject.DATA_HOST.Select(dh => new DataHostModel()
                    {
                        ID = dh.DATA_HOST_ID,
                        onlineDataLocation = dh.PORTAL_URL,
                        description = dh.DESCRIPTION,
                        name = dh.HOST_NAME
                    }).FirstOrDefault(),
                    Organizations = aProject.COOPERATORS.Where(c => c.ORGANIZATION_SYSTEM_ID > 0).Select(ot => new OrganizationResource
                    {
                        OrganizationSystemID = ot.ORGANIZATION_SYSTEM.ORGANIZATION_SYSTEM_ID,
                        OrganizationID = ot.ORGANIZATION_SYSTEM.ORG_ID,
                        OrganizationName = ot.ORGANIZATION_SYSTEM.ORGANIZATION.ORGANIZATION_NAME,
                        DivisionID = ot.ORGANIZATION_SYSTEM.DIV_ID,
                        DivisionName = ot.ORGANIZATION_SYSTEM.DIVISION.DIVISION_NAME,
                        SectionID = ot.ORGANIZATION_SYSTEM.SEC_ID,
                        SectionName = ot.ORGANIZATION_SYSTEM.SECTION.SECTION_NAME,
                        ScienceBaseID = ot.ORGANIZATION_SYSTEM.SCIENCE_BASE_ID
                    }).ToList(),
                    Contacts = GetProjectContacts(Convert.ToInt32(aProject.PROJECT_ID), aLaMPRDS),
                    Publications = aLaMPRDS.PUBLICATIONS.Where(p => p.PROJECT.Any(pp => pp.PROJECT_ID == aProject.PROJECT_ID)).ToList()
                };
                if (aProject != null)
                    aProject.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            }//end using


            return new OperationResult.OK { ResponseResource = aFullProject };
        }//end httpMethod get        

        
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetManagedProjects")]
        public OperationResult GetManagedProjects()
        {
            List<PROJECT> projectList;

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    if(IsAuthorized(AdminRole))
                    {
                        projectList = aLaMPRDS.PROJECTS.OrderBy(p => p.PROJECT_ID).ToList();
                    }
                    else
                    {
                        projectList = aLaMPRDS.PROJECTS.Where(p => p.DATA_MANAGER.USERNAME == username).OrderBy(p => p.PROJECT_ID).ToList();
                    }

                    if (projectList != null)
                        projectList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = projectList };
        }//end httpMethod get

       
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "ReassignProject")]
        public OperationResult ReassignProject(Int32 dataManagerId, Int32 projectId)
        {
            PROJECT project;

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //projectList = aLaMPRDS.PROJECTS.Where(p => p.READY_FLAG == flag).ToList();
                    project = aLaMPRDS.PROJECTS.Where(p => p.PROJECT_ID == projectId).FirstOrDefault();

                    if (project != null)
                    {
                        DATA_MANAGER newDM = aLaMPRDS.DATA_MANAGER.Where(d => d.DATA_MANAGER_ID == dataManagerId).FirstOrDefault();

                        if (newDM != null)
                        {
                            project.DATA_MANAGER_ID = newDM.DATA_MANAGER_ID;
                            aLaMPRDS.SaveChanges();
                        }
                    }

                }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = project };
        }//end httpMethod get

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(PROJECT aProject)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aProject.NAME))
            {return new OperationResult.BadRequest();}

            LaMPGPServiceAgent gpAgent = null;

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                   //Check for existing project
                  // ProjectExists thisProj = aProject as ProjectExists;

                   if (!Exists(aLaMPRDS.PROJECTS, ref aProject))
                    {
                        aProject.DATA_MANAGER_ID = (IsAuthorized(AdminRole) && aProject.DATA_MANAGER_ID > 0) ? 
                            aProject.DATA_MANAGER_ID : GetLoggedInDataManager(aLaMPRDS).DATA_MANAGER_ID;

                        //added created date
                        aProject.CREATED_STAMP = DateTime.Now.Date;

                        //add it
                        aLaMPRDS.PROJECTS.AddObject(aProject);
                        aLaMPRDS.SaveChanges();

                    }

                   //flagged ready
                   gpAgent = new LaMPGPServiceAgent();
                   List<FullSite> projSites = aProject.SITE.Select(x => new FullSite(x)).ToList();

                   if (projSites.Count > 0)
                   {
                       //there's sites, now see if we are adding them or removing them
                       if (aProject.READY_FLAG != null)
                       {
                           if (aProject.READY_FLAG > 0)
                           {
                               //remove then add
                               gpAgent.DELETESite(ConfigurationManager.AppSettings["AGSSiglUpdate"], projSites);
                               gpAgent.POSTSite(ConfigurationManager.AppSettings["AGSSiglUpdate"], projSites);
                           }
                           else
                           {
                               //ready flag is 0 - remove them 
                               gpAgent.DELETESite(ConfigurationManager.AppSettings["AGSSiglUpdate"], projSites);
                           }
                       }
                   }

                   if (aProject != null)
                       aProject.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aProject };
        }
      
        #endregion

        #region PutMethods
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 projectId, PROJECT instance)
        {
            //Return BadRequest if missing required fields
            if ((projectId <= 0))
            {return new OperationResult.BadRequest();}

            LaMPGPServiceAgent gpAgent = null;
            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    PROJECT ObjectToBeUpdated = aLaMPRDS.PROJECTS.SingleOrDefault(p => p.PROJECT_ID == projectId);

                    if (!IsAuthorizedToEdit(ObjectToBeUpdated.DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" };

                    //Name
                    ObjectToBeUpdated.NAME = instance.NAME;

                    //Start date
                    ObjectToBeUpdated.START_DATE = (DateTime.Equals(instance.START_DATE, ObjectToBeUpdated.START_DATE) ?
                         ObjectToBeUpdated.START_DATE : instance.START_DATE);

                    //End date
                    ObjectToBeUpdated.END_DATE = (DateTime.Equals(instance.END_DATE, ObjectToBeUpdated.END_DATE) ?
                         ObjectToBeUpdated.END_DATE : instance.END_DATE);

                    //URL
                    ObjectToBeUpdated.URL = instance.URL;

                    //Additional Info
                    ObjectToBeUpdated.ADDITIONAL_INFO = instance.ADDITIONAL_INFO;
                    
                    //Data Manager
                    ObjectToBeUpdated.DATA_MANAGER_ID = instance.DATA_MANAGER_ID;

                    //ScienceBase Manager
                    ObjectToBeUpdated.SCIENCE_BASE_ID = instance.SCIENCE_BASE_ID;

                    //Description
                    ObjectToBeUpdated.DESCRIPTION = instance.DESCRIPTION;

                    //Status
                    ObjectToBeUpdated.PROJ_STATUS_ID = instance.PROJ_STATUS_ID;

                    //Duration
                    ObjectToBeUpdated.PROJ_DURATION_ID = instance.PROJ_DURATION_ID;
                    
                    //Ready for mapper flag
                    ObjectToBeUpdated.READY_FLAG = instance.READY_FLAG;
                    
                    //Edited timestamp
                    ObjectToBeUpdated.LAST_EDITED_STAMP = DateTime.Now.Date;

                    aLaMPRDS.SaveChanges();

                    //flagged ready
                    gpAgent = new LaMPGPServiceAgent();
                    List<FullSite> projSites = ObjectToBeUpdated.SITE.Select(x => new FullSite(x)).ToList();

                    if (projSites.Count > 0)
                    {
                        //there's sites, now see if we are adding them or removing them
                        if (ObjectToBeUpdated.READY_FLAG != null)
                        {
                            if (ObjectToBeUpdated.READY_FLAG > 0)
                            {
                               // remove then add
                                gpAgent.DELETESite(ConfigurationManager.AppSettings["AGSSiglUpdate"], projSites);
                                gpAgent.POSTSite(ConfigurationManager.AppSettings["AGSSiglUpdate"], projSites);
                            }
                            else
                            {
                                //ready flag is 0 - remove them 
                                gpAgent.DELETESite(ConfigurationManager.AppSettings["AGSSiglUpdate"], projSites);
                            }
                        }
                    }

                    if (instance != null)
                        instance.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                
                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = instance };
        }        
        #endregion

        #region DeleteMethods
        [LaMPRequiresRole(new string[] { AdminRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName="DeleteAllDMProjects")]
        public OperationResult DeleteAllDMProjects(Int32 dataManagerId)
        {
            List<PROJECT> projectList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                projectList = aLaMPRDS.PROJECTS.Where(p => p.DATA_MANAGER_ID == dataManagerId).OrderBy(p => p.PROJECT_ID).ToList();

                foreach (PROJECT p in projectList)
                {
                    this.Delete(Convert.ToInt32(p.PROJECT_ID));
                    aLaMPRDS.SaveChanges();
                }
            }//end using


            return new OperationResult.OK {  };
        }//end httpMethod get
        
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 projectId)
        {
            //Return BadRequest if missing required fields
            if (projectId <= 0 || projectId == null)
            { return new OperationResult.BadRequest();}
            
            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    PROJECT ObjectToBeDeleted = aLaMPRDS.PROJECTS.SingleOrDefault(p => p.PROJECT_ID == projectId);

                    if (!IsAuthorizedToEdit(ObjectToBeDeleted.DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" };

                    //delete it
                    aLaMPRDS.PROJECTS.DeleteObject(ObjectToBeDeleted);
                    aLaMPRDS.SaveChanges();

                    //delete all project cooperators (organizations)
                    List<PROJECT_COOPERATORS> projCoops = aLaMPRDS.PROJECT_COOPERATORS.Where(pc => pc.PROJECT_ID == projectId).ToList();
                    if (projCoops.Count >= 1)
                    {
                        foreach (PROJECT_COOPERATORS projcoop in projCoops)
                        {
                            aLaMPRDS.PROJECT_COOPERATORS.DeleteObject(projcoop);
                            aLaMPRDS.SaveChanges();
                        }
                    }

                    //delete all project contacts
                    List<PROJECT_CONTACTS> projContacts = aLaMPRDS.PROJECT_CONTACTS.Where(pc => pc.PROJECT_ID == projectId).ToList();
                    if (projContacts.Count >= 1)
                    {
                        foreach (PROJECT_CONTACTS pc in projContacts)
                        {
                            aLaMPRDS.PROJECT_CONTACTS.DeleteObject(pc);
                            aLaMPRDS.SaveChanges();
                        }
                    }
                    
                    //delete all project keywords
                    List<PROJECT_KEYWORDS> projKeys = aLaMPRDS.PROJECT_KEYWORDS.Where(pc => pc.PROJECT_ID == projectId).ToList();
                    if (projKeys.Count >= 1)
                    {
                        foreach (PROJECT_KEYWORDS pkey in projKeys)
                        {
                            aLaMPRDS.PROJECT_KEYWORDS.DeleteObject(pkey);
                            aLaMPRDS.SaveChanges();
                        }
                    }

                    //delete all project objectives
                    List<PROJECT_OBJECTIVES> projObjs = aLaMPRDS.PROJECT_OBJECTIVES.Where(pc => pc.PROJECT_ID == projectId).ToList();
                    if (projObjs.Count >= 1)
                    {
                        foreach (PROJECT_OBJECTIVES pObjs in projObjs)
                        {
                            aLaMPRDS.PROJECT_OBJECTIVES.DeleteObject(pObjs);
                            aLaMPRDS.SaveChanges();
                        }
                    }                    

                    //delete all project publications
                    List<PROJECT_PUBLICATIONS> projPubs = aLaMPRDS.PROJECT_PUBLICATIONS.Where(pc => pc.PROJECT_ID == projectId).ToList();
                    if (projPubs.Count >= 1)
                    {
                        foreach (PROJECT_PUBLICATIONS pPubs in projPubs)
                        {
                            aLaMPRDS.PROJECT_PUBLICATIONS.DeleteObject(pPubs);
                            aLaMPRDS.SaveChanges();
                        }
                    }

                    //delete data_hosts
                    List<DATA_HOST> projDHs = aLaMPRDS.DATA_HOST.Where(pc => pc.PROJECT_ID == projectId).ToList();
                    if (projDHs.Count >= 1)
                    {
                        foreach (DATA_HOST DH in projDHs)
                        {
                            aLaMPRDS.DATA_HOST.DeleteObject(DH);
                            aLaMPRDS.SaveChanges();
                        }
                    }

                    //delete all sites for this project
                    List<SITE> projSites = aLaMPRDS.SITE.Where(s => s.PROJECT_ID == projectId).ToList();
                    LaMPGPServiceAgent gpAgent = null;                    
                    
                    if (projSites.Count >= 1)
                    {     
                        //first Delete to gpservices
                        gpAgent = new LaMPGPServiceAgent();
                        List<FullSite> fullSites = projSites.Select(x => new FullSite(x)).ToList();
                        gpAgent.DELETESite(ConfigurationManager.AppSettings["AGSSiglUpdate"], fullSites);
                   
                        foreach (SITE site in projSites)
                        {                       
                            //delete all site frequ8ency
                            List<SITE_FREQUENCY> siteFreqs = aLaMPRDS.SITE_FREQUENCY.Where(pc => pc.SITE_ID == site.SITE_ID).ToList();
                            if (siteFreqs.Count >= 1)
                            {
                                foreach (SITE_FREQUENCY siteF in siteFreqs)
                                {
                                    aLaMPRDS.SITE_FREQUENCY.DeleteObject(siteF);
                                    aLaMPRDS.SaveChanges();
                                }
                            }
                            //delete all site media
                            List<SITE_MEDIA> siteMeds = aLaMPRDS.SITE_MEDIA.Where(pc => pc.SITE_ID == site.SITE_ID).ToList();
                            if (siteMeds.Count >= 1)
                            {
                                foreach (SITE_MEDIA sm in siteMeds)
                                {
                                    aLaMPRDS.SITE_MEDIA.DeleteObject(sm);
                                    aLaMPRDS.SaveChanges();
                                }
                            }
                            //delete all site parameters
                            List<SITE_PARAMETERS> siteParams = aLaMPRDS.SITE_PARAMETERS.Where(pc => pc.SITE_ID == site.SITE_ID).ToList();
                            if (siteParams.Count >= 1)
                            {
                                foreach (SITE_PARAMETERS sp in siteParams)
                                {
                                    aLaMPRDS.SITE_PARAMETERS.DeleteObject(sp);
                                    aLaMPRDS.SaveChanges();
                                }
                            }
                            //delete all site resources
                            List<SITE_RESOURCE> siteResourses = aLaMPRDS.SITE_RESOURCE.Where(pc => pc.SITE_ID == site.SITE_ID).ToList();
                            if (siteResourses.Count >= 1)
                            {
                                foreach (SITE_RESOURCE sr in siteResourses)
                                {
                                    aLaMPRDS.SITE_RESOURCE.DeleteObject(sr);
                                    aLaMPRDS.SaveChanges();
                                }
                            }
                            
                            aLaMPRDS.SITE.DeleteObject(site);
                            aLaMPRDS.SaveChanges();
                        }
                    }

                    aLaMPRDS.SaveChanges();

                }// end using

            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        #endregion

        #endregion

        #region "Helper Methods"

        private Boolean Exists(System.Data.Objects.IObjectSet<PROJECT> projects, ref PROJECT aProject) //private Boolean Exists(System.Data.Objects.IObjectSet<PROJECT> projects, ref ProjectExists aProject)
        
        {
            PROJECT existingProject;
            PROJECT thisProject = aProject;
            //check if it exists
            try
            {
                existingProject = projects.FirstOrDefault(p => string.Equals(p.NAME.ToUpper().Trim(), thisProject.NAME.ToUpper().Trim()) &&
                                                            (p.START_DATE.Value == thisProject.START_DATE.Value || !thisProject.START_DATE.HasValue) &&
                                                            (p.END_DATE.Value == thisProject.END_DATE.Value || !thisProject.END_DATE.HasValue) &&
                                                            (string.Equals(p.DESCRIPTION.ToUpper().Trim(), thisProject.DESCRIPTION.ToUpper().Trim()) || string.IsNullOrEmpty(thisProject.DESCRIPTION)) &&
                                                            (string.Equals(p.SCIENCE_BASE_ID.ToUpper().Trim(), thisProject.SCIENCE_BASE_ID.ToUpper().Trim()) || string.IsNullOrEmpty(thisProject.SCIENCE_BASE_ID)) &&                                                            
                                                            (string.Equals(p.URL.ToUpper().Trim(), thisProject.URL.ToUpper().Trim()) || string.IsNullOrEmpty(thisProject.URL)));


                if (existingProject == null)
                    return false;

                //if exists then update ref contact
                aProject = new ExistingProject(existingProject);                
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        
        private List<ContactModel> GetProjectContacts(int projectId, LaMPDSEntities aLaMPRDS)
        {
            List<CONTACT> projContacts = aLaMPRDS.CONTACTS.Where(c => c.PROJECT.Any(pc => pc.PROJECT_ID == projectId)).ToList();
            List<ContactModel> allContacts = new List<ContactModel>();
            foreach (CONTACT c in projContacts)
            {
                ContactModel cm = new ContactModel();
                cm.ContactId = c.CONTACT_ID;
                cm.ScienceBaseId = c.SCIENCE_BASE_ID;
                cm.ContactName = c.NAME;
                cm.ContactOrgName = OrganizationSystemHandler.GetOrganizationFullName(c.ORGANIZATION_SYSTEM);
                cm.ContactEmail = c.EMAIL;
                cm.ContactPhone = c.PHONE;
                allContacts.Add(cm);
            }
            return allContacts;
        }

        private string GetManagerName(DATA_MANAGER dm)
        {
            string dmName = string.Empty;
            dmName = dm.FNAME + " " + dm.LNAME;

            return dmName;
        }//end httpMethod get
        
        private string GetProjManager(decimal p,LaMPDSEntities aLaMPRDS)
        {
            string name = string.Empty;
 	        DATA_MANAGER aDataManager = aLaMPRDS.PROJECTS.SingleOrDefault(
                                   a => a.PROJECT_ID == p).DATA_MANAGER;
            if (aDataManager.LNAME.StartsWith("ScienceBase"))
                return name = "ScienceBase";
            if (aDataManager.LNAME.StartsWith("Manager"))
                return name = "Unassigned";

            return name = aDataManager.FNAME + " " + aDataManager.LNAME;
        }

        private string GetManagerOrg(LaMPDSEntities aLaMPRDS, DATA_MANAGER dm)
        {
            string orgName = "";
            if (dm.ORGANIZATION_SYSTEM_ID.HasValue)
            {
                orgName = aLaMPRDS.ORGANIZATION_SYSTEM.Where(o => o.ORGANIZATION_SYSTEM_ID == dm.ORGANIZATION_SYSTEM_ID.Value).FirstOrDefault().ORGANIZATION.ORGANIZATION_NAME;
                
            }
            return orgName;
        }
        
        #endregion
    }//end class ProjectHandler
}//end namespace