using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ctl
{

    /// <summary>
    /// Provides support for RFC3966: "The tel URI for Telephone Numbers".
    /// </summary>
    public class TelephoneUri
    {
        static readonly Regex parseRe, parseGenericParam, validateNumberRe, validateParameterNameRe, validateParameterValueRe, validateIsdnSubAddressRe, validateExtensionRe, validateContextRe;
        readonly ParameterCollection parameters = new ParameterCollection();
        string number, extension, isdnSubaddress, context;

        /// <summary>
        /// The phone number. Can either be a local or a global number.
        /// </summary>
        public string Number
        {
            get => number;
            set
            {
                number = validateNumberRe.IsMatch(value) ? value : throw new ArgumentException($"The given value '{value}' is not a valid phone number.");
            }
        }

        /// <summary>
        /// If true, the number is a global number.
        /// </summary>
        public bool IsGlobalNumber => Number?.Length > 0 && Number[0] == '+';

        /// <summary>
        /// The local phone number. If the URI is a global number, an exception is thrown.
        /// </summary>
        public string LocalNumber => IsGlobalNumber == false ? Number : throw new Exception($"The {nameof(TelephoneUri)} is holding a local number.");

        /// <summary>
        /// The global phone number. If the URI is a local number, an exception is thrown.
        /// </summary>
        public string GlobalNumber => IsGlobalNumber ? Number : throw new Exception($"The {nameof(TelephoneUri)} is holding a global number.");

        /// <summary>
        /// The phone's extension number.
        /// </summary>
        /// <remarks>
        /// The extension number is specified through the 'ext' parameter, and is mutually exclusive with specifying an ISDN sub-address.
        /// </remarks>
        public string Extension
        {
            get => extension;
            set
            {
                extension = validateExtensionRe.IsMatch(value) ? value : throw new ArgumentException($"The given extension '{value}' is malformed.");
            }
        }

        /// <summary>
        /// The number's ISDN sub-address.
        /// </summary>
        /// <remarks>
        /// The ISDN sub-address is specified through the 'isub' parameter, and is mutually exclusive with specifying an extension.
        /// </remarks>
        public string IsdnSubaddress
        {
            get => isdnSubaddress;
            set
            {
                isdnSubaddress = validateIsdnSubAddressRe.IsMatch(value) ? value : throw new ArgumentException($"The given ISDN sub-address '{value}' is malformed.");
            }
        }

        /// <summary>
        /// The local number's context.
        /// </summary>
        /// <remarks>
        /// The local context is specified through the 'phone-context' parameter, and must be specified if the phone number is local.
        /// </remarks>
        public string LocalContext
        {
            get => context;
            set
            {
                context = validateContextRe.IsMatch(value) ? value : throw new ArgumentException($"The given local number context '{value}' is malformed.");
            }
        }

        /// <summary>
        /// A collection of unspecified general parameters for the phone number.
        /// </summary>
        public IDictionary<string, string> Parameters => parameters;

        /// <summary>
        /// Returns a string that represents the current phone number.
        /// </summary>
        /// <returns>A string representing the current phone number.</returns>
        public override string ToString()
        {
            // validate.

            if (string.IsNullOrEmpty(Number))
            {
                return "{empty}";
            }

            StringBuilder validationErrors = new StringBuilder();

            void AddError(string error)
            {
                if (validationErrors == null)
                {
                    validationErrors = new StringBuilder($"The {nameof(TelephoneUri)} is not valid:");
                }
                else
                {
                    validationErrors.Append(", ");
                }

                validationErrors.Append(error);
            }

            if (!IsGlobalNumber && string.IsNullOrEmpty(LocalContext))
            {
                AddError($"The {nameof(Number)} property contains a local number, requiring the {nameof(LocalContext)} property to be filled.");
            }

            if (IsGlobalNumber && !string.IsNullOrEmpty(LocalContext))
            {
                AddError($"The {nameof(Number)} property contains a global number, requiring the {nameof(LocalContext)} property to be null.");
            }

            if (!string.IsNullOrEmpty(Extension) && !string.IsNullOrEmpty(IsdnSubaddress))
            {
                AddError($"The {nameof(Extension)} and {nameof(IsdnSubaddress)} properties are mutually exclusive: only one may be defined.");
            }

            if (validationErrors != null)
            {
                throw new InvalidOperationException(validationErrors.ToString());
            }

            StringBuilder sb = new StringBuilder();

            sb.Append("tel:").Append(Number);

            if (!string.IsNullOrEmpty(Extension))
            {
                sb.Append(";ext=").Append(Extension);
            }

            if (!string.IsNullOrEmpty(IsdnSubaddress))
            {
                sb.Append(";isub=").Append(IsdnSubaddress);
            }

            if (!string.IsNullOrEmpty(LocalContext))
            {
                sb.Append(";phone-context=").Append(LocalContext);
            }

            foreach (var kvp in parameters)
            {
                sb.Append(';').Append(kvp.Key);

                if (!string.IsNullOrEmpty(kvp.Value))
                {
                    sb.Append('=').Append(kvp.Value);
                }
            }

            return sb.ToString();
        }

        TelephoneUri()
        {
        }

        /// <summary>
        /// Initializes a new telephone URI.
        /// </summary>
        /// <param name="uri">A string to parse as a telephone URI.</param>
        public TelephoneUri(string uri)
        {
            TryParseImpl(uri, this, true);
        }

        /// <summary>
        /// Tries to parse a new telephone URI.
        /// </summary>
        /// <param name="uri">A string to parse as a telephone URI.</param>
        /// <param name="telUri">If successful, receives the initialized URI. If unsuccessful, receives null.</param>
        /// <returns>If successful, true. If unsuccessful, false.</returns>
        public static bool TryParse(string uri, out TelephoneUri telUri)
        {
            TelephoneUri newUri = new TelephoneUri();

            if (TryParseImpl(uri, newUri, false))
            {
                telUri = newUri;
                return true;
            }

            telUri = null;
            return false;
        }

        static bool TryParseImpl(string uri, TelephoneUri newUri, bool throwOnError)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));
            if (newUri == null) throw new ArgumentNullException(nameof(newUri));

            Match m = parseRe.Match(uri);

            if (!m.Success)
            {
                return throwOnError ? throw new ArgumentException("The given string is not a valid telephone URI", nameof(uri)) : false;
            }

            StringBuilder validationErrors = null;

            void AddError(string error)
            {
                if (validationErrors == null)
                {
                    validationErrors = new StringBuilder("The given string is not a valid telephone URI: ");
                }
                else
                {
                    validationErrors.Append(", ");
                }

                validationErrors.Append(error);
            }

            Group gnum = m.Groups["gnum"];
            newUri.Number = gnum.Success ? gnum.Value : m.Groups["lnum"].Value;

            Group ext = m.Groups["ext"];
            if (ext.Success)
            {
                if (ext.Captures.Count > 1)
                {
                    AddError("URI must have at most one extension.");
                }

                newUri.Extension = ext.Value;
            }

            Group isub = m.Groups["isub"];
            if (isub.Success)
            {
                if (isub.Captures.Count > 1)
                {
                    AddError("URI must have at most one ISDN sub-address.");
                }

                newUri.IsdnSubaddress = isub.Value;
            }

            Group context = m.Groups["context"];
            if (context.Success)
            {
                if (context.Captures.Count > 1)
                {
                    AddError("URI must have at most one local context.");
                }

                newUri.LocalContext = context.Value;
            }

            foreach (Capture c in m.Groups["gparam"].Captures)
            {
                Match pm = parseGenericParam.Match(c.Value);

                if (!pm.Success)
                {
                    // this should never happen, and indicates the below regex is bad.
                    throw new Exception("Parameter parser inconsistency.");
                }

                string pname = pm.Groups["pname"].Value;

                Group pvalGroup = pm.Groups["pval"];
                string pval = pvalGroup.Success ? pvalGroup.Value : null;

                try
                {
                    newUri.Parameters.Add(pname, pval);
                }
                catch (ArgumentException)
                {
                    AddError($"URI has duplicate parameter '{pname}'.");
                }
            }

            if (gnum.Success == false && newUri.LocalContext == null)
            {
                AddError("Local URIs must have a context.");
            }

            if (gnum.Success == true && newUri.LocalContext != null)
            {
                AddError("Global URIs must not have a local context.");
            }

            if (newUri.IsdnSubaddress != null && newUri.Extension != null)
            {
                AddError("URI must not have both an ISDN sub-address and an extension.");
            }

            return
                validationErrors == null ? true :
                throwOnError == false ? false :
                throw new ArgumentException(validationErrors.ToString(), nameof(uri));
        }

        static TelephoneUri()
        {
            string digit = "0-9";
            string hexDigit = "0-9a-fA-F";
            string visualSeparator = "\\-.()";
            string alphanum = "0-9a-zA-Z";

            string paramUnreserved = "[\\]/:&+$";
            string unreserved = $"{alphanum}\\-_.!~*'()";
            string reserved = ";/?:@&=+$,";

            string pctEncoded = $"(?:%[{hexDigit}]{{2}})";

            string domainLabel = $"[{alphanum}](?:[{alphanum}-]*[{alphanum}])?";
            string topLevel = $"[a-zA-Z](?:[{alphanum}-]*[{alphanum}])?";
            string domainName = $"(?:{domainLabel}[.])*{topLevel}[.]?";

            string globalNumber = $"\\+[{digit}{visualSeparator}]*[{digit}][{digit}{visualSeparator}]*";
            string localNumber = $"[{hexDigit}*#{visualSeparator}]*[{hexDigit}*#][{hexDigit}*#{visualSeparator}]*";
            string genericParameterName = $"[{alphanum}]+";
            string genericParameterValue = $"(?:[{paramUnreserved}{unreserved}]|{pctEncoded})+";
            string genericParameter = $"(?<gparam>{genericParameterName}(?:={genericParameterValue})?)";
            string genericParameterWithNames = $"^(?<pname>{genericParameterName})(?:=(?<pval>{genericParameterValue}))?$";
            string isdnSubAddressValue = $"(?:[{reserved}{unreserved}]|{pctEncoded})+";
            string isdnSubAddressParameter = $"isub=(?<isub>{isdnSubAddressValue})";
            string extensionValue = $"[{digit}{visualSeparator}]+";
            string extensionParameter = $"ext=(?<ext>{extensionValue})";
            string contextValue = $"(?:{domainName})|(?:{globalNumber})";
            string contextParameter = $"phone-context=(?<context>{contextValue})";

            string anyParameter = $"(?:{isdnSubAddressParameter})|(?:{extensionParameter})|(?:{contextParameter})|(?:{genericParameter})";

            string uri = $"^tel:(?:(?<gnum>{globalNumber})|(?<lnum>{localNumber}))(?:;(?:{anyParameter}))*$";

            parseRe = new Regex(uri, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            parseGenericParam = new Regex(genericParameterWithNames, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            validateNumberRe = new Regex($"^(?:{globalNumber})|(?:{localNumber})$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            validateParameterNameRe = new Regex($"^{genericParameterName}$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            validateParameterValueRe = new Regex($"^{genericParameterValue}$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            validateIsdnSubAddressRe = new Regex($"^{isdnSubAddressValue}$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            validateExtensionRe = new Regex($"^{extensionValue}$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            validateContextRe = new Regex($"^{contextValue}$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

        sealed class ParameterCollection : SortedDictionary<string, string>, IDictionary<string, string>, ICollection<KeyValuePair<string, string>>
        {
            public ParameterCollection()
                : base(StringComparer.OrdinalIgnoreCase)
            {
            }

            public new void Add(string key, string value)
            {
                if (key == null) throw new ArgumentNullException(nameof(key));

                bool badKey = !validateParameterNameRe.IsMatch(key);

                string parameterToUse;

                switch (key.ToLowerInvariant())
                {
                    case "ext":
                        parameterToUse = nameof(Extension);
                        break;
                    case "isub":
                        parameterToUse = nameof(IsdnSubaddress);
                        break;
                    case "phone-context":
                        parameterToUse = nameof(LocalContext);
                        break;
                    default:
                        parameterToUse = null;
                        break;
                }

                bool badValue = !string.IsNullOrEmpty(value) && !validateParameterValueRe.IsMatch(value);

                if (badKey || parameterToUse != null || badValue)
                {
                    StringBuilder sb = new StringBuilder("The parameter is invalid: ");

                    if (parameterToUse != null)
                    {
                        sb.Append($"the key '{nameof(key)}' must be added through the {parameterToUse} parameter.");
                    }
                    else
                    {
                        if (badKey)
                        {
                            sb.Append($"the key '{nameof(key)}' is malformed.");
                        }

                        if (badValue)
                        {
                            if (badKey) sb.Append(", ");
                            sb.Append($"the value '{nameof(value)}' is malformed.");
                        }
                    }

                    throw new ArgumentException(sb.ToString());
                }

                base.Add(key, value);
            }

            void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
            {
                Add(item.Key, item.Value);
            }
        }
    }
}
