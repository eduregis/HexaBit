#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using HexaBit.Core;

[CustomEditor(typeof(HeroData))]
public class HeroDataEditor : Editor {
    private HeroData data;
    private bool showAbilities = true;

    private void OnEnable() {
        data = (HeroData)target;
    }

    public override void OnInspectorGUI() {
        serializedObject.Update();

        // ---- SECTION: General ----
        EditorGUILayout.LabelField("General Info", EditorStyles.boldLabel);
        data.heroName = EditorGUILayout.TextField("Hero Name", data.heroName);

        // Exibe o ícone do herói
        data.icon = (Sprite)EditorGUILayout.ObjectField("Icon", data.icon, typeof(Sprite), false);

        EditorGUILayout.Space(10);

        // ---- SECTION: Level Up Visuals ----
        EditorGUILayout.LabelField("Level Up Visuals", EditorStyles.boldLabel);
        data.levelUpUpperBody = (Sprite)EditorGUILayout.ObjectField("Level Up Upper Body", data.levelUpUpperBody, typeof(Sprite), false);
        data.levelUpLowerBody = (Sprite)EditorGUILayout.ObjectField("Level Up Lower Body", data.levelUpLowerBody, typeof(Sprite), false);

        EditorGUILayout.Space(10);

        // ---- SECTION: Starting Weapon ----
        EditorGUILayout.LabelField("Starting Weapon", EditorStyles.boldLabel);
        data.startingWeapon = (WeaponData)EditorGUILayout.ObjectField("Starting Weapon", data.startingWeapon, typeof(WeaponData), false);

        EditorGUILayout.Space(10);

        // ---- SECTION: Base Stats ----
        EditorGUILayout.LabelField("Base Stats", EditorStyles.boldLabel);
        data.maxHealth = EditorGUILayout.IntField("Max Health", data.maxHealth);
        data.moveSpeed = EditorGUILayout.FloatField("Move Speed", data.moveSpeed);
        data.armor = EditorGUILayout.IntField("Armor", data.armor);
        data.damageMultiplier = EditorGUILayout.FloatField("Damage Multiplier", data.damageMultiplier);
        data.projectileSpeedMultiplier = EditorGUILayout.FloatField("Projectile Speed Multiplier", data.projectileSpeedMultiplier);
        data.growthMultiplier = EditorGUILayout.FloatField("Growth Multiplier", data.growthMultiplier);

        EditorGUILayout.Space(10);

        // ---- SECTION: Animation ----
        EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);
        data.animatorController = (RuntimeAnimatorController)EditorGUILayout.ObjectField("Animator Controller", data.animatorController, typeof(RuntimeAnimatorController), false);

        EditorGUILayout.Space(10);

        // ---- SECTION: Special Abilities ----
        EditorGUILayout.LabelField("Special Abilities", EditorStyles.boldLabel);
        showAbilities = EditorGUILayout.Foldout(showAbilities, $"Abilities ({data.abilities.Count})");
        if (showAbilities) {
            DrawAbilitiesList();
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed) {
            EditorUtility.SetDirty(data);
        }
    }

    private void DrawAbilitiesList() {
        for (int i = 0; i < data.abilities.Count; i++) {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.BeginHorizontal();
            data.abilities[i].abilityName = EditorGUILayout.TextField("Name", data.abilities[i].abilityName);
            if (GUILayout.Button("X", GUILayout.Width(25))) {
                data.abilities.RemoveAt(i);
                break;
            }
            EditorGUILayout.EndHorizontal();

            data.abilities[i].description = EditorGUILayout.TextField("Description", data.abilities[i].description);
            data.abilities[i].type = (AbilityType)EditorGUILayout.EnumPopup("Type", data.abilities[i].type);
            data.abilities[i].valuePerLevel = EditorGUILayout.FloatField("Value Per Level", data.abilities[i].valuePerLevel);
            data.abilities[i].maxLevel = EditorGUILayout.IntField("Max Level", data.abilities[i].maxLevel);
            data.abilities[i].startLevel = EditorGUILayout.IntField("Start Level", data.abilities[i].startLevel);

            EditorGUILayout.EndVertical();
        }

        if (GUILayout.Button("+ Add Ability")) {
            data.abilities.Add(new CharacterAbility());
        }
    }
}
#endif