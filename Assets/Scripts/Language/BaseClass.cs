using SpecialVariableInstantiator = System.Func<UnityEngine.GameObject, LogicEngine.LogicEngineAPI, IVariable>;

namespace Language {
    public abstract class BaseClass : LogicObject {
        public abstract string ShouldInheritFrom();

        public abstract string BaseClassName();

        public abstract (string, SpecialVariableInstantiator)[] BaseVariables();
    }
}
