// ObjetoRecoleccionableBaseEditor.cs
using UnityEditor;
using UnityEngine;

public abstract class ObjetoRecoleccionableBaseEditor<T> : Editor where T : MonoBehaviour
{
    public override void OnInspectorGUI()
    {
        T objeto = (T)target;
        var piecePositioner = objeto.GetComponent<PiecePositioner>();
        var movable = objeto.GetComponent<MovableTileObject>();

        var vieneDelFuturoProp = serializedObject.FindProperty("vieneDelFuturo");
        var turnoApareceProp = serializedObject.FindProperty("turnoAparece");
        var tileCoordsFuturosInciertosProp = serializedObject.FindProperty("tileCoordsFuturosInciertos");
        var posicionRealProp = serializedObject.FindProperty("posicionReal");

        EditorGUILayout.LabelField("⭐ Configuración de Objeto Recolectable", EditorStyles.boldLabel);

        EditorGUILayout.PropertyField(vieneDelFuturoProp, new GUIContent("¿Viene del Futuro?"));
        serializedObject.ApplyModifiedProperties();

        bool vieneDelFuturo = vieneDelFuturoProp.boolValue;

        if (vieneDelFuturo)
        {
            EditorGUILayout.PropertyField(turnoApareceProp, new GUIContent("Turno Aparece"));
            EditorGUILayout.PropertyField(tileCoordsFuturosInciertosProp, new GUIContent("Posición Futuros Inciertos"));
            EditorGUILayout.PropertyField(posicionRealProp, new GUIContent("Posición Real al aparecer"));

            if (movable != null)
            {
                movable.activoEnTablero = false;
                EditorUtility.SetDirty(movable);
            }
        }
        else
        {
            if (piecePositioner != null)
            {
                piecePositioner.tileCoords = EditorGUILayout.Vector2IntField("Posición en Tablero", piecePositioner.tileCoords);
                EditorUtility.SetDirty(piecePositioner);
            }

            if (movable != null)
            {
                movable.activoEnTablero = true;
                EditorUtility.SetDirty(movable);
            }
        }

        if (GUI.changed)
            EditorUtility.SetDirty(objeto);
    }
}
