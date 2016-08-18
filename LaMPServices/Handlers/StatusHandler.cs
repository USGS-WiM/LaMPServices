//------------------------------------------------------------------------------
//----- StatusTypeHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles media type resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//               https://github.com/openrasta/openrasta/wiki/Handlers
//
//     
#region Comments
// 04.02.13 - jkn - Created public user for get requests
// 08.21.12 - jkn - Created
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using LaMPServices.Authentication;
using OpenRasta.Web;
using OpenRasta.Security;

namespace LaMPServices.Handlers
{
    public class StatusHandler:HandlerBase
    {
        #region Routed Methods

        #region GetMethods
        
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<STATUS_TYPE> statusList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                statusList = aLaMPRDS.STATUS_TYPE.OrderBy(s => s.STATUS_ID).ToList();

                if (statusList != null)
                    statusList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using

            return new OperationResult.OK { ResponseResource = statusList };
        }//end httpMethod get
        
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 statusId)
        {
            STATUS_TYPE aStatus;

            //return BadRequest if ther is no ID
            if (statusId <= 0)
            { return new OperationResult.BadRequest();}

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                aStatus = aLaMPRDS.STATUS_TYPE.SingleOrDefault(s => s.STATUS_ID == statusId);

                if (aStatus != null)
                    aStatus.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            }//end using

            return new OperationResult.OK { ResponseResource = aStatus };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName ="GetSiteStatus")] 
        public OperationResult GetSiteStatus(Int32 siteId)
        {
            STATUS_TYPE aStatus;

            //return BadRequest if ther is no ID
            if (siteId <= 0)
            {return new OperationResult.BadRequest();}

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                aStatus = aLaMPRDS.SITE.SingleOrDefault(s => s.SITE_ID == siteId).STATUS_TYPE;

                if (aStatus != null)
                    aStatus.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            }//end using
    
            return new OperationResult.OK { ResponseResource = aStatus };
        }//end httpMethod get

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(STATUS_TYPE aStatusType)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aStatusType.STATUS))
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    if (!Exists(aLaMPRDS.STATUS_TYPE, ref aStatusType))
                    {
                        //set ID
                        //aResource.RESOURCE_TYPE_ID = GetNextResourceID(aLaMPRDS.RESOURCE_TYPE);

                        aLaMPRDS.STATUS_TYPE.AddObject(aStatusType);
                        aLaMPRDS.SaveChanges();
                    }

                    if (aStatusType != null)
                        aStatusType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aStatusType };
        }

        #endregion

        #region PutMethods

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 statusId, STATUS_TYPE instance)
        {
            //Return BadRequest if missing required fields
            if ((statusId <= 0))
            { return new OperationResult.BadRequest(); }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    STATUS_TYPE ObjectToBeUpdated = aLaMPRDS.STATUS_TYPE.SingleOrDefault(r => r.STATUS_ID == statusId);

                    //STATUS
                    ObjectToBeUpdated.STATUS = (string.IsNullOrEmpty(instance.STATUS) ?
                        ObjectToBeUpdated.STATUS : instance.STATUS);

                    aLaMPRDS.SaveChanges();

                    if (instance != null)
                        instance.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = instance };
        }

        #endregion

        #region DeleteMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 statusId)
        {
            //Return BadRequest if missing required fields
            if (statusId <= 0)
            { return new OperationResult.BadRequest(); }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    STATUS_TYPE ObjectToBeDeleted = aLaMPRDS.STATUS_TYPE.SingleOrDefault(r => r.STATUS_ID == statusId);

                    //delete it
                    aLaMPRDS.STATUS_TYPE.DeleteObject(ObjectToBeDeleted);

                    aLaMPRDS.SaveChanges();

                }// end using

            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        #endregion

        #endregion

        #region "Helper Methods"
        private Boolean Exists(System.Data.Objects.IObjectSet<STATUS_TYPE> statusTypes, ref STATUS_TYPE aStatusType)
        {
            STATUS_TYPE existingStatus;
            STATUS_TYPE thisStatusType = aStatusType;
            //check if it exists
            try
            {
                existingStatus = statusTypes.FirstOrDefault(r => string.Equals(r.STATUS.ToUpper(), thisStatusType.STATUS.ToUpper()));

                if (existingStatus == null)
                    return false;

                //if exists then update ref contact
                aStatusType = existingStatus;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }
        #endregion


    }//end StatusTypeHandler
}//end namespace