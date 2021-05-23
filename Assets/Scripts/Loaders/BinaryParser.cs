using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class BinaryParser : ILogicParser<byte[]>, ILogicSaver<byte[]> {
    public Dictionary<string, ParsedNodeInfo> Parse(byte[] logicSource, IdGenerator idGenerator) {
        var stream = new MemoryStream(logicSource);
        var reader = new BinaryFormatter();
        
        return reader.Deserialize(stream) as Dictionary<string, ParsedNodeInfo>;
    }

    public byte[] Save(Dictionary<string, ParsedNodeInfo> logic) {
        var stream = new MemoryStream();
        var writer = new BinaryFormatter();
        
        writer.Serialize(stream, logic);
        return stream.ToArray();
    }
}
