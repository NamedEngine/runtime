using UnityEngine;
using Random = System.Random;

namespace Language.Operators {
    public class RandInt : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>(), new NullValue()},
            new IValue[] {new Value<int>(), new NullValue()},
        };
        readonly Random _random = new Random();
        
        public RandInt(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] arguments,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, arguments, constraintReference) { }

        protected override int InternalGet() {
            var minValue = Arguments[0] as Value<int> ?? int.MinValue;
            var maxValue = Arguments[1] as Value<int> ?? int.MaxValue;
            return _random.Next(minValue, maxValue);
        }
    }
}