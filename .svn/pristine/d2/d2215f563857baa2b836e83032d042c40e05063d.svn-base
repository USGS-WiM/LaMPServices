//------------------------------------------------------------------------------
//----- PublicationHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles publication resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//               https://github.com/openrasta/openrasta/wiki/Handlers
//
//     
#region Comments
// 04.02.13 - jkn - Created public user for get requests
// 04.25.12 - jkn - Created
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
    public class PublicationHandler:HandlerBase
    {
        #region Routed Methods

        #region GetMethods
      
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<PUBLICATION> pubsList;

                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {

                    pubsList = aLaMPRDS.PUBLICATIONS.OrderBy(p => p.PUBLICATION_ID).ToList();

                    if (pubsList != null)
                        pubsList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

            return new OperationResult.OK { ResponseResource = pubsList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 publicationId)
        {
            PUBLICATION aPub;

            //return BadRequest if ther is no ID
            if (publicationId <= 0)
            { return new OperationResult.BadRequest(); }

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                aPub = aLaMPRDS.PUBLICATIONS.SingleOrDefault(p => p.PUBLICATION_ID == publicationId);

                if (aPub != null)
                    aPub.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            }//end using

            return new OperationResult.OK { ResponseResource = aPub };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetProjectPublications")]
        public OperationResult GetProjectPublications(Int32 projectId)
        {
            List<PUBLICATION> pubsList;

                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {

                    //return the list of publications associated with project
                    pubsList = aLaMPRDS.PUBLICATIONS.Where(p => p.PROJECT.Any(pp => pp.PROJECT_ID == projectId)).ToList();

                    if (pubsList != null)
                        pubsList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using
  
            return new OperationResult.OK { ResponseResource = pubsList };
        }//end httpMethod get

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(PUBLICATION aPublication)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aPublication.TITLE) &&
               String.IsNullOrEmpty(aPublication.CITATION) &&
               String.IsNullOrEmpty(aPublication.DESCRIPTION) &&
               String.IsNullOrEmpty(aPublication.URL))
            {return new OperationResult.BadRequest(); }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {

                    //check if it exists
                    if (!Exists(aLaMPRDS.PUBLICATIONS, ref aPublication))
                    {
                        //set ID
                        //aPub.PUBLICATION_ID = GetNextPublicationID(aLaMPRDS.PUBLICATIONS);

                        aLaMPRDS.PUBLICATIONS.AddObject(aPublication);

                        aLaMPRDS.SaveChanges();
                    }

                    if (aPublication != null)
                        aPublication.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aPublication };
        }

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddProjectPublication")]
        public OperationResult AddProjectPublication(Int32 projectId, PUBLICATION aPublication)
        {
            PROJECT_PUBLICATIONS aProjectPublication = null;
            List<PUBLICATION> PubsList = null;
            //Return BadRequest if missing required fields
            if (projectId <= 0 || (String.IsNullOrEmpty(aPublication.TITLE) && 
                String.IsNullOrEmpty(aPublication.CITATION) &&
                String.IsNullOrEmpty(aPublication.DESCRIPTION) &&
                String.IsNullOrEmpty(aPublication.URL)))
            { return new OperationResult.BadRequest(); }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {

                    //get requested project
                    PROJECT requestedProject = aLaMPRDS.PROJECTS.First(p => p.PROJECT_ID == projectId);

                    //check if valid project
                    if (requestedProject == null)
                        return new OperationResult.BadRequest { Description = "Project does not exist" };

                    //check authorization
                    if (!IsAuthorizedToEdit(requestedProject.DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" };

                    if (!Exists(aLaMPRDS.PUBLICATIONS, ref aPublication))
                    {
                        //set ID
                        //aPublication.PUBLICATION_ID = GetNextPublicationID(aLaMPRDS.PUBLICATIONS);
                        aLaMPRDS.PUBLICATIONS.AddObject(aPublication);
                        aLaMPRDS.SaveChanges();
                    }//end if

                    //add to project
                    //first check if Project already contains publication
                    if (aLaMPRDS.PROJECT_PUBLICATIONS.FirstOrDefault(pp => pp.PUBLICATION_ID == aPublication.PUBLICATION_ID &&
                                                                          pp.PROJECT_ID == projectId) == null)
                    {//create one
                        aProjectPublication = new PROJECT_PUBLICATIONS();
                        //set ID and create ProjectContact
                        //aProjectPublication.PROJECT_PUBLICATION_ID = GetNextProjectPublicationID(aLaMPRDS.PROJECT_PUBLICATIONS);

                        aProjectPublication.PROJECT_ID = projectId;
                        aProjectPublication.PUBLICATION_ID = aPublication.PUBLICATION_ID;

                        aLaMPRDS.PROJECT_PUBLICATIONS.AddObject(aProjectPublication);
                        aLaMPRDS.SaveChanges();
                    }//end if

                    //return the list of contacts associated with project
                    PubsList = aLaMPRDS.PUBLICATIONS.Where(p => p.PROJECT.Any(pp => pp.PROJECT_ID == projectId)).ToList();

                    if (PubsList != null)
                        PubsList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = PubsList };
        }

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(List<PUBLICATION> pubsList)
        {
            //Return BadRequest if missing required fields
            if (pubsList.Count <= 0)
            {return new OperationResult.BadRequest();}

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    for (int i = 0; i < pubsList.Count; i++)
                    {
                        PUBLICATION item = pubsList[i];
                        if (!Exists(aLaMPRDS.PUBLICATIONS, ref item))
                        {
                            //item.PUBLICATION_ID = GetNextPublicationID(aLaMPRDS.PUBLICATIONS);
                            aLaMPRDS.PUBLICATIONS.AddObject(item);
                            aLaMPRDS.SaveChanges();
                            //update
                        }
                        //update publications to pass back
                        pubsList[i] = item;

                    }//next item

                    if (pubsList != null)
                        pubsList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));
                
                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = pubsList };
        }
        #endregion

        #region PutMethods

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 publicationId, PUBLICATION instance)
        {
            //Return BadRequest if missing required fields
            if ((publicationId <= 0))
            {return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    PUBLICATION ObjectToBeUpdated = aLaMPRDS.PUBLICATIONS.SingleOrDefault(p => p.PUBLICATION_ID == publicationId);

                    //Title
                    ObjectToBeUpdated.TITLE = (string.IsNullOrEmpty(instance.TITLE) ?
                        ObjectToBeUpdated.TITLE : instance.TITLE);

                    //description
                    ObjectToBeUpdated.DESCRIPTION = (string.IsNullOrEmpty(instance.DESCRIPTION) ?
                        ObjectToBeUpdated.DESCRIPTION : instance.DESCRIPTION);

                    //url
                    ObjectToBeUpdated.URL = (string.IsNullOrEmpty(instance.URL) ?
                        ObjectToBeUpdated.URL : instance.URL);

                    //Citation
                    ObjectToBeUpdated.CITATION = (string.IsNullOrEmpty(instance.CITATION) ?
                        ObjectToBeUpdated.CITATION : instance.CITATION);
                    
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
        [RequiresRole( AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 publicationId)
        {
            //Return BadRequest if missing required fields
            if (publicationId <= 0)
            {return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    PUBLICATION ObjectToBeDeleted = aLaMPRDS.PUBLICATIONS.SingleOrDefault(p => p.PUBLICATION_ID == publicationId);
                    //delete it
                    aLaMPRDS.PUBLICATIONS.DeleteObject(ObjectToBeDeleted);

                    aLaMPRDS.SaveChanges();

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveProjectPublication")]
        public OperationResult RemoveProjectPublication(Int32 projectId, PUBLICATION aPublication)
        {
            //Return BadRequest if missing required fields
            if (projectId <= 0)
            { return new OperationResult.BadRequest(); }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //get requested project
                    PROJECT requestedProject = aLaMPRDS.PROJECTS.First(p => p.PROJECT_ID == projectId);

                    //check if valid project
                    if (requestedProject == null)
                        return new OperationResult.BadRequest { Description = "Project does not exist" };

                    //check authorization
                    if (!IsAuthorizedToEdit(requestedProject.DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" };

                    //remove from project

                    //first check if Project already contains publication
                    PROJECT_PUBLICATIONS thisProjPub = aLaMPRDS.PROJECT_PUBLICATIONS.FirstOrDefault(pc => pc.PUBLICATION_ID == aPublication.PUBLICATION_ID &&
                                                                            pc.PROJECT_ID == projectId);

                    if (thisProjPub != null)
                    {//remove it

                        aLaMPRDS.PROJECT_PUBLICATIONS.DeleteObject(thisProjPub);
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
        private Boolean Exists(System.Data.Objects.IObjectSet<PUBLICATION> publications, ref PUBLICATION aPublication)
        {
            PUBLICATION existingPublication;
            PUBLICATION thisPublication = aPublication;
            //check if it exists
            try
            {
                existingPublication = publications.FirstOrDefault(p => string.Equals(p.TITLE.ToUpper(), thisPublication.TITLE.ToUpper()) &&
                                                                       (string.Equals(p.DESCRIPTION.ToUpper(), thisPublication.DESCRIPTION.ToUpper()) || string.IsNullOrEmpty(thisPublication.DESCRIPTION)) &&
                                                                       (string.Equals(p.CITATION.ToUpper(),thisPublication.CITATION.ToUpper()) || string.IsNullOrEmpty(thisPublication.CITATION)) &&
                                                                       (string.Equals(p.SCIENCE_BASE_ID.ToUpper(), thisPublication.SCIENCE_BASE_ID.ToUpper()) || string.IsNullOrEmpty(thisPublication.SCIENCE_BASE_ID)) &&
                                                                       (string.Equals(p.URL.ToUpper(),thisPublication.URL.ToUpper()) || string.IsNullOrEmpty(thisPublication.URL)));


                if (existingPublication == null)
                    return false;

                //if exists then update ref contact
                aPublication = existingPublication;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

      
        #endregion
     }//end PublicationHandler
}//end namespace