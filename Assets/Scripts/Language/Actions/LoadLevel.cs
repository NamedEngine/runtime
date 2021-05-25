using System.Collections;
using Rules;

namespace Language.Actions {
    public class LoadLevel : Action {  // this way nothing will be executed after loading new level
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<string>()},
        };

        public LoadLevel(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        protected override IEnumerator ActionLogic() {
            var levelPath = ((Value<string>) Context.Arguments[0]).Get();
            try {
                Context.Base.EngineAPI.LoadLevel(levelPath);
            }
            catch (MapParseException e) {
                throw new LogicException(nameof(LoadLevel), e.Message);
            }
            catch (FileLoadException e) {
                throw new LogicException(nameof(LoadLevel), e.Message);
            }

            return null;
        }
    }
}
