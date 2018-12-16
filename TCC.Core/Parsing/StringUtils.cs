﻿using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace TCC.Parsing
{

    public static class StringUtils
    {
        public static string ReplaceHtmlEscapes(string msg)
        {
            msg = msg.Replace("&lt;", "<");
            msg = msg.Replace("&gt;", ">");
            msg = msg.Replace("&#xA", "\n");
            msg = msg.Replace("&quot;", "\"");
            return msg;
        }

        public static byte[] StringToByteArray(string hex)
        {
            var numberChars = hex.Length / 2;
            var bytes = new byte[numberChars];
            using (var sr = new StringReader(hex))
            {
                for (var i = 0; i < numberChars; i++)
                    bytes[i] =
                      Convert.ToByte(new string(new[] { (char)sr.Read(), (char)sr.Read() }), 16);
            }
            return bytes;
        }
        public static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static long Hex8BStringToInt(string hex)
        {
            var sb = new StringBuilder();
            for (var i = 16 - 2; i >= 0; i -= 2)
            {
                sb.Append(hex[i]);
                sb.Append(hex[i + 1]);
            }
            var result = Convert.ToInt64(sb.ToString(), 16);
            return result;
        }
        public static float Hex4BStringToFloat(string hex)
        {
            var sb = new StringBuilder();
            for (var i = 8 - 2; i >= 0; i -= 2)
            {
                sb.Append(hex[i]);
                sb.Append(hex[i + 1]);
            }
            var num = UInt32.Parse(sb.ToString(), NumberStyles.AllowHexSpecifier);
            var floatVals = BitConverter.GetBytes(num);
            var result = BitConverter.ToSingle(floatVals, 0);
            return result;
        }

        public static int Hex4BStringToInt(string hex)
        {
            var sb = new StringBuilder();
            for (var i = 8 - 2; i >= 0; i -= 2)
            {
                sb.Append(hex[i]);
                sb.Append(hex[i + 1]);
            }
            var result = Convert.ToInt32(sb.ToString(), 16);
            return result;
        }
        public static int Hex2BStringToInt(string hex)
        {
            var sb = new StringBuilder();
            for (var i = 4 - 2; i >= 0; i -= 2)
            {
                sb.Append(hex[i]);
                sb.Append(hex[i + 1]);
            }
            var result = Convert.ToInt32(sb.ToString(), 16);
            return result;
        }
        public static int Hex1BStringToInt(string hex)
        {
            var sb = new StringBuilder();
            sb.Append(hex[0]);
            sb.Append(hex[1]);
            var result = Convert.ToInt32(sb.ToString(), 16);
            return result;
        }
        public static int GetStringEnd(string s, int startIndex, string terminator)
        {
            var endIndex = startIndex;
            var zeroes = false;
            while (!zeroes)
            {
                if (endIndex + 3 <= s.Length)
                {
                    var test = s[endIndex + 0].ToString() +
                                  s[endIndex + 1].ToString() +
                                  s[endIndex + 2].ToString() +
                                  s[endIndex + 3].ToString();

                    if (test == terminator)
                    {
                        zeroes = true;
                    }
                    else
                    {
                        endIndex += 4;
                    }
                }
                else { return startIndex; }
            }
            return endIndex;
        }

        public static string GetStringFromHex(string hex, int startIndex, string terminator)
        {
            var builder = new StringBuilder();
            for (var i = startIndex; i < GetStringEnd(hex, startIndex, terminator); i += 2)
            {
                builder.Append(hex[i].ToString() + hex[i + 1].ToString());
            }
            builder.Replace("00", "");
            var b = StringToByteArray(builder.ToString());
            return Encoding.UTF7.GetString(b);
        }
    }
}
