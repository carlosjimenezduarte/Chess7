using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class PawnController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition, IFicha
{
    [Header("Movimiento del Peón")]
    private int puntosMovimientoExtra = 0;
    private int puntosMovimientoBase = 1;
    private int puntosMovimientoActual;

    private Vector2Int posicionActual;
    private bool juegoActivo = false;
    public bool mostrandoMovimientos = false;

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
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
    }

    public void MostrarMovimientoPosible()
    {
    if (!juegoActivo) return;

    var rey = FindFirstObjectByType<KingController>();
    if (rey != null && rey.puntosAccionActual <= 0)
    {
        Debug.Log("⚠️ Rey sin PA, Peón no puede mostrar rango.");
        OcultarMovimientos();
        mostrandoMovimientos = false;
        return;
    }

    int rango = Mathf.Max(1, puntosMovimientoActual);
    Debug.Log($"Mostrando casillas alcanzables con rango {rango} PM del Peón.");

    foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
    {
        int distancia = Mathf.Abs(tile.tileCoords.x - posicionActual.x) + Mathf.Abs(tile.tileCoords.y - posicionActual.y);
        tile.HighlightMove(distancia <= rango);
    }
    }

    public void OcultarMovimientos()
    {
        foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
            tile.HighlightMove(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var gameManager = FindFirstObjectByType<ChessGameManager>();
        bool esNuevaSeleccion = gameManager.fichaSeleccionadaActual != this;
        gameManager.fichaSeleccionadaActual = this;

        var rey = FindFirstObjectByType<KingController>();
        if (rey != null && rey != this)
            rey.OcultarMovimientos();

        if (esNuevaSeleccion || !mostrandoMovimientos)
        {
            mostrandoMovimientos = true;
            MostrarMovimientoPosible();
            Debug.Log("🟢 Mostrando previsualización automática del Peón.");
        }
        else
        {
            mostrandoMovimientos = false;
            OcultarMovimientos();
            Debug.Log("🔴 Ocultando previsualización del Peón.");
        }
    }

    public void MoverA(Vector2Int nuevaPos, KingController rey)
{
    Debug.Log($"♙ {gameObject.name} intenta moverse. juegoActivo={juegoActivo}");
    if (!juegoActivo) return;

    int distancia = Mathf.Abs(posicionActual.x - nuevaPos.x) + Mathf.Abs(posicionActual.y - nuevaPos.y);
    int rango = Mathf.Max(1, puntosMovimientoActual);

    if (distancia > rango)
    {
        Debug.Log($"🚫 Movimiento no permitido: distancia {distancia} excede el rango {rango} PM del Peón.");
        return;
    }

    if (rey.puntosAccionActual <= 0)
    {
        Debug.Log($"🚫 Movimiento no permitido: el Rey no tiene PA.");
        return;
    }

    Debug.Log($"✅ Moviendo Peón desde {posicionActual} a {nuevaPos}, recorriendo {distancia} casillas. Consumirá 1 PA del Rey.");

    Vector2Int paso = posicionActual;

    while (paso != nuevaPos)
    {
        // Calcula el siguiente paso en línea recta (priorizando eje X primero)
        if (paso.x < nuevaPos.x) paso.x++;
        else if (paso.x > nuevaPos.x) paso.x--;

        if (paso.y < nuevaPos.y) paso.y++;
        else if (paso.y > nuevaPos.y) paso.y--;

        SetPosicionActual(paso);
        Debug.Log($"🚶 El Peón pasa por {paso}");

        foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
            efecto.RevisarSiPeonLlegó(paso, this);
    }

    // Mueve visual en Unity
    transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

    // Gasta recursos
    puntosMovimientoActual -= distancia;
    rey.puntosAccionActual -= 1;

    // Actualiza rango de movimiento y amenaza de Reina
    MostrarMovimientoPosible();
    mostrandoMovimientos = true;

    var reina = FindFirstObjectByType<QueenEnemyController>();
    if (reina != null)
        reina.VerificarAmenazaSobre(posicionActual);

    // 🚀 Revisa objetos recoleccionables en la nueva casilla
    RevisarObjetosEnCasilla();

    // Actualiza el HUD una sola vez al final
    FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

    // 🚩 Comprueba si llegó a coronar
    if (posicionActual == new Vector2Int(7, 7))
    {
        Debug.Log("♕ El Peón ha coronado en H8. Otorga bonus al Rey.");
        rey.puntosAccionActual += 7;
        rey.puntosMovimientoActual += 7;
        rey.GanarVida(3);
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
        Destroy(gameObject);
    }
}


    public void ReiniciarTurno()
    {
    puntosMovimientoActual = puntosMovimientoBase;
    Debug.Log($"♙ Nuevo turno del Peón: rango natural {puntosMovimientoActual} PM.");
    MostrarMovimientoPosible();
    mostrandoMovimientos = true;
    }

    public void GanarPuntoMovimiento(int cantidad)
{
    puntosMovimientoExtra += cantidad;
    puntosMovimientoActual += cantidad;
    Debug.Log($"El Peón gana +{cantidad} PM temporales. Ahora tiene {puntosMovimientoActual} para gastar.");
    MostrarMovimientoPosible();
    mostrandoMovimientos = true;
    FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
}

    public void DesactivarJuego()
    {
        juegoActivo = false;
    }

public void GanarPuntoMovimientoSilencioso(int cantidad)
{
    puntosMovimientoActual += cantidad;
    Debug.Log($"🤫 Peón gana +{cantidad} PM SOLO PARA ESTE TURNO. Ahora tiene {puntosMovimientoActual}.");
    FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
}
    
    private void RevisarObjetosEnCasilla()
{
    var objetos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
        .Where(obj => obj is IObjetoRecoleccionable);

    foreach (var obj in objetos)
    {
        // Primero localiza su posición
        Vector2Int pos = Vector2Int.zero;
        if (obj.TryGetComponent<PiecePositioner>(out var posr))
            pos = posr.tileCoords;
        else if (obj is IPieceWithPosition pieza)
            pos = pieza.GetPosicionActual();

        // Si está en la misma casilla
        if (pos == posicionActual)
        {
            Debug.Log($"♙ Peón en {posicionActual} absorbe objeto {obj.name}.");

                // Intenta "activar" su efecto de forma genérica
                if (obj.TryGetComponent<ITileEffect>(out var efecto))
                {
                    efecto.RevisarSiPeonLlegó(posicionActual, this);
                    MostrarMovimientoPosible(); 
            }
        }
    }
}


}
