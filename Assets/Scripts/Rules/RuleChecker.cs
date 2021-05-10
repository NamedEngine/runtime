using System;
using System.Linq;
using System.Reflection;

namespace Rules {
    public static class RuleChecker {
        public static void Check(Type ruleCollection, object[] arguments) {
            ruleCollection.GetMethods(BindingFlags.Public | BindingFlags.Static)
                .ToList()
                .ForEach(method => method.Invoke(null, arguments));
        }
        
        public static void CheckLogic(object[] arguments) {
            Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.Namespace == typeof(Logic.ClassNode).Namespace)
                .ToList().ForEach(t => Check(t, arguments));
        }
    }
}