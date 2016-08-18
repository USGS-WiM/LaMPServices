//------------------------------------------------------------------------------
//----- OrganizationHandler -----------------------------------------------------------
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
    public class OrganizationHandler:HandlerBase
    {
        #region Routed Methods
       
        #region GetMethods

        [HttpOperation(HttpMethod.GET, ForUriName="GetAllOrgs")]
        public OperationResult getAllOrganizations()
        {
            List<ORGANIZATION> orgs;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
               orgs = aLaMPRDS.ORGANIZATIONS.OrderBy(o => o.ORGANIZATION_NAME).ToList();
                
            }//end using

            if (orgs != null)
                orgs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            return new OperationResult.OK { ResponseResource = orgs };
        }//end httpMethod get
           
        [HttpOperation(HttpMethod.GET)]
        public OperationResult GetOrganization(Int32 organizationId)
        {            
            ORGANIZATION aOrg;

            ////return BadRequest if ther is no ID
            if (organizationId <= 0)
            { return new OperationResult.BadRequest(); }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                aOrg = aLaMPRDS.ORGANIZATIONS.SingleOrDefault(o => o.ORGANIZATION_ID == organizationId);

            }//end using

            if (aOrg != null)
                aOrg.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aOrg };
        }//end httpMethod get
   
        #endregion GetMethods

        #region PostMethods
  
        //Force the user to provide authentication 
        //post to ORGANIZATION TABLE (ORGANIZATION_NAME) 
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(ORGANIZATION aOrg)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aOrg.ORGANIZATION_NAME))
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //check if this org exists
                    if (!Exists(aLaMPRDS.ORGANIZATIONS,ref aOrg))
                    {
                        //post new organization, and then new organization_system                       
      
                        //Save
                        aLaMPRDS.ORGANIZATIONS.AddObject(aOrg);
                        aLaMPRDS.SaveChanges();
                    }
               
                }// end using
            }// end using

            if (aOrg != null)
                aOrg.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aOrg };
        }

       
        #endregion

        #region PutMethods

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 organizationId, ORGANIZATION instance)
        {
            //Return BadRequest if missing required fields
            if ((organizationId <= 0))
            { return new OperationResult.BadRequest(); }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    ORGANIZATION ObjectToBeUpdated = aLaMPRDS.ORGANIZATIONS.SingleOrDefault(o => o.ORGANIZATION_ID == organizationId);

                    ObjectToBeUpdated.ORGANIZATION_NAME = instance.ORGANIZATION_NAME;

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

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 organizationId)
        {
            //Return BadRequest if missing required fields
            if (organizationId <= 0)
            { return new OperationResult.BadRequest(); }
            
            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    ORGANIZATION ObjectToBeDeleted = aLaMPRDS.ORGANIZATIONS.SingleOrDefault(o => o.ORGANIZATION_ID == organizationId);
                    //delete it
                    aLaMPRDS.ORGANIZATIONS.DeleteObject(ObjectToBeDeleted);

                    aLaMPRDS.SaveChanges();

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        #endregion

        #endregion
        #region "Helper Methods"
       
        private Boolean Exists(System.Data.Objects.IObjectSet<ORGANIZATION> organizations, ref ORGANIZATION aOrganization)
        {
            ORGANIZATION existingOrganization;
            ORGANIZATION thisOrganization = aOrganization;
            
           // check if it exists
            try
            {
                //get all the Orgs where the name matches                
                existingOrganization = organizations.Where(o => string.Equals(o.ORGANIZATION_NAME.ToUpper(), thisOrganization.ORGANIZATION_NAME.ToUpper())).FirstOrDefault();

                if (existingOrganization == null)
                    return false;

                //if exists then update ref organization 
                aOrganization = existingOrganization;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }//end Exists
        #endregion
    }//end OrganizationHandler
}//end namespace