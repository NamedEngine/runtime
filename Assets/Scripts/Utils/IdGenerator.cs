using System;
using System.Collections.Generic;
using System.Linq;

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

    public string[] NewIds(int n) {
        if (n < 0) {
            throw new ArgumentOutOfRangeException(nameof(n), "Can't generate negative number of Ids");
        }

        return Enumerable.Range(0, n)
            .Select(_ => NewId())
            .ToArray();
    }

    public void Reset() {
        _ids.Clear();
    }
}