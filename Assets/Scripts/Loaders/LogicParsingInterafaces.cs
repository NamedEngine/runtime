﻿using System.Collections.Generic;

public interface ILogicParser<in T> {
    Dictionary<string, ParsedNodeInfo> Parse(T logicSource, IdGenerator idGenerator);
}

public interface ILogicSaver<out T> {
     T Save(Dictionary<string, ParsedNodeInfo> logic);
}
