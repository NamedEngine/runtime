using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using VariableDictionary = System.Collections.Generic.Dictionary<string, IVariable>;
using ValueInstatiator = System.Func<System.Collections.Generic.Dictionary<string, IVariable>, IValue[], IValue>;
using ChainableInstaniator = System.Func<System.Collections.Generic.Dictionary<string, IVariable>, IValue[], Chainable>;

public class ObjectTest : MonoBehaviour {
    LogicObject _logicObject;

    void Start() {
        Variable<T> ToVar<T>(T val) {
            return new Variable<T>(val);
        }

        _logicObject = gameObject.AddComponent<LogicObject>();
        
        var variables = new VariableDictionary {
            {"x", ToVar(1)},
            {"y", ToVar(3)},
        };

        ValueInstatiator plusInst = (dictionary, values) => new DummyPlus(new IValue[] {dictionary["x"], dictionary["y"]});
        ValueInstatiator toString = (dictionary, values) => new DummyToString(new IValue[] {values[0]});
        ChainableInstaniator wait3 = (dictionary, values) => new DummyWait(new IValue[] { ToVar(3f)});
        ChainableInstaniator wait05 = (dictionary, values) => new DummyWait(new IValue[] { ToVar(0.5f)});
        ChainableInstaniator log1 = (dictionary, values) => new DummyLog(new IValue[] {ToVar("Log from STATE 1: {0}"), values[1] });
        ChainableInstaniator logImpatince = (dictionary, values) => new DummyLog(new IValue[] {ToVar("IMPATIENCE!")});
        ChainableInstaniator log2 = (dictionary, values) => new DummyLog(new IValue[] {ToVar("Log from STATE 2")});
        ChainableInstaniator state1Setter = (dictionary, values) => new DummySetState(() => _logicObject.SetState("State 1"),new IValue[] {});
        ChainableInstaniator state2Setter = (dictionary, values) => new DummySetState(() => _logicObject.SetState("State 2"),new IValue[] {});
        ChainableInstaniator nce3 = (dictionary, values) => new DummyNce(new IValue[] {dictionary["y"]});
        

        ValueInstatiator[] vals11 = {plusInst, toString};
        ChainableInstaniator[] ch11 = {wait3, log1, state2Setter};
        (int, int)[] rels11 = {
            (0, 1),
            (1, 2)
        };
        var lci11 = new LogicChainInfo(vals11, ch11, rels11);
        var lc11 = gameObject.AddComponent<LogicChain>();
        lc11.SetupChain(variables, lci11);
        
        ValueInstatiator[] vals12 = {};
        ChainableInstaniator[] ch12 = {nce3, wait05, logImpatince};
        (int, int)[] rels12 = {
            (0, 1),
            (1, 2)
        };
        var lci12 = new LogicChainInfo(vals12, ch12, rels12);
        var lc12 = gameObject.AddComponent<LogicChain>();
        lc12.SetupChain(variables, lci12);
        
        var state1 = new LogicState(new [] {lc11, lc12});
        
        ValueInstatiator[] vals21 = {};
        ChainableInstaniator[] ch21 = {wait3, log2, state1Setter};
        (int, int)[] rels21 = {
            (0, 1),
            (1, 2)
        };
        var lci21 = new LogicChainInfo(vals21, ch21, rels21);
        var lc21 = gameObject.AddComponent<LogicChain>();
        lc21.SetupChain(variables, lci21);
        
        var state2 = new LogicState(new [] {lc21, lc21});

        var states = new Dictionary<string, LogicState> {
            {"State 1", state1},
            {"State 2", state2}
        };
        
        _logicObject.SetupObject(new LogicState(new LogicChain[] {}), states, "State 1", variables);

        StartCoroutine(ChangeObject());
    }

    void Update() {
        if (_logicObject)
            _logicObject.ProcessLogic();
    }

    IEnumerator ChangeObject() {
        yield return new WaitForSeconds(10);
        Debug.Log("DESTROYING FIRST OBJECT AND SETUPING SECOND");
        
        var newObject = _logicObject.Clone(gameObject);
        Destroy(_logicObject);
        // _logicObject = null;
        
        yield return new WaitForSeconds(10);
        Debug.Log("STARTING SECOND");
        
        _logicObject = newObject;
    }
}