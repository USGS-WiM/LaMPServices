﻿//------------------------------------------------------------------------------
//----- DataHostHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Data_Host resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//               https://github.com/openrasta/openrasta/wiki/Handlers
//
//     
#region Comments
// 04.02.13 - jkn - Created public user for get requests
// 03.12.13 - jkn - Created
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
    public class DataHostHandler : HandlerBase
    {
        #region Routed Methods

        #region GetMethods

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<DATA_HOST> dataHostList;

                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {

                    dataHostList = aLaMPRDS.DATA_HOST.OrderBy(dh => dh.DATA_HOST_ID).ToList();

                    if (dataHostList != null)
                        dataHostList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using

            return new OperationResult.OK { ResponseResource = dataHostList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 entityId)
        {
            DATA_HOST aDataHost;

            //return BadRequest if ther is no ID
            if (entityId <= 0)
            {
                return new OperationResult.BadRequest();
            }


                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {
                    aDataHost = aLaMPRDS.DATA_HOST.SingleOrDefault(dh => dh.DATA_HOST_ID == entityId);

                    if (aDataHost != null)
                        aDataHost.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using

            return new OperationResult.OK { ResponseResource = aDataHost };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetProjectDataHosts")]
        public OperationResult GetProjectDataHosts(Int32 projectId)
        {
            List<DATA_HOST> dataHostList;

                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {

                    //return the list of contacts associated with project
                    dataHostList = aLaMPRDS.PROJECTS.SingleOrDefault(p=>p.PROJECT_ID == projectId).DATA_HOST.ToList();

                    if (dataHostList != null)
                        dataHostList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using


            return new OperationResult.OK { ResponseResource = dataHostList };
        }//end httpMethod get

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(DATA_HOST aDataHost)
        {

            //Return BadRequest if missing required fields
            if (aDataHost.PROJECT_ID <=0 || (
                string.IsNullOrEmpty(aDataHost.HOST_NAME) &&
                string.IsNullOrEmpty(aDataHost.DESCRIPTION) &&
                string.IsNullOrEmpty(aDataHost.PORTAL_URL)))
            { return new OperationResult.BadRequest(); }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //check authorization
                    if (!IsAuthorizedToEdit(aLaMPRDS.PROJECTS.First(p => p.PROJECT_ID == aDataHost.PROJECT_ID).DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" };
                    
                    //check if it exists
                    if (!Exists(aLaMPRDS.DATA_HOST, ref aDataHost))
                    {
                        //set ID
                        //aDataHost.DATA_HOST_ID = GetNextDataHostID(aLaMPRDS.DATA_HOST);

                        aLaMPRDS.DATA_HOST.AddObject(aDataHost);
                        aLaMPRDS.SaveChanges();
                    }

                    if (aDataHost != null)
                        aDataHost.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aDataHost };
        }

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddProjectDataHost")]
        public OperationResult AddProjectDataHost(Int32 projectId, DATA_HOST aDataHost)
        {
            List<DATA_HOST> dataHostList = null;
            //Return BadRequest if missing required fields
            if (projectId <= 0 || (String.IsNullOrEmpty(aDataHost.HOST_NAME) &&
                string.IsNullOrEmpty(aDataHost.DESCRIPTION) &&
                string.IsNullOrEmpty(aDataHost.PORTAL_URL)))
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

                    aDataHost.PROJECT_ID = requestedProject.PROJECT_ID;

                    //check authorization
                    if (!IsAuthorizedToEdit(requestedProject.DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" }; 

                    if (!Exists(aLaMPRDS.DATA_HOST, ref aDataHost))
                    {
                        aLaMPRDS.DATA_HOST.AddObject(aDataHost);

                        //update project's last updated date
                        requestedProject.LAST_EDITED_STAMP = DateTime.Now.Date;

                        aLaMPRDS.SaveChanges();
                    }//end if

                    //return the list of contacts associated with project
                    dataHostList = aLaMPRDS.DATA_HOST.Where(f => f.PROJECT_ID == projectId).ToList();

                    if (dataHostList != null)
                        dataHostList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = dataHostList };
        }// end AddProjectFundingSource
        #endregion

        #region PutMethods

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 entityId, DATA_HOST instance)
        {
            //Return BadRequest if missing required fields
            if ((entityId <= 0))
            { return new OperationResult.BadRequest(); }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //check authorization
                    if (!IsAuthorizedToEdit(aLaMPRDS.PROJECTS.First(p => p.PROJECT_ID == instance.PROJECT_ID).DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" };

                    //fetch the object to be updated (assuming that it exists)
                    DATA_HOST ObjectToBeUpdated = aLaMPRDS.DATA_HOST.SingleOrDefault(dh => dh.DATA_HOST_ID == entityId);

                    //Organization
                    ObjectToBeUpdated.HOST_NAME = instance.HOST_NAME;
                    //Portal url
                    ObjectToBeUpdated.PORTAL_URL = instance.PORTAL_URL;
                    //Project
                    ObjectToBeUpdated.PROJECT_ID = (Decimal.Equals(instance.PROJECT_ID, ObjectToBeUpdated.PROJECT_ID) ?
                         ObjectToBeUpdated.PROJECT_ID : instance.PROJECT_ID);
                    //Description
                    ObjectToBeUpdated.DESCRIPTION = instance.DESCRIPTION;

                    //update the project's last updated date
                    PROJECT thisProj = aLaMPRDS.PROJECTS.Where(p => p.PROJECT_ID == ObjectToBeUpdated.PROJECT_ID).FirstOrDefault();
                    thisProj.LAST_EDITED_STAMP = DateTime.Now.Date;

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
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 entityId)
        {
            //Return BadRequest if missing required fields
            if (entityId <= 0)
            { return new OperationResult.BadRequest(); }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    DATA_HOST ObjectToBeDeleted = aLaMPRDS.DATA_HOST.SingleOrDefault(dh => dh.DATA_HOST_ID == entityId);

                    //check authorization
                    if (!IsAuthorizedToEdit(aLaMPRDS.PROJECTS.First(p => p.PROJECT_ID == ObjectToBeDeleted.PROJECT_ID).DATA_MANAGER.USERNAME))
                        return new OperationResult.Forbidden { Description = "Not Authorized" };
                    
                    //see if this was attached to a project
                    PROJECT thisProj = aLaMPRDS.PROJECTS.Where(p => p.PROJECT_ID == ObjectToBeDeleted.PROJECT_ID).FirstOrDefault();
                    thisProj.LAST_EDITED_STAMP = DateTime.Now.Date;

                    //delete it
                    aLaMPRDS.DATA_HOST.DeleteObject(ObjectToBeDeleted);

                    
                    aLaMPRDS.SaveChanges();

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveProjectDataHost")]
        public OperationResult RemoveProjectDataHost(Int32 projectId, Int32 dataHostId)// DATA_HOST aDataHost)
        {
            //Return BadRequest if missing required fields
            if (projectId <= 0 || dataHostId <= 0)
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

                    //first check if Project already contains organization
                    DATA_HOST thisProjDataHost = aLaMPRDS.DATA_HOST.FirstOrDefault(pc => pc.DATA_HOST_ID == dataHostId &&
                                                                            pc.PROJECT_ID == projectId);

                    if (thisProjDataHost != null)
                    {//remove it
                        //update project last edit date
                        PROJECT thisProj = aLaMPRDS.PROJECTS.Where(p => p.PROJECT_ID == thisProjDataHost.PROJECT_ID).FirstOrDefault();
                        thisProj.LAST_EDITED_STAMP = DateTime.Now.Date;

                        aLaMPRDS.DATA_HOST.DeleteObject(thisProjDataHost);
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
        private Boolean Exists(System.Data.Objects.IObjectSet<DATA_HOST> dataHosts, ref DATA_HOST aDataHost)
        {
            DATA_HOST existingDataHost;
            DATA_HOST thisDataHost = aDataHost;
            //check if it exists
            try
            {
                existingDataHost = dataHosts.FirstOrDefault(dh => dh.PROJECT_ID.Equals(thisDataHost.PROJECT_ID) &&
                                                                 (string.Equals(dh.HOST_NAME.ToUpper(), thisDataHost.HOST_NAME.ToUpper()) || string.IsNullOrEmpty(thisDataHost.PORTAL_URL)) &&
                                                                 (string.Equals(dh.PORTAL_URL.ToUpper(),thisDataHost.PORTAL_URL.ToUpper()) || string.IsNullOrEmpty(thisDataHost.PORTAL_URL)) &&
                                                                 (string.Equals(dh.DESCRIPTION.ToUpper(), thisDataHost.DESCRIPTION.ToUpper()) || string.IsNullOrEmpty(thisDataHost.DESCRIPTION)));

                if (existingDataHost == null)
                    return false;

                //if exists then update ref contact
                aDataHost = existingDataHost;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        //private decimal GetNextDataHostID(System.Data.Objects.IObjectSet<DATA_HOST> dataHosts)
        //{
        //    decimal nextKey = 1;
        //    if (dataHosts.Count() > 0)
        //    {
        //        nextKey = dataHosts.OrderByDescending(dh => dh.DATA_HOST_ID).First().DATA_HOST_ID + 1;
        //    }

        //    return nextKey;
        //}

        #endregion
    }//end ObjectiveHandler
}//end namespace