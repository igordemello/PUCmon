using UnityEngine;

namespace PUCmon.AR
{
    public sealed class ImageTargetPrototype : MonoBehaviour
    {
        [SerializeField] private Transform creature;
        [SerializeField] private float emergeDistance = 1.25f;
        [SerializeField] private float transitionSeconds = 0.8f;
        [SerializeField] private bool simulateTracking = true;

        private Vector3 hiddenPosition;
        private Vector3 visibleScale;
        private Quaternion baseRotation;
        private float visibility;
        private bool targetVisible;

        private void Awake()
        {
            if (creature == null)
            {
                Debug.LogError("Assign a creature to the image target prototype.", this);
                enabled = false;
                return;
            }

            hiddenPosition = creature.localPosition;
            visibleScale = creature.localScale;
            baseRotation = creature.localRotation;
            targetVisible = simulateTracking;
            visibility = targetVisible ? 1f : 0f;
            ApplyPose();
        }

        private void Update()
        {
            if (simulateTracking)
                targetVisible = Mathf.Repeat(Time.time, 6f) < 3f;

            visibility = Mathf.MoveTowards(
                visibility,
                targetVisible ? 1f : 0f,
                Time.deltaTime / Mathf.Max(transitionSeconds, 0.01f));

            ApplyPose();
        }

        public void TargetFound() => targetVisible = true;

        public void TargetLost() => targetVisible = false;

        private void ApplyPose()
        {
            float eased = Mathf.SmoothStep(0f, 1f, visibility);
            creature.localPosition = hiddenPosition + Vector3.back * (emergeDistance * eased);
            creature.localScale = Vector3.Lerp(visibleScale * 0.15f, visibleScale, eased);
            creature.localRotation = baseRotation * Quaternion.Euler(0f, 360f * eased, 0f);
        }
    }
}
