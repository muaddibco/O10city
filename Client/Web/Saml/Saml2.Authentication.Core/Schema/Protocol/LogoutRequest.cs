namespace dk.nita.saml20.Schema.Protocol
{
    using System;
    using System.Xml.Serialization;
    using Core;
    using Utils;

    /// <summary>
    /// A session participant or session authority sends a &lt;LogoutRequest&gt; message to indicate that a session
    /// has been terminated.
    /// </summary>
    [Serializable]
    [XmlType(Namespace=Saml2Constants.PROTOCOL)]
    [XmlRoot(ELEMENT_NAME, Namespace=Saml2Constants.PROTOCOL, IsNullable=false)]
    public class LogoutRequest : RequestAbstract
    {
        /// <summary>
        /// Specifies that the message is being sent because the principal wishes to terminate the indicated session.
        /// </summary>
        public const string REASON_USER = "urn:oasis:names:tc:SAML:2.0:logout:user";

        /// <summary>
        /// Specifies that the message is being sent because an administrator wishes to terminate the indicated session for 
        /// the principal.
        /// </summary>
        public const string REASON_ADMIN = "urn:oasis:names:tc:SAML:2.0:logout:admin";

        /// <summary>
        /// The XML Element name of this class
        /// </summary>
        public const string ELEMENT_NAME = "LogoutRequest";
        
        /// <summary>
        /// Gets or sets the item.
        /// The identifier and associated attributes (in plaintext or encrypted form) that specify the principal as
        /// currently recognized by the identity and service providers prior to this request.
        /// </summary>
        /// <value>The item.</value>
        [XmlElement("BaseID", typeof(BaseIDAbstract), Namespace = Saml2Constants.ASSERTION)]
        [XmlElement("EncryptedID", typeof(EncryptedElement), Namespace = Saml2Constants.ASSERTION)]
        [XmlElement("NameID", typeof(NameID), Namespace = Saml2Constants.ASSERTION)]
        public object Item { get; set; }

        /// <summary>
        /// Gets or sets the index of the session.
        /// </summary>
        /// <value>The index of the session.</value>
        [XmlElement("SessionIndex")]
        public string[] SessionIndex { get; set; }

        /// <summary>
        /// Gets or sets the reason.
        /// </summary>
        /// <value>The reason.</value>
        [XmlAttribute]
        public string Reason { get; set; }

        /// <summary>
        /// Gets or sets NotOnOrAfter.
        /// </summary>
        /// <value>The not on or after.</value>
        [XmlIgnore]
        public DateTime? NotOnOrAfter { get; set; }

        /// <summary>
        /// Gets or sets the issue instant string.
        /// </summary>
        /// <value>The issue instant string.</value>
        [XmlAttribute("NotOnOrAfter")]
        public string NotOnOrAfterString
        {
            get => NotOnOrAfter.HasValue ? Saml2Utils.ToUtcString(NotOnOrAfter.Value) : null;
            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    NotOnOrAfter = null;
                }
                else
                {
                    NotOnOrAfter = Saml2Utils.FromUtcString(value);
                }
            }
        }
    }
}