//------------------------------------------------------------------------------
//----- EntityObjectResource -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   EntityObject resources through the HTTP uniform interface.
//              Equivalent to the model in MVC.
//
//discussion:   Resources are plain-old CLR objects (POCO) the resources are POCO classes derived from the EF
//              SiteResource contains additional rederers of the derived EF POCO classes. 
//              https://github.com/openrasta/openrasta/wiki/Resources
//
//              EntityObjectResource contain the remaining entity object partial classes 
//              that where generated from the entity object model generator.
//              The partial classes were created in order to extend resource method to the 
//              generated objects
//
//              A partial class can implement more than one interface, 
//              but it cannot inherit from more than one base class. 
//              Therefore, all partial classes Inherits statements must specify the same base class.
//              http://msdn.microsoft.com/en-us/library/wa80x488.aspx
//              

#region Comments
// 03.06.13 - jkn - adapted from STNResourcesPartials
// 08.02.12 - jkn - added method to add links to specified related objects
// 06.29.12 - jkn - added additional entities, changed base from EntityObject to ResourceBase to HypermediaEntity
// 06.28.12 - jkn - Created
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Serialization;

namespace LaMPServices
{
    //  POST  /entities/{entityName}

    //  PUT   /entities/{entityName}/{entityId} 
    //  GET   /entities/{entityName}
    //        /entities/{entityName}/{entityId}

    //public partial class CATALOG_ : HypermediaEntity
    //{

    //    #region Overrided Methods

    //    protected override string getRelativeURI(refType rType)
    //    {
    //        string uriString = "";
    //        switch (rType)
    //        {
    //            case refType.GET:
    //            case refType.PUT:
    //            case refType.DELETE:
    //                uriString = string.Format("catalogs/{0}", this.CATALOG_ID);
    //                break;

    //            case refType.POST:
    //                uriString = "catalogs/";
    //                break;

    //        }
    //        return uriString;
    //    }

    //    protected override void addRelatedLinks(string BaseURI)
    //    {
    //        //calalog/{catalogId}/addMedium
    //        this.LINKS.Add(GetLinkResource(BaseURI, "add frequency", refType.POST, this.CATALOG_ID.ToString() +"/addFrequency"));
    //        //calalog/{catalogId}/addMedium
    //        this.LINKS.Add(GetLinkResource(BaseURI, "add medium", refType.POST, this.CATALOG_ID.ToString() + "/addMedium"));
    //        //calalog/{catalogId}/addParameter
    //        this.LINKS.Add(GetLinkResource(BaseURI, "add parameter", refType.POST, this.CATALOG_ID.ToString() + "/addParameter"));
    //        //calalog/{catalogId}/addMedium
    //        this.LINKS.Add(GetLinkResource(BaseURI, "add resource type", refType.POST, this.CATALOG_ID.ToString() + "/addResourcetype"));
    //        //calalog/{catalogId}/parameters
    //        this.LINKS.Add(GetLinkResource(BaseURI, "parameters", refType.GET, "/parameters"));
    //        //calalog/{catalogId}/resourcestypes
    //        this.LINKS.Add(GetLinkResource(BaseURI, "resource types", refType.GET, "/resourcetypes"));
    //        //calalog/{catalogId}/media
    //        this.LINKS.Add(GetLinkResource(BaseURI, "media", refType.GET, "/media"));
    //        //calalog/{catalogId}/frequency
    //        this.LINKS.Add(GetLinkResource(BaseURI, "frequencies", refType.GET, "/frequencies"));
    //        //calalog/{catalogId}/project
    //        this.LINKS.Add(GetLinkResource(BaseURI, "project", refType.GET, "/project"));
    //        //calalog/{catalogId}/site
    //        this.LINKS.Add(GetLinkResource(BaseURI, "site",refType.GET, "/site"));
    //    }
    //    #endregion

    //}//end Class CATALOG_

    public partial class CONTACT : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("contacts/{0}", this.CONTACT_ID);
                    break;

                case refType.POST:
                    uriString = "contacts";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //contacts/{contactId}/project
            this.LINKS.Add(GetLinkResource(BaseURI, "projects",refType.GET, "/projects"));
            //contacts/{contactId}/organization
            this.LINKS.Add(GetLinkResource(BaseURI, "organization", refType.GET, "/organization"));
           
        }
        #endregion

    }//end Class CONTACT

    public partial class DATA_HOST : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("dataHosts/{0}", this.DATA_HOST_ID);
                    break;

                case refType.POST:
                    uriString = "";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //dataHosts/{dataHosts}/project
            this.LINKS.Add(GetLinkResource(BaseURI, "project", refType.GET,"/project"));

        }
        #endregion

    }//end Class PUBLICATION

    public partial class DATA_MANAGER : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("dataManagers/{0}", this.DATA_MANAGER_ID);
                    break;

                case refType.POST:
                    uriString = "";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //dataManagers/{dataManagerId}/projects
            this.LINKS.Add(GetLinkResource(BaseURI, "projects", refType.GET,"/projects"));
            //dataManagers/{dataManagerId}/role
            this.LINKS.Add(GetLinkResource(BaseURI, "role", refType.GET, "/role"));
        }
        #endregion

    }//end Class DATA_MANAGER

    public partial class FREQUENCY_TYPE : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("frequencies/{0}", this.FREQUENCY_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "frequencies";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //frequencyTypes/{frequencyTypeId}/sites
            this.LINKS.Add(GetLinkResource(BaseURI, "sites", refType.GET, "/sites"));
        }
        #endregion

    }//end Class DATA_MANAGER

    public partial class KEYWORD : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("keywords/{0}", this.KEYWORD_ID);
                    break;

                case refType.POST:
                    uriString = "keywords";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //keywords/{keywordId}/projects
            this.LINKS.Add(GetLinkResource(BaseURI, "projects", refType.GET, "/projects"));
        }
        #endregion

    }//end Class KEYWORD

    public partial class LAKE_TYPE : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    //uriString = string.Format("Instruments/{0}", this.INSTRUMENT_ID);
                    uriString = string.Format("lakes/{0}", this.LAKE_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "lakes";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //lake/{lakeId}/sites
            this.LINKS.Add(GetLinkResource(BaseURI, "sites", refType.GET, "/sites"));
        }
        #endregion

    }//end Class LAKE_TYPE

    public partial class LOCATION : HypermediaEntity
    {

    
          #region Overrided Methods

    //    protected override string getRelativeURI(refType rType)
    //    {
    //        string uriString = "";
    //        switch (rType)
    //        {
    //            case refType.GET:
    //            case refType.PUT:
    //            case refType.DELETE:
    //                uriString = string.Format("sites/{0}", this.LOCATION_ID);
    //                break;

    //            case refType.POST:
    //                uriString = "sites";
    //                break;

    //        }
    //        return uriString;
    //    }
        protected override void addRelatedLinks(string BaseURI)
        {
            //site/{siteId}/catalogs
            //this.LINKS.Add(GetLinkResource(BaseURI, "catalogs", refType.GET, "/catalogs"));
            //site/{siteId}/status
            this.LINKS.Add(GetLinkResource(BaseURI, "status", refType.GET, "/status"));
            //site/{siteId}/lake
            this.LINKS.Add(GetLinkResource(BaseURI, "lake",  refType.GET,"/lake"));
        }
        #endregion

    }//end Class LOCATION

    public partial class MEDIA_TYPE : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("media/{0}", this.MEDIA_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "media";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //media/{mediaId}/sites
            this.LINKS.Add(GetLinkResource(BaseURI, "sites", refType.GET, "/sites"));
        }
        #endregion

    }//end Class MEDIA_TYPE

    public partial class OBJECTIVE_TYPE : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    uriString = string.Format("objectives/{0}", this.OBJECTIVE_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //objectives/{objectiveId}/projects
            this.LINKS.Add(GetLinkResource(BaseURI, "projects", refType.GET, "/projects"));

        }
        #endregion

    }//end Class PUBLICATION

    public partial class ORGANIZATION : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("organizations/{0}", this.ORGANIZATION_ID);
                    break;

                case refType.POST:
                    uriString = "organizations";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //organizations/{organizationId}/projects
            this.LINKS.Add(GetLinkResource(BaseURI, "projects",  refType.GET, "/projects"));
            //organizations/{organizationId}/contacts
            this.LINKS.Add(GetLinkResource(BaseURI, "contacts",  refType.GET, "/contacts"));
        }
        #endregion

    }//end Class ORGANIZATION

    public partial class PARAMETER_TYPE : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("parameters/{0}", this.PARAMETER_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "parameters";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //parameter/{parameterId}/sites
            this.LINKS.Add(GetLinkResource(BaseURI, "sites", refType.GET, "/sites"));
  
        }
        #endregion

    }//end Class PARAMETER_TYPE

    public partial class PROJECT : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("projects/{0}", this.PROJECT_ID);
                    break;

                case refType.POST:
                    uriString = "projects/";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //projects/{projectId}/AddContact
            this.LINKS.Add(GetLinkResource(BaseURI, "add contact", refType.POST, this.PROJECT_ID.ToString() + "/addContact"));
            //projects/{projectId}/addDataHost
            this.LINKS.Add(GetLinkResource(BaseURI, "add data host", refType.POST, this.PROJECT_ID.ToString() + "/addDataHost"));
            //projects/{projectId}/addKeyword
            this.LINKS.Add(GetLinkResource(BaseURI, "add keyword",  refType.POST, this.PROJECT_ID.ToString()+"/addKeyword"));
            //projects/{projectId}/addOrganization
            this.LINKS.Add(GetLinkResource(BaseURI, "add organization", refType.POST, this.PROJECT_ID.ToString() + "/addOrganization"));
            //projects/{projectId}/addObjective
            this.LINKS.Add(GetLinkResource(BaseURI, "add objective", refType.POST, this.PROJECT_ID.ToString() + "/addObjective"));
            //projects/{projectId}/addPublication
            this.LINKS.Add(GetLinkResource(BaseURI, "add publication", refType.POST, this.PROJECT_ID.ToString() + "/addPublication"));
            //projects/{projectId}/sites
            this.LINKS.Add(GetLinkResource(BaseURI, "sites",  refType.GET, "/sites"));
            //projects/{projectId}/contacts
            this.LINKS.Add(GetLinkResource(BaseURI, "contacts",  refType.GET, "/contacts"));
            //projects/{projectId}/dataHosts
            this.LINKS.Add(GetLinkResource(BaseURI, "data hosts", refType.GET, "/dataHosts"));
            //projects/{projectId}/dataManager
            this.LINKS.Add(GetLinkResource(BaseURI, "data manager", refType.GET, "/dataManager"));
            //projects/{projectId}/keywords
            this.LINKS.Add(GetLinkResource(BaseURI, "keywords", refType.GET, "/keywords"));
            //projects/{projectId}/objectives
            this.LINKS.Add(GetLinkResource(BaseURI, "objectives",  refType.GET, "/objectives"));
            //projects/{projectId}/organizations
            this.LINKS.Add(GetLinkResource(BaseURI, "organizations",  refType.GET, "/organizations"));
            //projects/{projectId}/publications
            this.LINKS.Add(GetLinkResource(BaseURI, "publications",  refType.GET,"/publications"));
                       

        }
        #endregion

    }//end Class PROJECT

    public partial class PUBLICATION : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                    uriString = string.Format("publications/{0}", this.PUBLICATION_ID);
                    break;

                case refType.POST:
                    uriString = "";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //publications/{publicationId}/projects
            this.LINKS.Add(GetLinkResource(BaseURI, "projects", refType.GET, "/projects"));

        }
        #endregion

    }//end Class PUBLICATION

    public partial class RESOURCE_TYPE : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("resourcetypes/{0}", this.RESOURCE_TYPE_ID);
                    break;

                case refType.POST:
                    uriString = "resourcetypes";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //resources/{resourceId}/sites
            this.LINKS.Add(GetLinkResource(BaseURI, "sites",  refType.GET, "/sites"));

        }
        #endregion

    }//end Class PUBLICATION

    public partial class STATUS_TYPE : HypermediaEntity
    {

        #region Overrided Methods

        protected override string getRelativeURI(refType rType)
        {
            string uriString = "";
            switch (rType)
            {
                case refType.GET:
                case refType.PUT:
                case refType.DELETE:
                    uriString = string.Format("status/{0}", this.STATUS_ID);
                    break;

                case refType.POST:
                    uriString = "status";
                    break;

            }
            return uriString;
        }
        protected override void addRelatedLinks(string BaseURI)
        {
            //status/{statusId}/site
            this.LINKS.Add(GetLinkResource(BaseURI, "sites", refType.GET, "/sites"));
        }
        #endregion

    }//end Class STATUS_TYPE
}//end namespace