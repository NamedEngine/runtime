using System;
using System.Collections.Generic;
using System.Linq;

public enum NodeType {
    Class,
    ClassRef,
    State,
    Variable,
    VariableRef,
    Operator,
    Condition,
    Action,
    Parameter
}

public static class NodeTypeConverter {
    public static readonly Dictionary<string, NodeType> NodeTypesByNotation = Enum
        .GetValues(typeof(NodeType))
        .Cast<NodeType>()
        .ToDictionary(t => t.ToString().StartWithLower(), t => t);
}