using OperatorInstantiator = System.Func<LogicObject, LogicEngine.LogicEngineAPI, DictionaryWrapper<string, IVariable>, IValue[], IValue>;
using ChainableInstantiator = System.Func<LogicObject, LogicEngine.LogicEngineAPI, DictionaryWrapper<string, IVariable>, IValue[], Chainable>;

public class LogicChainInfo {
    public readonly OperatorInstantiator[] OperatorInstantiators;
    public readonly ChainableInstantiator[] ChainableInstantiators;
    public readonly (int, int)[] ChainableRelations;

    public LogicChainInfo(OperatorInstantiator[] operatorInstantiators, ChainableInstantiator[] chainableInstantiators,
        (int, int)[] chainableRelations) {
        OperatorInstantiators = operatorInstantiators;
        ChainableInstantiators = chainableInstantiators;
        ChainableRelations = chainableRelations;
    }
}
