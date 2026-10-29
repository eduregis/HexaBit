using UnityEngine;
using System.Collections.Generic;

namespace HexaBit.Core {
    public partial class HeroController {

        public List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

        public float GlobalDamageMultiplier { get; private set; } = 1f;
        public float GlobalCooldownModifier { get; private set; } = 1f;
        public float GlobalAreaModifier { get; private set; } = 1f;
        public float GlobalPickupRadius { get; private set; } = 0.5f;
        public float GlobalXPMultiplier { get; private set; } = 1f;
        public float GlobalArmor { get; private set; } = 0f;

        private void RefreshStats() {
            if (heroData == null) return;

            float damageBonus = 0f;
            float cooldownBonus = 0f;
            float areaBonus = 0f;
            float speedBonus = 0f;
            float pickupBonus = 0f;
            float xpBonus = 0f;
            float hpBonus = 0f;
            float armorBonus = 0f;

            foreach (var active in activeBuffs) {
                float val = active.data.GetValue(active.currentLevel);
                switch (active.data.type) {
                    case AbilityType.Might: damageBonus += val; break;
                    case AbilityType.Cooldown: cooldownBonus += val; break;
                    case AbilityType.Area: areaBonus += val; break;
                    case AbilityType.MoveSpeed: speedBonus += val; break;
                    case AbilityType.MaxHealth: hpBonus += val; break;
                    case AbilityType.Armor: armorBonus += val; break;
                    case AbilityType.PickupRadius: pickupBonus += val; break;
                    case AbilityType.XPGain: xpBonus += val; break;
                }
            }

            GlobalDamageMultiplier = heroData.damageMultiplier * (1f + damageBonus);
            GlobalCooldownModifier = 1f / (1f + cooldownBonus);
            GlobalAreaModifier = 1f + areaBonus;
            GlobalArmor = heroData.armor + armorBonus;
            GlobalXPMultiplier = heroData.growthMultiplier * (1f + xpBonus);
            GlobalPickupRadius = 0.5f + pickupBonus;

            currentSpeed = heroData.moveSpeed * (1f + speedBonus);

            int newMaxHealth = Mathf.RoundToInt(heroData.maxHealth + hpBonus);
            if (newMaxHealth != maxHealth) {
                int diff = newMaxHealth - maxHealth;
                maxHealth = newMaxHealth;
                currentHealth += diff;
                if (currentHealth > maxHealth) currentHealth = maxHealth;
                OnHealthChanged?.Invoke();
            }
        }

        public void ApplyBuff(BuffData buffData) {
            ActiveBuff existing = activeBuffs.Find(b => b.data == buffData);
            if (existing != null) {
                existing.LevelUp();
            } else {
                activeBuffs.Add(new ActiveBuff(buffData, 1));
            }
            RefreshStats();
        }

        public override void TakeDamage(float amount, Color? popupColor = null) {
            float reduced = Mathf.Max(1f, amount - GlobalArmor);
            base.TakeDamage(reduced);
        }
    }
}