using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VariableDictionary = System.Collections.Generic.Dictionary<string, IVariable>;
using ValueInstatiator = System.Func<System.Collections.Generic.Dictionary<string, IVariable>, IValue[], IValue>;
using ChainableInstaniator = System.Func<System.Collections.Generic.Dictionary<string, IVariable>, IValue[], Chainable>;

public class ChainTest : MonoBehaviour {
    LogicChain _chain;
    void Start() {
        _chain = gameObject.AddComponent<LogicChain>();
        
        // var c0 = new DummyTrueCondition(new List<IValue>());
        // var c1 = new DummySyncAction1(new List<IValue>());
        // var c2 = new DummyAsyncAction1(new List<IValue>());
        // var c3 = new DummyFalseCondition(new List<IValue>());
        // var c4 = new DummySyncAction2(new List<IValue>());
        // var c5 = new DummyAsyncAction2(new List<IValue>());
        //
        // // c0.AddChild(c1);
        // // c0.AddChild(c2);
        // // c1.AddChild(c3);
        // // c2.AddChild(c3);
        // // c3.AddChild(c4);
        //
        // c0.AddChild(c5);
        // c0.AddChild(c2);
        // c5.AddChild(c4);
        // c2.AddChild(c1);
        // c4.AddChild(c3);
        // c1.AddChild(c3);
        //
        // _chain.SetRoot(c0);

        ChainableInstaniator c0 = (variables, values) => new DummyTrueCondition(new IValue[] { });
        ChainableInstaniator c1 = (variables, values) => new DummySyncAction1(new IValue[] { });
        ChainableInstaniator c2 = (variables, values) => new DummyAsyncAction1(new IValue[] { });
        ChainableInstaniator c3 = (variables, values) => new DummyFalseCondition(new IValue[] { });
        ChainableInstaniator c4 = (variables, values) => new DummySyncAction2(new IValue[] { });
        ChainableInstaniator c5 = (variables, values) => new DummyAsyncAction2(new IValue[] { });
        var instantiators = new [] {c0, c1, c2, c3, c4, c5};
        var relations = new [] {
            (0, 1),
            (0, 2),
            (1, 3),
            (2, 3),
            (3, 4)
        };

        var chainInfo = new LogicChainInfo(new ValueInstatiator[]{}, instantiators, relations);
        _chain.SetupChain(new VariableDictionary(), chainInfo);
        _chain.Execute();

        StartCoroutine(CopyTest());
    }

    IEnumerator CopyTest() {
        yield return new WaitForSeconds(3);
        Debug.Log("LESS COPY");
        var chainCopy = _chain.Clone(new VariableDictionary());
        chainCopy.Execute();
    }
}