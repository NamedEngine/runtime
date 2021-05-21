using UnityEngine;

public class BaseContext {
    public readonly LogicEngine.LogicEngineAPI EngineAPI;
    public readonly DictionaryWrapper<string, IVariable> VariableDict;
    public readonly string Name;
    
    public BaseContext(LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variableDict, string name) {
        EngineAPI = engineAPI;
        VariableDict = variableDict;
        Name = name;
    }
}

public class ArgumentLocationContext {
    public readonly BaseContext Base;
    public readonly LogicObject LogicObject;
    public readonly IValue[] PreparedOperators;
    
    public ArgumentLocationContext(BaseContext baseContex, LogicObject logicObject, IValue[] operators) {
        Base = baseContex;
        LogicObject = logicObject;
        PreparedOperators = operators;
    }
}

public class ConstrainableContext {
    public readonly BaseContext Base;
    public readonly GameObject BoundGameObject;
    public readonly IValue[] Arguments;
    
    public ConstrainableContext(BaseContext baseContex, GameObject boundGameObject, IValue[] arguments) {
        Base = baseContex;
        Arguments = arguments;
        BoundGameObject = boundGameObject;
    }

    public ConstrainableContext UpdateArguments(IValue[] newArguments) {
        return new ConstrainableContext(Base, BoundGameObject, newArguments);
    }
}
