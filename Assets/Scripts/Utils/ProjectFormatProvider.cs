using System;
using System.Globalization;

public class ProjectFormatProvider : IFormatProvider {
    readonly IFormatProvider _provider = new CultureInfo("en-US");
    
    public object GetFormat(Type formatType) {
        return _provider.GetFormat(formatType);
    }
}