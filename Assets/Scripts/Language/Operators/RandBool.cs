using System;

namespace Language.Operators {
    public class RandBool : Operator<bool> {
        static readonly IValue[][] ArgTypes = { };
        readonly Random _random = new Random();
        
        public RandBool(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            return _random.NextDouble() > 0.5;
        }
    }
}
