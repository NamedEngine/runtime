using OperatorInstantiator = System.Func<ArgumentLocationContext, IValue>;
using ChainableInstantiator = System.Func<ArgumentLocationContext, Chainable>;

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
