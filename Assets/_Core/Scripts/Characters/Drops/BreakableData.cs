using UnityEngine;

namespace HexaBit.Core {
    [CreateAssetMenu(fileName = "[breakable] New Breakable", menuName = "HexaBit/Breakable Data")]
    public class BreakableData : ScriptableObject {
        [Header("General")]
        public string objectName;
        public Sprite sprite;
        public int maxHealth = 20;
    }
}