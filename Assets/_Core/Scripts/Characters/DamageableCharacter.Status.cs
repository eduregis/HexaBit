using UnityEngine;
using System.Collections.Generic;

namespace HexaBit.Core {
    public abstract partial class DamageableCharacter : MonoBehaviour {

        // ---------- STATUS SYSTEM ----------
        protected List<StatusEffect> activeStatuses = new List<StatusEffect>();
        protected float baseSpeed;

        public float CurrentSpeed { get; protected set; }
        public float DamageMultiplier { get; protected set; } = 1f;

        public System.Action OnStatusChanged;

        public void ApplyStatus(StatusData statusData) {
            StatusEffect existing = activeStatuses.Find(s => s.data == statusData);
            if (existing != null) {
                existing.remainingDuration = statusData.duration;
                Debug.Log($"{gameObject.name}: Status {statusData.statusName} refreshed.");
                return;
            }

            StatusEffect newStatus = new StatusEffect(statusData);

            if (statusData.overrideSpriteColor && spriteRenderer != null) {
                spriteRenderer.color = statusData.spriteColorOverride;
            }

            activeStatuses.Add(newStatus);
            RecalculateStats();
            OnStatusChanged?.Invoke();
            Debug.Log($"{gameObject.name}: Status {statusData.statusName} applied.");
        }

        public void RemoveStatus(StatusData statusData) {
            StatusEffect effect = activeStatuses.Find(s => s.data == statusData);
            if (effect != null) {
                if (effect.visualEffectInstance != null)
                    Destroy(effect.visualEffectInstance);
                activeStatuses.Remove(effect);

                if (spriteRenderer != null) {
                    spriteRenderer.color = originalColor;
                }

                RecalculateStats();
                OnStatusChanged?.Invoke();
                Debug.Log($"{gameObject.name}: Status {statusData.statusName} removed.");
            }
        }

        public void ClearAllStatuses() {
            foreach (var effect in activeStatuses) {
                if (effect.visualEffectInstance != null)
                    Destroy(effect.visualEffectInstance);
            }
            activeStatuses.Clear();

            if (spriteRenderer != null)
                spriteRenderer.color = originalColor;

            RecalculateStats();
            OnStatusChanged?.Invoke();
        }

        protected virtual void UpdateStatuses() {
            if (activeStatuses.Count == 0) return;

            // Create a copy of the list to iterate safely
            List<StatusEffect> statusesCopy = new List<StatusEffect>(activeStatuses);
            List<StatusEffect> expired = new List<StatusEffect>();

            foreach (var status in statusesCopy) {
                // Check if status is still in the original list (could have been removed)
                if (!activeStatuses.Contains(status)) continue;

                status.Update(Time.deltaTime);
                ProcessStatusTick(status);

                if (status.IsExpired)
                    expired.Add(status);
            }

            // Remove expired statuses from the original list
            foreach (var status in expired) {
                if (activeStatuses.Contains(status)) {
                    if (status.visualEffectInstance != null)
                        Destroy(status.visualEffectInstance);
                    activeStatuses.Remove(status);
                    Debug.Log($"{gameObject.name}: Status {status.data.statusName} expired.");
                }
            }

            if (expired.Count > 0) {
                if (spriteRenderer != null)
                    spriteRenderer.color = originalColor;
                RecalculateStats();
                OnStatusChanged?.Invoke();
            }
        }

        protected virtual void ProcessStatusTick(StatusEffect status) {
            // Damage Over Time (DoT)
            if (status.data.applyDamageOverTime && status.data.dotTickInterval > 0) {
                if (status.timer >= status.data.dotTickInterval) {
                    float damage = status.data.dotDamagePerSecond * status.data.dotTickInterval;

                    Color? damageColor = status.data.spriteColorOverride;
                    Debug.Log($"Apply color: {damageColor}");
                    TakeDamage(damage, damageColor);

                    status.ResetTickTimer();
                }
            }

            // Heal Over Time (HoT)
            if (status.data.applyHealOverTime && status.data.hotTickInterval > 0) {
                if (status.timer >= status.data.hotTickInterval) {
                    float heal = status.data.hotHealPerSecond * status.data.hotTickInterval;
                    Heal(heal);
                    status.ResetTickTimer();
                }
            }
        }

        protected virtual void RecalculateStats() {
            DamageMultiplier = 1f;
            float speedMultiplier = 1f;

            foreach (var status in activeStatuses) {
                if (status.data.applySlow)
                    speedMultiplier *= (1f - status.data.slowPercentage);
                    speedMultiplier = Mathf.Clamp(speedMultiplier, 0.1f, 1f);
                if (status.data.applyDamageReduction)
                    DamageMultiplier *= (1f - status.data.damageReductionPercentage);
            }

            CurrentSpeed = baseSpeed * speedMultiplier;
        }
    }
}