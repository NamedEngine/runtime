﻿using System.Collections;
using Actions;
using Conditions;
using UnityEngine;

using VariableDictionary = System.Collections.Generic.Dictionary<string, IVariable>;
using OperatorInstantiator = System.Func<LogicObject, LogicEngine.LogicEngineAPI, DictionaryWrapper<string, IVariable>, IValue[], IValue>;
using ChainableInstantiator = System.Func<LogicObject, LogicEngine.LogicEngineAPI, DictionaryWrapper<string, IVariable>, IValue[], Chainable>;

public class ChainTest : MonoBehaviour {
    LogicChain _chain;
    void Start() {
        _chain = gameObject.AddComponent<LogicChain>();

        ChainableInstantiator c0 = (logicObject, engineAPI, variables, values) => new DummyTrueCondition(null, null, null, new IValue[] { }, false);
        ChainableInstantiator c1 = (logicObject, engineAPI, variables, values) => new DummySyncAction1(null, null, null, new IValue[] { }, false);
        ChainableInstantiator c2 = (logicObject, engineAPI, variables, values) => new DummyAsyncAction1(null, null, null, new IValue[] { }, false);
        ChainableInstantiator c3 = (logicObject, engineAPI, variables, values) => new DummyFalseCondition(null, null, null, new IValue[] { }, false);
        ChainableInstantiator c4 = (logicObject, engineAPI, variables, values) => new DummySyncAction2(null, null, null, new IValue[] { }, false);

        var instantiators = new [] {c1, c2, c3, c4, c0};
        var relations = new [] {
            (4, 0),
            (4, 1),
            (0, 2),
            (1, 2),
            (2, 3)
        };

        var chainInfo = new LogicChainInfo(new OperatorInstantiator[]{}, instantiators, relations);
        _chain.SetupChain(null, null,new VariableDictionary(), chainInfo);
        _chain.Execute();

        StartCoroutine(CopyTest());
    }

    IEnumerator CopyTest() {
        yield return new WaitForSeconds(3);
        Debug.Log("LESS COPY");
        var chainCopy = _chain.Clone(null, null, new VariableDictionary(), gameObject);
        chainCopy.Execute();
    }
}
