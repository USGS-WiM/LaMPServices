//------------------------------------------------------------------------------
//----- OrganizationHandler -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Handles Organizations resources through the HTTP uniform interface.
//              Equivalent to the controller in MVC.
//
//discussion:   Handlers are objects which handle all interaction with resources in 
//              this case the resources are POCO classes derived from the EF. 
//               https://github.com/openrasta/openrasta/wiki/Handlers
//
//     
#region Comments
// 04.02.13 - jkn - Created public user for get requests
// 04.26.12 - jkn - Created
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
    public class OrganizationHandler:HandlerBase
    {

        #region Routed Methods
        #region GetMethods
        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get()
        {
            List<ORGANIZATION> orgs;

                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {

                    orgs = aLaMPRDS.ORGANIZATIONS.OrderBy(o => o.ORGANIZATION_ID).ToList();

                }//end using

            if (orgs != null)
                orgs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            return new OperationResult.OK { ResponseResource = orgs };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName="GetUniqueOrgs")]
        public OperationResult getUniqueOrgs()
        {
            List<ORGANIZATION> orgs;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {
                List<ORGANIZATION> query = aLaMPRDS.ORGANIZATIONS.ToList();

                orgs = query.GroupBy(o => o.NAME).Select(or=>or.FirstOrDefault()).ToList();
                orgs = orgs.OrderBy(b => b.NAME).ToList();
            }//end using

            if (orgs != null)
                orgs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            return new OperationResult.OK { ResponseResource = orgs };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET)]
        public OperationResult Get(Int32 organizationId)
        {
            ORGANIZATION aOrg;

            //return BadRequest if ther is no ID
            if (organizationId <= 0)
            { return new OperationResult.BadRequest();}

                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {

                    aOrg = aLaMPRDS.ORGANIZATIONS.SingleOrDefault(o => o.ORGANIZATION_ID == organizationId);

                }//end using

            if (aOrg != null)
                aOrg.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aOrg };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetProjectOrganizations")]
        public OperationResult GetProjectOrganizations(Int32 projectId)
        {
            List<ORGANIZATION> orgs;

                using (LaMPDSEntities aLaMPRDS = GetRDS())
                {

                    orgs = aLaMPRDS.ORGANIZATIONS.Where(o=>o.PROJECT.Any(p=>p.PROJECT_ID== projectId))
                                                 .OrderBy(o=>o.ORGANIZATION_ID).ToList();

                }//end using
   
            if (orgs != null)
                orgs.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

            return new OperationResult.OK { ResponseResource = orgs };
        }//end httpMethod get

        [HttpOperation(HttpMethod.GET, ForUriName = "GetOrganizationByShortName")]
        public OperationResult GetOrganizationByShortName(string shortName)
        {
            ORGANIZATION aOrg;

            using (LaMPDSEntities aLaMPRDS = GetRDS())
            {

                aOrg = aLaMPRDS.ORGANIZATIONS.FirstOrDefault(o => string.Equals(o.SHORTNAME.ToUpper(),shortName.ToUpper()));

            }//end using

            if (aOrg != null)
                    aOrg.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            return new OperationResult.OK { ResponseResource = aOrg };
        }//end httpMethod get

        #endregion

        #region PostMethods
        /// 
        /// Force the user to provide authentication 
        /// 
        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST)]
        public OperationResult Post(ORGANIZATION aOrg)
        {
            //Return BadRequest if missing required fields
            if (String.IsNullOrEmpty(aOrg.NAME))
            {
                return new OperationResult.BadRequest();
            }

            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //check if it exists
                    if (!Exists(aLaMPRDS.ORGANIZATIONS,ref aOrg))
	                {
                        //each org needs to have an entry with just the name first, then add on divisions and sections as separate orgs
                        if (!string.IsNullOrEmpty(aOrg.DIVISION))
                        {
                            //first see if there is an Org already set up with just the name and no division yet
                            ORGANIZATION OrgExists = aLaMPRDS.ORGANIZATIONS.Where(x => x.NAME == aOrg.NAME && (string.IsNullOrEmpty(x.DIVISION) && string.IsNullOrEmpty(x.SECTION))).FirstOrDefault();
                            //if there's already an org with just the name, skip this and go right to posting the new name/div org
                            if (OrgExists == null)
                            {
                                //post org with name and empty division and empty section before posting with division
                                ORGANIZATION NameOrg = new ORGANIZATION();

                                NameOrg.NAME = aOrg.NAME;
                                NameOrg.SHORTNAME = GenerateShortName(aLaMPRDS.ORGANIZATIONS, aOrg.NAME, "", "");

                                aLaMPRDS.ORGANIZATIONS.AddObject(NameOrg);
                                aLaMPRDS.SaveChanges();
                            }
                        }
                        if (!string.IsNullOrEmpty(aOrg.SECTION))
                        {
                            //first see if there is an Org already set up with just the name and division but no section yet
                            ORGANIZATION OrgExists = aLaMPRDS.ORGANIZATIONS.Where(x => x.NAME == aOrg.NAME && x.DIVISION == aOrg.DIVISION && string.IsNullOrEmpty(x.SECTION)).FirstOrDefault();
                            //if there's already an org with name and division, skip this and go right to posting the new name/div/sec org
                            if (OrgExists == null)
                            {
                                //post org with name and division to have empty section before posting with section
                                ORGANIZATION DivOrg = new ORGANIZATION();
                                DivOrg.NAME = aOrg.NAME;
                                DivOrg.DIVISION = aOrg.DIVISION;

                                DivOrg.SHORTNAME = GenerateShortName(aLaMPRDS.ORGANIZATIONS, aOrg.NAME, aOrg.DIVISION, "");
                                aLaMPRDS.ORGANIZATIONS.AddObject(DivOrg);
                                aLaMPRDS.SaveChanges();
                            }
                        }
                        //generate new shortname
                        aOrg.SHORTNAME = GenerateShortName(aLaMPRDS.ORGANIZATIONS, aOrg.NAME, aOrg.DIVISION, aOrg.SECTION);
      
                        //Save
                        aLaMPRDS.ORGANIZATIONS.AddObject(aOrg);
                        aLaMPRDS.SaveChanges();
	                }
               
                }// end using
            }// end using

            if (aOrg != null)
                aOrg.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_individual);

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = aOrg };
        }

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.POST, ForUriName = "AddProjectOrganization")]
        public OperationResult AddProjectOrganization(Int32 projectId, ORGANIZATION aOrganization)
        {
            PROJECT_COOPERATORS aProjectCooperators = null;
            List<ORGANIZATION> CooperatorList = null;
            //Return BadRequest if missing required fields
            if (projectId <= 0 || String.IsNullOrEmpty(aOrganization.NAME))
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

                    if (!Exists(aLaMPRDS.ORGANIZATIONS, ref aOrganization))
                    {
                        //generate new shortname
                        aOrganization.SHORTNAME = GenerateShortName(aLaMPRDS.ORGANIZATIONS, aOrganization.NAME, aOrganization.DIVISION, aOrganization.SECTION);

                        aLaMPRDS.ORGANIZATIONS.AddObject(aOrganization);
                        aLaMPRDS.SaveChanges();
                    }//end if

                    //add to project

                    //first check if Project already contains organization
                    if (aLaMPRDS.PROJECT_COOPERATORS.FirstOrDefault(pc => pc.ORGANIZATION_ID == aOrganization.ORGANIZATION_ID &&
                                                                            pc.PROJECT_ID == projectId) == null)

                    {//create one
                        aProjectCooperators = new PROJECT_COOPERATORS();
                        //set ID and create ProjectContact
                        //aProjectCooperators.PROJECT_COOPERATOR_ID = GetNextProjectCooperatorID(aLaMPRDS.PROJECT_COOPERATORS);

                        aProjectCooperators.PROJECT_ID = projectId;
                        aProjectCooperators.ORGANIZATION_ID = aOrganization.ORGANIZATION_ID;

                        aLaMPRDS.PROJECT_COOPERATORS.AddObject(aProjectCooperators);
                        aLaMPRDS.SaveChanges();
                    }//end if

                    //return the list of contacts associated with project
                    CooperatorList = aLaMPRDS.ORGANIZATIONS.Where(o => o.PROJECT.Any(pc => pc.PROJECT_ID == projectId)).ToList();

                    if (CooperatorList != null)
                        CooperatorList.ForEach(x => x.LoadLinks(Context.ApplicationBaseUri.AbsoluteUri, linkType.e_group));

                }// end using

            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { ResponseResource = CooperatorList };
        }
        #endregion

        #region PutMethods

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.PUT)]
        public OperationResult Put(Int32 organizationId, ORGANIZATION instance)
        {
            //Return BadRequest if missing required fields
            if ((organizationId <= 0))
            {return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    ORGANIZATION ObjectToBeUpdated = aLaMPRDS.ORGANIZATIONS.SingleOrDefault(o => o.ORGANIZATION_ID == organizationId);
                    
                    //Name
                    ObjectToBeUpdated.NAME = (string.IsNullOrEmpty(instance.NAME) ?
                        ObjectToBeUpdated.NAME : instance.NAME);
           
                    //Division
                    ObjectToBeUpdated.DIVISION = (string.IsNullOrEmpty(instance.DIVISION) ?
                        ObjectToBeUpdated.DIVISION : instance.DIVISION);

                    //Section
                    ObjectToBeUpdated.SECTION = (string.IsNullOrEmpty(instance.SECTION) ?
                        ObjectToBeUpdated.SECTION : instance.SECTION);

                    //generate new shortname
                    ObjectToBeUpdated.SHORTNAME = GenerateShortName(aLaMPRDS.ORGANIZATIONS,ObjectToBeUpdated.NAME, ObjectToBeUpdated.DIVISION, ObjectToBeUpdated.SECTION);

                    

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
        public OperationResult Delete(Int32 organizationId)
        {
            //Return BadRequest if missing required fields
            if (organizationId <= 0)
            { return new OperationResult.BadRequest();}


            //Get basic authentication password
            using (EasySecureString securedPassword = GetSecuredPassword())
            {
                using (LaMPDSEntities aLaMPRDS = GetRDS(securedPassword))
                {
                    //fetch the object to be updated (assuming that it exists)
                    ORGANIZATION ObjectToBeDeleted = aLaMPRDS.ORGANIZATIONS.SingleOrDefault(o => o.ORGANIZATION_ID == organizationId);
                    //delete it
                    aLaMPRDS.ORGANIZATIONS.DeleteObject(ObjectToBeDeleted);

                    aLaMPRDS.SaveChanges();

                }// end using
            }// end using

            //Return object to verify persisitance
            return new OperationResult.OK { };
        }

        [LaMPRequiresRole(new string[] { AdminRole, ManagerRole })]
        [HttpOperation(HttpMethod.DELETE, ForUriName = "RemoveProjectOrganization")]
        public OperationResult RemoveProjectOrganization(Int32 projectId, ORGANIZATION aOrganization)
        {
            //Return BadRequest if missing required fields
            if (projectId <= 0 || String.IsNullOrEmpty(aOrganization.NAME))
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
                    PROJECT_COOPERATORS thisProjCoop = aLaMPRDS.PROJECT_COOPERATORS.FirstOrDefault(pc => pc.ORGANIZATION_ID == aOrganization.ORGANIZATION_ID &&
                                                                            pc.PROJECT_ID == projectId);

                    if (thisProjCoop != null)
                    {//remove it
                        
                        aLaMPRDS.PROJECT_COOPERATORS.DeleteObject(thisProjCoop);
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

        private Boolean Exists(System.Data.Objects.IObjectSet<ORGANIZATION> organizations, ref ORGANIZATION aOrganization)
        {
            List<ORGANIZATION> existingOrganization;
            ORGANIZATION thisOrganization = aOrganization;
            
            //check if it exists
            try
            {
                //get all the Orgs where the name matches                
                existingOrganization = organizations.Where(o => string.Equals(o.NAME.ToUpper(), thisOrganization.NAME.ToUpper())).ToList();

                if (existingOrganization != null)
                {
                    if (existingOrganization.Count >= 1)
                    {
                        //if there's no division, remove all those with a division or compare division name
                        if (!string.IsNullOrEmpty(aOrganization.DIVISION))
                            existingOrganization = existingOrganization.Where(o => !string.IsNullOrEmpty(o.DIVISION) && string.Equals(o.DIVISION.ToUpper(), thisOrganization.DIVISION.ToUpper())).ToList();
                        else
                            existingOrganization = existingOrganization.Where(o => string.IsNullOrEmpty(o.DIVISION)).ToList();

                        //if there's no section, remove all those with a section or compare section name
                        if (!string.IsNullOrEmpty(aOrganization.SECTION))
                            existingOrganization = existingOrganization.Where(o => !string.IsNullOrEmpty(o.SECTION) && string.Equals(o.SECTION.ToUpper(), thisOrganization.SECTION.ToUpper())).ToList();
                        else
                            existingOrganization = existingOrganization.Where(o => string.IsNullOrEmpty(o.SECTION)).ToList();
                    }
                }
                
                if (existingOrganization == null || existingOrganization.Count == 0)
                    return false;

                //if exists then update ref contact
                aOrganization = existingOrganization.FirstOrDefault();
                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }//end Exists

        private Boolean ShortNameExists(System.Data.Objects.IObjectSet<ORGANIZATION> organizations, string shortName)
        {
            ORGANIZATION existingOrganization;
            //check if it exists
            try
            {
                existingOrganization = organizations.FirstOrDefault(o => string.Equals(o.SHORTNAME.ToUpper(), shortName.ToUpper()));

                if (existingOrganization == null)
                    return false;

                return true;

            }
            catch (Exception)
            {
                return false;
            }
        }//end Exists

        private string GenerateShortName(System.Data.Objects.IObjectSet<ORGANIZATION> organizations, string orgName, string orgDiv, string orgSec)
        {
            string shrtNm = string.Empty;
            string shrtDiv = string.Empty;
            string shrtSec = string.Empty;
            string uniqueOrgabbr = string.Empty;
            //generate new shortname
            int incr = 0;
            do
            {
                incr++;
          
                if(! string.IsNullOrEmpty(orgName))
                    shrtNm = BuildAcronym(orgName, incr);

                if(!string.IsNullOrEmpty(orgDiv))
                    shrtDiv = "_" + BuildAcronym(orgDiv, incr);

                if(!string.IsNullOrEmpty(orgSec))
                    shrtSec = "_" + BuildAcronym(orgSec, incr);

                uniqueOrgabbr = shrtNm + shrtDiv + shrtSec;

            } while (ShortNameExists(organizations, uniqueOrgabbr));

            return uniqueOrgabbr;
        }//end GenerateShortName

        public static string FormatOrgString(ORGANIZATION org)
        {
            string ContactOrg = string.Empty;
            if (!string.IsNullOrEmpty(org.SECTION))
            {
                //there is a section
                ContactOrg = org.NAME + ", " + org.DIVISION + ", " + org.SECTION;
            }
            else if (!string.IsNullOrEmpty(org.DIVISION))
            {
                ContactOrg = org.NAME + ", " + org.DIVISION;
            }
            else
            {
                ContactOrg = org.NAME;
            }
            return ContactOrg;
        }
        
        private string BuildAcronym(string str, Int32 wrdLgth)
        {
            string acr = string.Empty;
            char[] delimiterChars = { ' ', ',', '.', ':', '\t' };
            try
            {
                string[] words = str.Split(delimiterChars);

                foreach (string wrd in words)
                {
                    string result = string.Empty;
                    if (ExcludeWord(wrd) || string.IsNullOrEmpty(wrd)) continue; //next s

                    if (wrd.First().Equals("(") && wrd.Last().Equals(")"))
                        result = wrd;
                    else
                        result = wrd.Length < wrdLgth ? wrd : wrd.Substring(0, wrdLgth);
                    
                    //add to acr
                    acr = acr + ToCapital(result);
                }//next

                return acr;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        
        }//end GetAcronym

        private Boolean ExcludeWord(string s)
        {
            switch (s.ToUpper())
            {
                case "THE":
                case "OF":
                case "AND":
                case "&":
                   return true;

                default:
                    return false;
            }//end switch
          }//end exlude
        #endregion
    }//end OrganizationHandler
}//end namespace