#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using HexaBit.Core;

[CustomEditor(typeof(WeaponData))]
public class WeaponDataEditor : Editor {
    private WeaponData data;
    private bool showLevels = true;

    private void OnEnable() {
        data = (WeaponData)target;
        ValidateAndSyncLevels();
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        // ---- SECTION: General Info ----
        EditorGUILayout.LabelField("General Info", EditorStyles.boldLabel);

        SerializedProperty localizedNameProp = serializedObject.FindProperty("localizedName");
        SerializedProperty localizedDescProp = serializedObject.FindProperty("localizedDescription");

        EditorGUILayout.PropertyField(localizedNameProp, new GUIContent("Localized Name"));
        EditorGUILayout.PropertyField(localizedDescProp, new GUIContent("Localized Description"));

        data.icon = (Sprite)EditorGUILayout.ObjectField("Icon", data.icon, typeof(Sprite), false);
        data.attackPrefab = (GameObject)EditorGUILayout.ObjectField("Attack Prefab", data.attackPrefab, typeof(GameObject), false);
        data.weaponPrefab = (GameObject)EditorGUILayout.ObjectField("Weapon Prefab", data.weaponPrefab, typeof(GameObject), false);

        // ---- SECTION: Status Data (Optionals) ----
        EditorGUILayout.LabelField("Status Data (Optionals)", EditorStyles.boldLabel);
        data.statusData = (StatusData)EditorGUILayout.ObjectField("Status Data", data.statusData, typeof(StatusData), false);
        EditorGUILayout.Space(10);

        // ---- SECTION: Dynamic Fields (Optionals) ----
        EditorGUILayout.LabelField("Dynamic Fields (Optionals)", EditorStyles.boldLabel);
        DrawCustomFields();

        EditorGUILayout.Space(10);

        // ---- SECTION: Levels (Table) ----
        EditorGUILayout.LabelField("Levels", EditorStyles.boldLabel);
        showLevels = EditorGUILayout.Foldout(showLevels, $"Levels ({data.levels.Count})");
        if (showLevels) {
            DrawLevelsTable();
        }

        // Button to add level
        EditorGUILayout.Space(5);
        if (GUILayout.Button("+ Add New Level (Lv." + (data.levels.Count + 1) + ")")) {
            AddNewLevel();
        }

        serializedObject.ApplyModifiedProperties();
    }

    // ------------------------------------------------------------
    // 1. RENDERS THE LIST OF DYNAMIC FIELDS
    // ------------------------------------------------------------
    private void DrawCustomFields() {
        for (int i = 0; i < data.customFields.Count; i++) {
            var def = data.customFields[i];
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();

            // Icon Field
            def.icon = (Sprite)EditorGUILayout.ObjectField(def.icon, typeof(Sprite), false, GUILayout.Width(50), GUILayout.Height(50));

            EditorGUILayout.BeginVertical();
            // Parameter Dropdown (Enum)
            def.parameter = (DynamicParameter)EditorGUILayout.EnumPopup("Parameter", def.parameter);

            // Type Display (Read-only)
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.EnumPopup("Type", def.parameter.GetFieldType());
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();

            // REMOVE button
            if (GUILayout.Button("X", GUILayout.Width(25), GUILayout.Height(50))) {
                RemoveCustomField(i);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                break;
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.EndVertical();
        }

        // Button ADD FIELD
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Field", GUILayout.Width(120))) {
            AddCustomField();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void AddCustomField() {
        data.customFields.Add(new DynamicFieldDefinition { parameter = DynamicParameter.Speed });
        ValidateAndSyncLevels();
        EditorUtility.SetDirty(data);
    }

    private void RemoveCustomField(int index) {
        DynamicParameter paramToRemove = data.customFields[index].parameter;
        data.customFields.RemoveAt(index);

        // Removes the corresponding value from ALL levels
        foreach (var level in data.levels) {
            level.customValues.RemoveAll(v => v.parameter == paramToRemove);
        }

        ValidateAndSyncLevels();
        EditorUtility.SetDirty(data);
    }

    // ------------------------------------------------------------
    // 2. DRAW THE LEVELS TABLE
    // ------------------------------------------------------------
    private void DrawLevelsTable() {
        if (data.levels.Count == 0) {
            EditorGUILayout.HelpBox("No levels created. Click 'Add New Level'.", MessageType.Info);
            return;
        }

        // --- HEADER ---
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        GUILayout.Label("Level", EditorStyles.boldLabel, GUILayout.Width(50));
        GUILayout.Label("Damage", EditorStyles.boldLabel, GUILayout.Width(60));
        GUILayout.Label("Cooldown", EditorStyles.boldLabel, GUILayout.Width(60));

        foreach (var def in data.customFields) {
            // Show icon in the table header
            GUIContent headerContent = new GUIContent(def.parameter.ToString());
            if (def.icon != null) {
                headerContent = new GUIContent(def.icon.texture, def.parameter.ToString());
            }
            GUILayout.Label(headerContent, EditorStyles.boldLabel, GUILayout.MinWidth(60), GUILayout.Height(25));
        }

        GUILayout.Label("", GUILayout.Width(25));
        EditorGUILayout.EndHorizontal();

        // --- TABLE ROWS (EACH LEVEL) ---
        for (int i = 0; i < data.levels.Count; i++) {
            var level = data.levels[i];
            EditorGUILayout.BeginHorizontal();

            GUILayout.Label($"Lv. {i + 1}", GUILayout.Width(50));

            level.damage = EditorGUILayout.FloatField(level.damage, GUILayout.Width(60));
            level.cooldown = EditorGUILayout.FloatField(level.cooldown, GUILayout.Width(60));

            // Dynamic fields
            foreach (var def in data.customFields) {
                var value = level.customValues.Find(v => v.parameter == def.parameter);
                if (value == null) {
                    value = new DynamicFieldValue {
                        parameter = def.parameter,
                        fieldType = def.parameter.GetFieldType()
                    };
                    level.customValues.Add(value);
                }

                // Draws the field according to its inferred type
                switch (value.fieldType) {
                    case DynamicFieldType.Int:
                        value.intValue = EditorGUILayout.IntField(value.intValue, GUILayout.MinWidth(50));
                        break;
                    case DynamicFieldType.Float:
                        value.floatValue = EditorGUILayout.FloatField(value.floatValue, GUILayout.MinWidth(50));
                        break;
                    case DynamicFieldType.Bool:
                        value.boolValue = EditorGUILayout.Toggle(value.boolValue, GUILayout.MinWidth(40));
                        break;
                }
            }

            if (GUILayout.Button("X", GUILayout.Width(25))) {
                RemoveLevel(i);
                break;
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    // ------------------------------------------------------------
    // 3. AUXILIARY FUNCTIONS (SYNC AND CREATION)
    // ------------------------------------------------------------
    private void ValidateAndSyncLevels() {
        foreach (var level in data.levels) {
            // Remove undefined values
            level.customValues.RemoveAll(v => !data.customFields.Exists(d => d.parameter == v.parameter));

            // Add missing values
            foreach (var def in data.customFields) {
                if (!level.customValues.Exists(v => v.parameter == def.parameter)) {
                    level.customValues.Add(new DynamicFieldValue {
                        parameter = def.parameter,
                        fieldType = def.parameter.GetFieldType()
                    });
                }
            }
        }

        EditorUtility.SetDirty(data);
    }

    private void AddNewLevel() {
        WeaponLevelData newLevel = new WeaponLevelData();
        newLevel.damage = 0f;
        newLevel.cooldown = 0f;

        foreach (var def in data.customFields) {
            newLevel.customValues.Add(new DynamicFieldValue {
                parameter = def.parameter,
                fieldType = def.parameter.GetFieldType()
            });
        }

        data.levels.Add(newLevel);
        EditorUtility.SetDirty(data);
    }

    private void RemoveLevel(int index) {
        data.levels.RemoveAt(index);
        EditorUtility.SetDirty(data);
    }
}
#endif