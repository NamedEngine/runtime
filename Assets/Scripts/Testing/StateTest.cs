using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VariableDictionary = System.Collections.Generic.Dictionary<string, IVariable>;
using ValueInstatiator = System.Func<System.Collections.Generic.Dictionary<string, IVariable>, IValue[], IValue>;
using ChainableInstaniator = System.Func<System.Collections.Generic.Dictionary<string, IVariable>, IValue[], Chainable>;
using Object = System.Object;

public class StateTest : MonoBehaviour {
    LogicState _state1;
    LogicState _state2;
    Dictionary<string, IVariable> _variables;
    
    void Start() {
        Variable<T> ToVar<T>(T val) {
            return new Variable<T>(val);
        }
        
        
        _variables = new VariableDictionary {
            {"x", ToVar(1)},
            {"y", ToVar(4)},
        };

        ValueInstatiator plusInst = (dictionary, values) => new DummyPlus(new IValue[] {dictionary["x"], dictionary["y"]});
        ValueInstatiator toString = (dictionary, values) => new DummyToString(new IValue[] {values[0]});
        ChainableInstaniator wait3 = (dictionary, values) => new DummyWait(new IValue[] { ToVar(3f)});
        ChainableInstaniator wait5 = (dictionary, values) => new DummyWait(new IValue[] { ToVar(5f)});
        ChainableInstaniator log1 = (dictionary, values) => new DummyLog(new IValue[] {ToVar("Log from STATE 1: {0}"), values[1] });
        ChainableInstaniator logPatince = (dictionary, values) => new DummyLog(new IValue[] {ToVar("PATIENCE!")});
        ChainableInstaniator log2 = (dictionary, values) => new DummyLog(new IValue[] {ToVar("Log from STATE 2")});

        ValueInstatiator[] vals11 = {plusInst, toString};
        ChainableInstaniator[] ch11 = {wait3, log1};
        (int, int)[] rels11 = {
            (0, 1)
        };
        var lci11 = new LogicChainInfo(vals11, ch11, rels11);
        var lc11 = gameObject.AddComponent<LogicChain>();
        lc11.SetupChain(_variables, lci11);
        
        ValueInstatiator[] vals12 = {};
        ChainableInstaniator[] ch12 = {wait5, logPatince};
        (int, int)[] rels12 = {
            (0, 1)
        };
        var lci12 = new LogicChainInfo(vals12, ch12, rels12);
        var lc12 = gameObject.AddComponent<LogicChain>();
        lc12.SetupChain(_variables, lci12);
        
        _state1 = new LogicState(new [] {lc11, lc12});
        
        ValueInstatiator[] vals21 = {};
        ChainableInstaniator[] ch21 = {wait3, log2};
        (int, int)[] rels21 = {
            (0, 1)
        };
        var lci21 = new LogicChainInfo(vals21, ch21, rels21);
        var lc21 = gameObject.AddComponent<LogicChain>();
        lc21.SetupChain(_variables, lci21);
        
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
        _state2.Start(_variables);
        _state2.ProcessLogic();
        yield return new WaitForSeconds(4f);
        Debug.Log("Finishing state 2");
        _state2.Finish();
    }
}