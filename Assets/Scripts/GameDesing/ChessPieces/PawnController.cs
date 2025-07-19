using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class PawnController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition, IFicha, IFichaAliada
{
    [Header("Rango de Movimiento")]
    private int rangoMovimientoBase = 1; // Siempre será 1
    private int rangoMovimientoExtra = 0; // Bonus temporal por pociones
    public bool esInamovible = false;
    public int RangoMovimientoActual => rangoMovimientoBase + rangoMovimientoExtra;

    private Vector2Int posicionActual;
    private bool juegoActivo = false;
    public bool mostrandoMovimientos = false;

    private void Start()
    {
        PiecePositioner piecePositioner = GetComponent<PiecePositioner>();
        posicionActual = piecePositioner != null ? piecePositioner.tileCoords : new Vector2Int(0, 0);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♙ Peón inició en {posicionActual}.");
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        posicionActual = nuevaPos;

        var movible = GetComponent<MovableTileObject>();
        if (movible != null) movible.tileCoords = nuevaPos;

        var posicionador = GetComponent<PiecePositioner>();
        if (posicionador != null) posicionador.tileCoords = nuevaPos;

        BoardManagerGlobal.Instance?.RegistrarMovimiento(this, nuevaPos);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♟️ Peón movido a {nuevaPos}.");
    }

    public Vector2Int GetPosicionActual() => posicionActual;

    public void ActivarJuego()
    {
        juegoActivo = true;
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void MostrarMovimientoPosible()
    {
        if (!juegoActivo) return;

        var rey = FindFirstObjectByType<KingController>();
        if (rey == null || rey.puntosAccionActual <= 0)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ Rey sin PA, Peón no puede moverse.");
            OcultarMovimientos();
            mostrandoMovimientos = false;
            return;
        }

        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔍 Mostrando rango de movimiento del Peón: {RangoMovimientoActual} casillas.");

        foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
        {
            int distancia = Mathf.Abs(tile.tileCoords.x - posicionActual.x) + Mathf.Abs(tile.tileCoords.y - posicionActual.y);
            tile.HighlightMove(distancia <= RangoMovimientoActual);
        }

        Vector2Int[] diagonales = new Vector2Int[]
        {
            new Vector2Int(1,1), new Vector2Int(-1,1),
            new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };

        foreach (var delta in diagonales)
        {
            Vector2Int diagonal = posicionActual + delta;
            if (diagonal.x < 0 || diagonal.y < 0 || diagonal.x > 7 || diagonal.y > 7) continue;

            var objetivo = BoardManagerGlobal.Instance.ObtenerObjetosEn(diagonal)
                .FirstOrDefault(obj => obj is IFichaEnemiga);

            if (objetivo != null)
            {
                Tile tile = BoardManagerGlobal.Instance.GetTileAt(diagonal);
                if (tile != null) tile.HighlightEnemyAttack(true);
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔪 Peón puede atacar a {objetivo} en {diagonal}.");
            }
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
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🟢 Mostrando previsualización automática del Peón.");
        }
        else
        {
            mostrandoMovimientos = false;
            OcultarMovimientos();
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🔴 Ocultando previsualización del Peón.");
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void MoverA(Vector2Int nuevaPos, KingController rey)
    {
        if (!juegoActivo) return;

        int distancia = Mathf.Abs(posicionActual.x - nuevaPos.x) + Mathf.Abs(posicionActual.y - nuevaPos.y);
        if (distancia > RangoMovimientoActual)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚫 Movimiento inválido. Distancia {distancia} excede el rango actual {RangoMovimientoActual}.");
            return;
        }

        if (rey.puntosAccionActual <= 0)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🚫 Movimiento inválido. Rey sin PA.");
            return;
        }

        // 💥 Ataque en diagonal
        if (Mathf.Abs(nuevaPos.x - posicionActual.x) == 1 && Mathf.Abs(nuevaPos.y - posicionActual.y) == 1)
        {
            var fichaEnDiagonal = BoardManagerGlobal.Instance.ObtenerObjetosEn(nuevaPos)
                .FirstOrDefault(obj => obj is IFichaEnemiga);

            if (fichaEnDiagonal != null)
            {
                SetPosicionActual(nuevaPos);
                transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);
                if (fichaEnDiagonal is IPieceWithPosition enemigo)
                    enemigo.SetPosicionActual(BoardManagerGlobal.DimensionDivina);
                Destroy(((MonoBehaviour)fichaEnDiagonal).gameObject);

                rey.puntosAccionActual--;
                MostrarMovimientoPosible();
                FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
                RevisarAmenazasGlobal();
                return;
            }
        }

        // 🔁 Movimiento paso a paso + interacción con efectos
        Vector2Int paso = posicionActual;
        while (paso != nuevaPos)
        {
            if (paso.x < nuevaPos.x) paso.x++;
            else if (paso.x > nuevaPos.x) paso.x--;
            if (paso.y < nuevaPos.y) paso.y++;
            else if (paso.y > nuevaPos.y) paso.y--;

            SetPosicionActual(paso);
            foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
                efecto.RevisarSiPeonLlegó(paso, this);
        }

        // ✅ Aquí movemos visualmente
        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

        // ✅ Descontar RANGO EXTRA si fue usado
        if (distancia > rangoMovimientoBase)
        {
            int extraUsado = distancia - rangoMovimientoBase;
            rangoMovimientoExtra = Mathf.Max(0, rangoMovimientoExtra - extraUsado);
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧪 Rango temporal reducido en {extraUsado}. Rango restante: {RangoMovimientoActual}.");
        }

        // ✅ Descontar PA
        rey.puntosAccionActual--;

        MostrarMovimientoPosible();
        RevisarObjetosEnCasilla();
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
        RevisarAmenazasGlobal();

        if (posicionActual == new Vector2Int(7, 7))
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("♕ Peón coronado en H8. Bonificaciones aplicadas.");
            rey.puntosAccionActual += 7;
            rey.puntosMovimientoActual += 7;
            rey.GanarVida(3);
            FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
            Destroy(gameObject);
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }


    public void RevisarObjetosEnCasilla()
    {
        foreach (var objeto in BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual))
        {
            if (objeto is ITileEffect efecto)
            {
                efecto.RevisarSiPeonLlegó(posicionActual, this);
                MostrarMovimientoPosible();
            }
        }
    }

    private void RevisarAmenazasGlobal()
    {
        foreach (var ficha in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IFichaEnemiga>())
            ficha.RevisarAmenazasEnZona();
    }

    public void AumentarRangoMovimiento(int cantidad)
    {
        rangoMovimientoExtra += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🏃 Peón ganó +{cantidad} de rango temporal. Total: {RangoMovimientoActual}.");
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
    }


    public void DesactivarJuego()
    {
        juegoActivo = false;
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void MostrarRango()
    {
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
    }

    public void OcultarRango()
    {
        OcultarMovimientos();
        mostrandoMovimientos = false;
    }

    public bool EstaActivo() => juegoActivo;
    
    public bool EsInamovible()
    {
    return esInamovible;
    }
}
