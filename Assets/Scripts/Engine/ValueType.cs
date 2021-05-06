using System;
using System.Collections.Generic;
using System.Linq;

public enum ValueType {
    Int,
    Float,
    Bool,
    String,
    Null
}

public static class ValueTypeConverter {
    static readonly Dictionary<ValueType, Type> TypesDict = new Dictionary<ValueType, Type> {
        {ValueType.Int, typeof(int)},
        {ValueType.Float, typeof(float)},
        {ValueType.Bool, typeof(bool)},
        {ValueType.String, typeof(string)},
    };
    static readonly Dictionary<Type, ValueType> ValueTypesDict = TypesDict.ToReverseDictionary();
    
    public static ValueType[] ValueTypes => TypesDict.Keys.ToArray();

    public static Type GetType(ValueType valueType) {
        return TypesDict[valueType];
    }

    public static ValueType GetValueType(Type valueType) {
        return ValueTypesDict[valueType];
    }
    
    public static IVariable GetVariableByType(ValueType type, string value) {
        T DefaultOrConvert<T>(Func<string, T> converter) {
            return value == "" ? default : converter(value);
        }

        try {
            switch (type) {
                case ValueType.String:
                    return new Variable<string>(value);
                case ValueType.Int:
                    var intValue = DefaultOrConvert(Convert.ToInt32);
                    return new Variable<int>(intValue);
                case ValueType.Float:
                    var floatValue = DefaultOrConvert(Convert.ToSingle);
                    return new Variable<float>(floatValue);
                case ValueType.Bool:
                    var boolValue = DefaultOrConvert(Convert.ToBoolean);
                    return new Variable<bool>(boolValue);
                default:
                    throw new ApplicationException("This should not be possible!");
            }
        }
        catch (FormatException) {
            throw new ArgumentException("");  // TODO: move to LANG RULES
        }
    }
    
    public static IValue GetValueByType(ValueType type, string value) {
        T DefaultOrConvert<T>(Func<string, T> converter) {
            return value == "" ? default : converter(value);
        }

        try {
            switch (type) {
                case ValueType.String:
                    return new ConcreteValue<string>(value);
                case ValueType.Int:
                    var intValue = DefaultOrConvert(Convert.ToInt32);
                    return new ConcreteValue<int>(intValue);
                case ValueType.Float:
                    var floatValue = DefaultOrConvert(Convert.ToSingle);
                    return new ConcreteValue<float>(floatValue);
                case ValueType.Bool:
                    var boolValue = DefaultOrConvert(Convert.ToBoolean);
                    return new ConcreteValue<bool>(boolValue);
                default:
                    throw new ApplicationException("This should not be possible!");
            }
        }
        catch (FormatException) {
            return null;
        }
    }
};