﻿using LaMPServices;

namespace LaMPWeb.Models
{   
    //post a project summary (with all it's parts)
    public class ProjectModel
    {
        public PROJECT project { get; set; }
        public string projOrgs { get; set; }
        public string projOrgsToRemove { get; set; }
        public string projObjectives { get; set; }
        public string projObjToRemove { get; set; }
        public string projKeywords { get; set; }
        public string projKeywordsToRemove { get; set; }
    }

    //post a project contact (with all it's parts)
    public class ProjectContact
    {
        public CONTACT Contact { get; set; }
        public decimal projId { get; set; }
        public int division { get; set; }
        public int section { get; set; }
    }

    //used for contact Infobox to show contact name and organization name
    public class ContactInfo
    {
        public decimal ContactId { get; set; }
        public string Name { get; set; }
        public string OrgName { get; set; }
    }

    //post a project publication
    public class ProjectPublication
    {
        public PUBLICATION Publication { get; set; }
        public decimal projId { get; set; }
    }

    //store all the pieces to POST to a site
    public class SiteModel
    {
        public PROJECT aProject { get; set; }
        public SITE aSite { get; set; }
        public string[] Params { get; set; }
        public string CreateResourceTypes { get; set; }
        public string ResourceTypes { get; set; }
        public string ResourcesToRemove { get; set; }
        public string CreateMediaTypes { get; set; }
        public string MediaTypes { get; set; }
        public string MediaToRemove {get;set;} 
        public string CreateFrequencyTypes { get; set; }
        public string FrequencyTypes { get; set; }
        public string FrequencyToRemove {get;set;}
        public string SiteParameters { get; set; }
        public string From { get; set; }
    }

    //site create gridview model
    public class SiteGridModel
    {
        public decimal SiteID { get; set; }
        public string isActive {get; set;}
        public string NAME {get; set;}
        public decimal? LATITUDE {get; set;}
        public decimal? LONGITUDE {get; set;}
        public string COUNTRY {get; set;}
        public string STATE_PROVINCE {get; set;}
        public string LAKE {get; set;}
        public string WATERBODY {get; set;}
        public string WATERSHED_HUC8 {get; set;}
        public string DESCRIPTION {get; set;}
        public string STATUS {get; set;}
        public string ResourceTypes {get; set;}
        public string MediaTypes {get; set;}
        public string FrequencyTypes {get; set;}
        public string START_DATE { get; set; }
        public string END_DATE { get; set; }
        public string SAMPLE_PLATFORM {get; set;}
        public string ADDITIONAL_INFO {get; set;}
        public string URL {get; set;}
        public string Parameters { get; set; }
    }
}