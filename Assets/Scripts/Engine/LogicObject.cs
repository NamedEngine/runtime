using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LogicObject : MonoBehaviour {
    LogicObject _baseObject;
    public string BaseClass => _baseObject != null ? _baseObject.Class : null;
    
    protected LogicState GeneralState;
    protected Dictionary<string, LogicState> LogicStates = new Dictionary<string, LogicState>();

    protected Dictionary<string, IVariable> ThisClassVariables = new Dictionary<string, IVariable>();
    public DictionaryWrapper<string, IVariable> Variables {
        get {
            if (!_baseObject) {
                return new DictionaryWrapper<string, IVariable>(new IReadOnlyDictionary<string, IVariable>[] {ThisClassVariables});
            }
            
            return new DictionaryWrapper<string, IVariable>(new IReadOnlyDictionary<string, IVariable>[] {ThisClassVariables, _baseObject.Variables});
        }
    }

    protected string ObjectClass;
    public string Class => ObjectClass;

    public bool IsClass(string otherClass) {
        var isClass = Class == otherClass;
        if (!isClass && _baseObject) {
            isClass = _baseObject.IsClass(otherClass);
        }

        return isClass;
    }
    
    protected string CurrentState = "";
    public void SetState(string state, LogicEngine.LogicEngineAPI engineAPI) {  // TODO: maybe reduce scope somehow
        if (CurrentState != "") {
            LogicStates[CurrentState].Finish();
        }
        
        CurrentState = state;
        LogicStates[CurrentState].Start(this, engineAPI, Variables.ToDictionary());
        LogicStates[CurrentState].ProcessLogic();
    }
    
    public void SetupObject(LogicState generalState, Dictionary<string, LogicState> logicStates, string currentState, Dictionary<string, IVariable> logicVariables, string objectClass) {
        GeneralState = generalState;
        LogicStates = logicStates;
        CurrentState = currentState;
        ThisClassVariables = logicVariables;
        ObjectClass = objectClass;
    }
    
    public void ProcessLogic() {
        GeneralState?.ProcessLogic();
        if (CurrentState != "") {
            LogicStates[CurrentState].ProcessLogic();
        }

        if (_baseObject) {
            _baseObject.ProcessLogic();
        }
    }

    public LogicObject Clone(GameObject objectToAttachTo, LogicEngine.LogicEngineAPI engineAPI = null) {
        var newObject = objectToAttachTo.AddComponent<LogicObject>();
        newObject._baseObject = _baseObject ? _baseObject.Clone(objectToAttachTo, engineAPI) : _baseObject;
        
        var clonedVariables = ThisClassVariables.ToDictionary(entry => entry.Key,
            entry => entry.Value.Clone(objectToAttachTo, engineAPI));
        newObject.ThisClassVariables = clonedVariables;
        
        var clonedGeneralState = GeneralState?.Clone(newObject, engineAPI, newObject.Variables.ToDictionary(), objectToAttachTo);
        var clonedStates = LogicStates.ToDictionary(entry => entry.Key,
            entry => entry.Value.Clone(newObject, engineAPI, newObject.Variables.ToDictionary(), objectToAttachTo));
        newObject.SetupObject(clonedGeneralState, clonedStates, CurrentState, clonedVariables, Class);
        
        return newObject;
    }

    public void Inherit(LogicObject inheritor) {
        if (inheritor._baseObject != null) {
            throw new ApplicationException();
        }
        
        // Debug.Log($":{inheritor.Class}: now inherits from :{Class}:");

        inheritor._baseObject = Clone(inheritor.gameObject);
        var inheritorVariableNames = inheritor.ThisClassVariables.Keys.ToArray();
        foreach (var variableName in inheritorVariableNames) {
            var got = inheritor._baseObject.Variables.TryGetValue(variableName, out var variable);
            if (!got) {
                continue;
            }

            var transferred = inheritor.ThisClassVariables[variableName].TryTransferValueTo(variable);
            if (!transferred) {
                throw new ArgumentException($"Inheritor({inheritor.Class}) variable {variableName} has inappropriate type");  // TODO: MOVE TO LANG RULES
            }

            inheritor.ThisClassVariables.Remove(variableName);
        }
    }
    
    void OnDestroy() {
        GeneralState?.Finish();
        foreach (var state in LogicStates.Values) {
            state.Finish();
            state.Destroy(Destroy);
            // not letting chains know they are being destroyed cause they are attached to the same object and are gone for good after the process
        }
    }
}
