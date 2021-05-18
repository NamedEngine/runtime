using System.Collections;
using System.Collections.Generic;
using Actions;
using Operators;
using UnityEngine;

using VariableDictionary = System.Collections.Generic.Dictionary<string, IVariable>;
using OperatorInstantiator = System.Func<LogicObject, LogicEngine.LogicEngineAPI, DictionaryWrapper<string, IVariable>, IValue[], IValue>;
using ChainableInstantiator = System.Func<LogicObject, LogicEngine.LogicEngineAPI, DictionaryWrapper<string, IVariable>, IValue[], Chainable>;

public class StateTest : MonoBehaviour {
    LogicState _state1;
    LogicState _state2;
    Dictionary<string, IVariable> _variables;
    
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

        OperatorInstantiator plusInst = (logicObject, engineAPI, dictionary, values) => new DummyPlus(null, null, null, new IValue[] {dictionary["x"], dictionary["y"]}, false);
        OperatorInstantiator toString = (logicObject, engineAPI, dictionary, values) => new DummyToString(null, null, null, new [] {values[0]}, false);
        ChainableInstantiator wait3 = (logicObject, engineAPI, dictionary, values) => new DummyWait(null, null, null, new IValue[] { ToVal(3f)}, false);
        ChainableInstantiator wait5 = (logicObject, engineAPI, dictionary, values) => new DummyWait(null, null, null, new IValue[] { ToVal(5f)}, false);
        ChainableInstantiator log1 = (logicObject, engineAPI, dictionary, values) => new DummyLog(null, null, null, new [] {ToVal("Log from STATE 1: {0}"), values[1] }, false);
        ChainableInstantiator logPatince = (logicObject, engineAPI, dictionary, values) => new DummyLog(null, null, null, new IValue[] {ToVal("PATIENCE!")}, false);
        ChainableInstantiator log2 = (logicObject, engineAPI, dictionary, values) => new DummyLog(null, null, null, new IValue[] {ToVal("Log from STATE 2")}, false);

        OperatorInstantiator[] vals11 = {plusInst, toString};
        ChainableInstantiator[] ch11 = {wait3, log1};
        (int, int)[] rels11 = {
            (0, 1)
        };
        var lci11 = new LogicChainInfo(vals11, ch11, rels11);
        var lc11 = gameObject.AddComponent<LogicChain>();
        lc11.SetupChain(null, null, _variables, lci11);
        
        OperatorInstantiator[] vals12 = {};
        ChainableInstantiator[] ch12 = {wait5, logPatince};
        (int, int)[] rels12 = {
            (0, 1)
        };
        var lci12 = new LogicChainInfo(vals12, ch12, rels12);
        var lc12 = gameObject.AddComponent<LogicChain>();
        lc12.SetupChain(null, null, _variables, lci12);
        
        _state1 = new LogicState(new [] {lc11, lc12});
        
        OperatorInstantiator[] vals21 = {};
        ChainableInstantiator[] ch21 = {wait3, log2};
        (int, int)[] rels21 = {
            (0, 1)
        };
        var lci21 = new LogicChainInfo(vals21, ch21, rels21);
        var lc21 = gameObject.AddComponent<LogicChain>();
        lc21.SetupChain(null, null, _variables, lci21);
        
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
        _state2.Start(null, null, _variables);
        _state2.ProcessLogic();
        yield return new WaitForSeconds(4f);
        Debug.Log("Finishing state 2");
        _state2.Finish();
    }
}
