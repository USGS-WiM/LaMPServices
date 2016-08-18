﻿//------------------------------------------------------------------------------
//----- Configuration -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Kentucky Water Science Center
//              
//  
//   purpose:   Configuration implements the IConfiurationSource interface. OpenRasta
//              will call the Configure method and use it to configure the application 
//              through a fluent interface using the Resource space as root objects. 
//
//discussion:   The ResourceSpace is where you can define the resources in the application and what
//              handles them and how thy are represented. 
//              https://github.com/openrasta/openrasta/wiki/Configuration
//
//     
#region Comments
// 04.15.12 - jkn - Created
#endregion                          
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Objects.DataClasses;

using OpenRasta.Authentication;
using OpenRasta.Authentication.Basic;
using OpenRasta.Codecs;
using OpenRasta.Codecs.WebForms;
using OpenRasta.Configuration;
using OpenRasta.DI;
using OpenRasta.IO;
using OpenRasta.Pipeline.Contributors;
using OpenRasta.Web.UriDecorators;

using LaMPServices.Handlers;
using LaMPServices.Codecs;
using LaMPServices.Resources;
using LaMPServices.Authentication;
using LaMPServices.PipeLineContributors;


namespace LaMPServices
{
    public class Configuration:IConfigurationSource
    {
        public void Configure()
        {
            using (OpenRastaConfiguration.Manual)
            {
                // specify the authentication scheme you want to use, you can register multiple ones
                ResourceSpace.Uses.CustomDependency<IAuthenticationScheme, BasicAuthenticationScheme>(DependencyLifetime.Singleton);

                // register your basic authenticator in the DI resolver
                ResourceSpace.Uses.CustomDependency<IBasicAuthenticator, LaMPBasicAuthentication>(DependencyLifetime.Transient);

                // Allow codec choice by extension 
                ResourceSpace.Uses.UriDecorator<ContentTypeExtensionUriDecorator>();
                ResourceSpace.Uses.PipelineContributor<ErrorCheckingContributor>();
                ResourceSpace.Uses.PipelineContributor<CrossDomainPipelineContributor>();
                
                AddAUTHENTICATION_Resources();
                AddCONTACT_Resources();
                AddDATA_MANAGER_Resources();
                AddDATAHOST_Resources();
                AddDIVISION_Resources();
                AddFREQUENCY_Resources();
                AddKEYWORD_Resources();
                AddLAKE_Resources();
                AddMEDIA_Resources();
                AddOBJECTIVE_Resources();
                AddORGANIZATION_Resources();
                AddOrganizationResource_Resources();
                AddORGANIZATION_SYSTEM_Resources();
                AddPARAMETER_Resources();
                AddPROJ_STATUS_Resources();
                AddPROJ_DURATION_Resources();
                AddPROJECT_Resources();
                AddPUBLICATION_Resources();
                AddRESOURCE_Resources();
                AddROLE_Resources();
                AddSECTION_Resources();
                AddSITE_Resources();
                AddSTATE_Resources();
                AddSTATUS_Resources();

            }//End using OpenRastaConfiguration.Manual


        }//End Configure()

        #region Helper methods
            
        
        private void AddAUTHENTICATION_Resources()
        {
            //Authentication
            ResourceSpace.Has.ResourcesOfType<Boolean>()
            .AtUri("/login")
            .HandledBy<LoginHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");  

        }//end AddAUTHENTICATION_Resources
        
        private void AddCONTACT_Resources()
        {
            //Contacts
            ResourceSpace.Has.ResourcesOfType<List<CONTACT>>()
                .AtUri("/contacts")
                .And.AtUri("/projects/{projectId}/addContact").Named("AddProjectContact")
                .And.AtUri("/projects/{projectId}/contacts").Named("GetProjectContacts")
                .And.AtUri("/OrganizationSystems/{orgSysId}/contacts").Named("GetOrgSysContacts")
                .HandledBy<ContactHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<CONTACT>()
                .AtUri("/contacts/{contactId}")
                .And.AtUri("/projects/{projectId}/RemoveProjectContact/{contactId}").Named("RemoveProjectContact")
                .HandledBy<ContactHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddCONTACT_Resources
        
        private void AddDATAHOST_Resources()
        {
            //DATA_HOSTS
            ResourceSpace.Has.ResourcesOfType<List<DATA_HOST>>()
               .AtUri("/datahosts")
               .And.AtUri("/projects/{projectId}/dataHosts").Named("GetProjectDataHosts")
                .And.AtUri("/projects/{projectId}/addDataHost").Named("AddProjectDataHost")
               .HandledBy<DataHostHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<DATA_HOST>()
                .AtUri("/datahosts/{entityId}")
                .And.AtUri("/projects/{projectId}/RemoveProjectDataHost/{dataHostId}").Named("RemoveProjectDataHost")
                .HandledBy<DataHostHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddDATAHOST_Resources
        
        private void AddDATA_MANAGER_Resources()
        {
            //DATA_MANAGER
            ResourceSpace.Has.ResourcesOfType<List<DATA_MANAGER>>()
                .AtUri("/dataManagers")
                .HandledBy<DataManagerHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<DATA_MANAGER>()
                .AtUri("/dataManagers/{dataManagerId}")
                .And.AtUri("dataManagers/{pass}/addDataManager").Named("AddDataManager")
                .And.AtUri("dataManagers/{email}/resetPassword").Named("ResetPassword")
                .And.AtUri("/dataManagers?username={userName}").Named("GetByUserName")
                .And.AtUri("/dataManagers?username={userName}&newP={newPassword}").Named("ChangeDataManagerPassword")
                .And.AtUri("/projects/{projectId}/dataManager").Named("GetProjectDataManager")
                .HandledBy<DataManagerHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            //DM_LIST_VIEW
            ResourceSpace.Has.ResourcesOfType<List<DM_LIST_VIEW>>()
                .AtUri("/dataManagers/DMList").Named("GetDMListModel")
                .HandledBy<DataManagerHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddDATA_MANAGER_Resources

        private void AddDIVISION_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<DIVISION>>()
            .AtUri("/Divisions").Named("getAllDivisions")
            .HandledBy<DivisionHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<DIVISION>()
            .AtUri("/Divisions/{divID}").Named("GetThisDivision")
            .HandledBy<DivisionHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");            
        }//end AddDIVISION_Resources

        private void AddFREQUENCY_Resources()
        {
            //FREQUENCY_TYPE
            ResourceSpace.Has.ResourcesOfType<List<FREQUENCY_TYPE>>()
               .AtUri("/frequencies")
               .And.AtUri("/sites/{siteId}/addFrequency").Named("AddSiteFrequency")
               .And.AtUri("/sites/{siteId}/frequencies").Named("GetSiteFrequencies")
               .HandledBy<FrequencyHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<FREQUENCY_TYPE>()
                .AtUri("/frequencies/{frequencyId}")
                .And.AtUri("/sites/{siteId}/removeFrequency/{frequencyId}").Named("RemoveSiteFrequency")
                .HandledBy<FrequencyHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end addFREQUENCY_TYPE_Resources
        
        private void AddKEYWORD_Resources()
        {
            //KEYWORDS
            ResourceSpace.Has.ResourcesOfType<List<KEYWORD>>()
                .AtUri("/keywords")
                .And.AtUri("/projects/{projectId}/addKeyword").Named("AddProjectKeyword")
                .And.AtUri("/projects/{projectId}/keywords").Named("GetProjectKeyword")
                .HandledBy<KeywordHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<KEYWORD>()
                .AtUri("/keywords/{keywordId}")
                .And.AtUri("/projects/{projectId}/removeKeyword/{keywordId}").Named("RemoveProjectKeyword")
                .And.AtUri("/keywords?term={term}").Named("GetKeywordByTerm")
                .HandledBy<KeywordHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddKEYWORD_Resources
        
        private void AddLAKE_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<LAKE_TYPE>>()
               .AtUri("/lakes")
               .HandledBy<LakeHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<LAKE_TYPE>()
                .AtUri("/lakes/{lakeId}")
                .And.AtUri("/sites/{siteId}/lake").Named("GetSiteLake")
                .HandledBy<LakeHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddLAKE_Resources
        
        private void AddMEDIA_Resources()
        {
            //MEDIA_TYPE
            ResourceSpace.Has.ResourcesOfType<List<MEDIA_TYPE>>()
               .AtUri("/media")
               .And.AtUri("/sites/{siteId}/addMedium").Named("AddSiteMedium")
               .And.AtUri("/sites/{siteId}/media").Named("GetSiteMedia")
               .HandledBy<MediaHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<MEDIA_TYPE>()
                .AtUri("/media/{mediaId}")
                .And.AtUri("/sites/{siteId}/removeMedium/{mediaId}").Named("RemoveSiteMedium")
                .HandledBy<MediaHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end addMEDIA_Resources
        
        private void AddOBJECTIVE_Resources()
        {
            //PUBLICATIONS
            ResourceSpace.Has.ResourcesOfType<List<OBJECTIVE_TYPE>>()
               .AtUri("/objectives")
               .And.AtUri("/projects/{projectId}/objectives").Named("GetProjectObjectives")
               .And.AtUri("/projects/{projectId}/addObjective").Named("AddProjectObjective")
               .HandledBy<ObjectiveHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<OBJECTIVE_TYPE>()
                .AtUri("/objectives/{objectiveId}")
                //.And.AtUri("/projects/{projectId}/removeObjective").Named("RemoveProjectObjective")
                .And.AtUri("/projects/{projectId}/removeObjective/{objectiveId}").Named("RemoveProjectObjective")
                .HandledBy<ObjectiveHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddOBJECTIVE_Resources
        
        private void AddORGANIZATION_Resources()
        {
            //ORGANIZATION
            ResourceSpace.Has.ResourcesOfType<List<ORGANIZATION>>()
            .AtUri("/Organizations").Named("GetAllOrgs")
            .HandledBy<OrganizationHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<ORGANIZATION>()
            .AtUri("/Organizations").Named("PostOrganization")
            .And.AtUri("/Organizations/{organizationId}").Named("GetThisOrganization")
            .HandledBy<OrganizationHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddORGANIZATION_Resources

        private void AddOrganizationResource_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<OrganizationResource>>()
            .AtUri("/OrgResources").Named("AllOrgResources")
            .And.AtUri("/projects/{projectId}/organizations").Named("GetProjectOrganizations")
            .And.AtUri("/projects/{projectId}/AddOrganizationSystem?organizationId={orgId}&divisionId={divId}&sectionId={secId}").Named("AddProjectOrgSystem")////
            .HandledBy<OrganizationResourceHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<OrganizationResource>()
            .AtUri("/OrgResources/{orgSysId}").Named("GetThisOrgSystem")
            .HandledBy<OrganizationResourceHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddOrganizationResource_Resources

        private void AddORGANIZATION_SYSTEM_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<ORGANIZATION_SYSTEM>>()
            .AtUri("/OrganizationSystems").Named("GetAllOrgSys")
            .HandledBy<OrganizationSystemHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<ORGANIZATION_SYSTEM>()
            .AtUri("/OrganizationSystems/{orgSysID}").Named("GetThisOrgSys")
            .And.AtUri("/projects/{projectId}/RemoveOrganization?orgId={orgSysId}").Named("RemoveProjectOrganization")
            .HandledBy<OrganizationSystemHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddORGANIZATION_SYSTEM_Resources

        private void AddPARAMETER_Resources()
        {
            //PARAMETER_TYPE

            ResourceSpace.Has.ResourcesOfType<List<PARAMETER_TYPE>>()
               .AtUri("/parameters")
               .And.AtUri("/GetParameterGroups").Named("GetParameterGroups")
               .And.AtUri("/sites/{siteId}/addParameter").Named("AddSiteParameter")
               .And.AtUri("/sites/{siteId}/parameters").Named("GetSiteParameters")
               .HandledBy<ParameterHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");   

            ResourceSpace.Has.ResourcesOfType<ParameterGroups>()
                .AtUri("/parameters?groupnames={groupNames}").Named("GetParametersByGroupName")
                .HandledBy<ParameterHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");   

            ResourceSpace.Has.ResourcesOfType<PARAMETER_TYPE>()
                .AtUri("/parameters/{parameterId}")
                .And.AtUri("/sites/{siteId}/removeParameter/{parameterId}").Named("RemoveSiteParameter")
                .HandledBy<ParameterHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");  

        }//end addPARAMETER_Resources
        
        private void AddPROJ_STATUS_Resources()
        {
            //PROJ_STATUS
            ResourceSpace.Has.ResourcesOfType<List<PROJ_STATUS>>()
               .AtUri("/ProjectStatus")              
               .HandledBy<ProjStatusHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<PROJ_STATUS>()
                .AtUri("/ProjectStatus/{entityId}") 
                .And.AtUri("/Projects/{projectId}/projStatus").Named("GetProjectStatus")
                .HandledBy<ProjStatusHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddPROJ_STATUS_Resources

        private void AddPROJ_DURATION_Resources()
        {
            //PROJ_DURATION
            ResourceSpace.Has.ResourcesOfType<List<PROJ_DURATION>>()
               .AtUri("/ProjectDuration")               
               .HandledBy<ProjDurationHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<PROJ_DURATION>()
                .AtUri("/ProjectDuration/{entityId}")
                .And.AtUri("/Projects/{projectId}/projDuration").Named("GetProjectDuration")
                .HandledBy<ProjDurationHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddPROJ_DURATION_Resources

        private void AddPROJECT_Resources()
        {
            //PROJECT
            ResourceSpace.Has.ResourcesOfType<List<PROJECT>>()
               .AtUri("/allProjects").Named("GetAllProjects")
               .And.AtUri("/contacts/{contactId}/projects").Named("GetContactProjects")
               .And.AtUri("/dataManagers/{dataManagerId}/projects").Named("GetDataManagersProjects")
               .And.AtUri("/keywords/{keywordId}/projects").Named("GetKeyWordProjects") 
               .And.AtUri("/publications/{publicationId}/projects").Named("GetPublicationProjects")
               .And.AtUri("/objectives/{objectiveId}/projects").Named("GetObjectiveProjects")
               .And.AtUri("/frequency/{frequencyId}/projects").Named("GetFreqSiteProjects")
               .And.AtUri("/lakes/{lakeId}/projects").Named("GetLakeSiteProjects")
               .And.AtUri("/media/{mediaId}/projects").Named("GetMediaSiteProjects")
               .And.AtUri("/parameters/{parameterId}/projects").Named("GetParameterSiteProjects")
               .And.AtUri("/resourcetypes/{resourceId}/projects").Named("GetResourceSiteProjects")
               .And.AtUri("/status/{statusId}/projects").Named("GetSiteStatusProjects")
               .And.AtUri("/OrganizationSystems/{orgSystemId}/projects").Named("GetOrgSysProject")
               .And.AtUri("/ProjectDuration/{durationId}/projects").Named("GetProjectDurationProjects")
               .And.AtUri("/ProjectStatus/{statusId}/projects").Named("GetProjectStatusProjects")
               .And.AtUri("/Projects?FlaggedProject={flag}").Named("GetFlaggedProjects")
               .And.AtUri("/projects?TotalCounts").Named("GetCounts") //no endpoint for this one
               .And.AtUri("/project?contactname={contactFname}").Named("") //no endpoint for this one
               .And.AtUri("/projects/{dataManagerId}/DeleteAllDMProjects").Named("DeleteAllDMProjects")
               .HandledBy<ProjectHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<List<ProjectRes>>()
                .AtUri("/projects/IndexProjects?DataManager={dataManagerID}").Named("GetIndexProjects")
                .HandledBy<ProjectHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
           
            ResourceSpace.Has.ResourcesOfType<FullProject>()
                .AtUri("/projects/GetFullProject/{scienceBaseId}").Named("GetFullProject")
                .HandledBy<ProjectHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<PROJECT>()
                .AtUri("/projects/{projectId}") 
                .And.AtUri("/projects/{projectId}/ReassignProject?DataManager={dataManagerId}").Named("ReassignProject") // should be regular put with data_manager_id already changed b4 sending
                .And.AtUri("/sites/{siteId}/project").Named("GetSiteProject")
                .And.AtUri("/dataHosts/{dataHostId}/projects").Named("GetDataHostProject")
                .And.AtUri("/fundSource/{fundSourceId}/projects").Named("GetFundSourceProject") //no endpoint for this one
                .HandledBy<ProjectHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddPROJECT_Resources
        
        private void AddPUBLICATION_Resources()
        {
            //PUBLICATIONS
            ResourceSpace.Has.ResourcesOfType<List<PUBLICATION>>()
               .AtUri("/publications")
               .And.AtUri("/projects/{projectId}/publications").Named("GetProjectPublications")
               .And.AtUri("/projects/{projectId}/addPublication").Named("AddProjectPublication")
               .HandledBy<PublicationHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<PUBLICATION>()
                .AtUri("/publications/{publicationId}")
                .And.AtUri("/projects/{projectId}/RemoveProjectPublication/{publicationId}").Named("RemoveProjectPublication")
                .HandledBy<PublicationHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddPUBLICATION_Resources
        
        private void AddRESOURCE_Resources()
        {
            //RESOURCE_TYPE
            ResourceSpace.Has.ResourcesOfType<List<RESOURCE_TYPE>>()
               .AtUri("/resourcetypes")
               .And.AtUri("/sites/{siteId}/addResourcetype").Named("AddSiteResourceType")
               .And.AtUri("/sites/{siteId}/resourcetypes").Named("GetSiteResourceTypes")
               .HandledBy<ResourceHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<RESOURCE_TYPE>()
                .AtUri("/resourcetypes/{resourceTypeId}")
               .And.AtUri("/sites/{siteId}/removeResourcetype/{resourceTypeId}").Named("RemoveSiteResourceType")
                .HandledBy<ResourceHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddRESOURCE_Resources
        
        private void AddROLE_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<ROLE>>()
               .AtUri("/roles")
               .HandledBy<RoleHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<ROLE>()
                .AtUri("/roles/{roleId}")
                .And.AtUri("dataManagers/{dataManagerId}/role").Named("GetDataManagersRole")
                .HandledBy<RoleHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddRole_Resources

        private void AddSECTION_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<SECTION>>()
            .AtUri("/Sections")
            .HandledBy<SectionHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<SECTION>()
            .AtUri("/Sections/{secID}")
            .HandledBy<SectionHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddSECTION_Resources

        private void AddSITE_Resources()
        {
            //SITE
            ResourceSpace.Has.ResourcesOfType<List<SITE>>()
               .AtUri("/sites")
               .And.AtUri("/projects/{projectId}/sites").Named("GetProjectSites")
               .And.AtUri("/lakes/{lakeId}/sites").Named("GetLakeSites")
               .And.AtUri("/status/{statusId}/sites").Named("GetStatusSites")
               .And.AtUri("/media/{mediaId}/sites").Named("GetMediaSites")
               .And.AtUri("/resourcetypes/{resourceTypeId}/sites").Named("GetResourceSites")
               .And.AtUri("/parameters/{parameterId}/sites").Named("GetParameterSites")
               .And.AtUri("/frequencies/{frequencyId}/sites").Named("GetFrequencySites")
               .And.AtUri("sites?Parameters={parameters}&FromDate={fromDate}&ToDate={toDate}&Duration={durationIDs}&Status={statusIDs}&ResComp={resComps}&Media={media}&Lake={lakes}&State={states}").Named("GetFilteredSites")
               .And.AtUri("sites/ProjectSites?ProjOrg={orgID}&ProjObjs={objIDs}&Duration={durationIDs}&Status={statusIDs}&StartDate={fromDate}&EndDate={toDate}&Lake={lakes}&State={states}").Named("GetFilteredProjectSites")
               .HandledBy<SiteHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<List<SITE_LIST_VIEW>>()
            .AtUri("/sites/siteView").Named("GetSitesView")
                .HandledBy<SiteHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<SITE>()
                .AtUri("/sites/{siteId}")
                .HandledBy<SiteHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<SiteMapModel>()
                .AtUri("/Sites/SiteInformation?siteId={siteId}").Named("GetSiteInformation")
                .HandledBy<SiteHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<List<SiteMapModel>>()
                .AtUri("/Sites/ProjectSitesInfo?ProjectId={projectId}").Named("GetProjectSitesInfo")
                .And.AtUri("/Sites/FullSiteInfo/{projectId}").Named("GetFullSitesInfo")
                .HandledBy<SiteHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddSITE_Resources
        
        private void AddSTATUS_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<STATUS_TYPE>>()
               .AtUri("/status")
               .HandledBy<StatusHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<STATUS_TYPE>()
                .AtUri("/status/{statusId}")
                .And.AtUri("/sites/{siteId}/status").Named("GetSiteStatus")
                .HandledBy<StatusHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");
        }//end AddStatus_Resources
        
        private void AddSTATE_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<string>>()
               .AtUri("/states").Named("GetSiteStates")
               .HandledBy<StateHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

        }//end AddState_Resources
        
        #endregion


    }//End class Configuration
}//End namespace