using System;
using System.Collections;
using System.Reflection;
using Actions;
using Conditions;
using UnityEngine;

using VariableDictionary = System.Collections.Generic.Dictionary<string, IVariable>;
using OperatorInstantiator = System.Func<LogicObject, System.Collections.Generic.Dictionary<string, IVariable>, IValue[], IValue>;
using ChainableInstantiator = System.Func<LogicObject, System.Collections.Generic.Dictionary<string, IVariable>, IValue[], Chainable>;

public class ChainTest : MonoBehaviour {
    LogicChain _chain;
    void Start() {
        _chain = gameObject.AddComponent<LogicChain>();
        
        // ConstructorInfo GC(Type[] params) => Type.GetType("Operators.DummySum")

        ChainableInstantiator c0 = (logicObject, variables, values) => new DummyTrueCondition(new IValue[] { }, false);
        ChainableInstantiator c1 = (logicObject, variables, values) => new DummySyncAction1(new IValue[] { }, false);
        ChainableInstantiator c2 = (logicObject, variables, values) => new DummyAsyncAction1(new IValue[] { }, false);
        ChainableInstantiator c3 = (logicObject, variables, values) => new DummyFalseCondition(new IValue[] { }, false);
        ChainableInstantiator c4 = (logicObject, variables, values) => new DummySyncAction2(new IValue[] { }, false);
        
        // oh god forgive me for i MOST LIKELY will use this monstrosity to do shit
        ChainableInstantiator c5 = (logicObject, variables, values) => 
            System.Type.GetType("Actions.DummyAsyncAction2")
                .GetConstructor(new [] {values.GetType(), true.GetType()})
                .Invoke(new object[] {new IValue[] { }, false}) as Chainable;
        
        // var instantiators = new [] {c0, c1, c2, c3, c4};
        // var relations = new [] {
        //     (0, 1),
        //     (0, 2),
        //     (1, 3),
        //     (2, 3),
        //     (3, 4)
        // };
        
        var instantiators = new [] {c1, c2, c3, c4, c0};
        var relations = new [] {
            (4, 0),
            (4, 1),
            (0, 2),
            (1, 2),
            (2, 3)
        };

        var chainInfo = new LogicChainInfo(new OperatorInstantiator[]{}, instantiators, relations);
        _chain.SetupChain(null, new VariableDictionary(), chainInfo);
        _chain.Execute();

        StartCoroutine(CopyTest());
    }

    IEnumerator CopyTest() {
        yield return new WaitForSeconds(3);
        Debug.Log("LESS COPY");
        var chainCopy = _chain.Clone(null, new VariableDictionary(), gameObject);
        chainCopy.Execute();
    }
}