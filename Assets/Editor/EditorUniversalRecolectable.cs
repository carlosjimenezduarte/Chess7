using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
public class EditorUniversalRecolectable : Editor
{
    public override void OnInspectorGUI()
    {
        MonoBehaviour mono = (MonoBehaviour)target;

        // 🛡️ Solo aplicar si implementa IObjetoRecoleccionable pero NO IObjetoRecoleccionableEspecial
        if (mono is not IObjetoRecoleccionable || mono is IObjetoRecoleccionableEspecial)
        {
            DrawDefaultInspector();
            return;
        }

        // Acceso general
        var objeto = mono;
        var piecePositioner = objeto.GetComponent<PiecePositioner>();
        var movable = objeto.GetComponent<MovableTileObject>();
        var conPosicion = objeto as IPieceWithPosition;

        // 🧪 Reflexión para acceder a los campos declarados por el recolectable
        var tipo = objeto.GetType();
        var vieneDelFuturoField = tipo.GetField("vieneDelFuturo");
        var turnoApareceField = tipo.GetField("turnoAparece");
        var tileCoordsFuturosField = tipo.GetField("tileCoordsFuturosInciertos");
        var posicionRealField = tipo.GetField("posicionReal");

        if (vieneDelFuturoField == null)
        {
            EditorGUILayout.HelpBox("⚠️ El objeto no contiene campo 'vieneDelFuturo'.", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("⭐ Configuración de Objeto Recolectable", EditorStyles.boldLabel);

        bool vieneDelFuturo = (bool)vieneDelFuturoField.GetValue(objeto);
        vieneDelFuturo = EditorGUILayout.Toggle("¿Viene del Futuro?", vieneDelFuturo);
        vieneDelFuturoField.SetValue(objeto, vieneDelFuturo);

        if (vieneDelFuturo)
        {
            if (turnoApareceField != null && tileCoordsFuturosField != null && posicionRealField != null)
            {
                int turno = (int)turnoApareceField.GetValue(objeto);
                turno = EditorGUILayout.IntField("Turno Aparece", turno);
                turnoApareceField.SetValue(objeto, turno);

                Vector2Int coordsFuturo = (Vector2Int)tileCoordsFuturosField.GetValue(objeto);
                coordsFuturo = EditorGUILayout.Vector2IntField("Posición Futuros Inciertos", coordsFuturo);
                tileCoordsFuturosField.SetValue(objeto, coordsFuturo);

                Vector2Int posReal = (Vector2Int)posicionRealField.GetValue(objeto);
                posReal = EditorGUILayout.Vector2IntField("Posición Real al aparecer", posReal);
                posicionRealField.SetValue(objeto, posReal);
            }

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
                Vector2Int nuevaPos = EditorGUILayout.Vector2IntField("Posición en Tablero", piecePositioner.tileCoords);
                if (nuevaPos != piecePositioner.tileCoords)
                {
                    piecePositioner.tileCoords = nuevaPos;
                    EditorUtility.SetDirty(piecePositioner);

                    // Sincroniza movable.tileCoords (si existe)
                    if (movable != null)
                    {
                        movable.tileCoords = nuevaPos;
                        EditorUtility.SetDirty(movable);
                    }

                    if (conPosicion != null)
                    {
                        conPosicion.SetPosicionActual(nuevaPos);
                    }

                    Repaint();
                }
            }

            if (movable != null)
            {
                movable.activoEnTablero = true;
                EditorUtility.SetDirty(movable);
            }
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(objeto);
        }
    }
}
