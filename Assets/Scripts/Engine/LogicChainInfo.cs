using ValueInstatiator = System.Func<System.Collections.Generic.Dictionary<string, IVariable>, IValue[], IValue>;
using ChainableInstaniator = System.Func<System.Collections.Generic.Dictionary<string, IVariable>, IValue[], Chainable>;

public class LogicChainInfo {
    public readonly ValueInstatiator[] ValueInstantiators;
    public readonly ChainableInstaniator[] ChainableInstantiators;
    public readonly (int, int)[] ChainableRelations;

    public LogicChainInfo(ValueInstatiator[] valueInstantiators, ChainableInstaniator[] chainableInstantiators,
        (int, int)[] chainableRelations) {
        ValueInstantiators = valueInstantiators;
        ChainableInstantiators = chainableInstantiators;
        ChainableRelations = chainableRelations;
    }
}