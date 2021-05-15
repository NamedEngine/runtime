using System;
using System.Collections.Generic;

namespace Rules.Logic {
    public interface ILogicChecker {
        List<Action<Dictionary<string, ParsedNodeInfo>, Dictionary<string, string>, TemporaryInstantiator>>
            GetCheckerMethods();
    }
}