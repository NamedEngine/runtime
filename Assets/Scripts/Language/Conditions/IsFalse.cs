namespace Language.Conditions {
    public class IsFalse : Condition {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<bool>()}
        };
        
        public IsFalse(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        protected override bool ConditionLogic() {
            return !(Context.Arguments[0] as Value<bool>);
        }
    }
}
