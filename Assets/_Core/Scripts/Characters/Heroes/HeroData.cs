using System.Collections.Generic;
using UnityEngine;

namespace HexaBit.Core {
    [CreateAssetMenu(fileName = "[hero] New Hero", menuName = "HexaBit/Hero Data")]
    public class HeroData : ScriptableObject {
        [Header("General")]
        public string heroName;
        public Sprite icon;

        [Header("Level Up Visuals")]
        public Sprite levelUpLowerBody;
        public Sprite levelUpUpperBody;

        [Header("Starting Weapon")]
        public WeaponData startingWeapon;

        [Header("Base Stats")]
        public int maxHealth = 100;
        public float moveSpeed = 3.5f;
        public int armor = 0;
        public float damageMultiplier = 1f;      // Might
        public float projectileSpeedMultiplier = 1f;
        public float growthMultiplier = 1f;

        [Header("Animation")]
        public RuntimeAnimatorController animatorController; // Assign the Override Controller here

        [Header("Special Abilities (optional)")]
        public List<CharacterAbility> abilities;
    }

    [System.Serializable]
    public class CharacterAbility {
        public string abilityName;
        public string description;
        public AbilityType type;
        public float valuePerLevel;
        public int maxLevel;
        public int startLevel;
    }

    public enum AbilityType {
        Might,
        Growth,
        MoveSpeed,
        ProjectileSpeed,
        Armor,
        MaxHealth,
        Amount,
        Cooldown,
        Area,
        PickupRadius,
        XPGain
    }

}