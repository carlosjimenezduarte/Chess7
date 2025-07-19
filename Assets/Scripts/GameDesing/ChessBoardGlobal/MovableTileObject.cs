using UnityEngine;
using System.Linq;

public class MovableTileObject : MonoBehaviour
{
    public Vector2Int tileCoords;
    public bool activoEnTablero = false;
    public bool esInamovible = false;

    private bool EsDentroTablero(Vector2Int p)
        => p.x >= 0 && p.x <= 7 && p.y >= 0 && p.y <= 7;

    public void MoverA(Vector2Int nuevaPos)
    {
        if (esInamovible)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🪨 {gameObject.name} es inamovible y no se moverá.");
            return;
        }

        if (EstaCasillaOcupada(nuevaPos))
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"⛔ {gameObject.name} no se moverá a {nuevaPos} porque está ocupado.");
            return;
        }

        tileCoords = nuevaPos;
        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

        var piecePositioner = GetComponent<PiecePositioner>();
        if (piecePositioner != null)
        {
            piecePositioner.tileCoords = nuevaPos;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧭 PiecePositioner de {gameObject.name} actualizado a {nuevaPos}");
        }

        if (TryGetComponent<IPieceWithPosition>(out var piece))
        {
            piece.SetPosicionActual(nuevaPos);
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧭 IPieceWithPosition de {gameObject.name} actualizado a {nuevaPos}");
        }

        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧭 {gameObject.name} movido global a {nuevaPos}");

        // 👇 Intentar recolección automática si aterriza sobre ficha aliada
        IntentarRecolectarSiEsPosible(nuevaPos);
    }

    private bool EstaCasillaOcupada(Vector2Int coords)
    {
        var objetosEnTile = BoardManagerGlobal.Instance.ObtenerObjetosEn(coords);

        foreach (var obj in objetosEnTile)
        {
            if ((object)obj == this) continue;

            bool esRecoleccionable = obj is IObjetoRecoleccionable;
            bool esFicha = obj is IFicha;

            if (obj is MovableTileObject mov && !mov.activoEnTablero)
                continue;

            if ((esRecoleccionable || esFicha))
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚫 La casilla {coords} está ocupada por {((MonoBehaviour)obj).gameObject.name} (Recoleccionable:{esRecoleccionable}, Ficha:{esFicha})");
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
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"✅ {gameObject.name} activado en tablero en {tileCoords}");
    }

    public bool EsInamovible()
    {
        return esInamovible;
    }

    private void IntentarRecolectarSiEsPosible(Vector2Int destino)
    {
    var receptor = BoardManagerGlobal.Instance.ObtenerObjetosEn(destino)
        .FirstOrDefault(obj => obj is IFichaAliada);

    if (receptor is IFichaAliada fichaAliada && this is IObjetoRecoleccionable recolectable && this is ITileEffect efecto)
    {
        efecto.RevisarSiFichaLlegó(destino, fichaAliada);
    }
    }
    
}
