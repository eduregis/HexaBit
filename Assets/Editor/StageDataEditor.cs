#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using HexaBit.Core;

[CustomEditor(typeof(StageData))]
public class StageDataEditor : Editor {
    private StageData data;

    // Foldout states
    private bool showWaves = true;
    private bool showEnemies = true;
    private bool showSubBoss = true;

    private void OnEnable() {
        data = (StageData)target;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        // --- Wave Settings ---
        EditorGUILayout.LabelField("Wave Settings", EditorStyles.boldLabel);
        showWaves = EditorGUILayout.Foldout(showWaves, "Show Wave Settings");
        if (showWaves) {
            EditorGUI.indentLevel++;
            data.spawnInterval = EditorGUILayout.FloatField("Spawn Interval (s)", data.spawnInterval);
            data.maxEnemies = EditorGUILayout.IntField("Max Enemies", data.maxEnemies);
            data.waveInterval = EditorGUILayout.FloatField("Wave Interval (s)", data.waveInterval);
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space(10);

        // --- Enemy Pool ---
        EditorGUILayout.LabelField("Enemy Pool", EditorStyles.boldLabel);

        // Draw Header
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
        GUILayout.Label("Enemy Data", EditorStyles.boldLabel, GUILayout.MinWidth(100));
        GUILayout.Label("Start Time (s)", EditorStyles.boldLabel, GUILayout.Width(80));
        GUILayout.Label("End Time (s)", EditorStyles.boldLabel, GUILayout.Width(80));
        GUILayout.Label("", GUILayout.Width(25)); // Delete button
        EditorGUILayout.EndHorizontal();

        // Draw List
        for (int i = 0; i < data.enemyPool.Count; i++) {
            var info = data.enemyPool[i];
            EditorGUILayout.BeginHorizontal();

            info.enemy = (EnemyData)EditorGUILayout.ObjectField(info.enemy, typeof(EnemyData), false, GUILayout.MinWidth(100));
            info.startTime = EditorGUILayout.FloatField(info.startTime, GUILayout.Width(80));
            info.endTime = EditorGUILayout.FloatField(info.endTime, GUILayout.Width(80));

            if (GUILayout.Button("X", GUILayout.Width(25))) {
                data.enemyPool.RemoveAt(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("+ Add Enemy to Pool")) {
            data.enemyPool.Add(new StageData.EnemySpawnInfo());
        }

        EditorGUILayout.Space(10);

        // --- Sub-Boss Settings ---
        EditorGUILayout.LabelField("Sub-Boss Settings", EditorStyles.boldLabel);
        showSubBoss = EditorGUILayout.Foldout(showSubBoss, "Show Sub-Boss Settings");
        if (showSubBoss) {
            EditorGUI.indentLevel++;
            data.subBossSettings.interval = EditorGUILayout.FloatField("Interval (s)", data.subBossSettings.interval);
            data.subBossSettings.scaleMultiplier = EditorGUILayout.FloatField("Scale Multiplier", data.subBossSettings.scaleMultiplier);
            data.subBossSettings.healthMultiplier = EditorGUILayout.FloatField("Health Multiplier", data.subBossSettings.healthMultiplier);
            data.subBossSettings.damageMultiplier = EditorGUILayout.FloatField("Damage Multiplier", data.subBossSettings.damageMultiplier);

            // Draw Status Pool
            SerializedProperty statusPoolProp = serializedObject.FindProperty("subBossSettings.statusPool");
            EditorGUILayout.PropertyField(statusPoolProp, new GUIContent("Status Pool"), true);

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed) {
            EditorUtility.SetDirty(data);
        }
    }
}
#endif