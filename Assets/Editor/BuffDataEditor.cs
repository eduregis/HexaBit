#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using HexaBit.Core;

[CustomEditor(typeof(BuffData))]
public class BuffDataEditor : Editor {
    private BuffData data;

    private void OnEnable() {
        data = (BuffData)target;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        // ---- SECTION: General Info ----
        EditorGUILayout.LabelField("General Info", EditorStyles.boldLabel);

        SerializedProperty localizedNameProp = serializedObject.FindProperty("localizedName");
        SerializedProperty localizedDescProp = serializedObject.FindProperty("localizedDescription");

        EditorGUILayout.PropertyField(localizedNameProp, new GUIContent("Localized Name"));
        EditorGUILayout.PropertyField(localizedDescProp, new GUIContent("Localized Description"));

        // Exibe o ícone de forma explícita
        data.icon = (Sprite)EditorGUILayout.ObjectField("Icon", data.icon, typeof(Sprite), false);

        EditorGUILayout.Space(10);

        // ---- SECTION: Settings ----
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        data.type = (AbilityType)EditorGUILayout.EnumPopup("Ability Type", data.type);

        EditorGUILayout.Space(10);

        // ---- SECTION: Values ----
        EditorGUILayout.LabelField("Values (Level 1 to 6)", EditorStyles.boldLabel);

        // Garante que o array tenha exatamente 6 posições
        if (data.values == null || data.values.Length != 6) {
            System.Array.Resize(ref data.values, 6);
        }

        for (int i = 0; i < data.values.Length; i++) {
            data.values[i] = EditorGUILayout.FloatField($"Level {i + 1}", data.values[i]);
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed) {
            EditorUtility.SetDirty(data);
        }
    }
}
#endif