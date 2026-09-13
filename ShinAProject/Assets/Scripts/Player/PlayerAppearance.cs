using ShinA.Inventory;
using UnityEngine;

namespace ShinA.Player
{
    public sealed class PlayerAppearance : MonoBehaviour
    {
        private const string LocalBodyLayerName = "LocalPlayerBody";

        [SerializeField] private PlayerSkinDefinition currentSkin;
        [SerializeField] private bool isLocalPlayer;

        private Transform firstPersonRoot;
        private FirstPersonController controller;
        private Camera localCamera;
        private GameObject activeFirstPersonModel;
        private GameObject activeThirdPersonModel;
        private GameObject activeFirstPersonItem;
        private GameObject activeThirdPersonItem;
        private Transform firstPersonHand;
        private Transform thirdPersonHand;
        private ItemDefinition equippedItem;
        private FirstPersonAttackAnimator attackAnimator;
        private int originalCameraMask;

        public PlayerSkinDefinition CurrentSkin => currentSkin;
        public bool IsLocalPlayer => isLocalPlayer;
        public ItemDefinition EquippedItem => equippedItem;

        public void Initialize(Transform cameraTransform, FirstPersonController playerController,
            PlayerSkinDefinition initialSkin = null, bool localPlayer = true)
        {
            firstPersonRoot = cameraTransform;
            controller = playerController;
            localCamera = cameraTransform != null ? cameraTransform.GetComponent<Camera>() : null;
            isLocalPlayer = localPlayer;

            if (localCamera != null)
            {
                originalCameraMask = localCamera.cullingMask;
            }

            RebuildAppearance(initialSkin);
        }

        public void SetSkin(PlayerSkinDefinition skin)
        {
            RebuildAppearance(skin);
        }

        public void EquipItem(ItemDefinition item)
        {
            equippedItem = item;
            RebuildEquippedItem();
        }

        public void PlayWeaponAttack(bool ranged)
        {
            attackAnimator?.Play(ranged);
        }

        // Multiplayer spawn code can call this after network ownership is known.
        public void SetLocalPlayer(bool localPlayer, Transform cameraTransform = null,
            FirstPersonController playerController = null)
        {
            isLocalPlayer = localPlayer;

            if (cameraTransform != null)
            {
                firstPersonRoot = cameraTransform;
                localCamera = cameraTransform.GetComponent<Camera>();
                originalCameraMask = localCamera != null ? localCamera.cullingMask : -1;
            }

            if (playerController != null)
            {
                controller = playerController;
            }

            RebuildAppearance(currentSkin);
        }

        private void RebuildAppearance(PlayerSkinDefinition skin)
        {
            currentSkin = skin;
            DestroyModel(ref activeFirstPersonModel);
            DestroyModel(ref activeThirdPersonModel);
            firstPersonHand = null;
            thirdPersonHand = null;

            Color skinColor = skin != null ? skin.FallbackArmColor : new Color(0.64f, 0.48f, 0.38f);
            Color clothesColor = skin != null ? skin.FallbackClothesColor : new Color(0.12f, 0.16f, 0.2f);

            activeThirdPersonModel = skin != null && skin.ThirdPersonBodyPrefab != null
                ? Instantiate(skin.ThirdPersonBodyPrefab, transform)
                : CreateFallbackBody(skinColor, clothesColor);
            activeThirdPersonModel.name = skin != null
                ? $"Third Person Body ({skin.SkinId})"
                : "Third Person Body (Default)";

            thirdPersonHand = CreateHandMount(activeThirdPersonModel.transform, transform, "Third Person Hand Mount");

            ConfigureThirdPersonVisibility();

            if (!isLocalPlayer || firstPersonRoot == null)
            {
                RebuildEquippedItem();
                return;
            }

            activeFirstPersonModel = skin != null && skin.FirstPersonArmsPrefab != null
                ? Instantiate(skin.FirstPersonArmsPrefab, firstPersonRoot)
                : CreateFallbackArms(skinColor);
            activeFirstPersonModel.name = skin != null
                ? $"First Person Arms ({skin.SkinId})"
                : "First Person Arms (Default)";

            firstPersonHand = CreateHandMount(activeFirstPersonModel.transform, firstPersonRoot, "First Person Hand Mount");
            ConfigureArmAnimation(activeFirstPersonModel);
            RebuildEquippedItem();
        }

        private void ConfigureThirdPersonVisibility()
        {
            int localBodyLayer = LayerMask.NameToLayer(LocalBodyLayerName);
            if (localBodyLayer < 0)
            {
                Debug.LogWarning($"Player layer '{LocalBodyLayerName}' is missing.", this);
                return;
            }

            int bodyLayer = isLocalPlayer ? localBodyLayer : 0;
            SetLayerRecursively(activeThirdPersonModel, bodyLayer);

            if (localCamera != null)
            {
                localCamera.cullingMask = isLocalPlayer
                    ? originalCameraMask & ~(1 << localBodyLayer)
                    : originalCameraMask;
            }
        }

        private void ConfigureArmAnimation(GameObject arms)
        {
            if (controller == null)
            {
                return;
            }

            Transform leftArm = FindDeepChild(arms.transform, "Left Arm");
            Transform rightArm = FindDeepChild(arms.transform, "Right Arm");
            FirstPersonArmAnimator animator = arms.GetComponent<FirstPersonArmAnimator>();
            if (animator == null)
            {
                animator = arms.AddComponent<FirstPersonArmAnimator>();
            }

            animator.Initialize(controller, leftArm, rightArm);
        }

        private Transform CreateHandMount(Transform modelRoot, Transform unscaledParent, string mountName)
        {
            Transform rightHand = FindDeepChild(modelRoot, "Right Hand") ?? FindDeepChild(modelRoot, "Right Arm") ?? modelRoot;
            GameObject mount = new(mountName);
            mount.transform.SetParent(unscaledParent, false);
            Vector3 offset = rightHand == modelRoot ? new Vector3(0.28f, -0.18f, 0.52f) : new Vector3(0f, 0.55f, 0f);
            HandMountFollower follower = mount.AddComponent<HandMountFollower>();
            follower.Initialize(rightHand, offset, Quaternion.Euler(90f, 0f, 0f));
            return mount.transform;
        }

        private void RebuildEquippedItem()
        {
            DestroyModel(ref activeFirstPersonItem);
            DestroyModel(ref activeThirdPersonItem);
            attackAnimator = null;

            if (equippedItem == null)
            {
                return;
            }

            if (thirdPersonHand != null)
            {
                activeThirdPersonItem = CreateEquippedVisual(thirdPersonHand, false);
                int bodyLayer = activeThirdPersonModel != null ? activeThirdPersonModel.layer : 0;
                SetLayerRecursively(activeThirdPersonItem, bodyLayer);
            }

            if (isLocalPlayer && firstPersonHand != null)
            {
                activeFirstPersonItem = CreateEquippedVisual(firstPersonHand, true);
                attackAnimator = activeFirstPersonItem.AddComponent<FirstPersonAttackAnimator>();
            }
        }

        private GameObject CreateEquippedVisual(Transform parent, bool firstPerson)
        {
            GameObject visual;
            if (equippedItem.EquippedPrefab != null)
            {
                visual = Instantiate(equippedItem.EquippedPrefab, parent);
                visual.transform.localScale = equippedItem.EquippedPrefab.transform.localScale;
            }
            else
            {
                PrimitiveType type = equippedItem is RangedWeaponDefinition
                    ? PrimitiveType.Cube
                    : equippedItem is MeleeWeaponDefinition ? PrimitiveType.Capsule : PrimitiveType.Sphere;
                visual = GameObject.CreatePrimitive(type);
                visual.transform.SetParent(parent, false);
                visual.transform.localScale = type == PrimitiveType.Capsule
                    ? new Vector3(0.09f, 0.38f, 0.09f)
                    : firstPerson ? Vector3.one * 0.16f : Vector3.one * 0.13f;
                Collider collider = visual.GetComponent<Collider>();
                if (collider != null)
                {
                    Destroy(collider);
                }

                visual.GetComponent<Renderer>().sharedMaterial = CreateMaterial(equippedItem.IconColor);
            }

            visual.name = $"Equipped - {equippedItem.ItemName}";
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.identity;
            foreach (Collider itemCollider in visual.GetComponentsInChildren<Collider>(true))
            {
                itemCollider.enabled = false;
            }
            return visual;
        }

        private GameObject CreateFallbackArms(Color color)
        {
            GameObject arms = new("First Person Arms");
            arms.transform.SetParent(firstPersonRoot, false);
            Material skinMaterial = CreateMaterial(color);

            CreatePart(PrimitiveType.Capsule, "Left Arm", arms.transform,
                new Vector3(-0.29f, -0.32f, 0.48f), new Vector3(0.1f, 0.32f, 0.1f),
                Quaternion.Euler(62f, 0f, -8f), skinMaterial);
            CreatePart(PrimitiveType.Capsule, "Right Arm", arms.transform,
                new Vector3(0.29f, -0.32f, 0.48f), new Vector3(0.1f, 0.32f, 0.1f),
                Quaternion.Euler(62f, 0f, 8f), skinMaterial);
            return arms;
        }

        private GameObject CreateFallbackBody(Color skinColor, Color clothesColor)
        {
            GameObject body = new("Third Person Body");
            body.transform.SetParent(transform, false);
            Material skinMaterial = CreateMaterial(skinColor);
            Material clothesMaterial = CreateMaterial(clothesColor);

            CreatePart(PrimitiveType.Sphere, "Head", body.transform,
                new Vector3(0f, 1.62f, 0f), new Vector3(0.34f, 0.38f, 0.34f), Quaternion.identity, skinMaterial);
            CreatePart(PrimitiveType.Cube, "Torso", body.transform,
                new Vector3(0f, 1.13f, 0f), new Vector3(0.58f, 0.68f, 0.3f), Quaternion.identity, clothesMaterial);
            CreatePart(PrimitiveType.Capsule, "Left Arm", body.transform,
                new Vector3(-0.4f, 1.13f, 0f), new Vector3(0.11f, 0.36f, 0.11f), Quaternion.identity, skinMaterial);
            CreatePart(PrimitiveType.Capsule, "Right Arm", body.transform,
                new Vector3(0.4f, 1.13f, 0f), new Vector3(0.11f, 0.36f, 0.11f), Quaternion.identity, skinMaterial);
            CreatePart(PrimitiveType.Capsule, "Left Leg", body.transform,
                new Vector3(-0.17f, 0.43f, 0f), new Vector3(0.16f, 0.4f, 0.16f), Quaternion.identity, clothesMaterial);
            CreatePart(PrimitiveType.Capsule, "Right Leg", body.transform,
                new Vector3(0.17f, 0.43f, 0f), new Vector3(0.16f, 0.4f, 0.16f), Quaternion.identity, clothesMaterial);
            return body;
        }

        private static GameObject CreatePart(PrimitiveType type, string name, Transform parent,
            Vector3 position, Vector3 scale, Quaternion rotation, Material material)
        {
            GameObject part = GameObject.CreatePrimitive(type);
            part.name = name;
            part.transform.SetParent(parent, false);
            part.transform.localPosition = position;
            part.transform.localScale = scale;
            part.transform.localRotation = rotation;
            part.GetComponent<Renderer>().sharedMaterial = material;
            Destroy(part.GetComponent<Collider>());
            return part;
        }

        private static Material CreateMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
            return new Material(shader) { color = color };
        }

        private static Transform FindDeepChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }

                Transform result = FindDeepChild(child, childName);
                if (result != null)
                {
                    return result;
                }
            }

            return null;
        }

        private static void SetLayerRecursively(GameObject target, int layer)
        {
            target.layer = layer;
            foreach (Transform child in target.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }

        private static void DestroyModel(ref GameObject model)
        {
            if (model != null)
            {
                Destroy(model);
                model = null;
            }
        }
    }
}
