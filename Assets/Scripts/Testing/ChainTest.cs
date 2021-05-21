using System.Collections;
using Actions;
using Conditions;
using UnityEngine;
using OperatorInstantiator = System.Func<ArgumentLocationContext, IValue>;
using ChainableInstantiator = System.Func<ArgumentLocationContext, Chainable>;

public class ChainTest : MonoBehaviour {
    LogicChain _chain;
    void Start() {
        _chain = gameObject.AddComponent<LogicChain>();

        ChainableInstantiator c0 = context => new DummyTrueCondition(null, false);
        ChainableInstantiator c1 = context => new DummySyncAction1(null, false);
        ChainableInstantiator c2 = context => new DummyAsyncAction1(null, false);
        ChainableInstantiator c3 = context => new DummyFalseCondition(null, false);
        ChainableInstantiator c4 = context => new DummySyncAction2(null, false);

        var instantiators = new [] {c1, c2, c3, c4, c0};
        var relations = new [] {
            (4, 0),
            (4, 1),
            (0, 2),
            (1, 2),
            (2, 3)
        };

        var chainInfo = new LogicChainInfo(new OperatorInstantiator[]{}, instantiators, relations);
        _chain.SetupChain(null, new BaseContext(null,null, null), chainInfo);
        _chain.Execute();

        StartCoroutine(CopyTest());
    }

    IEnumerator CopyTest() {
        yield return new WaitForSeconds(3);
        Debug.Log("LESS COPY");
        var chainCopy = _chain.Clone(null, new BaseContext(null,null, null), gameObject);
        chainCopy.Execute();
    }
}
