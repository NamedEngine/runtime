using System.Collections;
using System.Collections.Generic;
using Actions;
using Conditions;
using Operators;
using UnityEngine;

using VariableDictionary = System.Collections.Generic.Dictionary<string, IVariable>;
using OperatorInstantiator = System.Func<LogicObject, System.Collections.Generic.Dictionary<string, IVariable>, IValue[], IValue>;
using ChainableInstantiator = System.Func<LogicObject, System.Collections.Generic.Dictionary<string, IVariable>, IValue[], Chainable>;

public class ObjectTest : MonoBehaviour {
    LogicObject _logicObject;

    void Start() {
        Variable<T> ToVar<T>(T val) {
            return new Variable<T>(val);
        }
        ConcreteValue<T> ToVal<T>(T val) {
            return new ConcreteValue<T>(val);
        }

        _logicObject = gameObject.AddComponent<LogicObject>();
        
        var variables = new VariableDictionary {
            {"x", ToVar(1)},
            {"y", ToVar(3)},
        };

        OperatorInstantiator plusInst = (logicObject, dictionary, values) => new DummyPlus(null, new IValue[] {dictionary["x"], dictionary["y"]}, false);
        OperatorInstantiator toString = (logicObject, dictionary, values) => new DummyToString(null, new IValue[] {values[0]}, false);
        ChainableInstantiator wait3 = (logicObject, dictionary, values) => new DummyWait(null, new IValue[] { ToVal(3f)}, false);
        ChainableInstantiator wait05 = (logicObject, dictionary, values) => new DummyWait(null, new IValue[] { ToVal(0.5f)}, false);
        ChainableInstantiator log1 = (logicObject, dictionary, values) => new DummyLog(null, new IValue[] {ToVal("Log from STATE 1: {0}"), values[1] }, false);
        ChainableInstantiator logImpatince = (logicObject, dictionary, values) => new DummyLog(null, new IValue[] {ToVal("IMPATIENCE!")}, false);
        ChainableInstantiator log2 = (logicObject, dictionary, values) => new DummyLog(null, new IValue[] {ToVal("Log from STATE 2")}, false);
        ChainableInstantiator state1Setter = (logicObject, ictionary, values) => new DummySetState(() => logicObject.SetState("State 1"));
        ChainableInstantiator state2Setter = (logicObject, dictionary, values) => new DummySetState(() => logicObject.SetState("State 2"));
        ChainableInstantiator nce3 = (logicObject, dictionary, values) => new DummyNce(null, new IValue[] {dictionary["y"]}, false);
        

        OperatorInstantiator[] vals11 = {plusInst, toString};
        ChainableInstantiator[] ch11 = {wait3, log1, state2Setter};
        (int, int)[] rels11 = {
            (0, 1),
            (1, 2)
        };
        var lci11 = new LogicChainInfo(vals11, ch11, rels11);
        var lc11 = gameObject.AddComponent<LogicChain>();
        lc11.SetupChain(_logicObject, variables, lci11);
        
        OperatorInstantiator[] vals12 = {};
        ChainableInstantiator[] ch12 = {nce3, wait05, logImpatince};
        (int, int)[] rels12 = {
            (0, 1),
            (1, 2)
        };
        var lci12 = new LogicChainInfo(vals12, ch12, rels12);
        var lc12 = gameObject.AddComponent<LogicChain>();
        lc12.SetupChain(_logicObject, variables, lci12);
        
        var state1 = new LogicState(new [] {lc11, lc12});
        
        OperatorInstantiator[] vals21 = {};
        ChainableInstantiator[] ch21 = {wait3, log2, state1Setter};
        (int, int)[] rels21 = {
            (0, 1),
            (1, 2)
        };
        var lci21 = new LogicChainInfo(vals21, ch21, rels21);
        var lc21 = gameObject.AddComponent<LogicChain>();
        lc21.SetupChain(_logicObject, variables, lci21);
        
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
        Debug.Log("SETUPING SECOND");
        var newObject = _logicObject.Clone(gameObject);
        var tempObj = _logicObject;
        _logicObject = null;
        
        yield return new WaitForSeconds(4);
        Debug.Log("DESTROYING FIRST");
        Destroy(tempObj);
        
        yield return new WaitForSeconds(4);
        Debug.Log("STARTING SECOND");
        
        // newObject may have Second state as its initial one because
        // previous object was cloned having Second state as its current one
        // so CLONE ONLY NOT RUNNING OBJECTS A.K.A. CLASSES
        _logicObject = newObject;
        
        yield return new WaitForSeconds(10);
        Debug.Log("SETUPING THIRD");
        newObject = _logicObject.Clone(gameObject);
        tempObj = _logicObject;
        _logicObject = null;
        
        yield return new WaitForSeconds(4);
        Debug.Log("DESTROYING SECOND");
        Destroy(tempObj);
        
        yield return new WaitForSeconds(4);
        Debug.Log("STARTING THIRD");
        
        _logicObject = newObject;
    }
}