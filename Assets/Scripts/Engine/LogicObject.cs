using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LogicObject : MonoBehaviour {
    LogicState _generalState;
    Dictionary<string, LogicState> _logicStates;

    Dictionary<string, IVariable> _logicVariables;
    public Dictionary<string, IVariable> LogicVariables => _logicVariables;
    public string @class;
    
    string _currentState;
    public void SetState(string state) {  // TODO: maybe reduce scope somehow
        if (_currentState != "") {
            _logicStates[_currentState].Finish();
        }
        
        _currentState = state;
        _logicStates[_currentState].Start(this, _logicVariables);
        _logicStates[_currentState].ProcessLogic();
    }
    
    public void SetupObject(LogicState generalState, Dictionary<string, LogicState> logicStates, string currentState, Dictionary<string, IVariable> logicVariables) {
        _generalState = generalState;
        _logicStates = logicStates;
        _currentState = currentState;
        _logicVariables = logicVariables;
    }
    
    public void ProcessLogic() {
        _generalState.ProcessLogic();
        _logicStates[_currentState].ProcessLogic();
    }

    public LogicObject Clone(GameObject objectToAttachTo, string newClass = null) {
        var newObject = objectToAttachTo.AddComponent<LogicObject>();
        newObject.@class = newClass ?? @class;
        
        var clonedVariables = _logicVariables.ToDictionary(entry => entry.Key,
            entry => entry.Value.Clone(objectToAttachTo));
        
        var clonedGeneralState = _generalState.Clone(newObject, clonedVariables, objectToAttachTo);
        var clonedStates = _logicStates.ToDictionary(entry => entry.Key,
            entry => entry.Value.Clone(newObject, clonedVariables, objectToAttachTo));
        newObject.SetupObject(clonedGeneralState, clonedStates, _currentState, clonedVariables);
        return newObject;
    }
    
    void OnDestroy() {
        _generalState.Finish();
        foreach (var state in _logicStates.Values) {
            state.Finish();
            state.Destroy(Destroy);
            // not letting chains know they are being destroyed cause they are attached to the same object and are gone for good after the process
        }
    }
}
