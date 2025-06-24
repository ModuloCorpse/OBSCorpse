using CorpseLib;
using CorpseLib.DataNotation;

namespace OBSCorpse
{
    public class OBSScene(string name, int idx)
    {
        public class DataSerializer : ADataSerializer<OBSScene>
        {
            protected override OperationResult<OBSScene> Deserialize(DataObject reader)
            {
                if (reader.TryGet("sceneName", out string? sceneName) &&
                    reader.TryGet("sceneIndex", out int? sceneIndex))
                    return new(new(sceneName!, (int)sceneIndex!));
                return new("Deserialization error", "Missing scene name or index");
            }

            protected override void Serialize(OBSScene obj, DataObject writer)
            {
                writer["sceneName"] = obj.m_Name;
                writer["sceneIndex"] = obj.m_Idx;
            }
        }

        private readonly string m_Name = name;
        private readonly int m_Idx = idx;
        public string Name => m_Name;
        public int Idx => m_Idx;
        public override string ToString() => $"[Name: \"{m_Name}\", Idx: {m_Idx}]";
    }
}
