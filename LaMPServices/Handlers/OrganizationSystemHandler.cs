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
using System.Linq;
using System.Web;

using LaMPServices.Authentication;
using LaMPServices.Resources;
using OpenRasta.Web;
using OpenRasta.Security;

namespace LaMPServices.Handlers
{
    public class OrganizationSystemHandler:HandlerBase
    {
        #region Routed Methods
       
        #region GetMethods
      
        [HttpOperation(HttpMethod.GET, ForUriName = "GetAllOrgSys")]
        public OperationResult getAllOrganizationSystems()
        {
            List<ORGANIZATION_SYSTEM> orgs;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                orgs = aLaMPRDS.ORGANIZATION_SYSTEM.ToList();

            }//end using

            if (orgs != null)
                orgs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            return new OperationResult.OK { ResponseResource = orgs };
        }
               
        [HttpOperation(HttpMethod.GET, ForUriName="GetThisOrgSys")]
        public OperationResult GetOrganizationSystem(Int32 orgSysID)
        {
            ORGANIZATION_SYSTEM aOrg;            

            //return BadRequest if ther is no ID
            if (orgSysID <= 0)
            { return new OperationResult.BadRequest(); }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                aOrg = aLaMPRDS.ORGANIZATION_SYSTEM.SingleOrDefault(o => o.ORGANIZATION_SYSTEM_ID == orgSysID);

            }//end using

            if (aOrg != null)
                aOrg.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aOrg };
        }//end httpMethod get
        
        //Delete relationship between org and project
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "RemoveProjectOrganization")]
        public OperationResult RemoveProjectOrganization(Int32 projectId, Int32 orgSysId)
        {
            //Return BadRequest if missing required fields
            if (projectId <= 0 || orgSysId <= 0)
            { return new OperationResult.BadRequest(); }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //get requested project
                    PROJECT requestedProject = aLaMPRDS.PROJECTS.FirstOrDefault(p => p.PROJECT_ID == projectId);
                    ORGANIZATION_SYSTEM orgSystem = aLaMPRDS.ORGANIZATION_SYSTEM.FirstOrDefault(os => os.ORGANIZATION_SYSTEM_ID == orgSysId);

                    //check if valid project
                    if (requestedProject == null)
                        return new OperationResult.BadRequest { Description = "Project does not exist" };

                    //check authorization
                    if (!IsAuthorizedToEdit(requestedProject.DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" };

                    //remove from project

                    //first check if Project already contains organization
                    PROJECT_COOPERATORS thisProjCoop = aLaMPRDS.PROJECT_COOPERATORS.FirstOrDefault(pc => pc.ORGANIZATION_SYSTEM_ID == orgSysId &&
                                                                            pc.PROJECT_ID == projectId);

                    if (thisProjCoop != null)
                    {
                        //remove it
                        aLaMPRDS.PROJECT_COOPERATORS.DeleteObject(thisProjCoop);
                        aLaMPRDS.ORGANIZATION_SYSTEM.DeleteObject(orgSystem);
                        aLaMPRDS.SaveChanges();
                    }//end if

                }// end using

            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        #endregion GetMethods

        #region PostMethods
        //Force the user to provide authentication 
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(ORGANIZATION_SYSTEM anOrgSystem)
        {
            //Return BadRequest if missing required fields
            if (anOrgSystem.ORG_ID <= 0)
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                   //post new organization, and then new organization_system                        

                    //Save
                    aLaMPRDS.ORGANIZATION_SYSTEM.AddObject(anOrgSystem);
                    aLaMPRDS.SaveChanges();                   

                }// end using
            }// end using

            if (anOrgSystem != null)
                anOrgSystem.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = anOrgSystem };
        }
        
        #endregion

        #region PutMethods

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 orgSysID, ORGANIZATION_SYSTEM instance)
        {
            //Return BadRequest if missing required fields
            if ((orgSysID <= 0))
            { return new OperationResult.BadRequest(); }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    ORGANIZATION_SYSTEM ObjectToBeUpdated = aLaMPRDS.ORGANIZATION_SYSTEM.SingleOrDefault(o => o.ORGANIZATION_SYSTEM_ID == orgSysID);

                    
                    ObjectToBeUpdated.ORG_ID = instance.ORG_ID;
                    ObjectToBeUpdated.DIV_ID = instance.DIV_ID;
                    ObjectToBeUpdated.SEC_ID = instance.SEC_ID;
                    ObjectToBeUpdated.SCIENCE_BASE_ID = instance.SCIENCE_BASE_ID;
                    
                    aLaMPRDS.SaveChanges();
                }// end using
            }// end using

            if (instance != null)
                instance.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = instance };
        }

        #endregion

        #region DeleteMethods
        
        
        #endregion

        #endregion
        #region "Helper Methods"
        private Boolean Exists(System.Data.Objects.IObjectSet<ORGANIZATION_SYSTEM> orgSystems, ref ORGANIZATION_SYSTEM anOrgSystem)
        {
            ORGANIZATION_SYSTEM existingOrgSystem;
            ORGANIZATION_SYSTEM thisOrgSystem = anOrgSystem;

            // check if it exists
            try
            {
                //get all the Orgs where the name matches                
                existingOrgSystem = orgSystems.Where(o => o.ORG_ID == thisOrgSystem.ORG_ID &&
                                                            o.DIV_ID.Value == thisOrgSystem.DIV_ID.Value &&
                                                            o.SEC_ID.Value == thisOrgSystem.SEC_ID.Value &&
                                                            o.SCIENCE_BASE_ID == thisOrgSystem.SCIENCE_BASE_ID).FirstOrDefault();

                if (existingOrgSystem == null)
                    return false;

                //if exists then update ref organization 
                anOrgSystem = existingOrgSystem;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }//end Exists

        public static string GetOrganizationFullName(ORGANIZATION_SYSTEM orgSys)
        {
            string orgFullName = "";

            return orgFullName;
        }
        #endregion
    }//end OrganizationHandler
}//end namespace