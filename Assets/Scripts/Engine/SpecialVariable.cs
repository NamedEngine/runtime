using UnityEngine;

public abstract class SpecialVariable<T> : Variable<T> {
    protected GameObject BoundGameObject; 
    
    public SpecialVariable(GameObject gameObject) {
        BoundGameObject = gameObject;
    }

    public override IVariable Clone(GameObject objectToAttachTo) {
        return GetType().GetConstructor(new []{typeof(GameObject)}).Invoke(new object[] {objectToAttachTo}) as IVariable;
    }
}