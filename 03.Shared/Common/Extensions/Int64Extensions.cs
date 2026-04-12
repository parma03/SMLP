namespace Smlp.Shared.Common.Extensions;

public static class Int64Extensions
{
    public static string ToReadableFileSize(this long fileSize)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        var length = Convert.ToDouble(fileSize);
        var order = 0;

        while (length >= 1024 && order < sizes.Length - 1)
        {
            order++;
            length /= 1024;
        }

        return string.Format("{0:0.##} {1}", length, sizes[order]);
    }

    public static int GetAge(DateTime birthDate)
    {
        var n = DateTime.Now;
        var age = n.Year - birthDate.Year;

        if (n.Month < birthDate.Month || (n.Month == birthDate.Month && n.Day < birthDate.Day))
        {
            age--;
        }

        return age;
    }

    public static int StringToInt(string stringValue)
    {
        if (int.TryParse(stringValue, out var value))
        {
            return value;
        }
        else
        {
            return 0;
        }
    }
}
