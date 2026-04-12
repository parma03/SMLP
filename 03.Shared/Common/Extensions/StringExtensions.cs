using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using Smlp.Shared.Common.Constants;

namespace Smlp.Shared.Common.Extensions;

public static class StringExtensions
{
    public static string SplitWords(this string sentence)
    {
        var pattern = @"(?<=[A-Z])(?=[A-Z][a-z])|(?<=[^A-Z])(?=[A-Z])|(?<=[A-Za-z])(?=[^A-Za-z])";
        var regex = new Regex(pattern, RegexOptions.Compiled);
        var removedExistingSpace = sentence.Replace(" ", string.Empty);

        return regex.Replace(removedExistingSpace, " ");
    }

    public static string ReplaceNewLineToBr(this string text)
    {
        var pattern = @"(\r\n|\r|\n)+";
        var regex = new Regex(pattern, RegexOptions.Compiled);

        return regex.Replace(text, "<br />");
    }

    public static string Replace(this string source, IDictionary<string, string> replacements)
    {
        var result = new StringBuilder(source);

        foreach (var replacement in replacements)
        {
            result.Replace(replacement.Key, replacement.Value);
        }

        return result.ToString();
    }

    public static string ToSafeDisplayText(this string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return DefaultTextFor.Dash;
        }

        return text;
    }

    public static string ToFormatFileName(this string fileName)
    {
        // format : {type}_{person}{.}{extention}
        var arr = fileName?.Split(".");
        if (arr?.Length > 0)
        {
            fileName = string.Format("{0}_{1}{2}", arr[0], DateTime.Now.ToString("ddMMyyyy"), Path.GetExtension(fileName));
        }

        return fileName ?? "";
    }

    public static string ToCapitalizeEachWord(this string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return DefaultTextFor.Dash;
        }

        var cultInfo = new CultureInfo("en-US", false).TextInfo;
        return cultInfo.ToTitleCase(text);
    }

    public static string ToRoman(this int num)
    {
        var result = "";
        Dictionary<string, int> romanDic = new() {
            {"I", 1}, {"IV", 4}, {"V", 5}, {"IX", 9}, {"X", 10}, {"XL", 40}, {"L", 50}, {"XC", 90}, {"C", 100}, {"CD", 400}, {"D", 500}, {"CM",900}, {"M",1000}
        };

        foreach (var item in romanDic.Reverse())
        {
            if (num <= 0)
            {
                break;
            }

            while (num >= item.Value)
            {
                result += item.Key;
                num -= item.Value;
            }
        }

        return result;
    }
}
