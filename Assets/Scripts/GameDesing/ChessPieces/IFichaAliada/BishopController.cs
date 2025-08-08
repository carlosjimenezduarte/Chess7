using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class BishopController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition, IFicha, IFichaAliada
{
    [Header("Rango de Movimiento")]
    public int rangoMovimientoBase { get; set; } = 5;
    private int rangoMovimientoExtra = 0;
    public int rangoAtaque { get; set; } = 3; // 🔺 NUEVO: Rango fijo de ataque en diagonal

    public bool esInamovible = false;
    public int RangoMovimientoActual => rangoMovimientoBase + rangoMovimientoExtra;

    private Vector2Int posicionActual;
    private bool juegoActivo = false;
    public bool mostrandoMovimientos = false;

    private void Start()
    {
        PiecePositioner piecePositioner = GetComponent<PiecePositioner>();
        posicionActual = piecePositioner != null ? piecePositioner.tileCoords : new Vector2Int(0, 0);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♝ Alfil inició en {posicionActual}.");

        // 🔍 Verificar si el Alfil ya fue registrado en el tablero
        var objetosEnCasilla = BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual);
        bool yaRegistrado = objetosEnCasilla.Contains(this);

        if (!yaRegistrado)
        {
            BoardManagerGlobal.Instance.RegistrarMovimiento(this, posicionActual);
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"✅ ♝ Alfil registrado manualmente en {posicionActual}.");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"ℹ️ ♝ Alfil ya estaba registrado.");
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
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♝ Alfil movido a {nuevaPos}.");
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
        BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ Rey sin PA, Alfil no puede moverse.");
        OcultarMovimientos();
        mostrandoMovimientos = false;
        return;
    }

    BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔍 Mostrando rango de movimiento del Alfil: {RangoMovimientoActual} casillas diagonales.");

    Vector2Int[] direcciones = new Vector2Int[]
    {
        new Vector2Int(1,1), new Vector2Int(1,-1),
        new Vector2Int(-1,1), new Vector2Int(-1,-1)
    };

    // ✅ Casilla actual en verde
    Tile casillaActual = BoardManagerGlobal.Instance.GetTileAt(posicionActual);
    if (casillaActual != null)
        casillaActual.HighlightMove(true);

    foreach (var direccion in direcciones)
    {
        for (int i = 1; i <= RangoMovimientoActual; i++)
        {
            Vector2Int destino = posicionActual + direccion * i;

            if (destino.x < 0 || destino.y < 0 || destino.x > 7 || destino.y > 7)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"⛔ Casilla {destino} fuera del tablero. Se ignora.");
                break;
            }

            var tile = BoardManagerGlobal.Instance.GetTileAt(destino);
            if (tile == null) continue;

            var objetosEnCasilla = BoardManagerGlobal.Instance.ObtenerObjetosEn(destino).ToList();

            bool hayObstaculo = objetosEnCasilla.Any(obj => obj is IFichaAliada || obj is IFichaInmovil);
            if (hayObstaculo)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚫 Casilla {destino} bloqueada por obstáculo. Fin de esta dirección.");
                break;
            }

            var enemigo = objetosEnCasilla
                .FirstOrDefault(obj => obj is IFichaEnemiga && obj is IPieceWithPosition pwp && pwp.GetPosicionActual() == destino);

            if (enemigo != null)
            {
                tile.HighlightEnemyAttack(true);
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🎯 Enemigo real encontrado en {destino}: {enemigo}. Casilla marcada en fucsia.");
                break; // El alfil no puede seguir después de eliminar
            }

            tile.HighlightMove(true); // Casilla válida de movimiento
        }
    }

    mostrandoMovimientos = true;
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
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🟢 Mostrando previsualización automática del Alfil.");
        }
        else
        {
            mostrandoMovimientos = false;
            OcultarMovimientos();
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🔴 Ocultando previsualización del Alfil.");
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void MoverA(Vector2Int nuevaPos, KingController rey)
{
    if (!juegoActivo) return;

    int deltaX = Mathf.Abs(nuevaPos.x - posicionActual.x);
    int deltaY = Mathf.Abs(nuevaPos.y - posicionActual.y);

    // ⚠️ Validación: solo movimiento diagonal
    if (deltaX != deltaY)
    {
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"❌ Movimiento no válido. Alfil solo se desplaza en diagonal.");
        return;
    }

    int distancia = deltaX;

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

    // ⚔️ Verificar si es ataque
    var fichaEnemiga = BoardManagerGlobal.Instance.ObtenerObjetosEn(nuevaPos)
        .FirstOrDefault(obj => obj is IFichaEnemiga);

    if (fichaEnemiga != null)
    {
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🎯 Enemigo detectado en {nuevaPos}: {fichaEnemiga}");

        if (fichaEnemiga is IPieceWithPosition enemigo)
            enemigo.SetPosicionActual(BoardManagerGlobal.DimensionDivina);

        Destroy(((MonoBehaviour)fichaEnemiga).gameObject);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"💀 Ficha enemiga destruida por el Alfil en {nuevaPos}");
    }

    // 🔁 Recorrido paso a paso (para efectos tipo Expansion)
    Vector2Int paso = posicionActual;
    while (paso != nuevaPos)
    {
        paso.x += (nuevaPos.x > paso.x) ? 1 : -1;
        paso.y += (nuevaPos.y > paso.y) ? 1 : -1;

        SetPosicionActual(paso);

        // Activar efectos de casilla
        foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
            efecto.RevisarSiAlfilLlegó(paso, this); // Usa la misma lógica que el Peón
    }

    transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

    // 🔹 Ajuste de rango si hay bonus
    if (distancia > rangoMovimientoBase)
    {
        int extraUsado = distancia - rangoMovimientoBase;
        rangoMovimientoExtra = Mathf.Max(0, rangoMovimientoExtra - extraUsado);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧪 Rango temporal reducido en {extraUsado}. Rango restante: {RangoMovimientoActual}.");
    }

    // 🔹 Consumir PA del Rey
    rey.puntosAccionActual--;
    BoardManagerGlobal.Instance.AgregarMensajeInterno($"🎯 PA consumido. PA restantes: {rey.puntosAccionActual}");
    OcultarMovimientos();
    MostrarMovimientoPosible();
    RevisarObjetosEnCasilla();
    FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

    BoardManagerGlobal.Instance.NotificarMovimientoAliado(posicionActual);
    BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
}


    public void RevisarObjetosEnCasilla()
    {
        foreach (var objeto in BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual))
        {
            if (objeto is ITileEffect efecto)
            {
                efecto.RevisarSiAlfilLlegó(posicionActual, this);
                MostrarMovimientoPosible();
            }
        }
    }


    public void AumentarRangoMovimiento(int cantidad)
    {
        rangoMovimientoExtra += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🏃 Alfil ganó +{cantidad} de rango temporal. Total: {RangoMovimientoActual}.");
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
        rangoMovimientoBase = 5;
        rangoAtaque = 3;
    }

    public void AumentarRangoMovimientoSilencioso(int cantidad)
    {
        rangoMovimientoExtra += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🤫 Alfil ganó +{cantidad} de rango temporal en modo silencioso. Total: {RangoMovimientoActual}.");
    }
    public bool EstaActivo() => juegoActivo;
    public bool EsInamovible() => esInamovible;

    public void RecibirPenalizacionReina()
    {
        var reyalfil = FindFirstObjectByType<KingController>();
        if (reyalfil != null)
        {
            reyalfil.puntosAccionActual -= 1;

            // 🔹 Actualizar HUD inmediatamente
            var gameManager = FindFirstObjectByType<ChessGameManager>();
            if (gameManager != null)
                gameManager.ActualizarHUD();

            BoardManagerGlobal.Instance.AgregarMensajeInterno(
                $"♛ Alfil en {posicionActual} penalizado: -1 PA. PA actual del Rey: {reyalfil.puntosAccionActual}"
            );
        }
    }

    public void RecibirPenalizacionTorre()
    {
        var reyalfiltorre = FindFirstObjectByType<KingController>();
        if (reyalfiltorre != null)
        {
            reyalfiltorre.puntosAccionActual -= 1;

            // 🔹 Actualizar HUD inmediatamente
            var gameManager = FindFirstObjectByType<ChessGameManager>();
            if (gameManager != null)
                gameManager.ActualizarHUD();

            BoardManagerGlobal.Instance.AgregarMensajeInterno(
                $"♜ Alfil en {posicionActual} penalizado: -1 PA. PA actual del Rey: {reyalfiltorre.puntosAccionActual}"
            );
        }
    }

    public void RecibirPenalizacionAlfil()
    {
        var reyalfilalfil = FindFirstObjectByType<KingController>();
        if (reyalfilalfil != null)
        {
            reyalfilalfil.puntosAccionActual -= 1;

            // 🔹 Actualizar HUD inmediatamente
            var gameManager = FindFirstObjectByType<ChessGameManager>();
            if (gameManager != null)
                gameManager.ActualizarHUD();

            BoardManagerGlobal.Instance.AgregarMensajeInterno(
                $"♝ Alfil en {posicionActual} penalizado: -1 PA. PA actual del Rey: {reyalfilalfil.puntosAccionActual}"
            );
        }
    }

    
}
