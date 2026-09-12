using UnityEngine;

namespace ShinA.Player
{
    [DefaultExecutionOrder(1000)]
    public sealed class HandMountFollower : MonoBehaviour
    {
        private Transform target;
        private Vector3 targetLocalOffset;
        private Quaternion rotationOffset;

        public void Initialize(Transform followTarget, Vector3 localOffset, Quaternion localRotationOffset)
        {
            target = followTarget;
            targetLocalOffset = localOffset;
            rotationOffset = localRotationOffset;
            FollowTarget();
        }

        private void LateUpdate()
        {
            FollowTarget();
        }

        private void FollowTarget()
        {
            if (target == null)
            {
                return;
            }

            transform.SetPositionAndRotation(target.TransformPoint(targetLocalOffset), target.rotation * rotationOffset);
        }
    }
}
