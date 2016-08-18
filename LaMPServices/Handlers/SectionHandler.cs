//------------------------------------------------------------------------------
//----- SectionHandler -----------------------------------------------------------
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
    public class SectionHandler:HandlerBase
    {
        #region Routed Methods
       
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<SECTION> Secs;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                Secs = aLaMPRDS.SECTIONs.OrderBy(x => x.SECTION_NAME).ToList();
                
            }//end using

            return new OperationResult.OK { ResponseResource = Secs };
        }//end httpMethod get
   
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 secID)
        {
            SECTION aSection;

            ////return BadRequest if ther is no ID
            if (secID <= 0)
            { return new OperationResult.BadRequest(); }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                aSection = aLaMPRDS.SECTIONs.SingleOrDefault(o => o.SECTION_ID == secID);

            }//end using

            if (aSection != null)
                aSection.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aSection };
        }//end httpMethod get
         
        #endregion GetMethods

        #region PostMethods
           
        //Force the user to provide authentication 
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(SECTION aSec)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aSec.SECTION_NAME))
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //check if this org exists
                    if (!Exists(aLaMPRDS.SECTIONs, ref aSec))
                    {
                        //Save
                        aLaMPRDS.SECTIONs.AddObject(aSec);
                        aLaMPRDS.SaveChanges();
                    }

                }// end using
            }// end using

            if (aSec != null)
                aSec.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aSec };
        }

        #endregion

        #region PutMethods

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 sectionId, SECTION instance)
        {
            //Return BadRequest if missing required fields
            if ((sectionId <= 0))
            { return new OperationResult.BadRequest(); }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    SECTION ObjectToBeUpdated = aLaMPRDS.SECTIONs.SingleOrDefault(o => o.SECTION_ID == sectionId);

                    ObjectToBeUpdated.SECTION_NAME = instance.SECTION_NAME;

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

        //Delete ORGANIZATION
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 sectionId)
        {
            //Return BadRequest if missing required fields
            if (sectionId <= 0)
            { return new OperationResult.BadRequest(); }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    SECTION ObjectToBeDeleted = aLaMPRDS.SECTIONs.SingleOrDefault(o => o.SECTION_ID == sectionId);
                    //delete it
                    aLaMPRDS.SECTIONs.DeleteObject(ObjectToBeDeleted);

                    aLaMPRDS.SaveChanges();

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }        
        #endregion

        #endregion
        #region "Helper Methods"
        private Boolean Exists(System.Data.Objects.IObjectSet<SECTION> sections, ref SECTION aSection)
        {
            SECTION existingSection;
            SECTION thisSection = aSection;

            // check if it exists
            try
            {
                //get all the Orgs where the name matches                
                existingSection = sections.Where(o => string.Equals(o.SECTION_NAME.ToUpper(), thisSection.SECTION_NAME.ToUpper())).FirstOrDefault();

                if (existingSection == null)
                    return false;

                //if exists then update ref organization 
                aSection = existingSection;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }//end Exists
        
        #endregion
    }//end SectionHandler
}//end namespace