﻿//------------------------------------------------------------------------------
//----- DataManagerHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Data Manager resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//              https://github.com/openrasta/openrasta/wiki/Handlers
//
//     
#region Comments
// 04.26.12 - jkn - Created
#endregion                          


using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading;
using System.ComponentModel;

using LaMPServices.Authentication;
using LaMPServices.Resources;
using OpenRasta.Web;
using OpenRasta.Security;


namespace LaMPServices.Handlers
{
    public class DataManagerHandler:HandlerBase
    {

        #region Routed Methods

        #region GetMethods
        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<DATA_MANAGER> managerList = new List<DATA_MANAGER>();

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                 {
                     managerList = aLaMPRDS.DATA_MANAGER.OrderBy(m => m.DATA_MANAGER_ID).ToList();

                     if (managerList != null)
                         managerList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                 }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = managerList };
        }//end httpMethod get

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 dataManagerId)
        {
            DATA_MANAGER aDataManager;

            //return BadRequest if ther is no ID
            if (dataManagerId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                 {
                     aDataManager = aLaMPRDS.DATA_MANAGER.SingleOrDefault(
                                    m => m.DATA_MANAGER_ID == dataManagerId);

                     if (aDataManager != null)
                         aDataManager.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                 }//end using
             }//end using

            return new OperationResult.OK { ResponseResource = aDataManager };
        }//end httpMethod get

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName="GetProjectDataManager")]
        public OperationResult GetProjectDataManager(Int32 projectId)
        {
            DATA_MANAGER aDataManager;

            //return BadRequest if ther is no ID
            if (projectId <= 0)
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    aDataManager = aLaMPRDS.PROJECTS.SingleOrDefault(
                                   p => p.PROJECT_ID == projectId).DATA_MANAGER;

                    if (aDataManager != null)
                        aDataManager.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = aDataManager };
        }//end httpMethod get

        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetDMListModel")]
        public OperationResult GetDataManagerList()
        {
            List<DM_LIST_VIEW> DMList = new List<DM_LIST_VIEW>();
             
            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    DMList = aLaMPRDS.DM_LIST_VIEW.ToList();
                   
                }
            }
            return new OperationResult.OK { ResponseResource = DMList };
        }//end 

        /// 
        /// Force the user to provide authentication 
        /// 
        [RequiresAuthentication]
        [HttpOperation(HttpMethod.GET, ForUriName = "GetByUserName")]
        public OperationResult Get(string userName)
        {
            userName = this.username;
            DATA_MANAGER aDataManager;

            //Return BadRequest if there is no ID
            if (userName == null)
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    aDataManager = aLaMPRDS.DATA_MANAGER.SingleOrDefault(
                             m => m.USERNAME.ToLower() == userName.ToLower());

                    if (aDataManager != null)
                        aDataManager.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

                }//end using
            }//end using

            return new OperationResult.OK { ResponseResource = aDataManager };
        }//end HttpMethod.GET
 
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.GET, ForUriName = "ChangeDataManagerPassword")]
        public OperationResult ChangeDataManagerPassword(string userName, string newPassword)
        {
            DATA_MANAGER aDataManager;

            //Return BadRequest if missing required fields
            if ((string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(newPassword)))
            { return new OperationResult.BadRequest() { ResponseResource = "Invalid arguments" }; }

            if (!IsAuthorized(AdminRole) && Context.User.Identity.Name != username)
            { return new OperationResult.Forbidden(); }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    aDataManager = aLaMPRDS.DATA_MANAGER.SingleOrDefault(
                                                            m => m.USERNAME == userName);

                    if (aDataManager == null )
                    { return new OperationResult.BadRequest() { ResponseResource = "no manager exists" }; }

                    // edit user profile using db stored procedure
                    // stored db throws errors internally but does not pass pass error
                    aLaMPRDS.OWNERPROFILE_EDITPASSWORD(aDataManager.USERNAME, newPassword);
                }//end using
            }//end using

            if (aDataManager != null)
                aDataManager.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aDataManager };

        }//end HttpMethod.PUT

        [HttpOperation(HttpMethod.GET, ForUriName= "ResetPassword")]
        public OperationResult ResetPassword(string email)
        {
            DATA_MANAGER aDataManager = null;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                aDataManager = aLaMPRDS.DATA_MANAGER.Where(dm => dm.EMAIL == email).FirstOrDefault();

                if (aDataManager != null)
                {
                    string newPassword = buildDefaultPassword(aDataManager);
                    //aLaMPRDS.OWNERPROFILE_EDITPASSWORD(aDataManager.USERNAME, newPassword);       
             
                    //send email letting them know
                    var client = new SmtpClient();

                    // Create a mailing address that includes a UTF8 character in the display name.
                    MailAddress from = new MailAddress("troddick@usgs.gov",
                       "SiGL " + (char)0xD8 + " Admin",
                    System.Text.Encoding.UTF8);
                    // Set destinations for the e-mail message.
                    MailAddress to = new MailAddress(email);
                    // Specify the message content.
                    MailMessage message = new MailMessage(from, to);
                    message.Body = "Your password has been reset. Here is your new password between the brackets. PASSWORD: [ " + newPassword + " ]. Please log in to the SiGL_DMS application using this new password." +
                        "Navigate to your ACCOUNT page and change your password.";
                    // Include some non-ASCII characters in body and subject.
                    //string someArrows = new string(new char[] { '\u2190', '\u2191', '\u2192', '\u2193' });
                    //message.Body += Environment.NewLine + someArrows;
                    //message.BodyEncoding = System.Text.Encoding.UTF8;
                    message.Subject = "SiGL_DMS password reset";
                    message.SubjectEncoding = System.Text.Encoding.UTF8;

                    
                    client.SendCompleted += (s, e) => {
                        client.Dispose();
                        message.Dispose();
                    };
                    client.SendAsync(message, null);
                }
            }
            return new OperationResult.OK { };
        }
        //static bool mailSent = false;
        //private static void SendCompletedCallback(object sender, AsyncCompletedEventArgs e)
        //{
        //    // Get the unique identifier for this asynchronous operation.
        //    String token = (string)e.UserState;

        //    if (e.Cancelled)
        //    {
        //        Console.WriteLine("[{0}] Send canceled.", token);
        //    }
        //    if (e.Error != null)
        //    {
        //        Console.WriteLine("[{0}] {1}", token, e.Error.ToString());
        //    }
        //    else
        //    {
        //        Console.WriteLine("Message sent.");
        //    }
        //    mailSent = true;
        //}
        //public static void Main(string[] args)
        //{
        //    // Command line argument must the the SMTP host.
        //    SmtpClient client = new SmtpClient(args[0]);
        //    // Specify the e-mail sender.
        //    // Create a mailing address that includes a UTF8 character in the display name.
        //    MailAddress from = new MailAddress("troddick@usgs.gov",
        //       "SiGL " + (char)0xD8 + " Admin",
        //    System.Text.Encoding.UTF8);
        //    // Set destinations for the e-mail message.
        //    MailAddress to = new MailAddress("ben@contoso.com");
        //    // Specify the message content.
        //    MailMessage message = new MailMessage(from, to);
        //    message.Body = "This is a test e-mail message sent by an application. ";
        //    // Include some non-ASCII characters in body and subject.
        //    string someArrows = new string(new char[] { '\u2190', '\u2191', '\u2192', '\u2193' });
        //    message.Body += Environment.NewLine + someArrows;
        //    message.BodyEncoding = System.Text.Encoding.UTF8;
        //    message.Subject = "test message 1" + someArrows;
        //    message.SubjectEncoding = System.Text.Encoding.UTF8;
        //    // Set the method that is called back when the send operation ends.
        //    client.SendCompleted += new
        //    SendCompletedEventHandler(SendCompletedCallback);
        //    // The userState can be any object that allows your callback 
        //    // method to identify this send operation.
        //    // For this example, the userToken is a string constant.
        //    string userState = "test message1";
        //    client.SendAsync(message, userState);
        //    Console.WriteLine("Sending message... press c to cancel mail. Press any other key to exit.");
        //    string answer = Console.ReadLine();
        //    // If the user canceled the send, and mail hasn't been sent yet,
        //    // then cancel the pending operation.
        //    if (answer.StartsWith("c") && mailSent == false)
        //    {
        //        client.SendAsyncCancel();
        //    }
        //    // Clean up.
        //    message.Dispose();
        //    Console.WriteLine("Goodbye.");
        //}
        
        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(DATA_MANAGER aDataManager)
        {
            //Return BadRequest if missing required fields
            if (string.IsNullOrEmpty(aDataManager.USERNAME) ||
                string.IsNullOrEmpty(aDataManager.FNAME) ||
                string.IsNullOrEmpty(aDataManager.LNAME) ||
                string.IsNullOrEmpty(aDataManager.EMAIL))
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    
                    //Prior to running stored procedure, check if username exists
                    if (!Exists(aLaMPRDS.DATA_MANAGER, ref aDataManager))
                    {
                        //Check if USERNAME exists
                        if (aLaMPRDS.DATA_MANAGER.FirstOrDefault(m => String.Equals(m.USERNAME.ToUpper().Trim(), 
                                                                                    aDataManager.USERNAME.ToUpper().Trim())) != null) 
                            return new OperationResult.BadRequest { Description = "Username exists" };

                        // Create user profile using db stored procedure
                        // stored db throws errors internally but does not pass pass error

                        aLaMPRDS.OWNERPROFILE_ADD(aDataManager.USERNAME, buildDefaultPassword(aDataManager));
                        aLaMPRDS.OWNERPROFILE_ADDROLE(aDataManager.USERNAME, aDataManager.ROLE_ID);
                        aLaMPRDS.DATA_MANAGER.AddObject(aDataManager);
                        aLaMPRDS.SaveChanges();
                    }

                }//end using
            }//end using

            if (aDataManager != null)
                aDataManager.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);


            //Return OK instead of created, Flex incorrectly treats 201 as error
            return new OperationResult.OK { ResponseResource = aDataManager };
        }//end HttpMethod.POST
        

        [LaMPRequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddDataManager")]
        public OperationResult AddDataManager(string pass, DATA_MANAGER aDataManager)
        {
            DATA_MANAGER createdDataManager = new DATA_MANAGER();

            //Return BadRequest if missing required fields
            if ((string.IsNullOrEmpty(aDataManager.USERNAME) || aDataManager.ROLE_ID <= 0))
            { return new OperationResult.BadRequest(); }

            try
            {
                //Get basic authentication password
                using (EasySecureString securedPassword = GetSecuredPassword())
                {
                    using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                    {
                        if (!Exists(aLaMPRDS.DATA_MANAGER, ref aDataManager))
                        {
                            //Prior to running stored procedure, check if username exists
                            if (aLaMPRDS.DATA_MANAGER.FirstOrDefault(m => String.Equals(m.USERNAME.ToUpper().Trim(),
                                                                                aDataManager.USERNAME.ToUpper().Trim())) != null)
                                return new OperationResult.BadRequest { Description = "Username exists" };

                            // Create user profile using db stored procedure
                            aLaMPRDS.OWNERPROFILE_ADD(aDataManager.USERNAME, pass);
                            aLaMPRDS.OWNERPROFILE_ADDROLE(aDataManager.USERNAME, aDataManager.ROLE_ID);
                            aLaMPRDS.DATA_MANAGER.AddObject(aDataManager);
                            aLaMPRDS.SaveChanges();

                        }//end if
                        createdDataManager = aLaMPRDS.DATA_MANAGER.FirstOrDefault(m => String.Equals(m.USERNAME.ToUpper().Trim(),
                                                                                aDataManager.USERNAME.ToUpper().Trim()));
                    }//end using

                }//end using

                return new OperationResult.OK { ResponseResource = createdDataManager };
            }
            catch
            {
                return new OperationResult.BadRequest();
            }
        }//end HttpMethod.POST
       

        #endregion

        #region PutMethods
        /*****
         * Update entity object (single row) in the database by primary key
         * 
         * Returns: the updated table row entity object
         ****/
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 dataManagerId, DATA_MANAGER aDataManager)
        {
            //Return BadRequest if missing required fields
            if ((string.IsNullOrEmpty(aDataManager.USERNAME) || aDataManager.ROLE_ID <= 0))
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    DATA_MANAGER ObjectToBeUpdated = aLaMPRDS.DATA_MANAGER.SingleOrDefault(m => m.DATA_MANAGER_ID == dataManagerId);

                    //FirstName
                    ObjectToBeUpdated.FNAME = (string.IsNullOrEmpty(aDataManager.FNAME) ?
                        ObjectToBeUpdated.FNAME : aDataManager.FNAME);
                    //Last Name
                    ObjectToBeUpdated.LNAME = (string.IsNullOrEmpty(aDataManager.LNAME) ?
                        ObjectToBeUpdated.LNAME : aDataManager.LNAME);
                    //OrganizationId
                    ObjectToBeUpdated.ORGANIZATION_SYSTEM_ID = (int.Equals(aDataManager.ORGANIZATION_SYSTEM_ID, ObjectToBeUpdated.ORGANIZATION_SYSTEM_ID) ?
                        ObjectToBeUpdated.ORGANIZATION_SYSTEM_ID : aDataManager.ORGANIZATION_SYSTEM_ID);
                    //Phone
                    ObjectToBeUpdated.PHONE = (string.IsNullOrEmpty(aDataManager.PHONE) ?
                        ObjectToBeUpdated.PHONE : aDataManager.PHONE);
                    //Email
                    ObjectToBeUpdated.EMAIL = (string.IsNullOrEmpty(aDataManager.EMAIL) ?
                        ObjectToBeUpdated.EMAIL : aDataManager.EMAIL);
                   //USERNAME (doesn't change here)
                    ObjectToBeUpdated.USERNAME = (string.IsNullOrEmpty(aDataManager.USERNAME) ?
                        ObjectToBeUpdated.USERNAME : aDataManager.USERNAME);
                    //RoleId (doesn't change here)
                    ObjectToBeUpdated.ROLE_ID = (int.Equals(aDataManager.ROLE_ID, ObjectToBeUpdated.ROLE_ID) ?
                        ObjectToBeUpdated.ROLE_ID : aDataManager.ROLE_ID);

                    aLaMPRDS.SaveChanges();

                }//end using
            }//end using

            if (aDataManager != null)
                aDataManager.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aDataManager };

        }//end HttpMethod.PUT

        #endregion

        #region DeleteMethods
        /// 
        /// Force the user to provide authentication and authorization 
        ///
        [RequiresRole(AdminRole)]
        [HttpOperation(HttpMethod.DELETE)]
        public OperationResult Delete(Int32 dataManagerId)
        {
            DATA_MANAGER ObjectToBeDeleted = null;

            //Return BadRequest if missing required fields
            if (dataManagerId <= 0)
            {
                return new OperationResult.BadRequest();
            }


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {

                    // create user profile using db stored proceedure
                    try
                    {
                        //fetch the object to be updated (assuming that it exists)
                        ObjectToBeDeleted = aLaMPRDS.DATA_MANAGER.SingleOrDefault(
                                                m => m.DATA_MANAGER_ID == dataManagerId);

                        //Try to remove user profile first
                        aLaMPRDS.OWNERPROFILE_REMOVE(ObjectToBeDeleted.USERNAME);

                        //delete it
                        aLaMPRDS.DATA_MANAGER.DeleteObject(ObjectToBeDeleted);
                        aLaMPRDS.SaveChanges();
                        //Return object to verify persisitance

                        return new OperationResult.OK { };

                    }
                    catch (Exception)
                    {
                        //TODO: relay failure type message 
                        // EX. if profile failed to be removed
                        return new OperationResult.BadRequest();
                    }

                }// end using
            } //end using
        }//end HTTP.DELETE
        #endregion

        #endregion

        #region "Helper Methods"
        private Boolean Exists(System.Data.Objects.IObjectSet<DATA_MANAGER> dataManagers, ref DATA_MANAGER aDataManager)
        {
            DATA_MANAGER existingDataManager;
            DATA_MANAGER thisDataManager = aDataManager;
            //check if it exists
            try
            {
                existingDataManager = dataManagers.FirstOrDefault(m => string.Equals(m.USERNAME.ToUpper(), thisDataManager.USERNAME.ToUpper()) &&
                                                                            (string.Equals(m.LNAME.ToUpper(), thisDataManager.LNAME.ToUpper()) || string.IsNullOrEmpty(thisDataManager.LNAME)) &&
                                                                            (string.Equals(m.EMAIL.ToUpper(), thisDataManager.EMAIL.ToUpper()) || string.IsNullOrEmpty(thisDataManager.EMAIL)));

                if (existingDataManager == null)
                    return false;

                //if exists then update ref contact
                aDataManager = existingDataManager;
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }

        private string buildDefaultPassword(DATA_MANAGER dm)
        {
            //LaMPDefau1t+numbercharInlastname+first2letterFirstName
            return "LaMPDefau1t"+ dm.LNAME.Count() + dm.FNAME.Substring(0, 2);
        }//end buildDefaultPassword

        //private decimal GetNextManagerID(System.Data.Objects.IObjectSet<DATA_MANAGER> manager)
        //{
        //    decimal nextKey = 1;
        //    if (manager.Count() > 0)
        //    {
        //        nextKey = manager.OrderByDescending(m => m.DATA_MANAGER_ID).First().DATA_MANAGER_ID + 1;
        //    }

        //    return nextKey;
        //}
        #endregion
    }//end DataManagerHandler
}//end namespace