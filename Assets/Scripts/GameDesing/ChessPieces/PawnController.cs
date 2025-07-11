using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class PawnController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition
{
    public int puntosMovimientoActual = 1;  // PM: máximo alcance en una jugada (aumenta con pociones)

    private Vector2Int posicionActual;
    private bool juegoActivo = false;
    private bool mostrandoMovimientos = false;

    private void Start()
    {
        PiecePositioner piecePositioner = GetComponent<PiecePositioner>();
        if (piecePositioner != null)
        {
            posicionActual = piecePositioner.tileCoords;
            Debug.Log($"♙ Peón inició en {posicionActual}");
            
        }
        else
        {
            Debug.LogWarning("⚠️ No hay PiecePositioner en el Peón. Usando (0,0).");
            posicionActual = new Vector2Int(0, 0);
        }
    }

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        posicionActual = nuevaPos;
    }

    public Vector2Int GetPosicionActual()
    {
        return posicionActual;
    }

    public void ActivarJuego()
    {
        juegoActivo = true;
    }

    public void MostrarMovimientoPosible()
{
    if (!juegoActivo) return;

    Debug.Log($"♙ DEBUG: PM actual = {puntosMovimientoActual}");

    foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
    {
        int dx = Mathf.Abs(tile.tileCoords.x - posicionActual.x);
        int dy = Mathf.Abs(tile.tileCoords.y - posicionActual.y);
        bool esDireccionCardinal = (dx == 0 && dy > 0) || (dy == 0 && dx > 0);
        int distancia = dx + dy;

        bool puedeAlcanzar = esDireccionCardinal && distancia <= puntosMovimientoActual;

        Debug.Log($"♙ Tile {tile.tileCoords}, distancia {distancia}, puedeAlcanzar={puedeAlcanzar}");

        tile.HighlightMove(puedeAlcanzar);
    }
}

    private void OcultarMovimientos()
    {
        foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
            tile.HighlightMove(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!juegoActivo) return;

        mostrandoMovimientos = !mostrandoMovimientos;

        if (mostrandoMovimientos)
        {
            Debug.Log("🟢 Mostrando previsualización de movimientos del Peón.");
            MostrarMovimientoPosible();
        }
        else
        {
            Debug.Log("🔴 Ocultando previsualización de movimientos del Peón.");
            OcultarMovimientos();
        }
    }

    public void MoverA(Vector2Int nuevaPos)
    {
        if (!juegoActivo) return;

        int dx = Mathf.Abs(posicionActual.x - nuevaPos.x);
        int dy = Mathf.Abs(posicionActual.y - nuevaPos.y);
        int distancia = dx + dy;
        Debug.Log($"♙ Intentando mover desde {posicionActual} a {nuevaPos} -> distancia {distancia}, PM actual = {puntosMovimientoActual}");
        bool esDireccionCardinal = (dx == 0 && dy > 0) || (dy == 0 && dx > 0);

        if (!esDireccionCardinal)
        {
            Debug.Log("🚫 Movimiento inválido, no es dirección cardinal.");
            return;
        }

        if (distancia > puntosMovimientoActual)
        {
            Debug.Log("🚫 Movimiento inválido, no tiene suficientes PM.");
            return;
        }

        var rey = FindFirstObjectByType<KingController>();
        if (rey == null)
        {
            Debug.LogError("🚨 No se encontró al Rey.");
            return;
        }

        if (rey.puntosAccionActual < 1)
        {
            Debug.Log("🚫 El Rey no tiene PA suficientes para mover el Peón.");
            return;
        }

        Debug.Log($"♙ Moviendo Peón desde {posicionActual} a {nuevaPos}, usando 1 PA del Rey y consumiendo {distancia} PM.");

        Vector2Int paso = posicionActual;

        while (paso != nuevaPos)
        {
            if (paso.x < nuevaPos.x) paso.x++;
            else if (paso.x > nuevaPos.x) paso.x--;

            if (paso.y < nuevaPos.y) paso.y++;
            else if (paso.y > nuevaPos.y) paso.y--;

            SetPosicionActual(paso);

            Debug.Log($"🚶 El Peón pasa por {paso}");

            foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
            {
                efecto.RevisarSiPeonLlegó(paso, this);
            }
        }

        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);
        puntosMovimientoActual -= distancia;

        rey.ConsumirPA(1);

        MostrarMovimientoPosible();
        mostrandoMovimientos = true;

        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
    }

    public void GanarPuntoMovimiento(int cantidad)
    {
        puntosMovimientoActual += cantidad;
        Debug.Log($"♙ Peón gana +{cantidad} PM. Total: {puntosMovimientoActual}.");
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
    }
}
