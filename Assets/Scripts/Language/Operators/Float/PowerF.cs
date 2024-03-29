﻿using System;

namespace Language.Operators {
    public class PowerF : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>()},
            new IValue[] {new Value<float>()},
        };

        public PowerF(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override float InternalGet() {
            return Convert.ToSingle(Math.Pow((Value<float>) Context.Arguments[0], (Value<float>) Context.Arguments[1]));
        }
    }
}
