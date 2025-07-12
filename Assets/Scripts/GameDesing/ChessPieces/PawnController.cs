using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class PawnController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition
{
    [Header("Movimiento del Peón")]
    public int puntosMovimientoMax = 1;
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
            bool puedeAlcanzar = distancia <= rango;
            tile.HighlightMove(puedeAlcanzar);
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
        Debug.Log($"♙ {gameObject.name} juegoActivo={juegoActivo}");
        if (!juegoActivo) return;
        Debug.Log($"♙ {gameObject.name} juegoActivo={juegoActivo}");

        int distancia = Mathf.Abs(posicionActual.x - nuevaPos.x) + Mathf.Abs(posicionActual.y - nuevaPos.y);
        int rango = Mathf.Max(1, puntosMovimientoActual);

        if (distancia <= rango && rey.puntosAccionActual > 0)
        {
            Debug.Log($"Moviendo al Peón desde {posicionActual} a {nuevaPos}, recorriendo {distancia} casillas. Consumirá 1 PA del Rey.");
            Debug.Log($"♙ {gameObject.name} juegoActivo={juegoActivo}");
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
                    efecto.RevisarSiPeonLlegó(paso, this);
            }

            transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);
            puntosMovimientoActual -= distancia; // reduce PM temporal por si es una poción
            rey.puntosAccionActual -= 1;         // siempre consume 1 PA

            MostrarMovimientoPosible();
            mostrandoMovimientos = true;

            // 🚀 NUEVO: que la Reina actualice amenaza hacia el Peón
            var reina = FindFirstObjectByType<QueenEnemyController>();
            if (reina != null)
                reina.VerificarAmenazaSobre(posicionActual);

            FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

            if (posicionActual == new Vector2Int(7, 7))
            {
                Debug.Log("♕ El Peón ha coronado en H8. Otorgando bonus al Rey.");
                rey.puntosAccionActual += 7;
                rey.puntosMovimientoActual += 7;
                rey.GanarVida(3);
                FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
                Destroy(gameObject);
            }
        }
        else
        {
            if (distancia > rango)
                Debug.Log("🚫 Movimiento no permitido, no hay suficiente rango PM del Peón.");
            else if (rey.puntosAccionActual <= 0)
                Debug.Log("🚫 Movimiento no permitido, no hay suficientes PA del Rey.");
        }
    }


    public void ReiniciarTurno()
    {
        puntosMovimientoActual = puntosMovimientoMax; // vuelve a su rango natural
        Debug.Log($"♙ Nuevo turno del Peón: rango {puntosMovimientoActual} PM.");
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
    }

    public void GanarPuntoMovimiento(int cantidad)
    {
        puntosMovimientoActual += cantidad;
        Debug.Log($"El Peón gana +{cantidad} PM. Total ahora: {puntosMovimientoActual}.");
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
    Debug.Log($"🤫 El Peón gana +{cantidad} PM silenciosamente. Total: {puntosMovimientoActual}.");
    FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
    }

}
