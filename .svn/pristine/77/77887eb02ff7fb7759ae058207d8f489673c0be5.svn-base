//------------------------------------------------------------------------------
//----- FrequencyTypeHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles frequency type resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//               https://github.com/openrasta/openrasta/wiki/Handlers
//
//     
#region Comments
// 04.02.13 - jkn - Created public user for get requests
// 05.02.12 - jkn - Created
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
    public class FrequencyHandler:HandlerBase
    {
        #region Routed Methods

        #region GetMethods
 
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<FREQUENCY_TYPE> frequencyList;

                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {

                    frequencyList = aLaMPRDS.FREQUENCY_TYPE.OrderBy(f => f.FREQUENCY_TYPE_ID).ToList();

                }//end using
     
            if (frequencyList != null)
                frequencyList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            return new OperationResult.OK { ResponseResource = frequencyList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 frequencyId)
        {
            FREQUENCY_TYPE aFrequencyType;

            //return BadRequest if ther is no ID
            if (frequencyId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                aFrequencyType = aLaMPRDS.FREQUENCY_TYPE.SingleOrDefault(f => f.FREQUENCY_TYPE_ID == frequencyId);
                    
            }//end using

            if (aFrequencyType != null)
                aFrequencyType.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
 

            return new OperationResult.OK { ResponseResource = aFrequencyType };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetSiteFrequencies")]
        public OperationResult GetSiteFrequencies(Int32 siteId)
        {
            List<FREQUENCY_TYPE> frequencyList;

            //return BadRequest if ther is no ID
            if (siteId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                frequencyList = aLaMPRDS.SITE_FREQUENCY.Where(cf => cf.SITE_ID == siteId)
                                                            .Select(cf=>cf.FREQUENCY_TYPE)
                                                                .OrderBy(f=>f.FREQUENCY_TYPE_ID).ToList();

            }//end using


            if (frequencyList != null)
                frequencyList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            return new OperationResult.OK { ResponseResource = frequencyList };
        }//end httpMethod get
        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(FREQUENCY_TYPE aFrequency)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aFrequency.FREQUENCY))
            {return new OperationResult.BadRequest();}

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    
                    if (!Exists(aLaMPRDS.FREQUENCY_TYPE, ref aFrequency))
                    {
                    //set ID
                    //aFrequency.FREQUENCY_TYPE_ID = GetNextFrequencyID(aLaMPRDS.FREQUENCY_TYPE);

                        aLaMPRDS.FREQUENCY_TYPE.AddObject(aFrequency);
                    aLaMPRDS.SaveChanges();
                    
                    }
                    
                }// end using
            }// end using

            if (aFrequency != null)
                aFrequency.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aFrequency };
        }//end Post

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddSiteFrequency")]
        public OperationResult AddSiteFrequency(Int32 siteId, FREQUENCY_TYPE aFrequencyType)
        {
            SITE_FREQUENCY aSiteFrequency = null;
            List<FREQUENCY_TYPE> frequencyeList = null;
            //Return BadRequest if missing required fields
            if (siteId <= 0 || String.IsNullOrEmpty(aFrequencyType.FREQUENCY))
            { return new OperationResult.BadRequest(); }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {

                    //check if valid project
                    if (aLaMPRDS.SITE.First(c => c.SITE_ID == siteId) == null)
                        return new OperationResult.BadRequest { Description = "Site does not exist" };

                    //check authorization
                    if (!IsAuthorizedToEdit(aLaMPRDS.SITE.FirstOrDefault(c => c.SITE_ID == siteId).PROJECT.DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" };

                    if (!Exists(aLaMPRDS.FREQUENCY_TYPE, ref aFrequencyType))
                    {
                        //set obj
                        aLaMPRDS.FREQUENCY_TYPE.AddObject(aFrequencyType);
                        aLaMPRDS.SaveChanges();
                    }//end if

                    //add to site
                    //first check if Site already contains parameter
                    if (aLaMPRDS.SITE_FREQUENCY.FirstOrDefault(cf => cf.FREQUENCY_TYPE_ID == aFrequencyType.FREQUENCY_TYPE_ID &&
                                                                        cf.SITE_ID == siteId) == null)
                    {//create one
                        aSiteFrequency = new SITE_FREQUENCY();
                        //set ID and create siteMedia
                        aSiteFrequency.SITE_ID = siteId;
                        aSiteFrequency.FREQUENCY_TYPE_ID = aFrequencyType.FREQUENCY_TYPE_ID;

                        aLaMPRDS.SITE_FREQUENCY.AddObject(aSiteFrequency);
                        aLaMPRDS.SaveChanges();
                    }//end if

                    //return the list of contacts associated with project
                    frequencyeList = aLaMPRDS.FREQUENCY_TYPE.Where(f => f.SITE_FREQUENCY.Any(cf => cf.SITE_ID == siteId)).ToList();

                    if (frequencyeList != null)
                        frequencyeList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = frequencyeList };
        }
        #endregion

        #region PutMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 frequencyId, FREQUENCY_TYPE instance)
        {
            //Return BadRequest if missing required fields
            if ((frequencyId <= 0))
            {return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    FREQUENCY_TYPE ObjectToBeUpdated = aLaMPRDS.FREQUENCY_TYPE.SingleOrDefault(f => f.FREQUENCY_TYPE_ID == frequencyId);

                    //FREQUENCY
                    ObjectToBeUpdated.FREQUENCY = (string.IsNullOrEmpty(instance.FREQUENCY) ?
                        ObjectToBeUpdated.FREQUENCY : instance.FREQUENCY);

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
        public OperationResult Delete(Int32 frequencyId)
        {
            //Return BadRequest if missing required fields
            if (frequencyId <= 0)
            { return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    FREQUENCY_TYPE ObjectToBeDeleted = aLaMPRDS.FREQUENCY_TYPE.SingleOrDefault(f => f.FREQUENCY_TYPE_ID == frequencyId);

                    //delete it
                    aLaMPRDS.FREQUENCY_TYPE.DeleteObject(ObjectToBeDeleted);
                    aLaMPRDS.SaveChanges();
                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveSiteFrequency")]
        public OperationResult RemoveSiteFrequency(Int32 siteId, FREQUENCY_TYPE aFrequencyType)
        {
            //Return BadRequest if missing required fields
            if (siteId <= 0 || String.IsNullOrEmpty(aFrequencyType.FREQUENCY))
            { return new OperationResult.BadRequest(); }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //check if valid project
                    if (aLaMPRDS.SITE.First(c => c.SITE_ID == siteId) == null)
                        return new OperationResult.BadRequest { Description = "Site does not exist" };

                    //check authorization
                    if (!IsAuthorizedToEdit(aLaMPRDS.SITE.FirstOrDefault(c => c.SITE_ID == siteId).PROJECT.DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" };

                    //remove from catalolg
                    SITE_FREQUENCY thisSiteFreq = aLaMPRDS.SITE_FREQUENCY.FirstOrDefault(cf => cf.FREQUENCY_TYPE_ID == aFrequencyType.FREQUENCY_TYPE_ID &&
                                                                        cf.SITE_ID == siteId);
                    if (thisSiteFreq != null)
                    {//remove it

                        aLaMPRDS.SITE_FREQUENCY.DeleteObject(thisSiteFreq);
                        aLaMPRDS.SaveChanges();
                    }//end if

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        #endregion
        #endregion

        #region "Helper Methods"
        private Boolean Exists(System.Data.Objects.IObjectSet<FREQUENCY_TYPE> frequencies, ref FREQUENCY_TYPE aFrequencyType)
        {
            FREQUENCY_TYPE existingFrequencyType;
            FREQUENCY_TYPE thisFrequencyType = aFrequencyType;
            //check if it exists
            try
            {
                existingFrequencyType = frequencies.FirstOrDefault(f => f.FREQUENCY == thisFrequencyType.FREQUENCY);


                if (existingFrequencyType == null)
                    return false;

                //if exists then update ref contact
                aFrequencyType = existingFrequencyType;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        //private decimal GetNextFrequencyID(System.Data.Objects.IObjectSet<FREQUENCY_TYPE> frequencies)
        //{
        //    decimal nextKey = 1;
        //    if (frequencies.Count() > 0)
        //    {
        //        nextKey = frequencies.OrderByDescending(f => f.FREQUENCY_TYPE_ID).First().FREQUENCY_TYPE_ID + 1;
        //    }

        //    return nextKey;
        //}
        #endregion
    }//end FrequencyTypehandler
}//end namespace