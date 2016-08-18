//------------------------------------------------------------------------------
//----- OrganizationResource -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2015 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//  
//   purpose:   Organization resources.
//              Equivalent to the model in MVC.
//
//discussion:   Resources are plain-old CLR objects (POCO) the resources are POCO classes derived from the EF
//              SiteResource contains additional rederers of the derived EF POCO classes. 
//              https://github.com/openrasta/openrasta/wiki/Resources
//
//     

#region Comments
// 03.07.13 - tr - Created
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Xml.Serialization;

namespace LaMPServices.Resources
{
    [XmlRoot("OrganizationSystem")]
    public class OrganizationResource
    {
        [XmlElement(typeof(decimal),
        ElementName = "OrganizationSystemID")]
        public decimal OrganizationSystemID { get; set; }

        [XmlElement(typeof(decimal?),
        ElementName = "OrganizationID")]
        public decimal? OrganizationID { get; set; }
        public bool ShouldSerializeOrganizationID()
        { return OrganizationID.HasValue; }

        [XmlElement(typeof(string),
        ElementName = "OrganizationName")]
        public string OrganizationName { get; set; }

        [XmlElement(typeof(decimal?),
        ElementName = "DivisionID")]
        public decimal? DivisionID { get; set; }
        public bool ShouldSerializeDivisionID()
        { return DivisionID.HasValue; }

        [XmlElement(typeof(string),
        ElementName = "DivisionName")]
        public string DivisionName { get; set; }

        [XmlElement(typeof(decimal?),
        ElementName = "SectionID")]
        public decimal? SectionID { get; set; }
        public bool ShouldSerializeSectionID()
        { return SectionID.HasValue; }       

        [XmlElement(typeof(string),
        ElementName = "SectionName")]
        public string SectionName { get; set; }

        [XmlElement(typeof(string),
        ElementName = "ScienceBaseID")]
        public string ScienceBaseID { get; set; }

    }
}//end namespace





