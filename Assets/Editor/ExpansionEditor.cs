using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Expansion))]
public class ExpansionEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector(); // Muestra los campos normales del inspector

        Expansion expansion = (Expansion)target;

        MovableTileObject mto = expansion.GetComponent<MovableTileObject>();
        if (mto != null)
        {
            if (!mto.esInamovible)
            {
                mto.esInamovible = true;
                EditorUtility.SetDirty(mto);
                Debug.Log($"🔒 {expansion.name}: esInamovible fue forzado a TRUE desde el Editor.");
            }

            EditorGUILayout.HelpBox("Este objeto es inamovible por diseño (Expansion).", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox("⚠️ Este objeto no tiene el componente MovableTileObject. Añádelo para que funcione correctamente.", MessageType.Warning);
        }
    }
}
