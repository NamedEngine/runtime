namespace Language.Operators {
    public class Name : Operator<string> {
        static readonly IValue[][] ArgTypes = { };
        
        public Name(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override string InternalGet() {
            return Context.Base.Name;
        }
    }
}
