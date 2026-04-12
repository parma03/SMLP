using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Smlp.Shared.Common.Responses;

namespace Smlp.Shared.Common.Extensions;

public static class EnumExtentions
{
    public static List<KeyValuePair<string, int>> GetEnumValuesAndDescriptions<T>()
    {
        var enumType = typeof(T);

        if (enumType.BaseType != typeof(Enum))
        {
            throw new ArgumentException("T is not System.Enum");
        }

        var enumValList = new List<KeyValuePair<string, int>>();

        foreach (var e in Enum.GetValues(typeof(T)))
        {
            var fi = e.GetType().GetField(e.ToString() ?? "");
            var attributes = (DescriptionAttribute[])fi!.GetCustomAttributes(typeof(DescriptionAttribute), false);

            enumValList.Add(new KeyValuePair<string, int>(((attributes.Length > 0) ? attributes[0].Description : e.ToString()) ?? "", (int)e));
        }

        return enumValList;
    }

    public static List<DropdownItemResponse> ToDropdownItem<T>()
    {
        var enumType = typeof(T);

        if (enumType.BaseType != typeof(Enum))
        {
            throw new ArgumentException("T is not System.Enum");
        }

        var list = new List<DropdownItemResponse>();

        foreach (var item in Enum.GetValues(typeof(T)))
        {
            var info = item.GetType().GetField(item.ToString() ?? string.Empty);
            var attributes = (DescriptionAttribute[])info!.GetCustomAttributes(typeof(DescriptionAttribute), false);

            list.Add(new DropdownItemResponse
            {
                Id = (int)item,
                Description = (attributes is not null ? ((attributes.Length > 0) ? attributes[0].Description : item.ToString()) : string.Empty) ?? ""
            });
        }

        return list;
    }

    public static List<DropdownEnumResponse<T>> ToDropdownEnum<T>()
    {
        var enumType = typeof(T);

        if (enumType.BaseType != typeof(Enum))
        {
            throw new ArgumentException("T is not System.Enum");
        }

        var list = new List<DropdownEnumResponse<T>>();

        foreach (var item in Enum.GetValues(typeof(T)))
        {
            var info = item.GetType().GetField(item.ToString() ?? string.Empty);
            var attributes = (DescriptionAttribute[])info!.GetCustomAttributes(typeof(DescriptionAttribute), false);

            list.Add(new DropdownEnumResponse<T>
            {
                Id = (T)item,
                Description = (attributes is not null ? ((attributes.Length > 0) ? attributes[0].Description : item.ToString()) : string.Empty) ?? ""
            });
        }

        return list;
    }

    public static int GetMaxValue<T>() where T : Enum
    {
        return Convert.ToInt32(Enum.GetValues(typeof(T)).Cast<T>().Max());
    }

    public static string GetDisplayName<T>(this T enumValue) where T : Enum
    {
        var memberInfo = typeof(T).GetMember(enumValue.ToString()).FirstOrDefault();

        if (memberInfo != null)
        {
            var displayAttribute = memberInfo.GetCustomAttribute<DisplayAttribute>();

            return displayAttribute?.GetName() ?? enumValue.ToString();
        }

        return enumValue.ToString();
    }
}
