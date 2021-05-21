namespace Language.Conditions {
    public class Once : Condition {
        static readonly IValue[][] ArgTypes = { };

        bool _activated;

        public Once(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool ConditionLogic() {
            var res = !_activated;
            _activated = true;
            return res;
        }
    }
}
