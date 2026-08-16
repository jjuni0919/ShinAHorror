using UnityEngine;

namespace ShinA.Player
{
    public sealed class PlayerAppearance : MonoBehaviour
    {
        [SerializeField] private PlayerSkinDefinition currentSkin;

        private Transform firstPersonRoot;
        private GameObject activeFirstPersonModel;

        public PlayerSkinDefinition CurrentSkin => currentSkin;

        public void Initialize(Transform cameraTransform, PlayerSkinDefinition initialSkin = null)
        {
            firstPersonRoot = cameraTransform;
            SetSkin(initialSkin);
        }

        public void SetSkin(PlayerSkinDefinition skin)
        {
            currentSkin = skin;

            if (activeFirstPersonModel != null)
            {
                Destroy(activeFirstPersonModel);
            }

            if (firstPersonRoot == null)
            {
                return;
            }

            activeFirstPersonModel = skin != null && skin.FirstPersonArmsPrefab != null
                ? Instantiate(skin.FirstPersonArmsPrefab, firstPersonRoot)
                : CreateFallbackArms(skin != null ? skin.FallbackArmColor : new Color(0.64f, 0.48f, 0.38f));
            activeFirstPersonModel.name = skin != null ? $"First Person Arms ({skin.SkinId})" : "First Person Arms (Default)";
        }

        private GameObject CreateFallbackArms(Color color)
        {
            GameObject arms = new("First Person Arms");
            arms.transform.SetParent(firstPersonRoot, false);

            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            Material material = new(shader) { color = color };

            CreateArm("Left Arm", arms.transform, new Vector3(-0.29f, -0.32f, 0.48f), -8f, material);
            CreateArm("Right Arm", arms.transform, new Vector3(0.29f, -0.32f, 0.48f), 8f, material);
            return arms;
        }

        private static void CreateArm(string name, Transform parent, Vector3 position, float roll, Material material)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            arm.name = name;
            arm.transform.SetParent(parent, false);
            arm.transform.localPosition = position;
            arm.transform.localScale = new Vector3(0.1f, 0.32f, 0.1f);
            arm.transform.localRotation = Quaternion.Euler(62f, 0f, roll);
            arm.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(arm.GetComponent<Collider>());
        }
    }
}
