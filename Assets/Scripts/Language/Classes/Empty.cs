using System.Collections.Generic;
using Variables;

namespace Language.Classes {
    public class Empty : LogicObject {
        public const string EmptyClassName = "";

        public Empty() {
            LogicVariables = new Dictionary<string, IVariable> {
                {nameof(DummyVisible), new DummyVisible(null)}
            };
            
            ObjectClass = EmptyClassName;
        }
    }
}