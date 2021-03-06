﻿//------------------------------------------------------------------------------
//----- StateHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//  
//   purpose:   Handles site States through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//               https://github.com/openrasta/openrasta/wiki/Handlers
//
//     
#region Comments
// 12.06.13 - tr - Created
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MoreLinq;

using LaMPServices.Authentication;
using OpenRasta.Web;
using OpenRasta.Security;

namespace LaMPServices.Handlers
{
    public class StateHandler:HandlerBase
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

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteStates")] // 
        public OperationResult GetSiteStates()
        {
            List<string> States = new List<string>();
            IEnumerable<SITE> query;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                //query = aLaMPRDS.SITE.DistinctBy(x => x.STATE_PROVINCE);
                
                //States = query.Select(p => p.STATE_PROVINCE).ToList();
                States = aLaMPRDS.SITE.DistinctBy(x => x.STATE_PROVINCE).Select(p => p.STATE_PROVINCE).ToList();
                
            }//end using

            return new OperationResult.OK { ResponseResource = States };
        }//end httpMethod get

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