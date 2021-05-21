namespace Language.Conditions {
    public class IsTrue : Condition {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<bool>()}
        };
        
        public IsTrue(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        protected override bool ConditionLogic() {
            return Context.Arguments[0] as Value<bool>;
        }
    }
}
