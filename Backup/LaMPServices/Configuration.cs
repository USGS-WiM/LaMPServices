//------------------------------------------------------------------------------
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
                AddFREQUENCY_Resources();
                AddKEYWORD_Resources();
                AddLAKE_Resources();
                AddMEDIA_Resources();
                AddOBJECTIVE_Resources();
                AddORGANIZATION_Resources();
                AddPARAMETER_Resources();
                AddPROJECT_Resources();
                AddPUBLICATION_Resources();
                AddRESOURCE_Resources();
                AddROLE_Resources();
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
                .And.AtUri("/organizations/{organizationId}/contacts").Named("GetOrganizationContacts")
                .HandledBy<ContactHandler>()
            .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
            .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<CONTACT>()
                .AtUri("/contacts/{contactId}")
                .And.AtUri("/projects/{projectId}/RemoveProjectContact").Named("RemoveProjectContact")
                .HandledBy<ContactHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

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
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<DATA_HOST>()
                .AtUri("/datahosts/{dataHostId}")
                .And.AtUri("/projects/{projectId}/RemoveProjectDataHost").Named("RemoveProjectDataHost")
                .HandledBy<DataHostHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");
        }//end AddDATAHOST_Resources
        
        private void AddDATA_MANAGER_Resources()
        {
            //DATA_MANAGER
            ResourceSpace.Has.ResourcesOfType<List<DATA_MANAGER>>()
                .AtUri("/dataManagers")
                .HandledBy<DataManagerHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<DATA_MANAGER>()
                .AtUri("/dataManagers/{dataManagerId}")
                .And.AtUri("dataManagers/{pass}/addDataManager").Named("AddDataManager")
                .And.AtUri("/dataManagers?username={userName}").Named("GetByUserName")
                .And.AtUri("/dataManagers?username={userName}&new={newPassword}").Named("ChangeDataManagerPassword")
                .And.AtUri("/projects/{projectId}/dataManager").Named("GetProjectDataManager")
                .HandledBy<DataManagerHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");
        }//end AddDATA_MANAGER_Resources
        
        private void AddFREQUENCY_Resources()
        {
            //FREQUENCY_TYPE
            ResourceSpace.Has.ResourcesOfType<List<FREQUENCY_TYPE>>()
               .AtUri("/frequencies")
               .And.AtUri("/sites/{siteId}/addFrequency").Named("AddSiteFrequency")
               .And.AtUri("/sites/{siteId}/frequencies").Named("GetSiteFrequencies")
               .HandledBy<FrequencyHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<FREQUENCY_TYPE>()
                .AtUri("/frequencies/{frequencyId}")
                .And.AtUri("/sites/{siteId}/removeFrequency").Named("RemoveSiteFrequency")
                .HandledBy<FrequencyHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

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
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<KEYWORD>()
                .AtUri("/keywords/{keywordId}")
                .And.AtUri("/projects/{projectId}/removeKeyword").Named("RemoveProjectKeyword")
                .And.AtUri("/keywords?term={term}").Named("GetKeywordByTerm")
                .HandledBy<KeywordHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

        }//end AddKEYWORD_Resources
        
        private void AddLAKE_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<LAKE_TYPE>>()
               .AtUri("/lakes")
               .HandledBy<LakeHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<LAKE_TYPE>()
                .AtUri("/lakes/{lakeId}")
                .And.AtUri("/sites/{siteId}/lake").Named("GetSiteLake")
                .HandledBy<LakeHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");
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
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<MEDIA_TYPE>()
                .AtUri("/media/{mediaTypeId}")
                .And.AtUri("/sites/{siteId}/removeMedium").Named("RemoveSiteMedium")
                .HandledBy<MediaHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

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
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<OBJECTIVE_TYPE>()
                .AtUri("/objectives/{objectiveId}")
                .And.AtUri("/projects/{projectId}/removeObjective").Named("RemoveProjectObjective")
                .HandledBy<ObjectiveHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");
        }//end AddOBJECTIVE_Resources
        
        private void AddORGANIZATION_Resources()
        {
            //ORGANIZATION
            ResourceSpace.Has.ResourcesOfType<List<ORGANIZATION>>()
               .AtUri("/organizations")
               .And.AtUri("/getUniqueOrgs").Named("GetUniqueOrgs")
               .And.AtUri("/projects/{projectId}/AddOrganization").Named("AddProjectOrganization")
               .And.AtUri("/projects/{projectId}/organizations").Named("GetProjectOrganizations")
               .HandledBy<OrganizationHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<ORGANIZATION>()
                .AtUri("/organizations/{organizationId}")
                .And.AtUri("/projects/{projectId}/RemoveOrganization").Named("RemoveProjectOrganization")
                .And.AtUri("/organizations?shortname={shortName}").Named("GetOrganizationByShortName")
                .HandledBy<OrganizationHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");
        }//end AddORGANIZATION_Resources
        
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
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");   

            ResourceSpace.Has.ResourcesOfType<ParameterGroups>()
                .AtUri("/parameters?groupnames={groupNames}").Named("GetParametersByGroupName")
                .HandledBy<ParameterHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");   

            ResourceSpace.Has.ResourcesOfType<PARAMETER_TYPE>()
                .AtUri("/parameters/{parameterId}")
                .And.AtUri("/sites/{siteId}/removeParameter").Named("RemoveSiteParameter")
                .HandledBy<ParameterHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");  

        }//end addPARAMETER_Resources
        
        private void AddPROJECT_Resources()
        {
            //PROJECT
            ResourceSpace.Has.ResourcesOfType<List<PROJECT>>()
               .AtUri("/projects")
               //.And.AtUri("/projects/managed").Named("GetManagedProjects")
               .And.AtUri("/contacts/{contactId}/projects").Named("GetContactProjects")
               .And.AtUri("/dataManagers/{dataManagerId}/projects").Named("GetDataManagersProjects")
               .And.AtUri("/keywords/{keywordId}/projects").Named("GetKeyWordProjects")
               .And.AtUri("/publications/{publicationId}/projects").Named("GetPublicationProjects")
               .And.AtUri("/objectives/{objectiveId}/projects").Named("GetObjectiveProjects")
               .And.AtUri("/organizations/{organizationId}/projects").Named("GetOrganizationProject")
               .And.AtUri("/project?contactname={contactFname}").Named("")
               .And.AtUri("/projects/{dataManagerId}/DeleteAllDMProjects").Named("DeleteAllDMProjects")
               .HandledBy<ProjectHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<List<ProjectRes>>()
                .AtUri("/projects/IndexProjects").Named("GetIndexProjects")
                .HandledBy<ProjectHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<FullProject>()
                .AtUri("/projects/GetFullProject/{scienceBaseId}").Named("GetFullProject")
                .HandledBy<ProjectHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1").ForExtension("xml")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9").ForExtension("json");

            ResourceSpace.Has.ResourcesOfType<PROJECT>()
                .AtUri("/projects/{projectId}")
                .And.AtUri("/sites/{siteId}/project").Named("GetSiteProject")
                .And.AtUri("/dataHosts/{dataHostId}/projects").Named("GetDataHostProject")
                .And.AtUri("/fundSource/{fundSourceId}/projects").Named("GetFundSourceProject")
                .HandledBy<ProjectHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");
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
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<PUBLICATION>()
                .AtUri("/publications/{publicationId}")
                .And.AtUri("/projects/{projectId}/RemoveProjectPublication").Named("RemoveProjectPublication")
                .HandledBy<PublicationHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");
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
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<RESOURCE_TYPE>()
                .AtUri("/resourcetypes/{resourceTypeId}")
               .And.AtUri("/sites/{siteId}/removeResourcetype").Named("RemoveSiteResourceType")
                .HandledBy<ResourceHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");
        }//end AddRESOURCE_Resources
        
        private void AddROLE_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<ROLE>>()
               .AtUri("/roles")
               .HandledBy<RoleHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<ROLE>()
                .AtUri("/roles/{roleId}")
                .And.AtUri("dataManagers/{dataManagerId}/role").Named("GetDataManagersRole")
                .HandledBy<RoleHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

        }//end AddRole_Resources
        
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
               .And.AtUri("sites?ParamType={paramTypes}&Parameters={parameters}&FromDate={fromDate}&ToDate={toDate}&ResComp={resComps}&Media={media}&Lake={lakes}&State={states}").Named("GetFilteredSites")
               .And.AtUri("sites/ProjectSites?ProjOrg={orgID}&ProjObjs={objIDs}&StartDate={fromDate}&EndDate={toDate}&Lake={lakes}&State={states}").Named("GetFilteredProjectSites")
               .HandledBy<SiteHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<SITE>()
                .AtUri("/sites/{siteId}")
                .HandledBy<SiteHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<SiteMapModel>()
                .AtUri("/Sites/SiteInformation?siteId={siteId}").Named("GetSiteInformation")
                .HandledBy<SiteHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<List<SiteMapModel>>()
                .AtUri("/Sites/ProjectSitesInfo?ProjectId={projectId}").Named("GetProjectSitesInfo")
                .HandledBy<SiteHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");
        }//end AddSITE_Resources
        
        private void AddSTATUS_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<STATUS_TYPE>>()
               .AtUri("/status")
               .HandledBy<StatusHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

            ResourceSpace.Has.ResourcesOfType<STATUS_TYPE>()
                .AtUri("/status/{statusId}")
                .And.AtUri("/sites/{siteId}/status").Named("GetSiteStatus")
                .HandledBy<StatusHandler>()
                .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
                .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");
        }//end AddStatus_Resources
        
        private void AddSTATE_Resources()
        {
            ResourceSpace.Has.ResourcesOfType<List<string>>()
               .AtUri("/states").Named("GetSiteStates")
               .HandledBy<StateHandler>()
               .TranscodedBy<SimpleUTF8XmlSerializerCodec>(null).ForMediaType("application/xml;q=1")
               .And.TranscodedBy<JsonDotNetCodec>(null).ForMediaType("application/json;q=0.9");

        }//end AddState_Resources
        
        #endregion


    }//End class Configuration
}//End namespace