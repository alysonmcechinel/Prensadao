using System.ComponentModel;
using System.Reflection;

namespace Prensadao.Application.Helpers;

public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());

        if (field == null)
            return value.ToString();

        var attribute = field.GetCustomAttributes<DescriptionAttribute>().FirstOrDefault();
        return attribute?.Description ?? value.ToString();
    }
}
