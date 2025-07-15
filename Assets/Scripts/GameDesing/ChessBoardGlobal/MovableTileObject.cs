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
    public bool activoEnTablero = false;

    private bool EsDentroTablero(Vector2Int p)
    => p.x >= 0 && p.x <= 7 && p.y >= 0 && p.y <= 7;

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

        // 🚀 SOLO chequea si está activo en el tablero
        if (!obj.activoEnTablero) continue;

        bool esRecoleccionable = obj.TryGetComponent<IObjetoRecoleccionable>(out var recolectable);
        bool esFicha = obj.TryGetComponent<IFicha>(out var ficha);

        if ((esRecoleccionable || esFicha) && obj.tileCoords == coords)
        {
            Debug.Log($"🚫 La casilla {coords} está ocupada por {obj.gameObject.name}");
            return true;
        }
    }
    return false;
    }

    public void ActivarEnTablero(Vector2Int nuevaPos)
    {
        tileCoords = nuevaPos;
        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);
        activoEnTablero = true;
        Debug.Log($"✅ {gameObject.name} activado en tablero en {tileCoords}");
    }


}
