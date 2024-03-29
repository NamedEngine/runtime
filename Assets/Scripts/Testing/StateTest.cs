﻿using System.Collections;
using Actions;
using Operators;
using UnityEngine;
using VariableDictionary = System.Collections.Generic.Dictionary<string, IVariable>;
using OperatorInstantiator = System.Func<ArgumentLocationContext, IValue>;
using ChainableInstantiator = System.Func<ArgumentLocationContext, Chainable>;

public class StateTest : MonoBehaviour {
    LogicState _state1;
    LogicState _state2;
    VariableDictionary _variables;
    
    void Start() {
        Variable<T> ToVar<T>(T val) {
            return new Variable<T>(val);
        }
        ConcreteValue<T> ToVal<T>(T val) {
            return new ConcreteValue<T>(val);
        }
        
        
        _variables = new VariableDictionary {
            {"x", ToVar(1)},
            {"y", ToVar(4)},
        };

        OperatorInstantiator plusInst = context => new DummyPlus(
            new ConstrainableContext(context.Base, null, new IValue[] {context.Base.VariableDict["x"], context.Base.VariableDict["y"]}),
            false);
        OperatorInstantiator toString = context => new DummyToString(
            new ConstrainableContext(context.Base, null, new [] {context.PreparedOperators[0]}),
                false);
        ChainableInstantiator wait3 = context => new DummyWait(
            new ConstrainableContext(context.Base, null, new IValue[] { ToVal(3f)}),
                false);
        ChainableInstantiator wait5 = context => new DummyWait(
            new ConstrainableContext(context.Base, null, new IValue[] { ToVal(5f)}),
                false);
        ChainableInstantiator log1 = context => new DummyLog(
            new ConstrainableContext(context.Base, null, new [] {ToVal("Log from STATE 1: {0}"), context.PreparedOperators[1] }),
                false);
        ChainableInstantiator logPatince = context => new DummyLog(
            new ConstrainableContext(context.Base, null, new IValue[] {ToVal("PATIENCE!")}),
                false);
        ChainableInstantiator log2 = context => new DummyLog(
            new ConstrainableContext(context.Base, null, new IValue[] {ToVal("Log from STATE 2")}),
                false);

        OperatorInstantiator[] vals11 = {plusInst, toString};
        ChainableInstantiator[] ch11 = {wait3, log1};
        (int, int)[] rels11 = {
            (0, 1)
        };
        var lci11 = new LogicChainInfo(vals11, ch11, rels11);
        var lc11 = gameObject.AddComponent<LogicChain>();
        lc11.SetupChain(null, new BaseContext(null, _variables, null), lci11);
        
        OperatorInstantiator[] vals12 = {};
        ChainableInstantiator[] ch12 = {wait5, logPatince};
        (int, int)[] rels12 = {
            (0, 1)
        };
        var lci12 = new LogicChainInfo(vals12, ch12, rels12);
        var lc12 = gameObject.AddComponent<LogicChain>();
        lc12.SetupChain(null, new BaseContext(null, _variables, null), lci12);
        
        _state1 = new LogicState(new [] {lc11, lc12});
        
        OperatorInstantiator[] vals21 = {};
        ChainableInstantiator[] ch21 = {wait3, log2};
        (int, int)[] rels21 = {
            (0, 1)
        };
        var lci21 = new LogicChainInfo(vals21, ch21, rels21);
        var lc21 = gameObject.AddComponent<LogicChain>();
        lc21.SetupChain(null, new BaseContext(null, _variables, null), lci21);
        
        _state2 = new LogicState(new [] {lc21, lc21});

        StartCoroutine(Test());
    }

    IEnumerator Test() {
        Debug.Log("Starting state 1");
        _state1.ProcessLogic();
        // yield return new WaitForSeconds(4.999f);
        yield return new WaitForSeconds(5f);
        Debug.Log("Finishing state 1");
        _state1.Finish();
        
        
        Debug.Log("Starting state 2");
        _state2.Start(null, new BaseContext(null, _variables, null));
        _state2.ProcessLogic();
        yield return new WaitForSeconds(4f);
        Debug.Log("Finishing state 2");
        _state2.Finish();
    }
}
