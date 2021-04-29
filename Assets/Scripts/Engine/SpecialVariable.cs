using UnityEngine;
using Debug = System.Diagnostics.Debug;

public abstract class SpecialVariable<T> : Variable<T> {
    protected GameObject BoundGameObject; 
    
    public SpecialVariable(GameObject gameObject) {
        BoundGameObject = gameObject;
    }

    public override IVariable Clone(GameObject objectToAttachTo) {
        var constructor = GetType().GetConstructor(new[] {typeof(GameObject)});
        Debug.Assert(constructor != null, nameof(constructor) + " != null");
        
        return constructor.Invoke(new object[] {objectToAttachTo}) as IVariable;
    }
}