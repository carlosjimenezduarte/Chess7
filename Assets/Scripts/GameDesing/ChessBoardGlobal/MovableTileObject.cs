using UnityEngine;

/// <summary>
/// Script universal para objetos con TileCoords que pueden moverse.
/// Actualiza su propia lógica, el PiecePositioner si existe,
/// y también IPieceWithPosition si lo implementa.
/// Ahora verifica si la casilla destino está ocupada antes de moverse.
/// </summary>
public class MovableTileObject : MonoBehaviour
{
    public Vector2Int tileCoords;

    public void MoverA(Vector2Int nuevaPos)
    {
        if (EstaCasillaOcupada(nuevaPos))
        {
            Debug.Log($"⛔ {gameObject.name} no se moverá a {nuevaPos} porque está ocupado.");
            return;
        }

        tileCoords = nuevaPos;
        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

        var piecePositioner = GetComponent<PiecePositioner>();
        if (piecePositioner != null)
        {
            piecePositioner.tileCoords = nuevaPos;
            Debug.Log($"🧭 PiecePositioner de {gameObject.name} actualizado a {nuevaPos}");
        }

        if (TryGetComponent<IPieceWithPosition>(out var piece))
        {
            piece.SetPosicionActual(nuevaPos);
            Debug.Log($"🧭 IPieceWithPosition de {gameObject.name} actualizado a {nuevaPos}");
        }

        Debug.Log($"🧭 {gameObject.name} movido global a {nuevaPos}");
    }

    private bool EstaCasillaOcupada(Vector2Int coords)
    {
    var otros = FindObjectsByType<MovableTileObject>(FindObjectsSortMode.None);
    foreach (var obj in otros)
    {
        if (obj == this) continue;

        // Si este otro objeto es un objeto recoleccionable
        if (obj.TryGetComponent<IObjetoRecoleccionable>(out var recolectable))
        {
            if (obj.tileCoords == coords)
            {
                Debug.Log($"🚫 La casilla {coords} está ocupada por otro objeto recoleccionable: {obj.gameObject.name}");
                return true;
            }
        }
    }

    // ✅ No encontró conflicto con otro objeto recoleccionable
    return false;
    }

}
