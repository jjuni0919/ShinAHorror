using ShinA.Settings;
using UnityEngine;

namespace ShinA.Player
{
    public interface IPlayerInteractable
    {
        string InteractionPrompt { get; }
        void Interact(GameObject player);
    }

    public sealed class PlayerWorldInteractor : MonoBehaviour
    {
        [SerializeField, Min(0.5f)] private float interactionDistance = 3f;

        private Camera viewCamera;
        private FirstPersonController controller;

        public IPlayerInteractable FocusedInteractable { get; private set; }

        public void Initialize(Camera playerCamera, FirstPersonController playerController)
        {
            viewCamera = playerCamera;
            controller = playerController;
        }

        private void Update()
        {
            UpdateFocusedInteractable();
            if (FocusedInteractable != null &&
                PlayerInputBindings.WasPressedThisFrame(PlayerAction.WorldInteract))
            {
                FocusedInteractable.Interact(gameObject);
            }
        }

        private void UpdateFocusedInteractable()
        {
            FocusedInteractable = null;
            if (viewCamera == null || (controller != null && !controller.CanAct))
            {
                return;
            }

            Ray ray = new(viewCamera.transform.position, viewCamera.transform.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, interactionDistance, ~0,
                    QueryTriggerInteraction.Ignore))
            {
                return;
            }

            MonoBehaviour[] behaviours = hit.collider.GetComponentsInParent<MonoBehaviour>(true);
            foreach (MonoBehaviour behaviour in behaviours)
            {
                if (behaviour is IPlayerInteractable interactable)
                {
                    FocusedInteractable = interactable;
                    return;
                }
            }
        }
    }
}
