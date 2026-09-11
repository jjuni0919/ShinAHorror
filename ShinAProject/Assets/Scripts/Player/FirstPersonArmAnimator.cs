using UnityEngine;

namespace ShinA.Player
{
    public sealed class FirstPersonArmAnimator : MonoBehaviour
    {
        [SerializeField] private float walkFrequency = 6f;
        [SerializeField] private float runFrequency = 10f;
        [SerializeField] private float crouchFrequency = 2.5f;
        [SerializeField] private float walkSwingAngle = 7f;
        [SerializeField] private float runSwingAngle = 14f;
        [SerializeField] private float crouchSwingAngle = 3.5f;
        [SerializeField] private float smoothing = 12f;

        private FirstPersonController controller;
        private Transform leftArm;
        private Transform rightArm;
        private Quaternion leftBaseRotation;
        private Quaternion rightBaseRotation;
        private Vector3 basePosition;
        private float cycle;

        public void Initialize(FirstPersonController targetController, Transform left, Transform right)
        {
            controller = targetController;
            leftArm = left;
            rightArm = right;
            basePosition = transform.localPosition;

            if (leftArm != null)
            {
                leftBaseRotation = leftArm.localRotation;
            }

            if (rightArm != null)
            {
                rightBaseRotation = rightArm.localRotation;
            }
        }

        private void LateUpdate()
        {
            if (controller == null)
            {
                return;
            }

            bool moving = controller.MovementAmount > 0.01f;
            float frequency = controller.IsRunning
                ? runFrequency
                : controller.IsCrouching ? crouchFrequency : walkFrequency;
            float swingAngle = controller.IsRunning
                ? runSwingAngle
                : controller.IsCrouching ? crouchSwingAngle : walkSwingAngle;

            if (moving)
            {
                cycle += Time.deltaTime * frequency;
            }

            float movementBlend = moving ? controller.MovementAmount : 0f;
            float swing = Mathf.Sin(cycle) * swingAngle * movementBlend;
            float bob = Mathf.Abs(Mathf.Sin(cycle)) * 0.018f * movementBlend;
            float damping = 1f - Mathf.Exp(-smoothing * Time.deltaTime);

            transform.localPosition = Vector3.Lerp(
                transform.localPosition, basePosition + Vector3.down * bob, damping);

            if (leftArm != null)
            {
                Quaternion target = leftBaseRotation * Quaternion.Euler(swing, 0f, 0f);
                leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, target, damping);
            }

            if (rightArm != null)
            {
                Quaternion target = rightBaseRotation * Quaternion.Euler(-swing, 0f, 0f);
                rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, target, damping);
            }
        }
    }
}
