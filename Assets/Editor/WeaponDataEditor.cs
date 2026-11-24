#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using HexaBit.Core;

[CustomEditor(typeof(WeaponData))]
public class WeaponDataEditor : Editor {
    private WeaponData data;

    private void OnEnable() {
        data = (WeaponData)target;
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

        EditorGUILayout.Space(10);

        // ---- SECTION: Status Data ----
        EditorGUILayout.LabelField("Status Data (Optionals)", EditorStyles.boldLabel);
        data.statusData = (StatusData)EditorGUILayout.ObjectField("Status Data", data.statusData, typeof(StatusData), false);
        EditorGUILayout.Space(10);

        // ---- SECTION: Dynamic Fields ----
        EditorGUILayout.LabelField("Dynamic Fields", EditorStyles.boldLabel);
        DrawCustomFields();
        EditorGUILayout.Space(10);

        // ---- SECTION: Curves ----
        EditorGUILayout.LabelField("Level Curves (Lv.1 = X:0, Lv.6 = X:5)", EditorStyles.boldLabel);

        SerializedProperty damageCurveProp = serializedObject.FindProperty("damageCurve");
        SerializedProperty cooldownCurveProp = serializedObject.FindProperty("cooldownCurve");

        DrawCurveWithFields(damageCurveProp, "Damage", isInt: false);
        EditorGUILayout.Space(5);
        DrawCurveWithFields(cooldownCurveProp, "Cooldown", isInt: false);

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed) {
            EditorUtility.SetDirty(data);
        }
    }

    // ------------------------------------------------------------
    // 1. RENDERS THE LIST OF DYNAMIC FIELDS
    // ------------------------------------------------------------
    private void DrawCustomFields() {
        SerializedProperty customFieldsProp = serializedObject.FindProperty("customFields");

        for (int i = 0; i < data.customFields.Count; i++) {
            var def = data.customFields[i];
            var defProp = customFieldsProp.GetArrayElementAtIndex(i);

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

            // Localized Name Field
            SerializedProperty localizedNameProp = defProp.FindPropertyRelative("localizedName");
            EditorGUILayout.PropertyField(localizedNameProp, new GUIContent("Localized Name"));

            SerializedProperty appearsInDescProp = defProp.FindPropertyRelative("appearsInUpgradeDescription");
            EditorGUILayout.PropertyField(appearsInDescProp, new GUIContent("Appears in Upgrade Description"));

            // Render the curve and numeric fields for this dynamic field
            SerializedProperty curveProp = defProp.FindPropertyRelative("curve");
            bool isIntType = def.parameter.GetFieldType() == DynamicFieldType.Int;

            DrawCurveWithFields(curveProp, "Level Curve", isIntType);

            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("+ Add Field", GUILayout.Width(120))) {
            AddCustomField();
        }
        EditorGUILayout.EndHorizontal();
    }

    private void AddCustomField() {
        var newDef = new DynamicFieldDefinition { parameter = DynamicParameter.Speed };
        newDef.curve = AnimationCurve.Linear(0, 1, 5, 5);
        data.customFields.Add(newDef);
        EditorUtility.SetDirty(data);
    }

    private void RemoveCustomField(int index) {
        data.customFields.RemoveAt(index);
        EditorUtility.SetDirty(data);
    }

    // ------------------------------------------------------------
    // 2. HELPER METHOD TO DRAW CURVE + NUMERIC FIELDS
    // ------------------------------------------------------------
    private void DrawCurveWithFields(SerializedProperty curveProp, string label, bool isInt) {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        // Draw the standard Unity curve field (the green graph)
        EditorGUILayout.PropertyField(curveProp, new GUIContent(label));

        // Get the actual AnimationCurve object from the property
        AnimationCurve curve = curveProp.animationCurveValue;

        // Draw 6 numeric fields for the 6 levels
        EditorGUILayout.BeginHorizontal();
        for (int i = 0; i < 6; i++) {
            float currentValue = curve.Evaluate(i);

            EditorGUILayout.BeginVertical(GUILayout.Width(40));
            GUILayout.Label($"Lv.{i + 1}", EditorStyles.centeredGreyMiniLabel);

            float newValue;
            if (isInt) {
                newValue = EditorGUILayout.IntField(Mathf.RoundToInt(currentValue));
            } else {
                newValue = EditorGUILayout.FloatField(currentValue);
            }

            // If the value changed in the field, update the curve
            if (!Mathf.Approximately(currentValue, newValue)) {
                // Remove existing key at this time, if any
                for (int k = curve.length - 1; k >= 0; k--) {
                    if (Mathf.Approximately(curve.keys[k].time, i)) {
                        curve.RemoveKey(k);
                    }
                }
                // Add the new key
                curve.AddKey(i, newValue);

                // Assign back to the property so Unity saves it
                curveProp.animationCurveValue = curve;
            }
            EditorGUILayout.EndVertical();
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}
#endif