//------------------------------------------------------------------------------
//----- LaMPGroupTypeHandler -----------------------------------------------------------
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
    public class LakeHandler:HandlerBase
    {
       #region Routed Methods

        #region GetMethods
      
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<LAKE_TYPE> lakeList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                lakeList = aLaMPRDS.LAKE_TYPE.OrderBy(lg => lg.LAKE_TYPE_ID).ToList();

            }//end using

            if (lakeList != null)
                lakeList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
            
            return new OperationResult.OK { ResponseResource = lakeList };
        }//end httpMethod get
        
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 lakeId)
        {
            LAKE_TYPE aLake;

            //return BadRequest if ther is no ID
            if (lakeId <= 0)
            { return new OperationResult.BadRequest();}

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                aLake = aLaMPRDS.LAKE_TYPE.SingleOrDefault(lg => lg.LAKE_TYPE_ID == lakeId);

            }//end using
     
            if (aLake != null)
                aLake.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aLake };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteLake")] // was "GetSiteLake"
        public OperationResult GetSiteLake(Int32 siteId)
        {
            LAKE_TYPE aLake;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                aLake = aLaMPRDS.SITE.SingleOrDefault(s => s.SITE_ID == siteId).LAKE_TYPE;

            }//end using

            if (aLake != null)
                aLake.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aLake };
        }//end httpMethod get

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(LAKE_TYPE aLake)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aLake.LAKE))
            {return new OperationResult.BadRequest();}

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    if (!Exists(aLaMPRDS.LAKE_TYPE, ref aLake))
                    {
                        aLaMPRDS.LAKE_TYPE.AddObject(aLake);
                        aLaMPRDS.SaveChanges();
                    }
                }// end using
            }// end using

            if (aLake != null)
                aLake.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aLake };
        }//end Post

        #endregion

        #region PutMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 lakeId, LAKE_TYPE instance)
        {
            //Return BadRequest if missing required fields
            if ((lakeId <= 0))
            {return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    LAKE_TYPE ObjectToBeUpdated = aLaMPRDS.LAKE_TYPE.SingleOrDefault(lg => lg.LAKE_TYPE_ID == lakeId);

                    //LaMPGroup
                    ObjectToBeUpdated.LAKE = (string.IsNullOrEmpty(instance.LAKE) ?
                        ObjectToBeUpdated.LAKE : instance.LAKE);

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
        public OperationResult Delete(Int32 lakeId)
        {
            //Return BadRequest if missing required fields
            if (lakeId <= 0)
            {return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    LAKE_TYPE ObjectToBeDeleted = aLaMPRDS.LAKE_TYPE.SingleOrDefault(lg => lg.LAKE_TYPE_ID == lakeId);

                    //delete it
                    aLaMPRDS.LAKE_TYPE.DeleteObject(ObjectToBeDeleted);
                    aLaMPRDS.SaveChanges();
 
                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        #endregion

        #endregion
        #region "Helper Methods"
        private Boolean Exists(System.Data.Objects.IObjectSet<LAKE_TYPE> lakes, ref LAKE_TYPE aLake)
        {
            LAKE_TYPE existingLake;
            LAKE_TYPE thisLake = aLake;
            //check if it exists
            try
            {
                existingLake = lakes.FirstOrDefault(l => string.Equals(l.LAKE.ToUpper(),thisLake.LAKE.ToUpper()));

                if (existingLake == null)
                    return false;

                //if exists then update ref contact
                aLake = existingLake;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        //private decimal GetNextLaKeTypeID(System.Data.Objects.IObjectSet<LAKE_TYPE> lakes)
        //{
        //    decimal nextKey = 1;
        //    if (lakes.Count() > 0)
        //    {
        //        nextKey = lakes.OrderByDescending(l => l.LAKE_TYPE_ID).First().LAKE_TYPE_ID + 1;
        //    }

        //    return nextKey;
        //}
        #endregion
    }//end MediaTypeHandler
}//end namespace