using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace HexaBit.Core {
    [CreateAssetMenu(fileName = "UpgradePool", menuName = "HexaBit/Upgrade Pool Data")]
    public class UpgradePoolData : ScriptableObject {
        [Header("Weapons")]
        public List<WeaponData> weapons = new List<WeaponData>();

        [Header("Buffs")]
        public List<BuffData> buffs = new List<BuffData>();

        /// <summary>
        /// Returns a list of valid upgrades for the specific hero based on max levels and available slots.
        /// </summary>
        public List<Object> GetAvailableItems(HeroController hero) {
            List<Object> available = new List<Object>();

            if (hero == null) return available;

            // Check if hero has empty slots (Max 5 slots assumed by HeroController)
            bool hasEmptyWeaponSlot = hero.weaponSlots.Any(slot => slot == null || slot.data == null);
            bool hasEmptyBuffSlot = hero.activeBuffs.Count < 5;

            // --- FILTER WEAPONS ---
            foreach (var weapon in weapons) {
                if (weapon == null) continue;

                var equippedWeapon = hero.weaponSlots.FirstOrDefault(w => w != null && w.data == weapon);

                if (equippedWeapon != null) {
                    if (equippedWeapon.currentLevel < WeaponBase.MAX_LEVEL_INDEX) {
                        available.Add(weapon);
                    }
                } else {
                    // If not equipped, only add if there is an empty slot
                    if (hasEmptyWeaponSlot) {
                        available.Add(weapon);
                    }
                }
            }

            // --- FILTER BUFFS ---
            foreach (var buff in buffs) {
                if (buff == null) continue;

                var equippedBuff = hero.activeBuffs.Find(b => b.data == buff);

                if (equippedBuff != null) {
                    // If already active, only add if NOT at max level
                    if (equippedBuff.currentLevel < buff.MaxLevel - 1) {
                        available.Add(buff);
                    }
                } else {
                    // If not active, only add if there is an empty buff slot
                    if (hasEmptyBuffSlot) {
                        available.Add(buff);
                    }
                }
            }

            return available;
        }
    }
}