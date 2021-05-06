using UnityEngine;
using Random = System.Random;

namespace Language.Operators {
    public class RandBool : Operator<bool> {
        static readonly IValue[][] ArgTypes = { };
        readonly Random _random = new Random();
        
        public RandBool(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] arguments,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, arguments, constraintReference) { }

        protected override bool InternalGet() {
            return _random.NextDouble() > 0.5;
        }
    }
}