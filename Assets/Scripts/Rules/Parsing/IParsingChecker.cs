using System;
using System.Collections.Generic;

namespace Rules.Parsing {
    public interface IParsingChecker<TSource> {
        List<Action<TSource, string>>
            GetCheckerMethods();
    }
}