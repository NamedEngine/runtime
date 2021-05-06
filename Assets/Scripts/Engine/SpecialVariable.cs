using UnityEngine;
using Debug = System.Diagnostics.Debug;

public abstract class SpecialVariable<T> : Variable<T> {
    protected GameObject BoundGameObject;
    protected LogicEngine.LogicEngineAPI EngineAPI;
    
    public SpecialVariable(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI) {
        BoundGameObject = gameObject;
        EngineAPI = engineAPI;
    }

    public override IVariable Clone(GameObject objectToAttachTo, LogicEngine.LogicEngineAPI engineAPI) {
        var constructor = GetType().GetConstructor(new[] {typeof(GameObject), typeof(LogicEngine.LogicEngineAPI)});
        Debug.Assert(constructor != null, nameof(constructor) + " != null");
        
        return constructor.Invoke(new object[] {objectToAttachTo, engineAPI}) as IVariable;
    }
}