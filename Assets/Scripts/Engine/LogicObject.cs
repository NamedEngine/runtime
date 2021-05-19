using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

public class LogicObject : MonoBehaviour {
    LogicObject _baseObject;
    public string BaseClass => _baseObject != null ? _baseObject.Class : null;

    LogicState _generalState;
    Dictionary<string, LogicState> _logicStates = new Dictionary<string, LogicState>();

    Dictionary<string, IVariable> _thisClassVariables = new Dictionary<string, IVariable>();
    public DictionaryWrapper<string, IVariable> Variables {
        get {
            if (!_baseObject) {
                return new DictionaryWrapper<string, IVariable>(new IReadOnlyDictionary<string, IVariable>[] {_thisClassVariables});
            }
            
            return new DictionaryWrapper<string, IVariable>(new IReadOnlyDictionary<string, IVariable>[] {_thisClassVariables, _baseObject.Variables});
        }
    }

    public string Class { get; private set; }
    string _name;

    public bool IsClass(string otherClass) {
        var isClass = Class == otherClass;
        if (!isClass && _baseObject) {
            isClass = _baseObject.IsClass(otherClass);
        }

        return isClass;
    }

    public string[] GetInheritanceChain() {
        var chain = new List<string> {Class};
        if (BaseClass != null) {
            chain.AddRange(_baseObject.GetInheritanceChain());
        }

        return chain.ToArray();
    }

    string _currentState = "";
    public void SetState(string state, LogicEngine.LogicEngineAPI engineAPI) {  // TODO: maybe reduce scope somehow
        if (!string.IsNullOrEmpty(_currentState)) {
            _logicStates[_currentState].Finish();
        }

        var baseContext = new BaseContext(engineAPI, Variables, _name);
        
        _currentState = state;
        _logicStates[_currentState].Start(this, baseContext);
        _logicStates[_currentState].ProcessLogic();
    }

    protected LogicEngine.LogicEngineAPI EngineAPI;
    
    public void SetupObject(LogicState generalState, Dictionary<string, LogicState> logicStates, string currentState,
        Dictionary<string, IVariable> logicVariables, string objectClass, LogicEngine.LogicEngineAPI engineAPI, string objectName) {
        _generalState = generalState;
        _logicStates = logicStates;
        _currentState = currentState;
        _thisClassVariables = logicVariables;
        Class = objectClass;
        EngineAPI = engineAPI;
        _name = objectName;
    }

    public void BeforeStartProcessing() {
        if (_baseObject) {
            _baseObject.BeforeStartProcessing();
        }
        
        BeforeStartProcessingInternal();
    }

    protected virtual void BeforeStartProcessingInternal() { }
    
    public void ProcessLogic() {
        if (_baseObject) {
            _baseObject.ProcessLogic();
        }
        
        _generalState?.ProcessLogic();
        if (!string.IsNullOrEmpty(_currentState)) {
            _logicStates[_currentState].ProcessLogic();
        }
    }

    public LogicObject Clone(GameObject objectToAttachTo, LogicEngine.LogicEngineAPI engineAPI, string newObjectName) {
        var newObject = objectToAttachTo.AddComponent(GetType()) as LogicObject;
        Debug.Assert(newObject != null, nameof(newObject) + " != null");
        
        newObject._baseObject = _baseObject ? _baseObject.Clone(objectToAttachTo, engineAPI, null) : null;
        
        var clonedVariables = _thisClassVariables.ToDictionary(entry => entry.Key,
            entry => entry.Value.Clone(objectToAttachTo, engineAPI));
        newObject._thisClassVariables = clonedVariables;

        var baseContext = new BaseContext(engineAPI, newObject.Variables, newObjectName);
        
        var clonedGeneralState = _generalState?.Clone(newObject, baseContext, objectToAttachTo);
        var clonedStates = _logicStates.ToDictionary(entry => entry.Key,
            entry => entry.Value.Clone(newObject, baseContext, objectToAttachTo));
        newObject.SetupObject(clonedGeneralState, clonedStates, _currentState, clonedVariables, Class, engineAPI, newObjectName);
        
        return newObject;
    }

    public void Inherit(LogicObject inheritor, LogicEngine.LogicEngineAPI engineAPI) {
        if (inheritor._baseObject != null) {
            throw new ApplicationException();
        }
        
        // Debug.Log($":{inheritor.Class}: now inherits from :{Class}:");

        inheritor._baseObject = Clone(inheritor.gameObject, engineAPI, null);
        var inheritorVariableNames = inheritor._thisClassVariables.Keys.ToArray();
        foreach (var variableName in inheritorVariableNames) {
            var got = inheritor._baseObject.Variables.TryGetValue(variableName, out var variable);
            if (!got) {
                continue;
            }

            var transferred = inheritor._thisClassVariables[variableName].TryTransferValueTo(variable);
            if (!transferred) {
                throw new ArgumentException($"Inheritor({inheritor.Class}) variable {variableName} has inappropriate type");
            }

            inheritor._thisClassVariables.Remove(variableName);
        }
    }
    
    void OnDestroy() {
        _generalState?.Finish();
        foreach (var state in _logicStates.Values) {
            state.Finish();
            state.Destroy(Destroy);
            // not letting chains know they are being destroyed cause they are attached to the same object and are gone for good after the process
        }
    }
}
