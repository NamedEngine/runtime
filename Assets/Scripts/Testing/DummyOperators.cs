﻿using System;
using UnityEngine;
using Random = System.Random;

namespace Operators {
    public class DummyAnd : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[]{new Value<bool>()},
            new IValue[]{new Value<bool>()},
        };
        
        public DummyAnd(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            return Context.Arguments[0] as Value<bool> && Context.Arguments[1] as Value<bool>;
        }
    }

    public class DummyOr : Operator<bool> {
        static readonly IValue[][] ArgTypes = {
            new IValue[]{new Value<bool>()},
            new IValue[]{new Value<bool>()},
        };

        public DummyOr(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override bool InternalGet() {
            return Context.Arguments[0] as Value<bool> || Context.Arguments[1] as Value<bool>;
        }
    }

    public class DummyPlus : Operator<int> {
        static readonly IValue[][] ArgTypes = {
            new IValue[]{new Value<int>()},
            new IValue[]{new Value<int>()},
        };

        public DummyPlus(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }


        protected override int InternalGet() {
            return (Context.Arguments[0] as Value<int>) + (Context.Arguments[1] as Value<int>);
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

        public DummyToInt(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override int InternalGet() {
            switch (Context.Arguments[0]) {
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

        public DummyToString(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

        protected override string InternalGet() {
            switch (Context.Arguments[0]) {
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
        
        public DummyRandInt(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) { }

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
                if (obj.Class != _variableRef.ClassRef.ClassName) {
                    throw new ApplicationException("Programmer did something wrong");
                }

                if (!obj.Variables.ContainsKey(_variableRef.Name)) {
                    throw new ApplicationException("Programmer did something wrong");
                }

                var referencedVariable = obj.Variables[_variableRef.Name];
                if (referencedVariable.GetValueType() != GetValueType()) {
                    throw new ApplicationException("Programmer did something wrong");
                }
            }
    
            protected override T InternalGet() {
                return _parent.GetReferencedObject().Variables[_variableRef.Name] as Variable<T>;
            }

            public override void Set(T value) {
                
            }

            public override IVariable Clone(GameObject objectToAttachTo, LogicEngine.LogicEngineAPI engineAPI) {
                return new ProxyVariable(_variableRef, _parent);
            }
        }

        ProxyVariable _proxy;
        ProxyVariable Proxy => _proxy ?? (_proxy = new ProxyVariable(Context.Arguments[1] as VariableRef, this));

        public GetVariableByObjName(ConstrainableContext context, bool constraintReference) : base(ArgTypes, context, constraintReference) {
        }

        LogicObject GetReferencedObject() {
            var objName = (Value<string>) Context.Arguments[0];
            var variableRef = (VariableRef) Context.Arguments[1];
            var className = variableRef.ClassRef.ClassName;
            var obj =Context.Base.EngineAPI.GetObjectByName(objName, className);
            if (obj == null) {
                throw new ArgumentException("Could not find object with name \"" + objName + "\" and type \"" + className +"\"");
            }

            return obj;
        }
        
        public IVariable Clone(GameObject objectToAttachTo, LogicEngine.LogicEngineAPI engineAPI) {
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

        public override bool TryTransferValueTo(IVariable other) {
            return Proxy.TryTransferValueTo(other);
        }
    }
}
