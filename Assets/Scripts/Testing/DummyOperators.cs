using System;

namespace Operators {
    public class DummyAnd : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[]{new Value<bool>()},
            new IValue[]{new Value<bool>()},
        };
        
        public DummyAnd(IValue[] values, bool constraintReference) : base(ArgTypes, values, constraintReference) { }

        protected override bool InternalGet() {
            return Arguments[0] as Value<bool> && Arguments[1] as Value<bool>;
        }
    }

    public class DummyOr : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[]{new Value<bool>()},
            new IValue[]{new Value<bool>()},
        };

        public DummyOr(IValue[] values, bool constraintReference) : base(ArgTypes, values, constraintReference) { }

        protected override bool InternalGet() {
            return Arguments[0] as Value<bool> || Arguments[1] as Value<bool>;
        }
    }

    public class DummyPlus : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[]{new Value<int>()},
            new IValue[]{new Value<int>()},
        };

        public DummyPlus(IValue[] values, bool constraintReference) : base(ArgTypes, values, constraintReference) { }


        protected override int InternalGet() {
            return (Arguments[0] as Value<int>) + (Arguments[1] as Value<int>);
        }
    }

    public class DummyToInt : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {
                new Value<int>(),
                new Value<float>(),
                new Value<bool>(),
                new Value<string>(),
            },
        };

        public DummyToInt(IValue[] values, bool constraintReference) : base(ArgTypes, values, constraintReference) { }

        protected override int InternalGet() {
            switch (Arguments[0]) {
                case Value<int> intVal:
                    return intVal;
                case Value<float> floatVal:
                    return Convert.ToInt32(floatVal);
                case Value<bool> boolVal:
                    return Convert.ToInt32(boolVal);
                case Value<string> strVal:
                    return Convert.ToInt32(strVal);
                default:
                    throw new Exception("This should not be possible!");
            }
        }
    }

    public class DummyToString : Operator<string> {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {
                new Value<int>(),
                new Value<float>(),
                new Value<bool>(),
                new Value<string>(),
            },
        };

        public DummyToString(IValue[] values, bool constraintReference) : base(ArgTypes, values, constraintReference) { }

        protected override string InternalGet() {
            switch (Arguments[0]) {
                case Value<int> intVal:
                    return intVal.ToString();
                case Value<float> floatVal:
                    return floatVal.ToString();
                case Value<bool> boolVal:
                    return boolVal.ToString();
                case Value<string> strVal:
                    return strVal;
                default:
                    throw new Exception("This should not be possible!");
            }
        }
    }

    public class DummyRandInt : Operator<int> {
        static readonly IValue[][] ArgTypes = { };
        Random _random = new Random();
        
        public DummyRandInt(IValue[] arguments, bool constraintReference) : base(ArgTypes, arguments, constraintReference) { }

        protected override int InternalGet() {
            return _random.Next(int.MinValue, int.MaxValue);
        }
    }
}
