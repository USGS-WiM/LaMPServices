//------------------------------------------------------------------------------
//----- DivisionnHandler -----------------------------------------------------------
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
    public class DivisionHandler:HandlerBase
    {
        #region Routed Methods
       
        #region GetMethods

        [HttpOperation(HttpMethod.GET, ForUriName = "getAllDivisions")]
        public OperationResult getAllDivisions()
        {
            List<DIVISION> Divs;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                Divs = aLaMPRDS.DIVISIONs.OrderBy(x => x.DIVISION_NAME).ToList();
               
            }//end using           

            return new OperationResult.OK { ResponseResource = Divs };
        }
     
       [HttpOperation(HttpMethod.GET)]
        public OperationResult GetDivision(Int32 divID)
        {
            DIVISION aDivision;

            ////return BadRequest if ther is no ID
            if (divID <= 0)
            { return new OperationResult.BadRequest(); }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                aDivision = aLaMPRDS.DIVISIONs.SingleOrDefault(o => o.DIVISION_ID == divID);

            }//end using

            if (aDivision != null)
                aDivision.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aDivision };
        }//end httpMethod get
               
        #endregion GetMethods

        #region PostMethods
           
        //Force the user to provide authentication 
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(DIVISION aDiv)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aDiv.DIVISION_NAME))
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //check if this org exists
                    if (!Exists(aLaMPRDS.DIVISIONs, ref aDiv))
                    {
                        //Save
                        aLaMPRDS.DIVISIONs.AddObject(aDiv);
                        aLaMPRDS.SaveChanges();
                    }

                }// end using
            }// end using

            if (aDiv != null)
                aDiv.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aDiv };
        }

        #endregion

        #region PutMethods

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 divisionId, DIVISION instance)
        {
            //Return BadRequest if missing required fields
            if ((divisionId <= 0))
            { return new OperationResult.BadRequest(); }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    DIVISION ObjectToBeUpdated = aLaMPRDS.DIVISIONs.SingleOrDefault(o => o.DIVISION_ID == divisionId);

                    ObjectToBeUpdated.DIVISION_NAME = instance.DIVISION_NAME;
                    ObjectToBeUpdated.ORG_ID = instance.ORG_ID;

                    aLaMPRDS.SaveChanges();
                }// end using
            }// end using

            
            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = instance };
        }

        #endregion

        #region DeleteMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 divisionId)
        {
            //Return BadRequest if missing required fields
            if (divisionId <= 0)
            { return new OperationResult.BadRequest(); }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    DIVISION ObjectToBeDeleted = aLaMPRDS.DIVISIONs.SingleOrDefault(o => o.DIVISION_ID == divisionId);
                    //delete it
                    aLaMPRDS.DIVISIONs.DeleteObject(ObjectToBeDeleted);

                    aLaMPRDS.SaveChanges();

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        #endregion

        #endregion
        #region "Helper Methods"
       
        private Boolean Exists(System.Data.Objects.IObjectSet<DIVISION> divisions, ref DIVISION aDivision)
        {
            DIVISION existingDivision;
            DIVISION thisDivision = aDivision;

            // check if it exists
            try
            {
                //get all the Orgs where the name matches                
                existingDivision = divisions.Where(o => string.Equals(o.DIVISION_NAME.ToUpper(), thisDivision.DIVISION_NAME.ToUpper())).FirstOrDefault();

                if (existingDivision == null)
                    return false;

                //if exists then update ref organization 
                aDivision = existingDivision;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }//end Exists
       
        #endregion
    }//end DivisionHandler
}//end namespace