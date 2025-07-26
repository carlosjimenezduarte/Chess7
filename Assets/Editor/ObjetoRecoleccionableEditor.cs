using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class ObjetoRecoleccionableUniversalEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MonoBehaviour mb = (MonoBehaviour)target;

        // Solo aplica a objetos que implementen IObjetoRecoleccionable
        if (mb is not IObjetoRecoleccionable objeto)
            return;

        var piecePositioner = mb.GetComponent<PiecePositioner>();
        var movable = mb.GetComponent<MovableTileObject>();

        // Casteamos el objeto a su tipo concreto
        var tipo = mb.GetType();
        var vieneDelFuturoField = tipo.GetField("vieneDelFuturo");
        var turnoApareceField = tipo.GetField("turnoAparece");
        var tileCoordsFuturosField = tipo.GetField("tileCoordsFuturosInciertos");
        var posicionRealField = tipo.GetField("posicionReal");

        if (vieneDelFuturoField == null || turnoApareceField == null || tileCoordsFuturosField == null || posicionRealField == null)
        {
            EditorGUILayout.HelpBox("⚠️ Este objeto no tiene los campos requeridos. Asegúrate de declarar vieneDelFuturo, turnoAparece, tileCoordsFuturosInciertos y posicionReal como públicos o [SerializeField].", MessageType.Warning);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("⭐ Configuración de Objeto Recolectable", EditorStyles.boldLabel);

        // Mostrar y modificar 'vieneDelFuturo'
        bool vieneDelFuturo = (bool)vieneDelFuturoField.GetValue(mb);
        vieneDelFuturo = EditorGUILayout.Toggle("¿Viene del Futuro?", vieneDelFuturo);
        vieneDelFuturoField.SetValue(mb, vieneDelFuturo);

        if (vieneDelFuturo)
        {
            int turno = (int)turnoApareceField.GetValue(mb);
            turno = EditorGUILayout.IntField("Turno Aparece", turno);
            turnoApareceField.SetValue(mb, turno);

            Vector2Int coordsFuturo = (Vector2Int)tileCoordsFuturosField.GetValue(mb);
            coordsFuturo = EditorGUILayout.Vector2IntField("Posición Futuros Inciertos", coordsFuturo);
            tileCoordsFuturosField.SetValue(mb, coordsFuturo);

            Vector2Int posReal = (Vector2Int)posicionRealField.GetValue(mb);
            posReal = EditorGUILayout.Vector2IntField("Posición Real al aparecer", posReal);
            posicionRealField.SetValue(mb, posReal);

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
        {
            EditorUtility.SetDirty(mb);
        }
    }
}
