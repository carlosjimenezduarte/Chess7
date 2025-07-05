using UnityEngine;
using System.Linq;

public class KingController : MonoBehaviour
{
    public int puntosMovimientoMax = 3; // siempre 3 al reiniciar
    public int puntosMovimientoActual;
    public int puntosAccionActual = 5;
    public int turnosRestantes = 7;  // ejemplo base

    private Vector2Int posicionActual;

    private void Start()
    {
        puntosMovimientoActual = puntosMovimientoMax;
        posicionActual = new Vector2Int(0, 0);
        
    }

    public void MostrarMovimientoPosible()
    {
        Debug.Log("Mostrando casillas alcanzables con " + puntosMovimientoActual + " PM.");

        foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
        {
            int distancia = Mathf.Abs(tile.tileCoords.x - posicionActual.x) + Mathf.Abs(tile.tileCoords.y - posicionActual.y);
            bool puedeAlcanzar = distancia <= puntosMovimientoActual;
            tile.HighlightMove(puedeAlcanzar);
        }
    }

    public void MoverA(Vector2Int nuevaPos)
{
    int distancia = Mathf.Abs(posicionActual.x - nuevaPos.x) + Mathf.Abs(posicionActual.y - nuevaPos.y);

    if (distancia <= puntosMovimientoActual)
    {
        Debug.Log($"Moviendo al Rey de {posicionActual} a {nuevaPos}, consumiendo {distancia} PM.");
        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

        puntosMovimientoActual -= distancia;
        posicionActual = nuevaPos;

        MostrarMovimientoPosible();

        foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
        {
            efecto.RevisarSiReyLlegó(posicionActual, this);
        }

        FindFirstObjectByType<ChessGameManager>().ActualizarHUD(); // ✅ sin warning
    }
    else
    {
        Debug.Log("Movimiento no permitido, no hay suficientes PM.");
    }
}
    public void ReiniciarTurno()
    {
        puntosMovimientoActual = puntosMovimientoMax;
        Debug.Log($"Nuevo turno. El Rey tiene {puntosMovimientoActual} PM.");
        MostrarMovimientoPosible();
    }

    public void GanarPuntoMovimiento(int cantidad)
{
    puntosMovimientoActual += cantidad;
    Debug.Log($"El Rey ganó +{cantidad} PM y ahora tiene {puntosMovimientoActual} PM.");
    MostrarMovimientoPosible();

    FindFirstObjectByType<ChessGameManager>().ActualizarHUD();
}

    public void TestMoverRey()
    {
        Debug.Log("Botón test presionado. Moviendo Rey a (2,2).");
        MoverA(new Vector2Int(2, 2));
    }
    public void RestarTurno()
{
    turnosRestantes--;
    Debug.Log("Turnos restantes: " + turnosRestantes);
}
}
