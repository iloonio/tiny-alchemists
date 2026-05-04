using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(NetworkedPotion))]
public class NetworkedPotionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        NetworkedPotion potion = (NetworkedPotion)target;
        if (potion == null)
            return;

        GUILayout.Space(8);
        EditorGUILayout.LabelField("Potion Recipe", EditorStyles.boldLabel);

        string recipeText = potion._recipe.Value.ToString();
        string modifierText = string.Join(", ", potion._recipe.Value.GetModifiers());
        if (string.IsNullOrEmpty(modifierText))
            modifierText = "No Mods";

        EditorGUILayout.LabelField("Recipe", recipeText);
        EditorGUILayout.LabelField("Base", potion._recipe.Value.HasBase ? potion._recipe.Value.Base.ToString() : "NoBase");
        EditorGUILayout.LabelField("Modifiers", modifierText);
    }

    private void OnSceneGUI()
    {
        NetworkedPotion potion = (NetworkedPotion)target;
        if (potion == null)
            return;

        string recipeLabel = potion._recipe.Value.ToString();
        if (string.IsNullOrEmpty(recipeLabel))
            return;

        Handles.color = Color.cyan;
        Handles.Label(potion.transform.position + Vector3.up * 0.5f, recipeLabel);
    }
}
