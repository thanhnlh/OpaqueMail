﻿/*
 * OpaqueMail (http://opaquemail.org/).
 * 
 * Licensed according to the MIT License (http://mit-license.org/).
 * 
 * Copyright © Bert Johnson (http://bertjohnson.com/).
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the “Software”), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 * 
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Runtime;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace OpaqueMail
{
    /// <summary>
    /// Represents an email message that was either received using the ImapClient or Pop3Client classes or will be sent using the SmtpClient class.
    /// Includes OpaqueMail extensions to facilitate handling of secure S/MIME messages.
    /// </summary>
    public class MailMessage
    {
        #region Public Members
        /// <summary>Collection of all recipients of this message, based on To, CC, and Bcc paramaters.</summary>
        public List<string> AllRecipients { get; set; }
        public AlternateViewCollection AlternateViews { get; set; }
        /// <summary>Gets the attachment collection used to store data attached to this email message.</summary>
        /// <returns>A writable <see cref="T:System.Net.Mail.AttachmentCollection" />.</returns>
        public AttachmentCollection Attachments { get; set; }
        /// <summary>Gets the address collection that contains the blind carbon copy (BCC) recipients for this email message.</summary>
        /// <returns>A writable <see cref="T:System.Net.Mail.MailAddressCollection" /> object.</returns>
        public MailAddressCollection Bcc { get; set; }
        /// <summary>Gets or sets the message body.</summary>
        public string Body { get; set; }
        /// <summary>Content type of the message's body.</summary>
        public string BodyContentType { get; set; }
        /// <summary>Whether the body's contents were decoded from their transfer encoding.</summary>
        public bool BodyDecoded { get; set; }
        /// <summary>Gets or sets the encoding used to encode the message body.</summary>
        public Encoding BodyEncoding { get; set; }
        public TransferEncoding BodyTransferEncoding { get; set; }
        /// <summary>Gets the address collection that contains the carbon copy (CC) recipients for this email message.</summary>
        public MailAddressCollection CC { get; set; }
        /// <summary>Character set encoding of the message.</summary>
        public string CharSet { get; set; }
        /// <summary>Language of message content.</summary>
        public string ContentLanguage { get; set; }
        /// <summary>Content transfer encoding of the message.</summary>
        public string ContentTransferEncoding { get; set; }
        /// <summary>Primary content type of the message.</summary>
        public string ContentType { get; set; }
        /// <summary>Date sent.</summary>
        public DateTime Date { get; set; }
        /// <summary>Delivered-To header.</summary>
        public string DeliveredTo { get; set; }
        /// <summary>Gets or sets the delivery notifications for this email message.</summary>
        /// <returns>A <see cref="T:System.Net.Mail.DeliveryNotificationOptions" /> value that contains the delivery notifications for this message.</returns>
        public DeliveryNotificationOptions DeliveryNotificationOptions
        {
            [TargetedPatchingOptOut("Performance critical to inline this type of method across NGen image boundaries")]
            get
            {
                return this.deliveryStatusNotification;
            }
            set
            {
                if ((DeliveryNotificationOptions.OnSuccess | DeliveryNotificationOptions.OnFailure | DeliveryNotificationOptions.Delay) < value && value != DeliveryNotificationOptions.Never)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                this.deliveryStatusNotification = value;
            }
        }
        /// <summary>
        /// Extended email headers.
        /// Only populated when the MailMessage is instantiated with a parseExtendedHeaders setting of true.
        /// </summary>
        public ExtendedProperties ExtendedProperties { get; set; }
        /// <summary>Flags representing the processed state of the message.</summary>
        public Flags Flags { get; set; }
        /// <summary>Gets or sets the from address for this email message.</summary>
        /// <returns>A <see cref="T:System.Net.Mail.MailAddress" /> that contains the from address information.</returns>
        public MailAddress From { get; set; }
        /// <summary>Whether the message contains a body.</summary>
        public bool HasBody { get; set; }
        /// <summary>Whether the message contains headers.</summary>
        public bool HasHeaders { get; set; }
        /// <summary>Gets the email headers that are transmitted with this email message.</summary>
        /// <returns>A <see cref="T:System.Collections.Specialized.NameValueCollection" /> that contains the email headers.</returns>
        public NameValueCollection Headers { get; set; }
        /// <summary>Gets or sets the encoding used for the user-defined custom headers for this email message.</summary>
        /// <returns>The encoding used for user-defined custom headers for this email message.</returns>
        public Encoding HeadersEncoding { get; set; }
        /// <summary>Gets or sets a value indicating whether the mail message body is in Html.</summary>
        public bool IsBodyHtml { get; set; }
        /// <summary>Mailbox the message was read from.</summary>
        public string Mailbox { get; set; }
        /// <summary>Gets or sets the priority of this email message.</summary>
        /// <returns>A <see cref="T:System.Net.Mail.MailPriority" /> that contains the priority of this message.</returns>
        public MailPriority Priority { get; set; }
        /// <summary>UID as specified by the IMAP server.</summary>
        public int ImapUid { get; set; }
        /// <summary>Importance header.</summary>
        public string Importance { get; set; }
        /// <summary>Index as specified by the IMAP or POP3 server.</summary>
        public int Index { get; set; }
        /// <summary>Raw In-Reply-To header.</summary>
        public string InReplyTo { get; set; }
        /// <summary>In-Reply-To message IDs.</summary>
        public string[] InReplyToMessageIDs { get; set; }
        /// <summary>Message ID header.</summary>
        public string MessageId { get; set; }
        /// <summary>Text delimiting MIME message parts.</summary>
        public string MimeBoundaryName { get; set; }
        /// <summary>List of MIME parts from a multipart/* MIME encoded message.</summary>
        public List<MimePart> MimeParts { get; set; }
        /// <summary>Partial message unique ID.</summary>
        public string PartialMessageId { get; set; }
        /// <summary>Partial message unique number.</summary>
        public int PartialMessageNumber { get; set; }
        /// <summary>Encrypt the email's envelope.  When PgpSign is true, encryption is the second PGP operation.</summary>
        public bool PgpEncryptedEnvelope { get; set; }
        /// <summary>Sign the email.  When true, signing is the first PGP operation.</summary>
        public bool PgpSigned { get; set; }
        /// <summary>UIDL as specified by the POP3 server.</summary>
        public string Pop3Uidl { get; set; }
        /// <summary>Flags determining whether specialized properties are returned with a MailMessage.</summary>
        public MailMessageProcessingFlags ProcessingFlags { get; set; }
        /// <summary>String representation of the raw body received.</summary>
        public string RawBody { get; set; }
        /// <summary>Raw flags returned with the message.</summary>
        public HashSet<string> RawFlags { get; set; }
        /// <summary>String representation of the raw headers received.</summary>
        public string RawHeaders { get; set; }
        /// <summary>String representation of the entire raw message received.</summary>
        public string RawMessage
        {
            get
            {
                return RawHeaders + (RawHeaders.Length > 0 && RawBody.Length > 0 ? "\r\n\r\n" : "") + RawBody;
            }
        }
        /// <summary>Array of values of Received and X-Received headers.</summary>
        public string[] ReceivedChain { get; set; }
        /// <summary>Message IDs of previously referenced messages.</summary>
        public string[] References { get; set; }
        /// <summary>Gets or sets the ReplyTo address for the mail message.</summary>
        /// <returns>A MailAddress that indicates the value of the <see cref="P:System.Net.Mail.MailMessage.ReplyTo" /> field.</returns>
        [Obsolete("ReplyTo is obsoleted for this type.  Please use ReplyToList instead which can accept multiple addresses. http://go.microsoft.com/fwlink/?linkid=14202")]
        public MailAddress ReplyTo { get; set; }
        /// <summary>Gets or sets the list of addresses to reply to for the mail message.</summary>
        /// <returns>The list of the addresses to reply to for the mail message.</returns>
        public MailAddressCollection ReplyToList = new MailAddressCollection();
        /// <summary>Return-Path header.</summary>
        public string ReturnPath { get; set; }
        /// <summary>Gets or sets the sender's address for this email message.</summary>
        /// <returns>A <see cref="T:System.Net.Mail.MailAddress" /> that contains the sender's address information.</returns>
        public MailAddress Sender { get; set; }
        /// <summary>
        /// Size of the entire message.
        /// When sending email, this is a rough estimate only.
        /// </summary>
        public long Size
        {
            get
            {
                // If a size has been set when loading the message, return that size.
                if (loadedSize > -1)
                    return loadedSize;
                else
                {
                    // Calculate a rough estimate of its size.
                    long size = Body.Length;

                    foreach (Attachment attachment in Attachments)
                    {
                        using (Stream dataStream = attachment.ContentStream)
                        {
                            size += dataStream.Length;
                        }
                    }

                    return Body.Length;
                }
            }
            set
            {
                loadedSize = value;
            }
        }
        /// <summary>Encrypt the email's envelope.  When SmimeSign is true, encryption is the second S/MIME operation.</summary>
        public bool SmimeEncryptedEnvelope { get; set; }
        /// <summary>Type of subject identifier to use.</summary>
        /// <remarks>The default of "IssuerAndSerialNumber" is recommended for most use cases.</remarks>
        public SmimeEncryptionOptionFlags SmimeEncryptionOptionFlags { get; set; }
        /// <summary>Whether S/MIME settings for encryption and signing are explicitly required or only preferred.</summary>
        public SmimeSettingsMode SmimeSettingsMode { get; set; }
        /// <summary>Sign the email.  When true, signing is the first S/MIME operation.</summary>
        public bool SmimeSigned { get; set; }
        /// <summary>
        /// Certificate used when signing messages.
        /// Requires private key.
        /// </summary>
        public X509Certificate2 SmimeSigningCertificate { get; set; }
        /// <summary>Certificate chain used to sign the message.</summary>
        public X509Certificate2Collection SmimeSigningCertificateChain { get; set; }
        /// <summary>Determine how the S/MIME message will be signed.</summary>
        public SmimeSigningOptionFlags SmimeSigningOptionFlags { get; set; }
        /// <summary>Triple-wrap the email by signing, then encrypting the envelope, then signing the encrypted envelope.</summary>
        public bool SmimeTripleWrapped { get; set; }
        /// <summary>Determine how the S/MIME envelope will be encrypted.</summary>
        /// <summary>Gets or sets the subject line for this email message.</summary>
        /// <returns>A <see cref="T:System.String" /> that contains the subject content.</returns>
        public string Subject { get; set; }
        /// <summary>Gets or sets the encoding used for the subject content for this email message.</summary>
        /// <returns>An <see cref="T:System.Text.Encoding" /> that was used to encode the <see cref="P:System.Net.Mail.MailMessage.Subject" /> property.</returns>
        public Encoding SubjectEncoding { get; set; }
        /// <summary>X-Subject-Encryption header, as optionally used by OpaqueMail.</summary>
        public bool SubjectEncryption { get; set; }
        public SubjectIdentifierType SubjectIdentifierType { get; set; }
        /// <summary>Gets the address collection that contains the recipients of this email message.</summary>
        /// <returns>A writable <see cref="T:System.Net.Mail.MailAddressCollection" /> object.</returns>
        public MailAddressCollection To { get; set; }
        #endregion Public Members

        #region Protected Members
        /// <summary>Text delimiting S/MIME message parts related to signatures.</summary>
        protected string SmimeSignedCmsBoundaryName = "OpaqueMail-signature-boundary";
        /// <summary>Text delimiting MIME message parts in triple wrapped messages.</summary>
        protected string SmimeTripleSignedCmsBoundaryName = "OpaqueMail-triple-signature-boundary";
        #endregion Protected Members

        #region Private Members
        /// <summary>Size of the loaded message, as calculated in MailMessage's constructor.</summary>
        private long loadedSize = -1;
        /// <summary>Gets or sets the delivery notifications for this email message.</summary>
        private DeliveryNotificationOptions deliveryStatusNotification;
        #endregion Private Members

        #region Constructors
        /// <summary>
        /// Initializes an empty instance of the OpaqueMail.MailMessage class.
        /// </summary>
        public MailMessage()
            : base()
        {
            AllRecipients = new List<string>();
            AlternateViews = new AlternateViewCollection();
            Attachments = new AttachmentCollection();
            Bcc = new MailAddressCollection();
            Body = "";
            BodyContentType = "";
            BodyDecoded = false;
            CC = new MailAddressCollection();
            CharSet = "";
            ContentLanguage = "";
            ContentTransferEncoding = "";
            ContentType = "";
            DeliveredTo = "";
            Headers = new NameValueCollection();
            Importance = "";
            MimeBoundaryName = "OpaqueMail-boundary";
            ProcessingFlags = MailMessageProcessingFlags.IncludeRawHeaders | MailMessageProcessingFlags.IncludeRawBody;
            RawFlags = new HashSet<string>();
            ReturnPath = "";
            SmimeSigningCertificateChain = new X509Certificate2Collection();
            SmimeEncryptedEnvelope = false;
            SmimeEncryptionOptionFlags = SmimeEncryptionOptionFlags.RequireCertificateVerification;
            SmimeSettingsMode = SmimeSettingsMode.RequireExactSettings;
            SmimeSigned = false;
            SmimeSigningOptionFlags = SmimeSigningOptionFlags.SignTime;
            SmimeTripleWrapped = false;
            SubjectIdentifierType = SubjectIdentifierType.IssuerAndSerialNumber;
            To = new MailAddressCollection();
        }
        /// <summary>
        /// Initializes an empty instance of the OpaqueMail.MailMessage class by using the specified OpaqueMail.MailAddress class objects.
        /// </summary>
        /// <param name="from">A System.Net.Mail.MailAddress that contains the address of the sender of the email message.</param>
        /// <param name="to">A System.Net.Mail.MailAddress that contains the address of the recipient of the email message.</param>
        public MailMessage(MailAddress from, MailAddress to)
            : this()
        {
            From = from;
            To.Add(to);
        }
        /// <summary>
        /// Initializes an empty instance of the OpaqueMail.MailMessage class.
        /// </summary>
        /// <param name="from">A System.String that contains the address of the sender of the email message.</param>
        /// <param name="to">A System.String that contains the address of the recipient of the email message.</param>
        /// <param name="subject">A System.String that contains the subject text.</param>
        /// <param name="body">A System.String that contains the message body.</param>
        public MailMessage(string from, string to, string subject, string body)
            : this()
        {
            From = new MailAddress(from);
            To.Add(new MailAddress(to));
            Subject = subject;
            Body = body;
        }
        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.MailMessage class representing the message text passed in.
        /// </summary>
        /// <param name="messageText">The raw contents of the email message.</param>
        public MailMessage(string messageText)
            : this(messageText, MailMessageProcessingFlags.IncludeRawHeaders | MailMessageProcessingFlags.IncludeRawBody | MailMessageProcessingFlags.IncludeMIMEParts, false)
        {
        }
        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.MailMessage class representing the message text passed in with attachments procesed according to the attachment filter flags.
        /// </summary>
        /// <param name="messageText">The raw contents of the email message.</param>
        /// <param name="processingFlags">Flags determining whether specialized properties are returned with a MailMessage.</param>
        public MailMessage(string messageText, MailMessageProcessingFlags processingFlags)
            : this(messageText, processingFlags, false)
        {
        }
        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.MailMessage class representing the message text passed in with attachments procesed according to the attachment filter flags.
        /// </summary>
        /// <param name="messageText">The raw contents of the email message.</param>
        /// <param name="processingFlags">Flags determining whether specialized properties are returned with a MailMessage.</param>
        /// <param name="parseExtendedHeaders">Whether to populate the ExtendedHeaders object.</param>
        public MailMessage(string messageText, MailMessageProcessingFlags processingFlags, bool parseExtendedHeaders)
            : this()
        {
            // Default to no MIME boundary.
            MimeBoundaryName = null;

            // Prepare an object to track advanced headers.
            if (parseExtendedHeaders)
                ExtendedProperties = new ExtendedProperties();

            // Remember which specialized attachments to include.
            ProcessingFlags = processingFlags;

            // Fix messages whose carriage returns have been stripped.
            if (messageText.IndexOf("\r") < 0)
                messageText = messageText.Replace("\n", "\r\n");

            // Separate the headers for processing.
            string headers;
            int cutoff = messageText.IndexOf("\r\n\r\n");
            if (cutoff > -1)
                headers = messageText.Substring(0, cutoff);
            else
                headers = messageText;

            // Calculate the size of the message.
            Size = messageText.Length;

            // Temporary header variables to be processed by Functions.FromMailAddressString() later.
            string fromText = "";
            string toText = "";
            string ccText = "";
            string bccText = "";
            string replyToText = "";

            // Temporary header variables to be processed later.
            List<string> receivedChain = new List<string>();
            string receivedText = "";

            // Unfold any unneeded whitespace, then loop through each line of the headers.
            string[] headersList = Functions.UnfoldWhitespace(headers).Replace("\r", "").Split('\n');
            foreach (string header in headersList)
            {
                // Split header {name:value} pairs by the first colon found.
                int colonPos = header.IndexOf(":");
                if (colonPos > -1 && colonPos < header.Length - 1)
                {
                    string[] headerParts = new string[] { header.Substring(0, colonPos), header.Substring(colonPos + 1).TrimStart(new char[] { ' ' }) };
                    string headerType = headerParts[0].ToLower();
                    string headerValue = headerParts[1];

                    // Set header variables for common headers.
                    if (!string.IsNullOrEmpty(headerType) && !string.IsNullOrEmpty(headerValue))
                        Headers[headerType] = headerValue;

                    switch (headerType)
                    {
                        case "cc":
                            if (ccText.Length > 0)
                                ccText += ", ";
                            ccText += headerValue;
                            break;
                        case "content-transfer-encoding":
                            ContentTransferEncoding = headerValue;
                            switch (headerValue.ToLower())
                            {
                                case "base64":
                                    BodyTransferEncoding = TransferEncoding.Base64;
                                    break;
                                case "quoted-printable":
                                    BodyTransferEncoding = TransferEncoding.QuotedPrintable;
                                    break;
                                case "7bit":
                                    BodyTransferEncoding = TransferEncoding.SevenBit;
                                    break;
                                case "8bit":
                                    BodyTransferEncoding = TransferEncoding.EightBit;
                                    break;
                                default:
                                    BodyTransferEncoding = TransferEncoding.Unknown;
                                    break;
                            }
                            break;
                        case "content-language":
                            ContentLanguage = headerValue;
                            break;
                        case "content-type":
                            // If multiple content-types are passed, only process the first.
                            if (string.IsNullOrEmpty(ContentType))
                            {
                                ContentType = headerValue.Trim();
                                CharSet = Functions.ExtractMimeParameter(ContentType, "charset");
                                MimeBoundaryName = Functions.ExtractMimeParameter(ContentType, "boundary");

                                BodyContentType = ContentType.ToLower();
                                if (BodyContentType.IndexOf(";") > -1)
                                    BodyContentType = BodyContentType.Substring(0, BodyContentType.IndexOf(";"));

                                IsBodyHtml = BodyContentType.StartsWith("text/html");
                            }
                            break;
                        case "date":
                            string dateString = headerValue;

                            // Ignore extraneous datetime information.
                            int dateStringParenthesis = dateString.IndexOf("(");
                            if (dateStringParenthesis > -1)
                                dateString = dateString.Substring(0, dateStringParenthesis - 1);

                            // Remove timezone suffix.
                            if (dateString.Substring(dateString.Length - 4, 1) == " ")
                                dateString = dateString.Substring(0, dateString.Length - 4);

                            DateTime date;
                            DateTime.TryParse(dateString, out date);
                            Date = date;
                            break;
                        case "delivered-to":
                            DeliveredTo = headerValue;
                            break;
                        case "from":
                            fromText = headerValue;
                            break;
                        case "importance":
                            Importance = headerValue;
                            break;
                        case "in-reply-to":
                            InReplyTo = headerValue;

                            // Loop through references.
                            List<string> inReplyTo = new List<string>();
                            int inReplyToPos = 0;
                            while (inReplyToPos > -1)
                            {
                                inReplyToPos = headerValue.IndexOf("<", inReplyToPos);
                                if (inReplyToPos > -1)
                                {
                                    int referencesPos2 = headerValue.IndexOf(">", inReplyToPos);
                                    if (referencesPos2 > -1)
                                    {
                                        inReplyTo.Add(headerValue.Substring(inReplyToPos + 1, referencesPos2 - inReplyToPos - 1));
                                        inReplyToPos = referencesPos2 + 1;
                                    }
                                    else
                                        inReplyToPos = -1;
                                }
                            }
                            // If any references were found, apply to the message's array.
                            if (inReplyTo.Count > 0)
                                InReplyToMessageIDs = inReplyTo.ToArray();
                            else
                            {
                                if (!string.IsNullOrEmpty(headerValue))
                                    InReplyToMessageIDs = new string[] {headerValue};
                            }

                            break;
                        case "message-id":
                            // Ignore opening and closing <> characters.
                            MessageId = headerValue;
                            if (MessageId.StartsWith("<"))
                                MessageId = MessageId.Substring(1);
                            if (MessageId.EndsWith(">"))
                                MessageId = MessageId.Substring(0, MessageId.Length - 1);
                            break;
                        case "received":
                        case "x-received":
                            if (!string.IsNullOrEmpty(receivedText))
                                receivedChain.Add(receivedText);

                            receivedText = headerValue;
                            break;
                        case "references":
                            // Loop through references.
                            List<string> references = new List<string>();
                            int referencesPos = 0;
                            while (referencesPos > -1)
                            {
                                referencesPos = headerValue.IndexOf("<", referencesPos);
                                if (referencesPos > -1)
                                {
                                    int referencesPos2 = headerValue.IndexOf(">", referencesPos);
                                    if (referencesPos2 > -1)
                                    {
                                        references.Add(headerValue.Substring(referencesPos + 1, referencesPos2 - referencesPos - 1));
                                        referencesPos = referencesPos2 + 1;
                                    }
                                    else
                                        referencesPos = -1;
                                }
                            }
                            // If any references were found, apply to the message's array.
                            if (references.Count > 0)
                                References = references.ToArray();

                            break;
                        case "replyto":
                        case "reply-to":
                            replyToText = headerValue;
                            break;
                        case "return-path":
                            // Ignore opening and closing <> characters.
                            ReturnPath = headerValue;
                            if (ReturnPath.StartsWith("<"))
                                ReturnPath = ReturnPath.Substring(1);
                            if (ReturnPath.EndsWith(">"))
                                ReturnPath = ReturnPath.Substring(0, ReturnPath.Length - 1);
                            break;
                        case "sender":
                        case "x-sender":
                            if (headerValue.Length > 0)
                            {
                                MailAddressCollection senderCollection = MailAddressCollection.Parse(headerValue);
                                if (senderCollection.Count > 0)
                                    this.Sender = senderCollection[0];
                            }
                            break;
                        case "subject":
                            // Decode international strings and remove escaped linebreaks.
                            Subject = Functions.DecodeMailHeader(headerValue).Replace("\r", "").Replace("\n", "");
                            break;
                        case "to":
                            if (toText.Length > 0)
                                toText += ", ";
                            toText += headerValue;
                            break;
                        case "x-priority":
                            switch (headerValue.ToUpper())
                            {
                                case "LOW":
                                    Priority = MailPriority.Low;
                                    break;
                                case "NORMAL":
                                    Priority = MailPriority.Normal;
                                    break;
                                case "HIGH":
                                    Priority = MailPriority.High;
                                    break;
                            }
                            break;
                        case "x-subject-encryption":
                            bool subjectEncryption;
                            bool.TryParse(headerValue, out subjectEncryption);
                            SubjectEncryption = subjectEncryption;
                            break;
                        default:
                            break;
                    }

                    // Set header variables for advanced headers.
                    if (parseExtendedHeaders)
                    {
                        switch (headerType)
                        {
                            case "acceptlanguage":
                            case "accept-language":
                                ExtendedProperties.AcceptLanguage = headerValue;
                                break;
                            case "authentication-results":
                                ExtendedProperties.AuthenticationResults = headerValue;
                                break;
                            case "bounces-to":
                            case "bounces_to":
                                ExtendedProperties.BouncesTo = headerValue;
                                break;
                            case "content-description":
                                ExtendedProperties.ContentDescription = headerValue;
                                break;
                            case "dispositionnotificationto":
                            case "disposition-notification-to":
                                ExtendedProperties.DispositionNotificationTo = headerValue;
                                break;
                            case "dkim-signature":
                            case "domainkey-signature":
                            case "x-google-dkim-signature":
                                ExtendedProperties.DomainKeySignature = headerValue;
                                break;
                            case "domainkey-status":
                                ExtendedProperties.DomainKeyStatus = headerValue;
                                break;
                            case "errors-to":
                                ExtendedProperties.ErrorsTo = headerValue;
                                break;
                            case "list-unsubscribe":
                            case "x-list-unsubscribe":
                                ExtendedProperties.ListUnsubscribe = headerValue;
                                break;
                            case "mailer":
                            case "x-mailer":
                                ExtendedProperties.Mailer = headerValue;
                                break;
                            case "organization":
                            case "x-originator-org":
                            case "x-originatororg":
                            case "x-organization":
                                ExtendedProperties.OriginatorOrg = headerValue;
                                break;
                            case "original-messageid":
                            case "x-original-messageid":
                                ExtendedProperties.OriginalMessageId = headerValue;
                                break;
                            case "originating-email":
                            case "x-originating-email":
                                ExtendedProperties.OriginatingEmail = headerValue;
                                break;
                            case "precedence":
                                ExtendedProperties.Precedence = headerValue;
                                break;
                            case "received-spf":
                                ExtendedProperties.ReceivedSPF = headerValue;
                                break;
                            case "references":
                                ExtendedProperties.References = headerValue;
                                break;
                            case "resent-date":
                                string dateString = headerValue;

                                // Ignore extraneous datetime information.
                                int dateStringParenthesis = dateString.IndexOf("(");
                                if (dateStringParenthesis > -1)
                                    dateString = dateString.Substring(0, dateStringParenthesis - 1);

                                // Remove timezone suffix.
                                if (dateString.Substring(dateString.Length - 4) == " ")
                                    dateString = dateString.Substring(0, dateString.Length - 4);

                                DateTime.TryParse(dateString, out ExtendedProperties.ResentDate);
                                break;
                            case "resent-from":
                                ExtendedProperties.ResentFrom = headerValue;
                                break;
                            case "resent-message-id":
                                ExtendedProperties.ResentMessageID = headerValue;
                                break;
                            case "thread-index":
                                ExtendedProperties.ThreadIndex = headerValue;
                                break;
                            case "thread-topic":
                                ExtendedProperties.ThreadTopic = headerValue;
                                break;
                            case "user-agent":
                            case "useragent":
                                ExtendedProperties.UserAgent = headerValue;
                                break;
                            case "x-auto-response-suppress":
                                ExtendedProperties.AutoResponseSuppress = headerValue;
                                break;
                            case "x-campaign":
                            case "x-campaign-id":
                            case "x-campaignid":
                            case "x-mllistcampaign":
                            case "x-rpcampaign":
                                ExtendedProperties.CampaignID = headerValue;
                                break;
                            case "x-delivery-context":
                                ExtendedProperties.DeliveryContext = headerValue;
                                break;
                            case "x-maillist-id":
                                ExtendedProperties.MailListId = headerValue;
                                break;
                            case "x-msmail-priority":
                                ExtendedProperties.MSMailPriority = headerValue;
                                break;
                            case "x-originalarrivaltime":
                            case "x-original-arrival-time":
                                dateString = headerValue;

                                // Ignore extraneous datetime information.
                                dateStringParenthesis = dateString.IndexOf("(");
                                if (dateStringParenthesis > -1)
                                    dateString = dateString.Substring(0, dateStringParenthesis - 1);

                                // Remove timezone suffix.
                                if (dateString.Substring(dateString.Length - 4) == " ")
                                    dateString = dateString.Substring(0, dateString.Length - 4);

                                DateTime.TryParse(dateString, out ExtendedProperties.OriginalArrivalTime);
                                break;
                            case "x-originating-ip":
                                ExtendedProperties.OriginatingIP = headerValue;
                                break;
                            case "x-rcpt-to":
                                if (headerValue.Length > 1)
                                    ExtendedProperties.RcptTo = headerValue.Substring(1, headerValue.Length - 2);
                                break;
                            case "x-csa-complaints":
                            case "x-complaints-to":
                            case "x-reportabuse":
                            case "x-report-abuse":
                            case "x-mail_abuse_inquiries":
                                ExtendedProperties.ReportAbuse = headerValue;
                                break;
                            case "x-spam-score":
                                ExtendedProperties.SpamScore = headerValue;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            // Track all Received and X-Received headers.
            if (!string.IsNullOrEmpty(receivedText))
                receivedChain.Add(receivedText);
            ReceivedChain = receivedChain.ToArray();

            // Process the body if it's passed in.
            string body = "";

            // If common headers are unsent, we're parsing the body only.
            if (string.IsNullOrEmpty(fromText) && string.IsNullOrEmpty(Subject))
                body = messageText;
            else
            {
                HasHeaders = true;

                // Set the raw headers property if requested.
                if ((processingFlags & MailMessageProcessingFlags.IncludeRawHeaders) > 0)
                    RawHeaders = headers;

                if (cutoff > -1)
                {
                    body = messageText.Substring(cutoff + 4);

                    if (body.Length > 0)
                    {
                        // Ignore closing message indicators.
                        int closePos = body.Length;
                        if (body.EndsWith(")"))
                            closePos--;

                        // Ignore trailing linebreaks.
                        while (closePos > 1 && body.Substring(closePos - 2, 2) == "\r\n")
                            closePos -= 2;

                        if (closePos != body.Length)
                            body = body.Substring(0, closePos);
                    }
                }
            }

            if (!string.IsNullOrEmpty(body))
            {
                HasBody = true;

                // Only process MIME parts if a boundary name is passed in.
                if (!string.IsNullOrEmpty(MimeBoundaryName))
                {
                    // Parse body into MIME parts and process them.
                    MimeParts = MimePart.ExtractMIMEParts(ContentType, CharSet, ContentTransferEncoding, body, ProcessingFlags, 0);
                    if (MimeParts.Count > 0)
                        Task.Run(() => ProcessMimeParts()).Wait();
                    else
                    {
                        // We can no longer trust the external encoding, so infer if this body is Base-64 or Quoted-Printable encoded.
                        if (Functions.AppearsBase64(body))
                            ContentTransferEncoding = "base64";
                        else if (Functions.AppearsQuotedPrintable(body))
                            ContentTransferEncoding = "quoted-printable";

                        string encodedBody = body;
                        Body = Functions.Decode(body, ContentTransferEncoding, CharSet, BodyEncoding);
                        if (Body != encodedBody)
                            BodyDecoded = true;

                        if (Body.StartsWith("-----BEGIN PGP MESSAGE-----"))
                            PgpEncryptedEnvelope = true;
                        else if (Body.StartsWith("-----BEGIN PGP SIGNED MESSAGE-----"))
                            PgpSigned = true;

                        BodyContentType = ContentType;
                        if (BodyContentType.IndexOf(";") > -1)
                            BodyContentType = BodyContentType.Substring(0, BodyContentType.IndexOf(";"));

                        // Infer if text/html when no content type is specified.
                        if (BodyContentType != "text/html" && Body.ToUpper().Contains("<BODY"))
                            BodyContentType = "text/html";
                    }

                    if (!((processingFlags & MailMessageProcessingFlags.IncludeMIMEParts) > 0))
                        MimeParts = null;
                }
                else
                {
                    // If no encoding is specified (such as when requesting partial headers only), infer if this body is Base-64 or Quoted-Printable encoded.
                    if (string.IsNullOrEmpty(ContentTransferEncoding))
                    {
                        if (Functions.AppearsBase64(body))
                            ContentTransferEncoding = "base64";
                        else if (Functions.AppearsQuotedPrintable(body))
                            ContentTransferEncoding = "quoted-printable";
                    }

                    string encodedBody = body;
                    Body = Functions.Decode(body, ContentTransferEncoding, CharSet, BodyEncoding);
                    if (Body != encodedBody)
                        BodyDecoded = true;

                    if (Body.StartsWith("-----BEGIN PGP MESSAGE-----"))
                        PgpEncryptedEnvelope = true;
                    else if (Body.StartsWith("-----BEGIN PGP SIGNED MESSAGE-----"))
                        PgpSigned = true;

                    BodyContentType = ContentType;
                    if (BodyContentType.IndexOf(";") > -1)
                        BodyContentType = BodyContentType.Substring(0, BodyContentType.IndexOf(";"));

                    // Infer if text/html when no content type is specified.
                    if (BodyContentType != "text/html" && Body.ToUpper().Contains("<BODY"))
                        BodyContentType = "text/html";
                }

                // Set the raw body property if requested.
                if ((processingFlags & MailMessageProcessingFlags.IncludeRawBody) > 0)
                    RawBody = body;
            }

            // Parse String representations of addresses into MailAddress objects.
            if (fromText.Length > 0)
            {
                MailAddressCollection fromAddresses = MailAddressCollection.Parse(fromText);
                if (fromAddresses.Count > 0)
                    From = fromAddresses[0];
            }

            if (toText.Length > 0)
            {
                To = MailAddressCollection.Parse(toText);

                // Add the address to the AllRecipients collection.
                foreach (MailAddress toAddress in To)
                {
                    if (!AllRecipients.Contains(toAddress.Address))
                        AllRecipients.Add(toAddress.Address);
                }
            }

            if (ccText.Length > 0)
            {
                CC = MailAddressCollection.Parse(ccText);

                // Add the address to the AllRecipients collection.
                foreach (MailAddress ccAddress in CC)
                {
                    if (!AllRecipients.Contains(ccAddress.Address))
                        AllRecipients.Add(ccAddress.Address);
                }
            }

            if (bccText.Length > 0)
            {
                Bcc = MailAddressCollection.Parse(bccText);

                // Add the address to the AllRecipients collection.
                foreach (MailAddress bccAddress in Bcc)
                {
                    if (!AllRecipients.Contains(bccAddress.Address))
                        AllRecipients.Add(bccAddress.Address);
                }
            }

            if (replyToText.Length > 0)
            {
                ReplyToList = MailAddressCollection.Parse(replyToText);
            }
        }

        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.MailMessage class representing the message text passed in.
        /// </summary>
        /// <param name="header">The raw headers of the email message.</param>
        /// <param name="body">The raw body of the email message.</param>
        public MailMessage(string headers, string body)
        {
            new MailMessage(headers + "\r\n" + body);
        }
        #endregion Constructors

        #region Destructor
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
        #endregion Destructor

        #region Public Methods
        /// <summary>
        /// Attempt to decrypt a PGP protected message using the matching private key.
        /// </summary>
        /// <param name="decryptedMessage">If successful, the decrypted message.</param>
        /// <returns>Whether the decryption completed successfully.</returns>
        public bool DecryptPgp(Org.BouncyCastle.Bcpg.OpenPgp.PgpPrivateKey privateKey, out byte[] decryptedMessage)
        {
            string encryptedBody = "";
            
            // Process each MIME part.
            if (MimeParts != null)
            {
                for (int i = 0; i < MimeParts.Count; i++)
                {
                    MimePart mimePart = MimeParts[i];

                    // Check if the MIME part is encrypted or signed using PGP.
                    if (mimePart.Body.StartsWith("-----BEGIN PGP MESSAGE-----")){
                        encryptedBody = Functions.ReturnBetween(mimePart.Body, "-----BEGIN PGP MESSAGE-----\r\n", "\r\n-----END PGP MESSAGE-----");
                        break;
                    }
                }
            }
            else{
                if (Body.StartsWith("-----BEGIN PGP MESSAGE-----"))
                    encryptedBody = Functions.ReturnBetween(Body, "-----BEGIN PGP MESSAGE-----\r\n", "\r\n-----END PGP MESSAGE-----");
            }

            // Process an encrypted body if found.
            if (!string.IsNullOrEmpty(encryptedBody))
            {
                // Ignore the PGP headers.
                int doubleLineBreak = encryptedBody.IndexOf("\r\n\r\n");
                if (doubleLineBreak > -1)
                    encryptedBody = encryptedBody.Substring(doubleLineBreak + 4);

                // Attempt to decrypt the message and set the body if successful.
                bool decrypted = Pgp.Decrypt(Encoding.UTF8.GetBytes(encryptedBody), out decryptedMessage, privateKey);
                if (decrypted)
                {
                    // Ensure a valid encoding.
                    if (BodyEncoding == null)
                        BodyEncoding = Encoding.UTF8;

                    // Convert the byte array back to a string.
                    Body = BodyEncoding.GetString(decryptedMessage);

                    // If the body was successfully decrypted, attempt to decrypt attachments.
                    foreach (Attachment attachment in Attachments)
                    {
                        // Only process attachments with names ending in ".pgp".
                        if (attachment.Name.ToLower().EndsWith(".pgp"))
                        {
                            decrypted = Pgp.Decrypt(attachment.ContentStream, out decryptedMessage, privateKey);

                            if (decrypted)
                            {
                                attachment.ContentStream = new MemoryStream(decryptedMessage);
                                attachment.Name = attachment.Name.Substring(0, attachment.Name.Length - 4);
                            }
                        }
                    }
                }

                return decrypted;
            }
            else
            {
                decryptedMessage = null;
                return false;
            }
        }

        /// <summary>
        /// Attempt to encrypt a message using PGP with the specified private key.
        /// </summary>
        /// <param name="publicKeys">Collection of BouncyCastle public keys to be used for encryption.</param>
        /// <returns>Whether the encryption completed successfully.</returns>
        public bool EncryptPgp(IEnumerable<Org.BouncyCastle.Bcpg.OpenPgp.PgpPublicKey> publicKeys)
        {
            // Ensure a valid encoding.
            if (BodyEncoding == null)
                BodyEncoding = Encoding.UTF8;

            byte[] encryptedMessage = null;
            bool encrypted = Pgp.Encrypt(BodyEncoding.GetBytes(Body), "message", out encryptedMessage, publicKeys);

            if (encrypted)
            {
                Body = BodyEncoding.GetString(encryptedMessage);

                // If the body was successfully encrypted, attempt to encrypt attachments.
                foreach (Attachment attachment in Attachments)
                {
                    // Don't process attachments with names ending in ".pgp".
                    if (!attachment.Name.ToLower().EndsWith(".pgp"))
                    {
                        encrypted = Pgp.Encrypt(attachment.ContentStream, "message", out encryptedMessage, publicKeys);

                        if (encrypted)
                        {
                            attachment.ContentStream = new MemoryStream(encryptedMessage);
                            attachment.Name += ".pgp";
                        }
                    }
                }

                return true;
            }

            return false;
        }

        /// <summary>
        /// Initializes a populated instance of the OpaqueMail.MailMessage class representing the message in the specified file.
        /// </summary>
        /// <param name="path">The file to open for reading.</param>
        public static MailMessage LoadFile(string path)
        {
            if (File.Exists(path))
                return new MailMessage(File.ReadAllText(path));
            else
                return null;
        }

        /// <summary>
        /// Generate a multipart/mixed message containing the email's body, alternate views, and attachments.
        /// </summary>
        /// <param name="ContentTransferEncoding">Transfer encoding to use.</param>
        /// <param name="MimeBoundaryName">Text delimiting MIME message parts.</param>
        /// <param name="MimeAlternativeViewBoundaryName">Text delimiting MIME message alternative view parts.</param>
        public async Task<string> MimeEncode(string ContentTransferEncoding, string MimeBoundaryName)
        {
            // If no Content Transfer Encoding is specified, default to quoted-printable.
            if (string.IsNullOrEmpty(ContentTransferEncoding))
                ContentTransferEncoding = "quoted-printable";

            // Write out body of the message.
            StringBuilder MIMEBuilder = new StringBuilder(Constants.SMALLSBSIZE);

            MIMEBuilder.Append("This is a multi-part message in MIME format.\r\n\r\n");

            if (!string.IsNullOrEmpty(Body))
            {
                // Handle alternative views by encapsulating them in a multipart/alternative content type.
                if (AlternateViews.Count > 0)
                {
                    foreach (AlternateView alternateView in AlternateViews)
                    {
                        string mimePartContentTransferEncoding = "quoted-printable";
                        if (alternateView.TransferEncoding == TransferEncoding.Base64)
                            mimePartContentTransferEncoding = "base64";

                        MIMEBuilder.Append("--" + MimeBoundaryName + "\r\n");
                        MIMEBuilder.Append("Content-Type: " + alternateView.MediaType + "\r\n");
                        MIMEBuilder.Append("Content-Transfer-Encoding: " + mimePartContentTransferEncoding + "\r\n\r\n");

                        byte[] binaryData = new byte[alternateView.ContentStream.Length];
                        await alternateView.ContentStream.ReadAsync(binaryData, 0, binaryData.Length);

                        MIMEBuilder.Append(Functions.Encode(binaryData, mimePartContentTransferEncoding));
                        MIMEBuilder.Append("\r\n");
                    }
                }

                MIMEBuilder.Append("--" + MimeBoundaryName + "\r\n");

                if (this.IsBodyHtml)
                    MIMEBuilder.Append("Content-Type: text/html\r\n");
                else
                    MIMEBuilder.Append("Content-Type: text/plain\r\n");
                MIMEBuilder.Append("Content-Transfer-Encoding: " + ContentTransferEncoding + "\r\n\r\n");

                MIMEBuilder.Append(Functions.Encode(Body, ContentTransferEncoding));
                MIMEBuilder.Append("\r\n");
            }
            // Since we've processed the alternate views, they shouldn't be rendered again.
            this.AlternateViews.Clear();

            // MIME encode attachments.
            foreach (Attachment attachment in this.Attachments)
            {
                MIMEBuilder.Append("--" + MimeBoundaryName + "\r\n");
                if (attachment.Name.ToLower() == "smime.p7m")
                    MIMEBuilder.Append("Content-Type: application/pkcs7-mime; name=smime.p7m; smime-type=enveloped-data\r\n");
                else
                    MIMEBuilder.Append("Content-Type: application/octet-stream; file=" + attachment.Name + "\r\n");
                MIMEBuilder.Append("Content-Transfer-Encoding: base64\r\n");
                if (!string.IsNullOrEmpty(attachment.ContentId))
                    MIMEBuilder.Append("Content-ID: <" + attachment.ContentId + ">\r\n");
                MIMEBuilder.Append("Content-Disposition: attachment; filename=" + attachment.Name + "\r\n\r\n");

                byte[] binaryData = new byte[attachment.ContentStream.Length];
                await attachment.ContentStream.ReadAsync(binaryData, 0, (int)attachment.ContentStream.Length);

                // Base-64 encode the attachment.
                MIMEBuilder.Append(Functions.ToBase64String(binaryData, 0, binaryData.Length));
                MIMEBuilder.Append("\r\n");
            }
            // Since we've processed the attachments, they shouldn't be rendered again.
            this.Attachments.Clear();

            MIMEBuilder.Append("--" + MimeBoundaryName + "--");

            return MIMEBuilder.ToString();
        }

        /// <summary>
        /// Process a list of flags as returned by the IMAP server.
        /// </summary>
        /// <param name="flagsString">List of space-separated flags.</param>
        public int ParseFlagsString(string flagsString)
        {
            if (!string.IsNullOrEmpty(flagsString))
            {
                int flagCounter = 0;

                string[] flags = flagsString.Split(' ');
                foreach (string flag in flags)
                {
                    if (!string.IsNullOrEmpty(flag) && !RawFlags.Contains(flag))
                        RawFlags.Add(flag);

                    flagCounter++;

                    switch (flag.ToUpper())
                    {
                        case "\\ANSWERED":
                            Flags = Flags | Flags.Answered;
                            break;
                        case "\\DELETED":
                            Flags = Flags | Flags.Deleted;
                            break;
                        case "\\DRAFT":
                            Flags = Flags | Flags.Draft;
                            break;
                        case "\\FLAGGED":
                            Flags = Flags | Flags.Flagged;
                            break;
                        case "\\RECENT":
                            Flags = Flags | Flags.Recent;
                            break;
                        case "\\SEEN":
                            Flags = Flags | Flags.Seen;
                            break;
                    }
                }

                return flagCounter;
            }
            else
                return 0;
        }

        /// <summary>
        /// Ensure boundary names are unique.
        /// </summary>
        public void RandomizeBoundaryNames()
        {
            MimeBoundaryName = "OpaqueMail-boundary";

            string boundaryRandomness = "";
            Random randomGenerator = new Random();

            // Append 10 random characters.
            for (int i = 0; i < 10; i++)
            {
                int nextCharacter = randomGenerator.Next(1, 36);
                if (nextCharacter > 26)
                    boundaryRandomness += (char)(47 + nextCharacter);
                else
                    boundaryRandomness += (char)(64 + nextCharacter);
            }

            MimeBoundaryName += "-" + boundaryRandomness;
            SmimeSignedCmsBoundaryName += "-" + boundaryRandomness;
            SmimeTripleSignedCmsBoundaryName += "-" + boundaryRandomness;
        }

        /// <summary>
        /// Saves a text representation of the message to the file specified.
        /// </summary>
        /// <param name="path">The file to save to.</param>
        public void SaveFile(string path)
        {
            File.WriteAllText(path, RawHeaders + "\r\n\r\n" + RawBody);
        }

        /// <summary>
        /// Attempt to sign a PGP message using the specific private key.
        /// </summary>
        /// <param name="publicKey">The BouncyCastle public key associated with the signature.</param>
        /// <param name="privateKey">The BouncyCastle private key to be used for signing.</param>
        /// <param name="hashAlgorithmTag">The hash algorithm tag to use for signing.</param>
        /// <returns>Whether the signature completed successfully.</returns>
        public bool SignPgp(Org.BouncyCastle.Bcpg.OpenPgp.PgpPublicKey publicKey, Org.BouncyCastle.Bcpg.OpenPgp.PgpPrivateKey privateKey, Org.BouncyCastle.Bcpg.HashAlgorithmTag hashAlgorithmTag = Org.BouncyCastle.Bcpg.HashAlgorithmTag.Sha256)
        {
            // Ensure a valid encoding.
            if (BodyEncoding == null)
                BodyEncoding = Encoding.UTF8;

            string signature;
            bool signed = Pgp.Sign(BodyEncoding.GetBytes(Body), out signature, publicKey, privateKey, hashAlgorithmTag);

            if (signed)
            {
                Body = signature;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Attempt to verify a PGP signed message using the matching public key.
        /// </summary>
        /// <param name="publicKey">BouncyCastle public key to be used for verification.</param>
        /// <returns>Whether the message's signature is verified.</returns>
        public bool VerifyPgpSignature(Org.BouncyCastle.Bcpg.OpenPgp.PgpPublicKey publicKey)
        {
            string pgpSignedMessage = "";
            string pgpSignature = "";

            // Process each MIME part.
            if (MimeParts != null)
            {
                int pgpSignedMessageIndex = -1;
                int pgpSignatureIndex = -1;
                for (int i = 0; i < MimeParts.Count; i++)
                {
                    MimePart mimePart = MimeParts[i];

                    // Check if the MIME part is encrypted or signed using PGP.
                    if (mimePart.Body.StartsWith("-----BEGIN PGP SIGNED MESSAGE-----"))
                        pgpSignedMessageIndex = i;
                    else if (mimePart.Body.StartsWith("-----BEGIN PGP SIGNATURE-----"))
                        pgpSignatureIndex = i;
                }

                // Verify PGP signatures.
                if (pgpSignedMessageIndex > -1 && pgpSignatureIndex > -1)
                {
                    pgpSignedMessage = MimeParts[pgpSignedMessageIndex].Body;
                    pgpSignature = MimeParts[pgpSignatureIndex].Body;
                }
            }

            // If the signature isn't embedded as its own MIME part, extract from the body.
            if (string.IsNullOrEmpty(pgpSignedMessage) && string.IsNullOrEmpty(pgpSignature))
            {
                pgpSignedMessage = Functions.ReturnBetween(Body, "-----BEGIN PGP SIGNED MESSAGE-----\r\n", "\r\n-----BEGIN PGP SIGNATURE-----");
                pgpSignature = Functions.ReturnBetween(Body, "-----BEGIN PGP SIGNATURE-----\r\n", "\r\n-----END PGP SIGNATURE-----");
            }

            // If a signature is embedded, attempt to verify.
            if (!string.IsNullOrEmpty(pgpSignedMessage) && !string.IsNullOrEmpty(pgpSignature))
            {
                // Skip over PGP headers.
                int doubleLineBreak = pgpSignedMessage.IndexOf("\r\n\r\n");
                if (doubleLineBreak > -1)
                    pgpSignedMessage = pgpSignedMessage.Substring(doubleLineBreak + 4);
                doubleLineBreak = pgpSignature.IndexOf("\r\n\r\n");
                if (doubleLineBreak > -1)
                    pgpSignature = pgpSignature.Substring(doubleLineBreak + 4);

                // Verify the message with its signature.
                if (!string.IsNullOrEmpty(pgpSignedMessage) && !string.IsNullOrEmpty(pgpSignature))
                {
                    bool pgpSignatureVerified = Pgp.VerifySignature(Encoding.UTF8.GetBytes(pgpSignedMessage),
                        Encoding.UTF8.GetBytes(Functions.FromBase64(pgpSignature)), publicKey);

                    if (pgpSignatureVerified)
                        Body = pgpSignedMessage;

                    return pgpSignatureVerified;
                }
            }

            return false;
        }
        #endregion Public Methods

        #region Private Methods
        /// <summary>
        /// Iterate through each MIME part and extract relevant metadata.
        /// </summary>
        private void ProcessMimeParts()
        {
            // Keep track of S/MIME signing and envelope encryption.
            bool allMimePartsSigned = true, allMimePartsEncrypted = true, allMimePartsTripleWrapped = true;

            // Process each MIME part.
            int pgpSignatureIndex = -1;
            int pgpSignedMessageIndex = -1;
            for (int i = 0; i < MimeParts.Count; i++)
            {
                MimePart mimePart = MimeParts[i];

                int semicolon = mimePart.ContentType.IndexOf(";");
                if (semicolon > -1)
                {
                    string originalContentType = mimePart.ContentType;
                    mimePart.ContentType = mimePart.ContentType.Substring(0, semicolon);

                    if (mimePart.ContentType.ToUpper() == "MESSAGE/PARTIAL")
                    {
                        PartialMessageId = Functions.ExtractMimeParameter(originalContentType, "id");
                        int partialMessageNumber = 0;
                        if (int.TryParse(Functions.ExtractMimeParameter(originalContentType, "number"), out partialMessageNumber))
                            PartialMessageNumber = partialMessageNumber;
                    }
                }

                // Extract any signing certificates.  If this MIME part isn't signed, the overall message isn't signed.
                if (mimePart.SmimeSigned)
                {
                    if (mimePart.SmimeSigningCertificates.Count > 0 && SmimeSigningCertificate == null)
                    {
                        foreach (X509Certificate2 signingCert in mimePart.SmimeSigningCertificates)
                        {
                            if (!SmimeSigningCertificateChain.Contains(signingCert))
                            {
                                SmimeSigningCertificateChain.Add(signingCert);
                                SmimeSigningCertificate = signingCert;
                            }
                        }
                    }
                }
                else
                    allMimePartsSigned = false;

                // If this MIME part isn't marked as being in an encrypted envelope, the overall message isn't encrypted.
                if (!mimePart.SmimeEncryptedEnvelope)
                {
                    // Ignore signatures and encryption blocks when determining if everything is encrypted.
                    if (!mimePart.ContentType.StartsWith("application/pkcs7-signature") && !mimePart.ContentType.StartsWith("application/x-pkcs7-signature") && !mimePart.ContentType.StartsWith("application/pkcs7-mime"))
                        allMimePartsEncrypted = false;
                }

                // If this MIME part isn't marked as being triple wrapped, the overall message isn't triple wrapped.
                if (!mimePart.SmimeTripleWrapped)
                {
                    // Ignore signatures and encryption blocks when determining if everything is triple wrapped.
                    if (!mimePart.ContentType.StartsWith("application/pkcs7-signature") && !mimePart.ContentType.StartsWith("application/x-pkcs7-signature") && !mimePart.ContentType.StartsWith("application/pkcs7-mime"))
                        allMimePartsTripleWrapped = false;
                }

                // Set the default primary body, defaulting to text/html and falling back to any text/*.
                string contentTypeToUpper = mimePart.ContentType.ToUpper();
                if (Body.Length < 1)
                {
                    // If the MIME part is of type text/*, set it as the intial message body.
                    if (string.IsNullOrEmpty(mimePart.ContentType) || contentTypeToUpper.StartsWith("TEXT/"))
                    {
                        IsBodyHtml = contentTypeToUpper.StartsWith("TEXT/HTML");
                        Body = mimePart.Body;
                        BodyContentType = mimePart.ContentType;
                        CharSet = mimePart.CharSet;
                    }
                    else
                    {
                        // If the MIME part isn't of type text/*, treat it as an attachment.
                        MemoryStream attachmentStream = new MemoryStream(mimePart.BodyBytes);
                        Attachment attachment;
                        if (mimePart.ContentType.IndexOf("/") > -1)
                            attachment = new Attachment(attachmentStream, mimePart.Name, mimePart.ContentType);
                        else
                            attachment = new Attachment(attachmentStream, mimePart.Name);

                        attachment.ContentId = mimePart.ContentID;
                        Attachments.Add(attachment);
                    }
                }
                else
                {
                    // If the current body isn't text/html and this is, replace the default body with the current MIME part.
                    if (!ContentType.ToUpper().StartsWith("TEXT/HTML") && contentTypeToUpper.StartsWith("TEXT/HTML"))
                    {
                        // Add the previous default body as an alternate view.
                        MemoryStream alternateViewStream = new MemoryStream(Encoding.UTF8.GetBytes(Body));
                        AlternateView alternateView = new AlternateView(alternateViewStream, BodyContentType);
                        if (BodyTransferEncoding != TransferEncoding.Unknown)
                            alternateView.TransferEncoding = BodyTransferEncoding;
                        AlternateViews.Add(alternateView);

                        IsBodyHtml = true;
                        Body = mimePart.Body;
                        BodyContentType = mimePart.ContentType;
                        CharSet = mimePart.CharSet;
                    }
                    else
                    {
                        // If the MIME part isn't of type text/*, treat is as an attachment.
                        MemoryStream attachmentStream = new MemoryStream(mimePart.BodyBytes);
                        Attachment attachment;
                        if (mimePart.ContentType.IndexOf("/") > -1)
                            attachment = new Attachment(attachmentStream, mimePart.Name, mimePart.ContentType);
                        else
                            attachment = new Attachment(attachmentStream, mimePart.Name);
                        attachment.ContentId = mimePart.ContentID;
                        Attachments.Add(attachment);
                    }
                }

                // Check if the MIME part is encrypted or signed using PGP.
                if (mimePart.Body.StartsWith("-----BEGIN PGP MESSAGE-----"))
                    PgpEncryptedEnvelope = true;
                else if (mimePart.Body.StartsWith("-----BEGIN PGP SIGNED MESSAGE-----"))
                    pgpSignedMessageIndex = i;
                else if (mimePart.Body.StartsWith("-----BEGIN PGP SIGNATURE-----"))
                    pgpSignatureIndex = i;
            }

            if (pgpSignedMessageIndex > -1 && pgpSignatureIndex > -1)
                PgpSigned = true;

            // OpaqueMail optional setting for protecting the subject.
            if (SubjectEncryption && Body.StartsWith("Subject: "))
            {
                int linebreakPosition = Body.IndexOf("\r\n");
                if (linebreakPosition > -1)
                {
                    Body = Body.Substring(linebreakPosition + 2);

                    // Decode international strings and remove escaped linebreaks.
                    string subjectText = Body.Substring(9, linebreakPosition - 9);
                    Subject = Functions.DecodeMailHeader(subjectText).Replace("\r", "").Replace("\n", "");
                }
            }

            // Set the message's S/MIME attributes.
            SmimeSigned = allMimePartsSigned;
            SmimeEncryptedEnvelope = allMimePartsEncrypted;
            SmimeTripleWrapped = allMimePartsTripleWrapped;
        }
        #endregion Private Methods
    }

    /// <summary>
    /// Obsolete.  Represents an email message that was received using the ImapClient or Pop3Client classes.
    /// </summary>
    [Obsolete("Please use OpaqueMail.MailMessage instead, which supports both incoming and outbound messages.", true)]
    public class ReadOnlyMailMessage
    {
    }
}