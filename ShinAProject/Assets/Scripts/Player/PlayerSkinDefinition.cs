using UnityEngine;

namespace ShinA.Player
{
    [CreateAssetMenu(fileName = "PlayerSkin", menuName = "ShinA/Player Skin")]
    public sealed class PlayerSkinDefinition : ScriptableObject
    {
        [SerializeField] private string skinId = "default";
        [SerializeField] private GameObject firstPersonArmsPrefab;
        [SerializeField] private Color fallbackArmColor = new(0.64f, 0.48f, 0.38f, 1f);

        public string SkinId => skinId;
        public GameObject FirstPersonArmsPrefab => firstPersonArmsPrefab;
        public Color FallbackArmColor => fallbackArmColor;
    }
}
