//------------------------------------------------------------------------------
//----- FeatureResource -----------------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Jeremy K. Newson USGS Wisconsin Internet Mapping
//              Tonia Roddick USGS Wisconsin Internet Mapping
//  
//   purpose:   Feature resources.
//              Equivalent to the model in MVC.
//
//discussion:   Used for support in serializing for ESRI addHWM/addSIte layer geoprocessing support
//
//     

#region Comments
// 01.08.14 - TR - Created
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Xml.Serialization;

namespace LaMPServices.Resources
{
    public class Features
    {
        public List<Feature> features { get; set; }

        #region "Constructor"
        public Features()
        {
            features = new List<Feature>();
        }
        #endregion
    }

    public class Feature
    {
        #region "Properties"
        public FeatureBase attributes { get; set; }
        public Geometry geometry { get; set; }
        #endregion
        #region "Constructor"
        public Feature(FeatureBase attr)
        {
            this.attributes = attr;
            this.geometry = new Geometry(Convert.ToDouble(attr.LATITUDE), Convert.ToDouble(attr.LONGITUDE));
        }
        #endregion


        #region "Structures"
        //A structure is a value type. When a structure is created, the variable to which the struct is assigned holds 
        //the struct's actual data. When the struct is assigned to a new variable, it is copied. The new variable and 
        //the original variable therefore contain two separate copies of the same data. Changes made to one copy do not 
        //affect the other copy.

        //In general, classes are used to model more complex behavior, or data that is intended to be modified after a
        //class object is created. Structs are best suited for small data structures that contain primarily data that is 
        //not intended to be modified after the struct is created.
        public struct Geometry
        {
            #region "Properties"
            public double x;
            public double y;
            #endregion
            #region "Constructor"
            public Geometry(double lat, double longit)
            {
                y = lat;
                x = longit;

            }
            #endregion
        }

        #endregion
    }//end Features

    public class FeatureBase
    {
        public double? LATITUDE { get; set; }
        public double? LONGITUDE { get; set; }

        #region "Constructor"
        public FeatureBase()
        {

        }
        #endregion
        #region "Helper Methods"
        protected string LimitLength(string inString, int maxLength)
        {
            if (inString.Length <= maxLength)
            {
                return inString;
            }

            return inString.Substring(0, maxLength);
        }
        // returns the number of milliseconds since Jan 1, 1970 (useful for converting C# dates to JS dates)
        public long UnixTicks(DateTime dt)
        {
            DateTime d1 = DateTime.SpecifyKind(new DateTime(1970, 1, 1, 0, 0, 0, 0), DateTimeKind.Utc);
            DateTime d2 = DateTime.SpecifyKind(dt, DateTimeKind.Utc);
            TimeSpan ts = new TimeSpan(d2.Ticks - d1.Ticks);
            return (long)ts.TotalMilliseconds;
        }
        #endregion

    }

}//end namespace