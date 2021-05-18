using System;
using Actions;
using Operators;
using UnityEngine;

public class ValueTest : MonoBehaviour {
    void Start() {
        var boolVal1 = new Variable<bool>();
        var boolVal2 = new Variable<bool>(true);
        var boolVal3 = new Variable<bool>(true);
        var boolOp1 = new DummyAnd(null, null, null, new IValue[] {boolVal1, boolVal2}, false);
        var boolOp2 = new DummyAnd(null, null, null, new IValue[] {boolVal2, boolVal3}, false);
        var intOp1 = new DummyToInt(null, null, null, new IValue[] {boolOp1}, false);
        var intOp2 = new DummyToInt(null, null, null, new IValue[] {boolOp2}, false);
        var intOp3 = new DummyPlus(null, null, null, new IValue[] {intOp1, intOp2}, false);
        Debug.Log(intOp3.Get());

        IValue val1 = new Variable<bool>();
        IValue val2 = new DummyAnd(null, null, null, new [] {val1, val1}, false);

        // ReSharper disable once UnusedVariable
        var shouldWork1 = new DummySetBool(null, null, null, new [] {val1, val1}, false);
        // ReSharper disable once UnusedVariable
        var shouldWork2 = new DummySetBool(null, null, null, new [] {val2, val1}, false);

        try {
            // ReSharper disable once UnusedVariable
            var shouldFail = new DummySetBool(null, null, null, new[] {val1, val2}, false);
            throw new ApplicationException("Should not be thrown!");
        }
        catch {
            // ignore
        }
    }
}
