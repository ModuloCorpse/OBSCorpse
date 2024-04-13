using CorpseLib;
using CorpseLib.Json;

namespace OBSCorpse
{
    public class OBSScene(string name, int idx)
    {
        public class JsonSerializer : AJsonSerializer<OBSScene>
        {
            protected override OperationResult<OBSScene> Deserialize(JsonObject reader)
            {
                if (reader.TryGet("sceneName", out string? sceneName) &&
                    reader.TryGet("sceneIndex", out int? sceneIndex))
                    return new(new(sceneName!, (int)sceneIndex!));
                return new("Deserialization error", "Missing scene name or index");
            }

            protected override void Serialize(OBSScene obj, JsonObject writer)
            {
                writer["sceneName"] = obj.m_Name;
                writer["sceneIndex"] = obj.m_Idx;
            }
        }

        private readonly string m_Name = name;
        private readonly int m_Idx = idx;
        public string Name => m_Name;
        public int Idx => m_Idx;
        public override string ToString() => string.Format("[Name: \"{0}\", Idx: {1}]", m_Name, m_Idx);
    }
}
