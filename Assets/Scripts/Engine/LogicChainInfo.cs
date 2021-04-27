using OperatorInstantiator = System.Func<System.Collections.Generic.Dictionary<string, IVariable>, IValue[], IValue>;
using ChainableInstantiator = System.Func<System.Collections.Generic.Dictionary<string, IVariable>, IValue[], Chainable>;

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