using System;
using UnityEngine;
using Random = System.Random;

namespace Operators {
    public class DummyAnd : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[]{new Value<bool>()},
            new IValue[]{new Value<bool>()},
        };
        
        public DummyAnd(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values, bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override bool InternalGet() {
            return Arguments[0] as Value<bool> && Arguments[1] as Value<bool>;
        }
    }

    public class DummyOr : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[]{new Value<bool>()},
            new IValue[]{new Value<bool>()},
        };

        public DummyOr(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values, bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

        protected override bool InternalGet() {
            return Arguments[0] as Value<bool> || Arguments[1] as Value<bool>;
        }
    }

    public class DummyPlus : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[]{new Value<int>()},
            new IValue[]{new Value<int>()},
        };

        public DummyPlus(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values, bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }


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

        public DummyToInt(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values, bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

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

        public DummyToString(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] values, bool constraintReference) : base(ArgTypes, gameObject, engineAPI, values, constraintReference) { }

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
        
        public DummyRandInt(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, IValue[] arguments, bool constraintReference) : base(ArgTypes, gameObject, engineAPI, arguments, constraintReference) { }

        protected override int InternalGet() {
            return _random.Next(int.MinValue, int.MaxValue);
        }
    }

    public class GetVariableByObjName<T> : Operator<T>, IVariable {
        static readonly IValue[][] ArgTypes = {
            new [] { new Value<string>() },
            new [] { new VariableRef() }
        };
        
        class ProxyVariable : Variable<T> {
            readonly VariableRef _variableRef;
            readonly GetVariableByObjName<T> _parent;

            public ProxyVariable(VariableRef variableRef, GetVariableByObjName<T> parent) {
                _variableRef = variableRef;
                _parent = parent;
                // CheckReferencedObject(parent.GetReferencedObject());
            }

            void CheckReferencedObject(LogicObject obj) {   // TODO: remove after testing, i suppose
                if (obj.@class != _variableRef.ClassRef.ClassName) {
                    throw new ApplicationException("Programmer did something wrong");
                }

                if (!obj.LogicVariables.ContainsKey(_variableRef.Name)) {
                    throw new ApplicationException("Programmer did something wrong");
                }

                var referencedVariable = obj.LogicVariables[_variableRef.Name];
                if (referencedVariable.GetValueType() != GetValueType()) {
                    throw new ApplicationException("Programmer did something wrong");
                }
            }
    
            protected override T InternalGet() {
                return _parent.GetReferencedObject().LogicVariables[_variableRef.Name] as Variable<T>;
            }

            public override void Set(T value) {
                
            }

            public override IVariable Clone(GameObject objectToAttachTo) {
                return new ProxyVariable(_variableRef, _parent);
            }
        }

        ProxyVariable _proxy;
        ProxyVariable Proxy => _proxy ?? (_proxy = new ProxyVariable(Arguments[1] as VariableRef, this));

        public GetVariableByObjName(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI,
            IValue[] arguments, bool constraintReference) : base(ArgTypes, gameObject, engineAPI, arguments,
            constraintReference) {
        }

        LogicObject GetReferencedObject() {
            var objName = (Value<string>) Arguments[0];
            var variableRef = (VariableRef) Arguments[1];
            var obj = EngineAPI.GetObjectByName(objName, variableRef.ClassRef.ClassName);
            if (obj == null) {
                throw new ArgumentException();  // TODO
            }

            return obj;
        }
        
        public IVariable Clone(GameObject objectToAttachTo) {
            throw new NotImplementedException();
        }

        protected override T InternalGet() {
            return Proxy.Get();
        }

        public override bool Cast(IValue value) {
            return Proxy.Cast(value);
        }

        public override IValue PrepareForCast() {
            return Proxy.PrepareForCast();
        }

        public bool TryTransferValueTo(IVariable other) {
            return Proxy.TryTransferValueTo(other);
        }
    }
}
