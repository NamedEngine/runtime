using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public enum ValueType {
    Int,
    Float,
    Bool,
    String
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
};