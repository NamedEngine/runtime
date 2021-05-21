using System.Collections;

namespace Language.Actions {
    public class Sync : Action {
        static readonly IValue[][] ArgTypes = { };
        
        public Sync(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        protected override IEnumerator ActionLogic() {
            return null;
        }
    }
}
