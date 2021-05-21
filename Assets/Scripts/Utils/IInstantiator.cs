using System;
using UnityEngine;

public interface IInstantiator {
    Component GetInstance(Type t);
}
