﻿using System;
using UnityEngine;

namespace Language.Conditions {
    public class Once : Condition {
        static readonly IValue[][] ArgTypes = { };

        bool _activated;

        public Once(GameObject gameObject, LogicEngine.LogicEngineAPI engineAPI, DictionaryWrapper<string, IVariable> variables, IValue[] values,
            bool constraintReference) : base(ArgTypes, gameObject, engineAPI, variables, values, constraintReference) { }

        protected override bool ConditionLogic() {
            var res = !_activated;
            _activated = true;
            return res;
        }
    }
}
