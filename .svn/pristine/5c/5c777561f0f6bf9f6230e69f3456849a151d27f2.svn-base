//------------------------------------------------------------------------------
//----- STNWebSerializer.cs-----------------------------------------------------
//------------------------------------------------------------------------------

//-------1---------2---------3---------4---------5---------6---------7---------8
//       01234567890123456789012345678901234567890123456789012345678901234567890
//-------+---------+---------+---------+---------+---------+---------+---------+

// copyright:   2013 WiM - USGS

//    authors:  Tonia Roddick USGS Wisconsin Internet Mapping
//              
//  
//   purpose:   Extend the RestSharp serializer to allow for multiple types
//
//discussion:   
//
//     

#region Comments
// 02.26.13 - TR - Created 
#endregion


using System.IO;
using System.Text;
using System.Xml.Serialization;

using RestSharp.Serializers;
using LaMPServices.Codecs;

namespace LaMPWeb.Utilities
{
    /// <summary>
	/// Wrapper for RestSharp.Serializers.DotNetXmlSerializer
	/// </summary>
    public class LaMPWebSerializer : DotNetXmlSerializer 
    {
		/// <summary>
		/// Default constructor, does not specify namespace
		/// </summary>
        public LaMPWebSerializer()
		{
		}


		/// <summary>
		/// Serialize the object as XML
		/// </summary>
		/// <param name="obj">Object to serialize</param>
		/// <returns>XML as string</returns>
        //public string Serialize<T>(object obj)
        //{
        //    var ns = new XmlSerializerNamespaces();
        //    ns.Add(string.Empty, Namespace);
        //    var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T), LaMPXmlSerializerCodec.extraTypes);
        //    var writer = new EncodingStringWriter(Encoding);
        //    serializer.Serialize(writer, obj, ns);

        //    return writer.ToString();
        //}



        /// <summary>
        /// Need to subclass StringWriter in order to override Encoding
        /// </summary>
        protected class EncodingStringWriter : StringWriter
        {
            private readonly Encoding encoding;

            public EncodingStringWriter(Encoding encoding)
            {
                this.encoding = encoding;
            }

            public override Encoding Encoding
            {
                get { return encoding; }
            }
        }
        		
	}
}