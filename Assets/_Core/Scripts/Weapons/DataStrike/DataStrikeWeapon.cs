using UnityEngine;
using System.Collections.Generic;

namespace HexaBit.Core {
    public class DataStrikeWeapon : WeaponBase {
        protected override void Fire() {
            if (data.attackPrefab == null) return;

            int extraTargets = data.GetInt(currentLevel, DynamicParameter.ExtraTargets);
            float splashArea = data.GetFloat(currentLevel, DynamicParameter.SplashArea);

            List<Transform> targets = GetValidTargets(extraTargets);
            if (targets.Count == 0) return;

            int maxBolts = Mathf.Min(targets.Count, extraTargets + 1);
            for (int i = 0; i < maxBolts; i++) {
                Transform target = targets[i];
                if (target == null) continue;

                GameObject boltGO = Instantiate(data.attackPrefab, target.position, Quaternion.identity);
                DataStrikeEffect effect = boltGO.GetComponent<DataStrikeEffect>();
                if (effect != null) {
                    effect.Initialize(data, currentLevel, Vector2.zero, hero);
                    effect.SetTarget(target);
                }
            }
        }

        private List<Transform> GetValidTargets(int extraTargets) {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            List<Transform> validTargets = new List<Transform>();

            Camera mainCamera = Camera.main;
            if (mainCamera == null) return validTargets;

            foreach (var enemyGO in enemies) {
                EnemyController enemyCtrl = enemyGO.GetComponent<EnemyController>();
                if (enemyCtrl == null) continue;

                Vector3 viewportPos = mainCamera.WorldToViewportPoint(enemyGO.transform.position);
                if (viewportPos.x >= 0 && viewportPos.x <= 1 &&
                    viewportPos.y >= 0 && viewportPos.y <= 1 &&
                    viewportPos.z > 0)
                {
                    validTargets.Add(enemyGO.transform);
                }
            }

            validTargets.Sort((a, b) =>
                Vector3.Distance(transform.position, a.position)
                .CompareTo(Vector3.Distance(transform.position, b.position))
            );

            return validTargets;
        }
    }
}