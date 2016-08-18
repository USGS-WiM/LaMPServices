//------------------------------------------------------------------------------
//----- ContactHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Contact resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//               https://github.com/openrasta/openrasta/wiki/Handlers
//
//     
#region Comments
// 04.02.13 - jkn - Created public user for get requests
// 04.23.12 - jkn - Created
#endregion                          


using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Linq;
using System.Web;

using LaMPServices.Authentication;

using OpenRasta.Web;
using OpenRasta.Security;

namespace LaMPServices.Handlers
{
    public class ContactHandler: HandlerBase
    {
        #region Routed Methods

        #region GetMethods
        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<CONTACT> contactList;

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    contactList = aLaMPRDS.CONTACTS.OrderBy(c => c.CONTACT_ID).ToList();

                    if (contactList != null)
                        contactList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = contactList };
        }//end httpMethod get

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 contactId)
        {
            CONTACT aContact;

            //return BadRequest if ther is no ID
            if (contactId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                 {
                     aContact = aLaMPRDS.CONTACTS.SingleOrDefault(c => c.CONTACT_ID == contactId);

                     if (aContact != null)
                        aContact.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                 }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = aContact };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetProjectContacts")]
        public OperationResult GetProjectContacts(Int32 projectId)
        {
            List<CONTACT> contactList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                //return the list of contacts associated with project
                contactList = aLaMPRDS.CONTACTS.Where(c => c.PROJECT.Any(pc => pc.PROJECT_ID == projectId)).ToList();

                if (contactList != null)
                    contactList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using


            return new OperationResult.OK { ResponseResource = contactList };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetOrgSysContacts")]
        public OperationResult GetOrgSysContacts(Int32 orgSysId)
        {
            List<CONTACT> contactList;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                //return the list of contacts associated with project
                contactList = aLaMPRDS.ORGANIZATION_SYSTEM.SingleOrDefault(o => o.ORGANIZATION_SYSTEM_ID == orgSysId).CONTACTs.ToList();

                if (contactList != null)
                    contactList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            }//end using


            return new OperationResult.OK { ResponseResource = contactList };
        }//end httpMethod get
        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(CONTACT aContact)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aContact.NAME))
            {return new OperationResult.BadRequest();}

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                 {
                   
                    if( !Exists(aLaMPRDS.CONTACTS, ref aContact))
                    {
                        //set ID
                        //aContact.CONTACT_ID = GetNextContactID(aLaMPRDS.CONTACTS);

                        aLaMPRDS.CONTACTS.AddObject(aContact);
                        aLaMPRDS.SaveChanges();
                    }

                    if (aContact != null)
                        aContact.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);
                
                 }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aContact };
        }

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddProjectContact")]
        public OperationResult AddProjectContact(Int32 projectId, CONTACT aContact)
        {
            PROJECT_CONTACTS aProjectContact = null;
            List<CONTACT> contactList = null;
            //Return BadRequest if missing required fields
            if (projectId <= 0 || String.IsNullOrEmpty(aContact.NAME))
            { return new OperationResult.BadRequest ();}

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
                    
                    if (!Exists(aLaMPRDS.CONTACTS, ref aContact))
                    {
                        aLaMPRDS.CONTACTS.AddObject(aContact);

                        //update project's last updated date
                        requestedProject.LAST_EDITED_STAMP = DateTime.Now.Date;

                        aLaMPRDS.SaveChanges();
                    }//end if

                    //add to project
                    //first check if Project already contains member
                    if (aLaMPRDS.PROJECT_CONTACTS.FirstOrDefault(pc => pc.CONTACT_ID == aContact.CONTACT_ID && pc.PROJECT_ID == projectId) == null)
                    {//create one
                        aProjectContact = new PROJECT_CONTACTS();
                        
                        aProjectContact.PROJECT_ID = projectId;
                        aProjectContact.CONTACT_ID = aContact.CONTACT_ID;

                        aLaMPRDS.PROJECT_CONTACTS.AddObject(aProjectContact);
                        aLaMPRDS.SaveChanges();
                    }//end if

                    //return the list of contacts associated with project
                    contactList = aLaMPRDS.CONTACTS.Where(c => c.PROJECT.Any(pc => pc.PROJECT_ID == projectId)).ToList();

                    if (contactList != null)
                        contactList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = contactList };
        }

        #endregion

        #region PutMethods

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult PUT(Int32 contactId, CONTACT instance)
        {
            //Return BadRequest if missing required fields
            if ((contactId <= 0))
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    CONTACT ObjectToBeUpdated = aLaMPRDS.CONTACTS.SingleOrDefault(c => c.CONTACT_ID == contactId);

                    ObjectToBeUpdated.NAME = instance.NAME;
                    ObjectToBeUpdated.EMAIL = instance.EMAIL;
                    ObjectToBeUpdated.PHONE = instance.PHONE;
                    ObjectToBeUpdated.ORGANIZATION_SYSTEM_ID = instance.ORGANIZATION_SYSTEM_ID;
                    ObjectToBeUpdated.SCIENCE_BASE_ID = instance.SCIENCE_BASE_ID;

                    //see if this contact is attached to a project, if so, update the project's last edit date
                    List<PROJECT> projectList = aLaMPRDS.PROJECTS.Where(p => p.CONTACTS.Any(c => c.CONTACT_ID == contactId)).ToList();
                    projectList.ForEach(p => p.LAST_EDITED_STAMP = DateTime.Now.Date);

                    aLaMPRDS.SaveChanges();

                    //instance = ObjectToBeUpdated;

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
        public OperationResult Delete(Int32 contactId)
        {
            //Return BadRequest if missing required fields
            if (contactId <= 0)
            {return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    CONTACT ObjectToBeDeleted = aLaMPRDS.CONTACTS.SingleOrDefault(c => c.CONTACT_ID == contactId);
                    
                    //make sure this Contact isn't associated with any Project, if it is don't delete it
                    List<PROJECT_CONTACTS> projContacts = aLaMPRDS.PROJECT_CONTACTS.Where(x => x.CONTACT_ID == contactId).ToList();

                    if (projContacts == null || projContacts.Count < 1)
                    {
                        //update all the project's last edited date
                        projContacts.ForEach(pc => pc.PROJECT.LAST_EDITED_STAMP = DateTime.Now.Date);

                        //delete it
                        aLaMPRDS.CONTACTS.DeleteObject(ObjectToBeDeleted);

                        aLaMPRDS.SaveChanges();
                    }
                }// end using

            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveProjectContact")]
        public OperationResult RemoveProjectContact(Int32 projectId, Int32 contactId)// CONTACT aContact)
        {
            //Return BadRequest if missing required fields
            if (projectId <= 0 || contactId <= 0)//String.IsNullOrEmpty(aContact.NAME))
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
                    PROJECT_CONTACTS thisProjContact = aLaMPRDS.PROJECT_CONTACTS.FirstOrDefault(pc => pc.CONTACT_ID == contactId &&
                                                                            pc.PROJECT_ID == projectId);

                    if (thisProjContact != null)
                    {//remove it

                        //update all the project's last edited date
                        thisProjContact.PROJECT.LAST_EDITED_STAMP = DateTime.Now.Date;

                        aLaMPRDS.PROJECT_CONTACTS.DeleteObject(thisProjContact);
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
        private Boolean Exists(System.Data.Objects.IObjectSet<CONTACT> contacts, ref CONTACT aContact)
        {
            CONTACT existingContact;
            CONTACT thisContact = aContact;
            //check if it exists
            try
            {
                existingContact = contacts.FirstOrDefault(c => String.Equals(c.NAME.ToUpper(), thisContact.NAME.ToUpper()) &&
                                        (string.IsNullOrEmpty(thisContact.EMAIL) || string.Equals(c.EMAIL.ToUpper(), thisContact.EMAIL.ToUpper())) &&
                                        (string.IsNullOrEmpty(thisContact.PHONE) || string.Equals(c.PHONE.ToUpper(), thisContact.PHONE.ToUpper())) &&
                                        (string.IsNullOrEmpty(thisContact.SCIENCE_BASE_ID) || string.Equals(c.SCIENCE_BASE_ID.ToUpper(), thisContact.SCIENCE_BASE_ID.ToUpper())) &&
                                        (c.ORGANIZATION_SYSTEM_ID.Value == thisContact.ORGANIZATION_SYSTEM_ID.Value || !thisContact.ORGANIZATION_SYSTEM_ID.HasValue));

                if (existingContact == null)
                    return false;
                
               //if exists then update ref contact
                aContact = existingContact;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        //private decimal GetNextContactID(System.Data.Objects.IObjectSet<CONTACT> contacts)
        //{
        //    decimal nextKey = 1;
        //    if (contacts.Count() > 0)
        //    {
        //        nextKey = contacts.OrderByDescending(c => c.CONTACT_ID).First().CONTACT_ID + 1;
        //    }

        //    return nextKey;
        //}
        //private decimal GetNextProjectContactID(System.Data.Objects.IObjectSet<PROJECT_CONTACTS> projCont)
        //{
        //    decimal nextKey = 1;
        //    if (projCont.Count() > 0)
        //    {
        //        nextKey = projCont.OrderByDescending(c => c.PROJECT_CONTACTS_ID).First().PROJECT_CONTACTS_ID + 1;
        //    }

        //    return nextKey;
        //}
         #endregion

    }//end ContactHandler
}//end namespace