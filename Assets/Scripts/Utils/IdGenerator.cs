using System;
using System.Collections.Generic;

public class IdGenerator {
    readonly HashSet<string> _ids = new HashSet<string>();
    
    public string NewId() {
        var newId = Guid.NewGuid().ToString();
        while (_ids.Contains(newId)) {
            newId = Guid.NewGuid().ToString();
        }

        _ids.Add(newId);
        return newId;
    }
}