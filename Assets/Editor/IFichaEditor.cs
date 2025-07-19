using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class FichaEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MonoBehaviour mb = (MonoBehaviour)target;

        if (!(mb is IFicha))
            return;

        MovableTileObject mto = mb.GetComponent<MovableTileObject>();
        if (mto == null)
        {
            EditorGUILayout.HelpBox("⚠️ Esta ficha no tiene el componente MovableTileObject.", MessageType.Warning);
            return;
        }

        // Fuerza esInamovible = false
        if (mto.esInamovible)
        {
            mto.esInamovible = false;
            EditorUtility.SetDirty(mto);
            Debug.Log($"🔓 {mb.name}: esInamovible fue forzado a FALSE desde el Editor (Ficha debe ser movible).");
        }

        EditorGUILayout.HelpBox("Esta ficha es movible por diseño. esInamovible ha sido desactivado automáticamente.", MessageType.Info);
    }
}
