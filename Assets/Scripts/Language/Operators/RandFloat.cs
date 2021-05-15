using System;
using UnityEngine;
using Random = System.Random;

namespace Language.Operators {
    public class RandFloat : Operator<float> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<float>(), new NullValue()},
            new IValue[] {new Value<float>(), new NullValue()},
        };
        readonly Random _random = new Random();
        
        public RandFloat(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] arguments,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, arguments, constraintReference) { }

        protected override float InternalGet() {
            var minValue = Arguments[0] as Value<float> ?? float.MinValue;
            var maxValue = Arguments[1] as Value<float> ?? float.MaxValue;
            return minValue + Convert.ToSingle(_random.NextDouble() * Convert.ToDouble(maxValue - minValue));
        }
    }
}