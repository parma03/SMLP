using System.ComponentModel;
using System.Reflection;
using Smlp.Base.Common.Attributes;

namespace Smlp.Shared.Common.Extensions;

public static class DescriptionAttributeExtensions
{
    public static string GetDescription(this Enum enumValue)
    {
        var enumValueName = enumValue.ToString();

        var enumFieldInfo = enumValue.GetType().GetField(enumValueName);

        if (enumFieldInfo is null)
        {
            return enumValueName;
        }

        var descriptionAttributes = enumFieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

        if (descriptionAttributes is null)
        {
            return enumValueName;
        }

        if (!descriptionAttributes.Any())
        {
            return enumValueName;
        }

        return ((DescriptionAttribute)descriptionAttributes.First()).Description;
    }

    public static string GetCss(this Enum enumValue)
    {
        var enumValueName = enumValue.ToString();

        var enumFieldInfo = enumValue.GetType().GetField(enumValueName);

        if (enumFieldInfo is null)
        {
            return enumValueName;
        }

        var cssAttributes = enumFieldInfo.GetCustomAttributes(typeof(CssAttribute), false);

        if (cssAttributes is null)
        {
            return enumValueName;
        }

        if (!cssAttributes.Any())
        {
            return enumValueName;
        }

        return ((CssAttribute)cssAttributes.First()).Name;
    }

    public static string GetDescription<T>(this T type, string memberName) where T : class
    {
        var memberInfo = type.GetType().GetMember(memberName)[0];
        var descriptionAttributes = memberInfo.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);

        if (descriptionAttributes is null)
        {
            return memberName;
        }

        if (!descriptionAttributes.Any())
        {
            return memberName;
        }

        return ((DescriptionAttribute)descriptionAttributes.First()).Description;
    }

    public static string GetTypeDescription<T>(this T type) where T : class
    {
        var descriptionAttributes = type.GetType().GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);

        if (descriptionAttributes is null)
        {
            return type.GetType().ToString();
        }

        if (!descriptionAttributes.Any())
        {
            return type.GetType().ToString();
        }

        return ((DescriptionAttribute)descriptionAttributes.First()).Description;
    }

    public static string GetConstFieldDescription<T>(string fieldName)
    {
        var fieldInfo = typeof(T).GetField(fieldName, BindingFlags.Public | BindingFlags.Static);

        if (fieldInfo == null)
        {
            return fieldName;
        }

        var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), inherit: false);

        if (descriptionAttributes is null)
        {
            return fieldName;
        }

        if (!descriptionAttributes.Any())
        {
            return fieldName;
        }

        return ((DescriptionAttribute)descriptionAttributes.First()).Description;
    }

    public static int GetIntValue(this Enum enumValue)
    {
        return Convert.ToInt32(enumValue);
    }

    //public static string GetDescription(this object field)
    //{
    //    var fieldName = field.ToString();

    //    if (fieldName == null)
    //    {
    //        return string.Empty;
    //    }

    //    var fieldInfo = field.GetType().GetField(fieldName);

    //    if (fieldInfo == null)
    //    {
    //        return fieldName;
    //    }

    //    var descriptionAttributes = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

    //    if (descriptionAttributes is null)
    //    {
    //        return fieldName;
    //    }

    //    if (!descriptionAttributes.Any())
    //    {
    //        return fieldName;
    //    }

    //    return ((DescriptionAttribute)descriptionAttributes.First()).Description;
    //}
}
