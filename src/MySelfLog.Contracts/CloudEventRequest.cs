using System;

namespace MySelfLog.Contracts
{
    public class CloudEventRequest 
    {

        /// <summary>
        /// This attribute contains a value describing the type of event related to the originating occurrence. Often this attribute is used for routing, observability, policy enforcement, etc. The format of this is producer defined and might include information such as the version of the type - see Versioning of Attributes in the Primer for more information.
        /// </summary>
        public string Type { get; set; }
        //public string SpecVersion => "1.0";
        /// <summary>
        /// MUST be a non-empty URI-reference, an absolute URI is RECOMMENDED
        /// Examples:
        /// https://github.com/cloudevents
        /// sensors/tn-1234567/alerts
        /// </summary>
        public Uri Source { get; set; }
        /// <summary>
        /// MUST be unique within the scope of the producer
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Timestamp of when the occurrence happened
        /// </summary>
        public DateTime Time { get; set; }
        /// <summary>
        /// Example when sending Data as Json: 'application/json'
        /// </summary>
        public string DataContentType { get; set; }
        /// <summary>
        /// Identifies the schema that data adheres to. Incompatible changes to the schema SHOULD be reflected by a different URI. See Versioning of Attributes in the Primer for more information
        /// </summary>
        public Uri DataSchema { get; set; }
        /// <summary>
        /// The event payload. This specification does not place any restriction on the type of this information. It is encoded into a media format which is specified by the datacontenttype attribute (e.g. application/json), and adheres to the dataschema format when those respective attributes are present
        /// </summary>
        public dynamic Data { get; set; }
    }
}
