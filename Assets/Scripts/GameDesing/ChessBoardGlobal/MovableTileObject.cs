using UnityEngine;

/// <summary>
/// Script universal para objetos con TileCoords que pueden moverse.
/// Actualiza su propia lógica, el PiecePositioner si existe,
/// y también IPieceWithPosition si lo implementa.
/// </summary>
public class MovableTileObject : MonoBehaviour
{
    public Vector2Int tileCoords;

    public void MoverA(Vector2Int nuevaPos)
    {
        tileCoords = nuevaPos;
        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

        // 🔥 Si tiene PiecePositioner, también actualiza allí
        var piecePositioner = GetComponent<PiecePositioner>();
        if (piecePositioner != null)
        {
            piecePositioner.tileCoords = nuevaPos;
            Debug.Log($"🧭 PiecePositioner de {gameObject.name} actualizado a {nuevaPos}");
        }

        // 🔥 Si implementa IPieceWithPosition, también actualiza allí
        if (TryGetComponent<IPieceWithPosition>(out var piece))
        {
            piece.SetPosicionActual(nuevaPos);
            Debug.Log($"🧭 IPieceWithPosition de {gameObject.name} actualizado a {nuevaPos}");
        }

        Debug.Log($"🧭 {gameObject.name} movido global a {nuevaPos}");
    }
}
