using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class PawnController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition, IFicha, IFichaAliada
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
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♙ Peón inició en {posicionActual}.");
        }
        else
        {
            posicionActual = new Vector2Int(0, 0);
            BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ No hay PiecePositioner en el Peón. Usando (0,0).");
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        posicionActual = nuevaPos;
        BoardManagerGlobal.Instance?.RegistrarMovimiento(this, nuevaPos);

        var movible = GetComponent<MovableTileObject>();
        if (movible != null)
            movible.tileCoords = nuevaPos;

        var posicionador = GetComponent<PiecePositioner>();
        if (posicionador != null)
            posicionador.tileCoords = nuevaPos;

        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♟️ Peón movido a {nuevaPos}.");
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
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void MostrarMovimientoPosible()
    {
        if (!juegoActivo) return;

        var rey = FindFirstObjectByType<KingController>();
        if (rey != null && rey.puntosAccionActual <= 0)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ Rey sin PA, Peón no puede mostrar rango.");
            OcultarMovimientos();
            mostrandoMovimientos = false;
            return;
        }

        int rango = Mathf.Max(1, puntosMovimientoActual);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔍 Mostrando casillas alcanzables con rango {rango} PM del Peón.");

        foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
        {
            int distancia = Mathf.Abs(tile.tileCoords.x - posicionActual.x) + Mathf.Abs(tile.tileCoords.y - posicionActual.y);
            tile.HighlightMove(distancia <= rango);
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
                if (tile != null)
                {
                    tile.HighlightEnemyAttack(true);
                    BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔪 Peón puede atacar en diagonal a {objetivo} en {diagonal}");
                }
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
        int rango = Mathf.Max(1, puntosMovimientoActual);

        if (Mathf.Abs(nuevaPos.x - posicionActual.x) == 1 && Mathf.Abs(nuevaPos.y - posicionActual.y) == 1)
        {
            var fichaEnDiagonal = BoardManagerGlobal.Instance.ObtenerObjetosEn(nuevaPos)
                .FirstOrDefault(obj => obj is IFichaEnemiga);

            if (fichaEnDiagonal != null)
            {
                string nombre = ((MonoBehaviour)fichaEnDiagonal).name;
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"💥 Peón elimina a {nombre} en {nuevaPos}.");

                SetPosicionActual(nuevaPos);
                transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

                if (fichaEnDiagonal is IPieceWithPosition enemigo)
                    enemigo.SetPosicionActual(BoardManagerGlobal.DimensionDivina);

                Destroy(((MonoBehaviour)fichaEnDiagonal).gameObject);

                rey.puntosAccionActual -= 1;

                MostrarMovimientoPosible();
                FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

                RevisarAmenazasGlobal();
                BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
                return;
            }
        }

        if (distancia > rango)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚫 Movimiento no permitido: distancia {distancia} excede el rango {rango} PM del Peón.");
            return;
        }

        if (rey.puntosAccionActual <= 0)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🚫 Movimiento no permitido: el Rey no tiene PA.");
            return;
        }

        BoardManagerGlobal.Instance.AgregarMensajeInterno($"✅ Peón se moverá desde {posicionActual} a {nuevaPos} ({distancia} casillas).");

        Vector2Int paso = posicionActual;

        while (paso != nuevaPos)
        {
            if (paso.x < nuevaPos.x) paso.x++;
            else if (paso.x > nuevaPos.x) paso.x--;
            if (paso.y < nuevaPos.y) paso.y++;
            else if (paso.y > nuevaPos.y) paso.y--;

            SetPosicionActual(paso);
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚶 El Peón pasa por {paso}");

            foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
            {
                efecto.RevisarSiPeonLlegó(paso, this);
            }
        }

        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

        puntosMovimientoActual -= distancia;
        rey.puntosAccionActual -= 1;

        MostrarMovimientoPosible();
        mostrandoMovimientos = true;

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
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♟️ Peón en {posicionActual} revisa su casilla.");
        foreach (var objeto in BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual))
        {
            string nombre = ((MonoBehaviour)objeto).name;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"📦 Encontrado objeto: {objeto.GetType().Name} ({nombre})");

            if (objeto is ITileEffect efecto)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"♙ Peón absorbe efecto {efecto}.");
                efecto.RevisarSiPeonLlegó(posicionActual, this);
                MostrarMovimientoPosible();
            }
        }
    }

    private void RevisarAmenazasGlobal()
    {
        var fichasEnemigas = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IFichaEnemiga>();

        foreach (var ficha in fichasEnemigas)
        {
            ficha.RevisarAmenazasEnZona();
        }
    }

    public void ReiniciarTurno()
    {
        puntosMovimientoActual = puntosMovimientoBase;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♙ Nuevo turno del Peón: {puntosMovimientoActual} PM.");
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void GanarPuntoMovimiento(int cantidad)
    {
        puntosMovimientoExtra += cantidad;
        puntosMovimientoActual += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🏃 Peón gana +{cantidad} PM. Total: {puntosMovimientoActual}.");
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void DesactivarJuego()
    {
        juegoActivo = false;
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void GanarPuntoMovimientoSilencioso(int cantidad)
    {
        puntosMovimientoActual += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🤫 Peón gana +{cantidad} PM solo por este turno. Total: {puntosMovimientoActual}.");
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void MostrarRango()
    {
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void OcultarRango()
    {
        OcultarMovimientos();
        mostrandoMovimientos = false;
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }
    
    public bool EstaActivo() => juegoActivo;
}
