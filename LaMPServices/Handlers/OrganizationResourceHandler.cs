//------------------------------------------------------------------------------
//----- OrganizationSystemHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Organizations resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//               https://github.com/openrasta/openrasta/wiki/Handlers
//
//     
#region Comments
// 04.02.13 - jkn - Created public user for get requests
// 04.26.12 - jkn - Created
#endregion                          

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Web;

using LaMPServices.Authentication;
using LaMPServices.Resources;
using OpenRasta.Web;
using OpenRasta.Security;

namespace LaMPServices.Handlers
{
    public class OrganizationResourceHandler: HandlerBase
    {
        #region Routed Methods
       
        #region GetMethods
     
        [HttpOperation(HttpMethod.GET, ForUriName="AllOrgResources")]
        public OperationResult AllOrgResources()
        {
            List<OrganizationResource> orgResources;
            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                orgResources = aLaMPRDS.ORGANIZATION_SYSTEM.Select(x => new OrganizationResource
                {
                    OrganizationSystemID = x.ORGANIZATION_SYSTEM_ID,
                    OrganizationID = x.ORG_ID,
                    OrganizationName = x.ORGANIZATION.ORGANIZATION_NAME,
                    DivisionID = x.DIV_ID,
                    DivisionName = x.DIV_ID.HasValue ? x.DIVISION.DIVISION_NAME : "",
                    SectionID = x.SEC_ID,
                    SectionName = x.SEC_ID.HasValue ? x.SECTION.SECTION_NAME : "",
                    ScienceBaseID = x.SCIENCE_BASE_ID
                }).ToList();
            }
            return new OperationResult.OK { ResponseResource = orgResources };
        }//end httpMethod get

        //<OrganizationResource>
        [HttpOperation(HttpMethod.GET, ForUriName="GetThisOrgSystem")]
        public OperationResult GetOrgResources(Int32 orgSysId)
        {
            OrganizationResource orgRes;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                orgRes = aLaMPRDS.ORGANIZATION_SYSTEM.Where(os => os.ORGANIZATION_SYSTEM_ID == orgSysId).Select(x => new OrganizationResource
                {
                    OrganizationSystemID = x.ORGANIZATION_SYSTEM_ID,
                    OrganizationID = x.ORG_ID,
                    OrganizationName = x.ORGANIZATION.ORGANIZATION_NAME,
                    DivisionID = x.DIV_ID,
                    DivisionName = x.DIV_ID.HasValue ? x.DIVISION.DIVISION_NAME : "",
                    SectionID = x.SEC_ID,
                    SectionName = x.SEC_ID.HasValue ? x.SECTION.SECTION_NAME : "",
                    ScienceBaseID = x.SCIENCE_BASE_ID
                }).FirstOrDefault();
            }
            return new OperationResult.OK { ResponseResource = orgRes };
        }//end httpMethod get

        //<List<OrganizationResource>>
        [HttpOperation(HttpMethod.GET, ForUriName = "GetProjectOrganizations")]
        public OperationResult GetProjectOrganizations(Int32 projectId)
        {
            List<OrganizationResource> orgs;

                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {
                    orgs = aLaMPRDS.ORGANIZATION_SYSTEM.Where(o=>o.PROJECT.Any(p=>p.PROJECT_ID == projectId)).Select(x => new OrganizationResource {
                        OrganizationSystemID = x.ORGANIZATION_SYSTEM_ID,
                        OrganizationID = x.ORG_ID,
                        OrganizationName = x.ORGANIZATION.ORGANIZATION_NAME,
                        DivisionID = x.DIV_ID,
                        DivisionName = x.DIV_ID.HasValue ? x.DIVISION.DIVISION_NAME : "",
                        SectionID = x.SEC_ID,
                        SectionName = x.SEC_ID.HasValue ? x.SECTION.SECTION_NAME : "",
                        ScienceBaseID = x.SCIENCE_BASE_ID
                    }).ToList();
                   
                }//end using
   
            
            return new OperationResult.OK { ResponseResource = orgs };
        }//end httpMethod get
          
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "AddProjectOrganization")]
        public OperationResult AddProjectOrganization(Int32 projectId, Int32 orgId, [Optional] string divId, [Optional] string secId)
        {
            //adding project Organization_system relationship (organization_system_id is being added to relationship table)
            PROJECT_COOPERATORS aProjectCooperators = null;
            List<OrganizationResource> CooperatorList = null;

            //Return BadRequest if missing required fields
            if (projectId <= 0 || orgId <= 0)
            { return new OperationResult.BadRequest(); }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //get requested project
                    PROJECT requestedProject = aLaMPRDS.PROJECTS.First(p => p.PROJECT_ID == projectId);

                    Int32 dID = divId != null && divId != "0" ? Convert.ToInt32(divId) : 0;
                    Int32 sID = secId != null && secId != "0" ? Convert.ToInt32(secId) : 0;
                    ORGANIZATION_SYSTEM orgSystem = aLaMPRDS.ORGANIZATION_SYSTEM.FirstOrDefault(o => o.ORG_ID == orgId && o.DIV_ID == dID && o.SEC_ID == sID);
                    if (orgSystem == null)
                    {
                        //post it
                        ORGANIZATION_SYSTEM newOrgSys = new ORGANIZATION_SYSTEM();
                        newOrgSys.ORG_ID = orgId; newOrgSys.DIV_ID = dID; newOrgSys.SEC_ID = sID;
                        aLaMPRDS.ORGANIZATION_SYSTEM.AddObject(newOrgSys);
                        aLaMPRDS.SaveChanges();
                        orgSystem = newOrgSys;
                    }
                    //check if valid project
                    if (requestedProject == null || orgSystem == null)
                        return new OperationResult.BadRequest { Description = "Project or Organization does not exist" };

                    //check authorization
                    if (!IsAuthorizedToEdit(requestedProject.DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" };

                    //add to project
                    //first check if Project already contains organization
                    PROJECT_COOPERATORS exists = aLaMPRDS.PROJECT_COOPERATORS.FirstOrDefault(pc => pc.ORGANIZATION_SYSTEM_ID == orgSystem.ORGANIZATION_SYSTEM_ID && pc.PROJECT_ID == projectId);

                    //if (aLaMPRDS.PROJECT_COOPERATORS.FirstOrDefault(pc => pc.ORGANIZATION_SYSTEM_ID == orgSystem.ORGANIZATION_SYSTEM_ID && pc.PROJECT_ID == projectId) == null)
                    if (exists == null)
                    {//create one
                        aProjectCooperators = new PROJECT_COOPERATORS();

                        aProjectCooperators.PROJECT_ID = projectId;
                        aProjectCooperators.ORGANIZATION_SYSTEM_ID = orgSystem.ORGANIZATION_SYSTEM_ID;

                        aLaMPRDS.PROJECT_COOPERATORS.AddObject(aProjectCooperators);
                        aLaMPRDS.SaveChanges();
                    }//end if

                    //return the list of organizationRes associated with project
                    CooperatorList = aLaMPRDS.ORGANIZATION_SYSTEM.Where(o => o.PROJECT.Any(pc => pc.PROJECT_ID == projectId)).Select(os => new OrganizationResource
                    {
                        OrganizationSystemID = os.ORGANIZATION_SYSTEM_ID,
                        OrganizationID = os.ORG_ID,
                        OrganizationName = os.ORGANIZATION.ORGANIZATION_NAME,
                        DivisionID = os.DIV_ID,
                        DivisionName = os.DIV_ID.HasValue ? os.DIVISION.DIVISION_NAME : "",
                        SectionID = os.SEC_ID,
                        SectionName = os.SEC_ID.HasValue ? os.SECTION.SECTION_NAME : "",
                        ScienceBaseID = os.SCIENCE_BASE_ID
                    }).ToList();
                    
                }// end using

            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = CooperatorList };
        } 
        #endregion GetMethods

        
        #region PostMethods
        #endregion

        #endregion
    }//end OrganizationHandler
}//end namespace