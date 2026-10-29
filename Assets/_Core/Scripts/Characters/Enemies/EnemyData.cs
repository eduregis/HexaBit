using System.Collections.Generic;
using UnityEngine;

namespace HexaBit.Core {
    [CreateAssetMenu(fileName = "[enemy] New Enemy", menuName = "HexaBit/Enemy Data")]
    public class EnemyData : ScriptableObject {
        [Header("General Info")]
        public string enemyName;
        public Sprite icon;
        public GameObject prefab;

        [Header("Animation")]
        public RuntimeAnimatorController animatorController;

        [Header("Spawn Settings")]
        public float startTime = 0f;

        [Header("Base Stats")]
        public int baseHealth = 10;
        public int damage = 5;
        public float moveSpeed = 100f;
        public float knockbackResistance = 1f; // 1 = normal, 0 = imune
        public int xpReward = 1;

        [Header("Resistances")]
        public bool immuneToFreeze = false;
        public bool immuneToInstantKill = false;
        public bool immuneToDebuffs = false;

        [Header("Special Behaviors (Skills)")]
        public bool healthScalesWithPlayerLevel = false;
        public bool fixedDirection = false;      // Anda em linha reta
        public bool wavyMovement = false;        // Padrão ondulatório
        public bool passesThroughWalls = false;
        public bool selfDestruct = false;
        public int selfDestructDamage = 0;
        public bool isBoss = false;

        [Header("Scaling (optional)")]
        public float healthMultiplierPerMinute = 1.0f; // Aumento de HP por minuto
        public float speedMultiplierPerMinute = 1.0f;  // Aumento de velocidade por minuto
    }
}