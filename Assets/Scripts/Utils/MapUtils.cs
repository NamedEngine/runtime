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

    public static readonly MapObjectParameter EmptyClassParameter = new MapObjectParameter {
        Name = "Class",
        Type = ValueType.String,
        Value = nameof(Language.Classes.Empty)
    };

    public static MapObjectParameter GetClassParameter(MapObjectInfo info) {
        var classParameter = info.Parameters.FirstOrDefault(p => p.Name == EmptyClassParameter.Name);
        if (classParameter.IsDefault()) {
            classParameter = EmptyClassParameter;
        }

        return classParameter;
    }
}
