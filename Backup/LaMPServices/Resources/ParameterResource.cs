//------------------------------------------------------------------------------
//----- ParameterResource -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2012 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//  
//   purpose:   Parameter resources.
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
    [XmlRoot("PARAMETER_GROUPS")]
    public class ParameterGroups
    {
        [XmlElement(typeof(List<PARAMETER_TYPE>),
        ElementName = "BIOLOGICAL")]
        public List<PARAMETER_TYPE> Biological { get; set; }

        [XmlElement(typeof(List<PARAMETER_TYPE>),
        ElementName = "CHEMICAL")]
        public List<PARAMETER_TYPE> Chemical { get; set; }

        [XmlElement(typeof(List<PARAMETER_TYPE>),
        ElementName = "MICROBIOLOGICAL")]
        public List<PARAMETER_TYPE> Microbiological { get; set; }

        [XmlElement(typeof(List<PARAMETER_TYPE>),
        ElementName = "PHYSICAL")]
        public List<PARAMETER_TYPE> Physical { get; set; }

        [XmlElement(typeof(List<PARAMETER_TYPE>),
        ElementName = "TOXICOLOGICAL")]
        public List<PARAMETER_TYPE> Toxicological { get; set; }

    }    
}//end namespace





