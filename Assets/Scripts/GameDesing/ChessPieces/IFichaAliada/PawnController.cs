using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class PawnController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition, IFicha, IFichaAliada
{
    [Header("Rango de Movimiento")]
    public int rangoMovimientoBase { get; set; } = 1;
    private int rangoMovimientoExtra = 0;
    public int rangoAtaque { get; set; } = 1; // 🔺 NUEVO: Rango fijo de ataque en diagonal

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

        // 🔍 Verificar si el Peón ya fue registrado en el tablero
        var objetosEnCasilla = BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual);
        bool yaRegistrado = objetosEnCasilla.Contains(this);

        if (!yaRegistrado)
        {
            BoardManagerGlobal.Instance.RegistrarMovimiento(this, posicionActual);
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"✅ ♙ Peón registrado manualmente en {posicionActual}.");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"ℹ️ ♙ Peón ya estaba registrado.");
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            posicionActual = nuevaPos;
            return;
        }
#endif

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

        // 💥 Mostrar ataques en las diagonales dentro del rango de ataque
        Vector2Int[] diagonales = new Vector2Int[]
        {
        new Vector2Int(1,1), new Vector2Int(-1,1),
        new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };

        BoardManagerGlobal.Instance.AgregarMensajeInterno("🧪 Iniciando revisión de casillas diagonales para posibles ataques del Peón...");

        foreach (var delta in diagonales)
        {
            Vector2Int diagonal = posicionActual + delta;

            if (diagonal.x < 0 || diagonal.y < 0 || diagonal.x > 7 || diagonal.y > 7)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"⛔ Casilla {diagonal} fuera del tablero. Se ignora.");
                continue;
            }

            // ✅ Validación explícita de movimiento diagonal de una casilla
            if (!(Mathf.Abs(delta.x) == 1 && Mathf.Abs(delta.y) == 1))
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚫 Movimiento {delta} no es diagonal. Se ignora.");
                continue;
            }

            var objetosEnDiagonal = BoardManagerGlobal.Instance.ObtenerObjetosEn(diagonal).ToList();
            if (objetosEnDiagonal.Count == 0)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔍 Casilla {diagonal} está vacía. No hay enemigo.");
                continue;
            }

            var objetivo = objetosEnDiagonal.FirstOrDefault(obj =>
            obj is IFichaEnemiga && obj is IPieceWithPosition pwp && pwp.GetPosicionActual() == diagonal);

            if (objetivo != null)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"✅ Enemigo real encontrado en {diagonal}: {objetivo}");

                Tile tile = BoardManagerGlobal.Instance.GetTileAt(diagonal);
                if (tile != null)
                {
                    tile.HighlightEnemyAttack(true);
                    BoardManagerGlobal.Instance.AgregarMensajeInterno($"🎯 Casilla {diagonal} marcada como zona de ataque (fucsia).");
                }
                else
                {
                    BoardManagerGlobal.Instance.AgregarMensajeInterno($"⚠️ No se encontró Tile visual en {diagonal} para marcar ataque.");
                }
            }
            else
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔎 Ningún enemigo legítimo presente en {diagonal}. No se marcará.");
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

        bool esDiagonal = Mathf.Abs(nuevaPos.x - posicionActual.x) == 1 && Mathf.Abs(nuevaPos.y - posicionActual.y) == 1;

        // ⚔️ PRIORIDAD: Ataque diagonal
        if (esDiagonal)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧐 Intentando ataque diagonal desde {posicionActual} hacia {nuevaPos}...");

            var fichaEnDiagonal = BoardManagerGlobal.Instance.ObtenerObjetosEn(nuevaPos)
                .FirstOrDefault(obj => obj is IFichaEnemiga);

            if (fichaEnDiagonal != null)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🎯 Ficha enemiga detectada: {fichaEnDiagonal} en {nuevaPos}");

                // ✅ Ataque autorizado (rangoAtaque siempre = 1)
                SetPosicionActual(nuevaPos);
                transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

                if (fichaEnDiagonal is IPieceWithPosition enemigo)
                    enemigo.SetPosicionActual(BoardManagerGlobal.DimensionDivina);

                Destroy(((MonoBehaviour)fichaEnDiagonal).gameObject);
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"💀 Ficha enemiga destruida en {nuevaPos} por el Peón.");

                // 🔹 Consumir PA del Rey
                rey.puntosAccionActual--;
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"⚔️ Peón consumió 1 PA del Rey. PA restantes: {rey.puntosAccionActual}");

                MostrarMovimientoPosible();
                FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

                // ✅ Notificar al Árbitro para autorizar solo 1 ataque enemigo
                BoardManagerGlobal.Instance.NotificarMovimientoAliado(posicionActual);

                BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
                return;
            }
            else
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔍 No se encontró ficha enemiga en {nuevaPos}, ataque cancelado.");
                return;
            }
        }

        // 🔁 MOVIMIENTO ORTOGONAL (si no es ataque diagonal)
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

        // 🔹 Recorrido paso a paso (por efectos como Expansion o PusherUp)
        Vector2Int paso = posicionActual;
        while (paso != nuevaPos)
        {
            if (paso.x < nuevaPos.x) paso.x++;
            else if (paso.x > nuevaPos.x) paso.x--;
            if (paso.y < nuevaPos.y) paso.y++;
            else if (paso.y > nuevaPos.y) paso.y--;

            SetPosicionActual(paso);

            // Activar efectos de casilla
            foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
                efecto.RevisarSiPeonLlegó(paso, this);
        }

        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

        // 🔹 Ajuste de rango temporal si excede el base
        if (distancia > rangoMovimientoBase)
        {
            int extraUsado = distancia - rangoMovimientoBase;
            rangoMovimientoExtra = Mathf.Max(0, rangoMovimientoExtra - extraUsado);
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧪 Rango temporal reducido en {extraUsado}. Rango restante: {RangoMovimientoActual}.");
        }

        // 🔹 Consumir PA del Rey
        rey.puntosAccionActual--;

        MostrarMovimientoPosible();
        RevisarObjetosEnCasilla();
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

        // ✅ Notificar al Árbitro para que autorice ataque enemigo solo una vez
        BoardManagerGlobal.Instance.NotificarMovimientoAliado(posicionActual);

        // 🔹 Coronación de Peón (opcional)
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

    public void ReiniciarTurno()
    {
        rangoMovimientoBase = 1;
        rangoAtaque = 1;
    }

    public void AumentarRangoMovimientoSilencioso(int cantidad)
    {
        rangoMovimientoExtra += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🤫 Peón ganó +{cantidad} de rango temporal en modo silencioso. Total: {RangoMovimientoActual}.");
    }
    public bool EstaActivo() => juegoActivo;
    public bool EsInamovible() => esInamovible;

    public void RecibirPenalizacionPorReina()
    {
    var rey = FindFirstObjectByType<KingController>();
    if (rey != null)
    {
        rey.puntosAccionActual -= 1;

        // 🔹 Actualizar HUD inmediatamente
        var gameManager = FindFirstObjectByType<ChessGameManager>();
        if (gameManager != null)
            gameManager.ActualizarHUD();

        BoardManagerGlobal.Instance.AgregarMensajeInterno(
            $"♛ Peón en {posicionActual} penalizado: -1 PA. PA actual del Rey: {rey.puntosAccionActual}"
        );
    }
    }
    
}
