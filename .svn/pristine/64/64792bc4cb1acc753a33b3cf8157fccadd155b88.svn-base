//------------------------------------------------------------------------------
//----- ProjDurationHandler -----------------------------------------------------------
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
    public class ProjDurationHandler:HandlerBase
    {
       #region Routed Methods

        #region GetMethods
      
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<PROJ_DURATION> projDurationList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                projDurationList = aLaMPRDS.PROJ_DURATION.ToList();

            }//end using

           // if (projDurationList != null)
             //   projDurationList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            return new OperationResult.OK { ResponseResource = projDurationList };
        }//end httpMethod get
        
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            PROJ_DURATION aProjDuration;

            //return BadRequest if ther is no ID
            if (entityId <= 0)
            { return new OperationResult.BadRequest();}

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                aProjDuration = aLaMPRDS.PROJ_DURATION.SingleOrDefault(pd => pd.PROJ_DURATION_ID == entityId);

            }//end using

          //  if (aProjDuration != null)
            //    aProjDuration.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aProjDuration };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetProjectDuration")]
        public OperationResult ProjectDuration(Int32 projectId)
        {
            PROJ_DURATION aProjDuration;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                aProjDuration = aLaMPRDS.PROJECTS.SingleOrDefault(p => p.PROJECT_ID == projectId).PROJ_DURATION;

            }//end using

          //  if (aProjDuration != null)
            //    aProjDuration.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aProjDuration };
        }//end httpMethod get

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(PROJ_DURATION aProjDuration)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aProjDuration.DURATION_VALUE))
            {return new OperationResult.BadRequest();}

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    if (!Exists(aLaMPRDS.PROJ_DURATION, ref aProjDuration))
                    {
                        aLaMPRDS.PROJ_DURATION.AddObject(aProjDuration);
                        aLaMPRDS.SaveChanges();
                    }
                }// end using
            }// end using

          //  if (aProjDuration != null)
          //      aProjDuration.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aProjDuration };
        }//end Post

        #endregion

        #region PutMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, PROJ_DURATION instance)
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
                    PROJ_DURATION ObjectToBeUpdated = aLaMPRDS.PROJ_DURATION.SingleOrDefault(lg => lg.PROJ_DURATION_ID == entityId);

                    //LaMPGroup
                    ObjectToBeUpdated.DURATION_VALUE = (string.IsNullOrEmpty(instance.DURATION_VALUE) ?
                        ObjectToBeUpdated.DURATION_VALUE : instance.DURATION_VALUE);

                    aLaMPRDS.SaveChanges();
                }// end using
            }// end using

           // if (instance != null)
             //   instance.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

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
                    PROJ_DURATION ObjectToBeDeleted = aLaMPRDS.PROJ_DURATION.SingleOrDefault(lg => lg.PROJ_DURATION_ID == entityId);

                    //delete it
                    aLaMPRDS.PROJ_DURATION.DeleteObject(ObjectToBeDeleted);
                    aLaMPRDS.SaveChanges();
 
                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        #endregion

        #endregion
        #region "Helper Methods"
        private Boolean Exists(System.Data.Objects.IObjectSet<PROJ_DURATION> projStats, ref PROJ_DURATION aProjStat)
        {
            PROJ_DURATION existingProjStat;
            PROJ_DURATION thisProjStat = aProjStat;
            //check if it exists
            try
            {
                existingProjStat = projStats.FirstOrDefault(l => string.Equals(l.DURATION_VALUE.ToUpper(),thisProjStat.DURATION_VALUE.ToUpper()));

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