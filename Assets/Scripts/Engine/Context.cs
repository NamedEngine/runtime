using UnityEngine;

public class BaseContex {
    public readonly LogicEngine.LogicEngineAPI EngineAPI;
    public readonly DictionaryWrapper<string, IVariable> VariableDict;
    
    public BaseContex(LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variableDict) {
        EngineAPI = engineAPI;
        VariableDict = variableDict;
    }
}

public class ArgumentLocationContext {
    public readonly BaseContex Base;
    public readonly LogicObject LogicObject;
    public readonly IValue[] PreparedOperators;
    
    public ArgumentLocationContext(BaseContex baseContex, LogicObject logicObject, IValue[] operators) {
        Base = baseContex;
        LogicObject = logicObject;
        PreparedOperators = operators;
    }
}

public class ConstrainableContext {
    public readonly BaseContex Base;
    public readonly GameObject BoundGameObject;
    public readonly IValue[] Arguments;
    
    public ConstrainableContext(BaseContex baseContex, GameObject boundGameObject, IValue[] arguments) {
        Base = baseContex;
        Arguments = arguments;
        BoundGameObject = boundGameObject;
    }
}
