//------------------------------------------------------------------------------
//----- DMResource -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2016 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//  
//   purpose:   DMList resources.
//              Equivalent to the model in MVC.
//
//discussion:   Resources are plain-old CLR objects (POCO) the resources are POCO classes derived from the EF
//              https://github.com/openrasta/openrasta/wiki/Resources
//
//     

#region Comments
// 01.06.16 - tr - Created
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Xml.Serialization;

namespace LaMPServices.Resources
{
    [XmlRoot("ArrayOfDMs")]
    public class DMListRes
    {
        [XmlElement(typeof(decimal),
        ElementName = "DATA_MANAGER_ID")]
        public decimal DATA_MANAGER_ID { get; set; }

        [XmlElement(typeof(decimal),
        ElementName = "DATA_MANAGER_ID")]
        public decimal? ORGANIZATION_SYSTEM_ID { get; set; }

        [XmlElement(typeof(string),
        ElementName = "FULLNAME")]
        public string FULLNAME { get; set; }

        [XmlElement(typeof(string),
        ElementName = "FNAME")]
        public string FNAME { get; set; }

        [XmlElement(typeof(string),
        ElementName = "LNAME")]
        public string LNAME { get; set; }

        [XmlElement(typeof(string),
        ElementName = "ROLE_NAME")]
        public string ROLE_NAME { get; set; }

        [XmlElement(typeof(decimal),
        ElementName = "PROJECT_COUNT")]
        public decimal? PROJECT_COUNT { get; set; }
        public bool ShouldSerializePROJECT_COUNT()
        { return PROJECT_COUNT.HasValue; }

    }
}