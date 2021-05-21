using System.Collections;

namespace Language.Actions {
    public class LoadLevel : Action {  // this way nothing will be executed after loading new level
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
        };

        public LoadLevel(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var levelPath = ((Value<string>) Context.Arguments[0]).Get();
            var res = Context.Base.EngineAPI.LoadLevel(levelPath);
            if (!res) {
                throw new LogicException(nameof(LoadLevel), $"Could not load level {levelPath}");
            }

            return null;
        }
    }
}
