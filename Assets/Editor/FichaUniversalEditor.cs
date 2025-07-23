using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class FichaUniversalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MonoBehaviour mb = (MonoBehaviour)target;

        // Verificamos si implementa alguna de las interfaces
        bool esFicha = mb is IFicha;
        bool esInmovil = mb is IFichaInmovil;

        if (!esFicha)
            return;

        MovableTileObject mto = mb.GetComponent<MovableTileObject>();
        if (mto == null)
        {
            EditorGUILayout.HelpBox("⚠️ Esta ficha no tiene el componente MovableTileObject.", MessageType.Warning);
            return;
        }

        if (esInmovil)
        {
            if (!mto.esInamovible)
            {
                mto.esInamovible = true;
                EditorUtility.SetDirty(mto);
                Debug.Log($"🔒 {mb.name}: esInamovible fue forzado a TRUE (IFichaInmovil).");
            }
            EditorGUILayout.HelpBox("✅ Este objeto es inamovible por diseño (IFichaInmovil).", MessageType.Info);
        }
        else
        {
            if (mto.esInamovible)
            {
                mto.esInamovible = false;
                EditorUtility.SetDirty(mto);
                Debug.Log($"🔓 {mb.name}: esInamovible fue forzado a FALSE (Ficha movible).");
            }
            EditorGUILayout.HelpBox("ℹ️ Este objeto es una ficha movible (IFicha).", MessageType.Info);
        }
    }
}
