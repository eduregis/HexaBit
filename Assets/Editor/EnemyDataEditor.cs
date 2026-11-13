#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using HexaBit.Core;

[CustomEditor(typeof(EnemyData))]
public class EnemyDataEditor : Editor {
    private EnemyData data;

    private void OnEnable() {
        data = (EnemyData)target;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        // ---- SECTION: General Info ----
        EditorGUILayout.LabelField("General Info", EditorStyles.boldLabel);
        data.enemyName = EditorGUILayout.TextField("Enemy Name", data.enemyName);

        // Exibe o ícone do inimigo (igual ao HeroData)
        data.icon = (Sprite)EditorGUILayout.ObjectField("Icon", data.icon, typeof(Sprite), false);
        data.prefab = (GameObject)EditorGUILayout.ObjectField("Prefab", data.prefab, typeof(GameObject), false);

        EditorGUILayout.Space(10);

        // ---- SECTION: Animation ----
        EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
        data.animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Animator Controller", data.animatorController, typeof(RuntimeAnimatorController), false);

        EditorGUILayout.Space(10);

        // ---- SECTION: Base Stats ----
        EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
        data.baseHealth = EditorGUILayout.IntField("Base Health", data.baseHealth);
        data.damage = EditorGUILayout.IntField("Damage", data.damage);
        data.moveSpeed = EditorGUILayout.FloatField("Move Speed", data.moveSpeed);
        data.knockbackResistance = EditorGUILayout.FloatField("Knockback Resistance", data.knockbackResistance);

        EditorGUILayout.Space(10);

        // ---- SECTION: Ranged Settings (Conditional) ----
        EditorGUILayout.LabelField("Ranged Settings", EditorStyles.boldLabel);
        data.isRanged = EditorGUILayout.Toggle("Is Ranged", data.isRanged);

        // Use an indent and background box to visually group the ranged settings
        if (data.isRanged) {
            EditorGUI.indentLevel++;
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            data.attackRange = EditorGUILayout.FloatField("Attack Range", data.attackRange);
            data.attackCooldown = EditorGUILayout.FloatField("Attack Cooldown", data.attackCooldown);

            EditorGUILayout.EndVertical();
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // ---- SECTION: Resistances ----
        EditorGUILayout.LabelField("Resistances", EditorStyles.boldLabel);
        data.immuneToFreeze = EditorGUILayout.Toggle("Immune To Freeze", data.immuneToFreeze);
        data.immuneToInstantKill = EditorGUILayout.Toggle("Immune To Instant Kill", data.immuneToInstantKill);
        data.immuneToDebuffs = EditorGUILayout.Toggle("Immune To Debuffs", data.immuneToDebuffs);

        EditorGUILayout.Space(10);

        // ---- SECTION: Special Behaviors (Skills) ----
        EditorGUILayout.LabelField("Special Behaviors (Skills)", EditorStyles.boldLabel);
        data.healthScalesWithPlayerLevel = EditorGUILayout.Toggle("Health Scales With Level", data.healthScalesWithPlayerLevel);
        data.fixedDirection = EditorGUILayout.Toggle("Fixed Direction", data.fixedDirection);
        data.wavyMovement = EditorGUILayout.Toggle("Wavy Movement", data.wavyMovement);
        data.passesThroughWalls = EditorGUILayout.Toggle("Passes Through Walls", data.passesThroughWalls);

        data.selfDestruct = EditorGUILayout.Toggle("Self Destruct", data.selfDestruct);
        if (data.selfDestruct) {
            EditorGUI.indentLevel++;
            data.selfDestructDamage = EditorGUILayout.IntField("Self Destruct Damage", data.selfDestructDamage);
            EditorGUI.indentLevel--;
        }

        data.isBoss = EditorGUILayout.Toggle("Is Boss", data.isBoss);

        EditorGUILayout.Space(10);

        // ---- SECTION: Scaling ----
        EditorGUILayout.LabelField("Scaling (Optional)", EditorStyles.boldLabel);
        data.healthMultiplierPerMinute = EditorGUILayout.FloatField("Health Multiplier / Min", data.healthMultiplierPerMinute);
        data.speedMultiplierPerMinute = EditorGUILayout.FloatField("Speed Multiplier / Min", data.speedMultiplierPerMinute);

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed) {
            EditorUtility.SetDirty(data);
        }
    }
}
#endif