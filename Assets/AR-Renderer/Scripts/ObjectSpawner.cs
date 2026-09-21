using UnityEngine;

namespace Qualium_Systems.WebXRPlugin.AR_Renderer.Scripts
{

    public class ObjectSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        Vector3 hitPointPosition;

        private void OnEnable()
        {
            EventBus.onHitPointUpdated += UpdateHitPointPosition;
        }

        private void OnDisable()
        {
            EventBus.onHitPointUpdated -= UpdateHitPointPosition;
        }

        void UpdateHitPointPosition(Vector3 hitPointPosition)
        {
            this.hitPointPosition = hitPointPosition;
        }

        public void SpawnObject()
        {
            Instantiate(prefab, hitPointPosition, Quaternion.identity);
        }
    }
}