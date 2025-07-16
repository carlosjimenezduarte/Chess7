using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Potion1PM))]
public class ObjetoRecoleccionableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Potion1PM objeto = (Potion1PM)target;
        var piecePositioner = objeto.GetComponent<PiecePositioner>();
        var movable = objeto.GetComponent<MovableTileObject>();

        EditorGUILayout.LabelField("⭐ Configuración de Objeto Recolectable", EditorStyles.boldLabel);

        objeto.vieneDelFuturo = EditorGUILayout.Toggle("¿Viene del Futuro?", objeto.vieneDelFuturo);

        if (objeto.vieneDelFuturo)
        {
            objeto.turnoAparece = EditorGUILayout.IntField("Turno Aparece", objeto.turnoAparece);
            objeto.tileCoordsFuturosInciertos = EditorGUILayout.Vector2IntField("Posición Futuros Inciertos", objeto.tileCoordsFuturosInciertos);
            objeto.posicionReal = EditorGUILayout.Vector2IntField("Posición Real al aparecer", objeto.posicionReal);

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
