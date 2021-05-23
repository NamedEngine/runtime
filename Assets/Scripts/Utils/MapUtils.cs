using System.Linq;
using System.Xml.Linq;

public static class MapUtils {
    public const string RectTypeName = "rectangle";
    public static string GetObjectType(XElement obj, string defaultType) {
        var notProperties = obj.Elements()
            .Where(el => el.Name != "properties")
            .ToArray();
        if (notProperties.Length == 0) {
            return defaultType;
        }

        return notProperties.First().Name.ToString();
    }
}
