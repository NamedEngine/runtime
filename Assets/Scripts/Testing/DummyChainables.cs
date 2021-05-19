using System;
using System.Collections;
using System.Linq;
using UnityEngine;

namespace Actions {
    public class DummySyncAction1 : Action {
        static readonly IValue[][] ArgTypes = { };
        
        protected override IEnumerator ActionLogic() {
            return null;
        }

        public DummySyncAction1(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
    }
    public class DummySyncAction2 : Action {
        static readonly IValue[][] ArgTypes = { };
        
        protected override IEnumerator ActionLogic() {
            return null;
        }

        public DummySyncAction2(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
    }

    public class DummyAsyncAction1 : Action {
        static readonly IValue[][] ArgTypes = { };
        
        protected override IEnumerator ActionLogic() {
            yield return new WaitForSeconds(1);
        }

        public DummyAsyncAction1(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
    }

    public class DummyAsyncAction2 : Action {
        static readonly IValue[][] ArgTypes = { };
        
        protected override IEnumerator ActionLogic() {
            yield return new WaitForSeconds(2);
        }

        public DummyAsyncAction2(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
    }

    public class DummyLog : Action {
        static readonly IValue[][] ArgTypes = new[] {
            new IValue[] {new Value<string>()},
        }.Concat(Enumerable.Repeat(new IValue[] {
            new Value<int>(),
            new Value<float>(),
            new Value<bool>(),
            new Value<string>(),
            new NullValue(),
        }, 100).ToArray());
        
        public DummyLog(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        
        // ReSharper disable Unity.PerformanceAnalysis
        protected override IEnumerator ActionLogic() {
            string ValueToString(IValue val) {
                switch (val) {
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
            
            var format = (Value<string>) Context.Arguments[0];
            var other = Context.Arguments
                .Skip(1)
                .Select(ValueToString)
                .ToArray();
            var message = string.Format(format, other);
            Debug.Log(message);

            return null;
        }
    }

    public class DummyWait : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {
                new Value<int>(),
                new Value<float>()
            },
        };
        
        public DummyWait(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
        protected override IEnumerator ActionLogic() {
            switch (Context.Arguments[0]) {
                case Value<int> intVal:
                    yield return new WaitForSeconds(intVal);
                    break;
                case Value<float> floatVal:
                    yield return new WaitForSeconds(floatVal);
                    break;
                default:
                    throw new Exception("This should not be possible!");
            }
        }
    }

    public class DummySetBool : Action {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<bool>()},
            new IValue[] {new Variable<bool>()},
        };

        public DummySetBool(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override IEnumerator ActionLogic() {
            ((Variable<bool>) Context.Arguments[1]).Set((Value<bool>) Context.Arguments[0]);
            return null;
        }
    }
}

public class DummySetState : Action {
    static readonly IValue[][] ArgTypes = { };
    readonly System.Action _stateSetter;

    public DummySetState(System.Action stateSetter) : base(ArgTypes,null, false) {
        _stateSetter = stateSetter;
    }
    protected override IEnumerator ActionLogic() {
        _stateSetter();
        return null;
    }
}

namespace Conditions {
    public class DummyTrueCondition : Condition {
        static readonly IValue[][] ArgTypes = { };

        protected override bool ConditionLogic() {
            return true;
        }

        public DummyTrueCondition(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
    }

    public class DummyFalseCondition : Condition {
        static readonly IValue[][] ArgTypes = { };

        protected override bool ConditionLogic() {
            return false;
        }

        public DummyFalseCondition(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }
    }

    public class DummyNce : Condition {
        static readonly IValue[][] ArgTypes = {
            new IValue[] {new Value<int>()}
        };

        int _activationsLeft;

        public DummyNce(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) {
            _activationsLeft = (Value<int>) context.Arguments[0];
        }

        protected override bool ConditionLogic() {
            _activationsLeft = Math.Max(-1, --_activationsLeft);
            return _activationsLeft >= 0;
        }
    }
}
