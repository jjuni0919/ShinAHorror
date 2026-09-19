using UnityEngine;

namespace ShinA.Player
{
    public sealed class FirstPersonAttackAnimator : MonoBehaviour
    {
        private Vector3 basePosition;
        private Quaternion baseRotation;
        private float duration;
        private float elapsed;
        private bool ranged;

        private void Awake()
        {
            basePosition = transform.localPosition;
            baseRotation = transform.localRotation;
            enabled = false;
        }

        public void Play(bool isRanged)
        {
            ranged = isRanged;
            duration = isRanged ? 0.16f : 0.34f;
            elapsed = 0f;
            enabled = true;
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            float normalized = duration > 0f ? Mathf.Clamp01(elapsed / duration) : 1f;
            float pulse = Mathf.Sin(normalized * Mathf.PI);

            if (ranged)
            {
                transform.localPosition = basePosition + Vector3.back * (pulse * 0.1f);
                transform.localRotation = baseRotation * Quaternion.Euler(-pulse * 8f, 0f, 0f);
            }
            else
            {
                transform.localPosition = basePosition + new Vector3(pulse * 0.08f, -pulse * 0.05f, pulse * 0.12f);
                transform.localRotation = baseRotation * Quaternion.Euler(-pulse * 65f, pulse * 18f, pulse * 10f);
            }

            if (normalized >= 1f)
            {
                transform.localPosition = basePosition;
                transform.localRotation = baseRotation;
                enabled = false;
            }
        }

        private void OnDisable()
        {
            transform.localPosition = basePosition;
            transform.localRotation = baseRotation;
        }
    }
}
