using UnityEngine;

namespace HexaBit.Core {
    public class DropCollector : MonoBehaviour {
        [Header("References")]
        [SerializeField] private HeroController heroController;

        [Header("Collection Settings")]
        [SerializeField] private float baseRadius = 1.5f;

        private CircleCollider2D collectorCollider;

        private void Awake() {
            collectorCollider = GetComponent<CircleCollider2D>();
            if (collectorCollider == null) {
                collectorCollider = gameObject.AddComponent<CircleCollider2D>();
            }

            collectorCollider.isTrigger = true;
            collectorCollider.radius = baseRadius;

            if (heroController == null) {
                heroController = GetComponentInParent<HeroController>();
            }
        }

        private void Start() => UpdateRadius();
        private void Update() => UpdateRadius();

        private void UpdateRadius() {
            if (heroController == null) return;

            float totalRadius = baseRadius + heroController.GlobalPickupRadius;
            if (collectorCollider != null) {
                collectorCollider.radius = totalRadius;
            }
        }

        private void OnTriggerEnter2D(Collider2D other) {
            if (heroController == null || heroController.IsDead) return;

            PickupBase pickup = other.GetComponent<PickupBase>();
            if (pickup != null && !pickup.IsCollected) {
                pickup.SetTargetHero(heroController);
            }
        }
    }
}