﻿namespace Language.Operators {
    public class NotEqual : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()},
            new IValue[] {new Value<int>()},
        };

        public NotEqual(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            return ((Value<int>) Context.Arguments[0]).Get() != ((Value<int>) Context.Arguments[1]).Get();
        }
    }
}
