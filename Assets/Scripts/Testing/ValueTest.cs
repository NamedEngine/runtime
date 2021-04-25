using System;
using System.Collections.Generic;
using UnityEngine;

public class ValueTest : MonoBehaviour {
    void Start() {
        var boolVal1 = new LogicVariable<bool>();
        var boolVal2 = new LogicVariable<bool>(true);
        var boolVal3 = new LogicVariable<bool>(true);
        var boolOp1 = new DummyAnd(new IValue[] {boolVal1, boolVal2});
        var boolOp2 = new DummyAnd(new IValue[] {boolVal2, boolVal3});
        var intOp1 = new DummyToInt(new IValue[] {boolOp1});
        var intOp2 = new DummyToInt(new IValue[] {boolOp2});
        var intOp3 = new DummyPlus(new IValue[] {intOp1, intOp2});
        Debug.Log(intOp3.Get());
    }
}