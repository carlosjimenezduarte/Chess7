using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class IFichaInmovilEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI(); // Dibuja el inspector predeterminado

        MonoBehaviour mb = (MonoBehaviour)target;

        // Solo aplica si el objeto implementa IFichaInmovil
        if (mb is IFichaInmovil)
        {
            MovableTileObject mto = mb.GetComponent<MovableTileObject>();
            if (mto != null)
            {
                if (!mto.esInamovible)
                {
                    mto.esInamovible = true;
                    EditorUtility.SetDirty(mto);
                    Debug.Log($"🔒 {mb.name}: esInamovible fue forzado a TRUE por IFichaInmovilEditor.");
                }

                EditorGUILayout.HelpBox("Este objeto está marcado como inamovible (IFichaInmovil).", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠️ Este objeto no tiene el componente MovableTileObject. Añádelo para funcionar correctamente.", MessageType.Warning);
            }
        }
    }
}
