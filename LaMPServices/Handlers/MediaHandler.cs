﻿//------------------------------------------------------------------------------
//----- MediaTypeHandler -----------------------------------------------------------
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
    public class MediaHandler: HandlerBase
    {
        
        #region Routed Methods

        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<MEDIA_TYPE> mediaList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                mediaList = aLaMPRDS.MEDIA_TYPE.OrderBy(m => m.MEDIA_TYPE_ID).ToList();

            }//end using

            if (mediaList != null)
                mediaList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
            
            return new OperationResult.OK { ResponseResource = mediaList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 mediaTypeId)
        {
            MEDIA_TYPE aMedia;

            //return BadRequest if ther is no ID
            if (mediaTypeId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                aMedia = aLaMPRDS.MEDIA_TYPE.SingleOrDefault(m => m.MEDIA_TYPE_ID == mediaTypeId);

            }//end using

            if (aMedia != null)
                aMedia.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aMedia };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName ="GetSiteMedia")]
        public OperationResult GetSiteMedia(Int32 siteId)
        {
            List<MEDIA_TYPE> mediaList;

            //return BadRequest if ther is no ID
            if (siteId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                mediaList = aLaMPRDS.SITE_MEDIA.Where(cm => cm.SITE_ID == siteId)
                                                                .Select(cm=>cm.MEDIA_TYPE)
                                                                .OrderBy(m=>m.MEDIA_TYPE_ID).ToList();
 
            }//end using

            if (mediaList != null)
                mediaList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            return new OperationResult.OK { ResponseResource = mediaList };
        }//end httpMethod get

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(MEDIA_TYPE aMedia)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aMedia.MEDIA))
            {return new OperationResult.BadRequest();}

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    if(!Exists(aLaMPRDS.MEDIA_TYPE, ref aMedia))
                    {
                        aLaMPRDS.MEDIA_TYPE.AddObject(aMedia);
                        aLaMPRDS.SaveChanges();
                    }
                }// end using
            }// end using

            if (aMedia != null)
                aMedia.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aMedia };
        }//end Post

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddSiteMedium")]
        public OperationResult AddSiteMedium(Int32 siteId, MEDIA_TYPE aMedia)
        {
            SITE_MEDIA aSiteMedia = null;
            List<MEDIA_TYPE> mediaList = null;
            //Return BadRequest if missing required fields
            if (siteId <= 0 || String.IsNullOrEmpty(aMedia.MEDIA))
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

                    if (!Exists(aLaMPRDS.MEDIA_TYPE, ref aMedia))
                    {
                        //set ID
                        //aParameter.PARAMETER_TYPE_ID = GetNextParameterID(aLaMPRDS.PARAMETER_TYPE);
                        aLaMPRDS.MEDIA_TYPE.AddObject(aMedia);
                        aLaMPRDS.SaveChanges();
                    }//end if

                    //add to site
                    //first check if Site already contains parameter
                    if (aLaMPRDS.SITE_MEDIA.FirstOrDefault(cm => cm.MEDIA_TYPE_ID == aMedia.MEDIA_TYPE_ID &&
                                                                         cm.SITE_ID == siteId) == null)
                    {//create one
                        aSiteMedia = new SITE_MEDIA();
                        //set ID and create siteMedia
                        aSiteMedia.SITE_ID = siteId;
                        aSiteMedia.MEDIA_TYPE_ID = aMedia.MEDIA_TYPE_ID;

                        aLaMPRDS.SITE_MEDIA.AddObject(aSiteMedia);
                        aLaMPRDS.SaveChanges();
                    }//end if

                    //return the list of contacts associated with project
                    mediaList = aLaMPRDS.MEDIA_TYPE.Where(m => m.SITE_MEDIA.Any(cm => cm.SITE_ID == siteId)).ToList();

                    if (mediaList != null)
                        mediaList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = mediaList };
        }
        #endregion

        #region PutMethods

        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 mediaId, MEDIA_TYPE instance)
        {
            //Return BadRequest if missing required fields
            if ((mediaId <= 0))
            {return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    MEDIA_TYPE ObjectToBeUpdated = aLaMPRDS.MEDIA_TYPE.SingleOrDefault(m => m.MEDIA_TYPE_ID == mediaId);

                    //MEDIA
                    ObjectToBeUpdated.MEDIA = (string.IsNullOrEmpty(instance.MEDIA) ?
                        ObjectToBeUpdated.MEDIA : instance.MEDIA);

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
        public OperationResult Delete(Int32 mediaId)
        {
            //Return BadRequest if missing required fields
            if (mediaId <= 0)
            { return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    MEDIA_TYPE ObjectToBeDeleted = aLaMPRDS.MEDIA_TYPE.SingleOrDefault(m => m.MEDIA_TYPE_ID == mediaId);

                    //delete it
                    aLaMPRDS.MEDIA_TYPE.DeleteObject(ObjectToBeDeleted);
                    aLaMPRDS.SaveChanges();

                }// end using

            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveSiteMedium")]
        public OperationResult RemoveSiteMedium(Int32 siteId, Int32 mediaId)//MEDIA_TYPE aMedia)
        {
            //Return BadRequest if missing required fields
            if (siteId <= 0 || mediaId <= 0)
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
                    SITE_MEDIA thisCatMed = aLaMPRDS.SITE_MEDIA.FirstOrDefault(cm => cm.MEDIA_TYPE_ID == mediaId &&
                                                                         cm.SITE_ID == siteId);
                    if (thisCatMed != null)
                    {//remove it
                        aLaMPRDS.SITE_MEDIA.DeleteObject(thisCatMed);
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
        private Boolean Exists(System.Data.Objects.IObjectSet<MEDIA_TYPE> media, ref MEDIA_TYPE aMedia)
        {
            MEDIA_TYPE existingMedia;
            MEDIA_TYPE thisMedia = aMedia;
            //check if it exists
            try
            {
                existingMedia = media.FirstOrDefault(m => string.Equals(m.MEDIA.ToUpper(),thisMedia.MEDIA.ToUpper()));

                if (existingMedia == null)
                    return false;

                //if exists then update ref
                aMedia = existingMedia;
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