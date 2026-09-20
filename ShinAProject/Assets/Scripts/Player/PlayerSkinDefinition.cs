using UnityEngine;

namespace ShinA.Player
{
    [CreateAssetMenu(fileName = "PlayerSkin", menuName = "ShinA/플레이어 스킨")]
    public sealed class PlayerSkinDefinition : ScriptableObject
    {
        [SerializeField] private string skinId = "default";
        [SerializeField] private GameObject firstPersonArmsPrefab;
        [SerializeField] private GameObject thirdPersonBodyPrefab;
        [SerializeField] private Color fallbackArmColor = new(0.64f, 0.48f, 0.38f, 1f);
        [SerializeField] private Color fallbackClothesColor = new(0.12f, 0.16f, 0.2f, 1f);

        public string SkinId => skinId;
        public GameObject FirstPersonArmsPrefab => firstPersonArmsPrefab;
        public GameObject ThirdPersonBodyPrefab => thirdPersonBodyPrefab;
        public Color FallbackArmColor => fallbackArmColor;
        public Color FallbackClothesColor => fallbackClothesColor;
    }
}
