using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using ImageProcessorCore;

namespace Folke.Forum.Infrastructure
{
    public class HtmlSanitizer
    {
        public string Output { get; private set; }

        public Dictionary<string, Image> Images { get; }

        private static bool IsSpace(int c)
        {
            return c == ' ' || c == '\r' || c == '\n' || c == '\t';
        }

        private static bool IsAlpha(int c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }

        private static readonly Dictionary<string, bool> allowedTags = new Dictionary<string, bool>();
        private static readonly Dictionary<string, string> allowedAttributes = new Dictionary<string, string>();

        static HtmlSanitizer()
        {
            allowedTags["div"] = true;
            allowedTags["b"] = true;
            allowedTags["strong"] = true;
            allowedTags["i"] = true;
            allowedTags["a"] = true;
            allowedTags["p"] = true;
            allowedTags["br"] = true;
            allowedTags["img"] = true;
            allowedTags["em"] = true;
            allowedTags["ul"] = true;
            allowedTags["li"] = true;
            allowedTags["ol"] = true;
            allowedTags["blockquote"] = true;
            allowedTags["font"] = true;
            allowedAttributes["a"] = "href";
            allowedAttributes["font"] = "size";
        }

        public HtmlSanitizer(string input)
        {
            Images = new Dictionary<string, Image>();

            var output = new StringBuilder();
            var stream = new StringReader(input);
            int chr;
            while ((chr = stream.Read()) >= 0)
            {
                if (chr == '<')
                {
                    bool closing = false;
                    //Début d'une balise
                    var tagName = new StringBuilder();
                    int tagChar;
                    bool startWithSpaces = false;
                    while ((tagChar = stream.Read()) >= 0)
                    {
                        if (tagChar == '/' && tagName.Length == 0)
                        {
                            closing = true;
                        }
                        else if (IsSpace(tagChar))
                        {
                            if (tagName.Length > 0)
                                break;
                            startWithSpaces = true;
                        }
                        else if (IsAlpha(tagChar))
                        {
                            tagName.Append((char)tagChar);
                        }
                        else
                        {
                            break;
                        }
                    }

                    var tagNameValue = tagName.ToString().ToLowerInvariant();
                    if (allowedTags.ContainsKey(tagNameValue) && tagChar >= 0)
                    {
                        output.Append('<');
                        if (closing)
                        {
                            output.Append('/');
                            output.Append(tagNameValue);
                            output.Append('>');
                        }
                        else
                        {
                            while (IsSpace(tagChar))
                                tagChar = stream.Read();

                            output.Append(tagNameValue);
                            while (tagChar != '/' && tagChar != '>' && tagChar >= 0)
                            {
                                var attributeName = new StringBuilder();
                                attributeName.Append((char)tagChar);
                                while ((tagChar = stream.Read()) >= 0)
                                {
                                    if (!IsAlpha(tagChar))
                                        break;
                                    attributeName.Append((char)tagChar);
                                }
                                while (IsSpace(tagChar))
                                    tagChar = stream.Read();
                                if (tagChar != '=')
                                    continue;
                                tagChar = stream.Read();
                                while (IsSpace(tagChar))
                                    tagChar = stream.Read();
                                if (tagChar != '"')
                                    continue;
                                var value = new StringBuilder();
                                while ((tagChar = stream.Read()) >= 0)
                                {
                                    if (tagChar == '"')
                                        break;
                                    value.Append((char)tagChar);
                                }

                                if (tagChar == '"')
                                    tagChar = stream.Read();

                                while (IsSpace(tagChar))
                                    tagChar = stream.Read();

                                var attributeNameValue = attributeName.ToString().ToLowerInvariant();

                                if (allowedAttributes.ContainsKey(tagNameValue) && attributeNameValue == allowedAttributes[tagNameValue])
                                {
                                    output.Append(" ");
                                    output.Append(attributeNameValue);
                                    output.Append("=\"");
                                    output.Append(value);
                                    output.Append("\"");
                                }
                                else if (tagNameValue == "img" && attributeNameValue == "src")
                                {
                                    var valueValue = value.ToString();
                                    if (valueValue.IndexOf("http://", StringComparison.Ordinal) == 0 || valueValue.IndexOf("https://", StringComparison.Ordinal) == 0)
                                    {
                                        output.Append(" src=\"");
                                        output.Append(valueValue);
                                        output.Append("\"");
                                    }
                                    else
                                    {
                                        var match = Regex.Match(valueValue, @"data:(.*?);base64,(.*)");
                                        if (match.Success)
                                        {
                                            var scheme = match.Groups[1].Value;
                                            var data = match.Groups[2].Value;
                                            if (scheme == "image/jpeg" || scheme == "image/png")
                                            {
                                                var md5 = MD5.Create();
                                                var binary = Convert.FromBase64String(data);
                                                var baseName = BitConverter.ToString(md5.ComputeHash(binary, 0, binary.Length));
                                                var fullSizeImage = new Image(new MemoryStream(binary));
                                                string extension = fullSizeImage.CurrentImageFormat.Encoder.Extension;
                                                if (extension != null)
                                                {
                                                    string fileName = baseName + "." + extension;
                                                    string url = "/Content/upload/" + fileName;
                                                    Images[fileName] = fullSizeImage;
                                                    output.Append(" src=\"");
                                                    output.Append(url);
                                                    output.Append("\"");
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            if (tagChar == '/' || tagChar == '>')
                                output.Append((char)tagChar);
                        }
                    }
                    else
                    {
                        output.Append("&lt;");
                        if (startWithSpaces)
                            output.Append(" ");
                        if (closing)
                            output.Append('/');
                        output.Append(tagName);
                        if (tagChar != -1)
                            output.Append((char)tagChar);
                    }
                    if (tagChar == -1)
                        break;
                }
                else
                    output.Append((char)chr);
            }
            Output = output.ToString();
        }
    }
}
