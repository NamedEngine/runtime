using System.Collections;

namespace Language {
    public class SetState : Action {
        static readonly IValue[][] ArgTypes = { };
        readonly System.Action _stateSetter;

        public SetState(System.Action stateSetter) : base(ArgTypes,null,  null, new IValue[] {}, false) {
            _stateSetter = stateSetter;
        }
        protected override IEnumerator ActionLogic() {
            _stateSetter();
            return null;
        }
    }
}