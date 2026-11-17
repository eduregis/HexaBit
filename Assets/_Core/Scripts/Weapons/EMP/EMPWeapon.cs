using UnityEngine;
using UnityEngine.Events;

namespace HexaBit.Core {
    public class EMPWeapon : WeaponBase {

        private EMPField activeField;


        [Header("Events")]
        [Tooltip("Called when the weapon fires.")]
        public UnityEvent OnFireEvent;

        protected override void Fire() {
            // Only spawn if no field exists yet
            if (activeField != null) return;

            if (data.attackPrefab == null) {
                Debug.LogWarning("EMPWeapon: attackPrefab is null!");
                return;
            }

            // Instantiate the field as a child of the hero
            GameObject fieldGO = Instantiate(data.attackPrefab, hero.transform);
            fieldGO.transform.localPosition = Vector3.zero;

            activeField = fieldGO.GetComponent<EMPField>();
            if (activeField != null) {
                OnFireEvent?.Invoke();
                activeField.Initialize(data, currentLevel, hero);
            } else {
                Debug.LogError("EMPWeapon: attackPrefab does not have EMPField component!");
                Destroy(fieldGO);
            }
        }

        // Override LevelUp to refresh the field with new stats
        public override void LevelUp() {
            // Call base to increment level
            base.LevelUp();

            // Refresh the field (destroy old and create new with updated level)
            RefreshField();
        }

        // Destroys the current field and creates a new one with the current level
        private void RefreshField() {
            // Destroy existing field if any
            if (activeField != null) {
                Destroy(activeField.gameObject);
                activeField = null;
            }

            // Recreate the field with the new level
            Fire(); // This will instantiate a new field using the updated currentLevel
        }

        private void OnDestroy() {
            if (activeField != null) {
                Destroy(activeField.gameObject);
                activeField = null;
            }
        }
    }
}