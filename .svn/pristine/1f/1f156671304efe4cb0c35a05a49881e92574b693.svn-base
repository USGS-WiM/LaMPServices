//------------------------------------------------------------------------------
//----- ProjStatusHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2015 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
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
// 01.27.15 - TR - Created
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
    public class ProjStatusHandler:HandlerBase
    {
       #region Routed Methods

        #region GetMethods
      
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<PROJ_STATUS> projStatList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                projStatList = aLaMPRDS.PROJ_STATUS.ToList();

            }//end using

           // if (projStatList != null)
           //     projStatList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
            
            return new OperationResult.OK { ResponseResource = projStatList };
        }//end httpMethod get
        
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            PROJ_STATUS aProjStat;

            //return BadRequest if ther is no ID
            if (entityId <= 0)
            { return new OperationResult.BadRequest();}

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                aProjStat = aLaMPRDS.PROJ_STATUS.SingleOrDefault(ps => ps.PROJ_STATUS_ID == entityId);

            }//end using

           // if (aProjStat != null)
             //   aProjStat.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aProjStat };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetProjectStatus")]
        public OperationResult ProjectStatus(Int32 projectId)
        {
            PROJ_STATUS aProjStat;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                aProjStat = aLaMPRDS.PROJECTS.SingleOrDefault(p => p.PROJECT_ID == projectId).PROJ_STATUS;

            }//end using

         //   if (aProjStat != null)
           //     aProjStat.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aProjStat };
        }//end httpMethod get

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(PROJ_STATUS aProjStatus)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aProjStatus.STATUS_VALUE))
            {return new OperationResult.BadRequest();}

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    if (!Exists(aLaMPRDS.PROJ_STATUS, ref aProjStatus))
                    {
                        aLaMPRDS.PROJ_STATUS.AddObject(aProjStatus);
                        aLaMPRDS.SaveChanges();
                    }
                }// end using
            }// end using

           // if (aProjStatus != null)
             //   aProjStatus.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aProjStatus };
        }//end Post

        #endregion

        #region PutMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, PROJ_STATUS instance)
        {
            //Return BadRequest if missing required fields
            if ((entityId <= 0))
            {return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    PROJ_STATUS ObjectToBeUpdated = aLaMPRDS.PROJ_STATUS.SingleOrDefault(lg => lg.PROJ_STATUS_ID == entityId);

                    //LaMPGroup
                    ObjectToBeUpdated.STATUS_VALUE = (string.IsNullOrEmpty(instance.STATUS_VALUE) ?
                        ObjectToBeUpdated.STATUS_VALUE : instance.STATUS_VALUE);

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
        public OperationResult Delete(Int32 entityId)
        {
            //Return BadRequest if missing required fields
            if (entityId <= 0)
            {return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    PROJ_STATUS ObjectToBeDeleted = aLaMPRDS.PROJ_STATUS.SingleOrDefault(lg => lg.PROJ_STATUS_ID == entityId);

                    //delete it
                    aLaMPRDS.PROJ_STATUS.DeleteObject(ObjectToBeDeleted);
                    aLaMPRDS.SaveChanges();
 
                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        #endregion

        #endregion
        #region "Helper Methods"
        private Boolean Exists(System.Data.Objects.IObjectSet<PROJ_STATUS> projStats, ref PROJ_STATUS aProjStat)
        {
            PROJ_STATUS existingProjStat;
            PROJ_STATUS thisProjStat = aProjStat;
            //check if it exists
            try
            {
                existingProjStat = projStats.FirstOrDefault(l => string.Equals(l.STATUS_VALUE.ToUpper(),thisProjStat.STATUS_VALUE.ToUpper()));

                if (existingProjStat == null)
                    return false;

                //if exists then update ref contact
                aProjStat = existingProjStat;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        
        #endregion
    }//end MediaTypeHandler
}//end namespace