using System;
using UnityEngine;

namespace Language.Operators {
    public class GetVariableByObjName<T> : Operator<T>, IVariable {
        static readonly IValue[][] ArgTypes = {
            new IValue[] { new Value<string>() },
            new IValue[] { new VariableRef() }
        };
        
        class ProxyVariable : Variable<T> {
            readonly VariableRef _variableRef;
            readonly GetVariableByObjName<T> _parent;

            public ProxyVariable(VariableRef variableRef, GetVariableByObjName<T> parent) {
                _variableRef = variableRef;
                _parent = parent;
            }
    
            protected override T InternalGet() {
                return _parent.GetReferencedObject().Variables[_variableRef.Name] as Variable<T>;
            }

            public override void Set(T value) {
                ((Variable<T>) _parent.GetReferencedObject().Variables[_variableRef.Name]).Set(value);
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
            var obj = Context.Base.EngineAPI.GetObjectByName(objName, className);
            if (obj == null) {
                throw new LogicException(nameof(GetVariableByObjName<T>),
                    $"Could not find object with name \"{objName}\" and type \"{className}\"");
            }

            return obj;
        }
        
        public IVariable Clone(GameObject objectToAttachTo, LogicEngine.LogicEngineAPI engineAPI) {
            throw new ApplicationException("Should never be called");
        }

        protected override T InternalGet() {
            return Proxy.Get();
        }

        public override bool Cast(IValue value) {
            return value?.PrepareForCast() is Variable<T>;
        }

        public override IValue PrepareForCast() {
            if (ConstraintReference) {
                return new Variable<T>();
            }
            return Proxy.PrepareForCast();
        }

        public override bool TryTransferValueTo(IVariable other) {
            return Proxy.TryTransferValueTo(other);
        }
    }
}
