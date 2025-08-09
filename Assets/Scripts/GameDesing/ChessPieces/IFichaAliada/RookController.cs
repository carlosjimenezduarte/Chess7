using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class RookController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition, IFicha, IFichaAliada
{
    [Header("Rangos")]
    public int rangoMovimientoBase { get; set; } = 5;
    private int rangoMovimientoExtra = 0;
    public int rangoAtaque { get; set; } = 3;

    public bool esInamovible = false;
    public int RangoMovimientoActual => rangoMovimientoBase + rangoMovimientoExtra;

    private Vector2Int posicionActual;
    private bool juegoActivo = false;
    public bool mostrandoMovimientos = false;

    private void Start()
    {
        posicionActual = GetComponent<PiecePositioner>()?.tileCoords ?? new Vector2Int(0, 0);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♜ Torre inició en {posicionActual}.");

        var objetosEnCasilla = BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual);
        bool yaRegistrado = objetosEnCasilla.Contains(this);

        if (!yaRegistrado)
        {
            BoardManagerGlobal.Instance.RegistrarMovimiento(this, posicionActual);
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"✅ ♜ Torre registrada manualmente en {posicionActual}.");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"ℹ️ ♜ Torre ya estaba registrada.");
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        posicionActual = nuevaPos;
        GetComponent<MovableTileObject>().tileCoords = nuevaPos;
        GetComponent<PiecePositioner>().tileCoords = nuevaPos;
        BoardManagerGlobal.Instance?.RegistrarMovimiento(this, nuevaPos);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♜ Torre movida a {nuevaPos}.");
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
            BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ Rey sin PA, Torre no puede moverse.");
            OcultarMovimientos();
            mostrandoMovimientos = false;
            return;
        }

        BoardManagerGlobal.Instance.AgregarMensajeInterno("🔍 Mostrando posibles movimientos ortogonales de la Torre:");

        Vector2Int[] direcciones = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };
        // 🔵 Iluminar la casilla actual de la Torre
        var casillaActual = BoardManagerGlobal.Instance.GetTileAt(posicionActual);
        if (casillaActual != null)
        {
            casillaActual.HighlightMove(true);
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔵 Casilla central del Caballo ({posicionActual}) marcada como centro.");
        }
        foreach (var dir in direcciones)
        {
            for (int i = 1; i <= RangoMovimientoActual; i++)
            {
                
                Vector2Int destino = posicionActual + dir * i;
                if (BoardManagerGlobal.Instance.HayObstaculoEntreAliado(posicionActual, destino, this))
                {
                    break;
                }               
                if (!BoardManagerGlobal.Instance.EsCasillaAccesiblePorAliado(destino))
                {
                    BoardManagerGlobal.Instance.AgregarMensajeInterno($"⛔ Casilla {destino} no accesible o fuera del tablero.");
                    break;
                }

                var tile = BoardManagerGlobal.Instance.GetTileAt(destino);
                if (tile == null) break;

                tile.HighlightMove(true);
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🟦 Casilla {destino} marcada como movimiento válido.");

                // Detener si hay objeto recoleccionable o enemigo (no se puede pasar a través)
                var objetos = BoardManagerGlobal.Instance.ObtenerObjetosEn(destino);
                if (objetos.Any(obj => obj is IFicha || obj is IFichaInmovil))
                    break;
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
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🟢 Mostrando previsualización automática de la Torre.");
        }
        else
        {
            mostrandoMovimientos = false;
            OcultarMovimientos();
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🔴 Ocultando previsualización de la Torre.");
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void MoverA(Vector2Int nuevaPos, KingController rey)
    {
    if (!juegoActivo) return;

    Vector2Int delta = nuevaPos - posicionActual;

    // 1) Patrón ortogonal + rango
    bool esMovimientoValido = false;
    if (delta.x == 0 || delta.y == 0)
    {
        int distancia = Mathf.Abs(delta.x) + Mathf.Abs(delta.y);
        esMovimientoValido = distancia <= RangoMovimientoActual;
    }

    if (!esMovimientoValido)
    {
        BoardManagerGlobal.Instance.AgregarMensajeInterno(
            $"🚫 Movimiento inválido para la Torre desde {posicionActual} a {nuevaPos} (patrón/rango).");
        return;
    }

    // 2) Bloqueo por obstáculo en el trayecto (aliado, inmóvil o recoleccionable)
    if (BoardManagerGlobal.Instance.HayObstaculoEntreAliado(posicionActual, nuevaPos, this))
    {
        BoardManagerGlobal.Instance.AgregarMensajeInterno(
            $"🛑 Movimiento bloqueado: hay un obstáculo entre {posicionActual} y {nuevaPos}.");
        return;
    }

    // 3) Evaluar destino (enemigo/recoleccionable permitido)
    var objetosEnDestino = BoardManagerGlobal.Instance.ObtenerObjetosEn(nuevaPos);
    var enemigo = objetosEnDestino.FirstOrDefault(o => o is IFichaEnemiga);

    // 4) Mover
    SetPosicionActual(nuevaPos);
    transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

    // 5) Resolver combate si hay enemigo
    if (enemigo != null)
    {
        if (enemigo is IPieceWithPosition enemigoPos)
            enemigoPos.SetPosicionActual(BoardManagerGlobal.DimensionDivina);

        Destroy(((MonoBehaviour)enemigo).gameObject);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"💀 Torre eliminó a un enemigo en {nuevaPos}.");
    }

    // 6) Disparar efectos de casilla (recolecciones, trampas, etc.)
    foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
        efecto.RevisarSiTorreLlegó(nuevaPos, null);

    // 7) Coste y refrescos
    rey.puntosAccionActual--;
    OcultarMovimientos();
    MostrarMovimientoPosible();
    StartCoroutine(EvaluarCasillasDeAtaque());
    FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
    BoardManagerGlobal.Instance.NotificarMovimientoAliado(posicionActual);
    BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();  
    }


    private IEnumerator EvaluarCasillasDeAtaque()
    {
    yield return new WaitForSeconds(0.2f);

    BoardManagerGlobal.Instance.AgregarMensajeInterno("🔁 Evaluando casillas de ataque reales de la Torre...");

    Vector2Int[] direcciones = new Vector2Int[]
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
    };

    foreach (var dir in direcciones)
    {
        for (int i = 1; i <= rangoAtaque; i++)
        {
            Vector2Int destino = posicionActual + dir * i;

            // ⛔ Bordes del tablero
            if (destino.x < 0 || destino.y < 0 || destino.x > 7 || destino.y > 7)
                break;

            // 🧱 Obstáculo ENTRE origen y destino bloquea la línea
            if (BoardManagerGlobal.Instance.HayObstaculoEntreAliado(posicionActual, destino, this))
                break;

            var tile = BoardManagerGlobal.Instance.GetTileAt(destino);
            if (tile == null) break;

            var objetos = BoardManagerGlobal.Instance.ObtenerObjetosEn(destino);

            // 👥 Aliado / Inmóvil en destino: bloquea (no es atacable)
            if (objetos.Any(obj => obj is IFichaAliada || obj is IFichaInmovil))
                break;

            // 🎒 Recoleccionable en destino: también bloquea (no mirar más allá)
            if (objetos.Any(obj => obj is IObjetoRecoleccionable))
                break;

            // 🎯 Enemigo en destino: marcar y cortar la línea
            if (objetos.Any(obj => obj is IFichaEnemiga))
            {
                tile.HighlightEnemyAttack(true);
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🎯 Casilla {destino} marcada como zona de ataque (post-movimiento).");
                break;
            }

            // Sin enemigo: continuar explorando hasta rangoAtaque
        }
    }
    }


    public void AumentarRangoMovimiento(int cantidad)
    {
        rangoMovimientoExtra += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"📏 Torre ganó +{cantidad} de rango temporal. Total: {RangoMovimientoActual}.");
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
    }

    public void DesactivarJuego() => juegoActivo = false;
    public void MostrarRango() => MostrarMovimientoPosible();
    public void OcultarRango() => OcultarMovimientos();
    public void ReiniciarTurno()
    {
        rangoMovimientoBase = 5;
        rangoAtaque = 3;
        StartCoroutine(EvaluarCasillasDeAtaque());
    }

    public void AumentarRangoMovimientoSilencioso(int cantidad)
    {
        rangoMovimientoExtra += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🤫 Torre ganó +{cantidad} de rango temporal en modo silencioso. Total: {RangoMovimientoActual}.");
    }

    public bool EstaActivo() => juegoActivo;
    public bool EsInamovible() => esInamovible;

    public void RecibirPenalizacionReina()
    {
        var reytorre = FindFirstObjectByType<KingController>();
        if (reytorre != null)
        {
            reytorre.puntosAccionActual -= 1;
            FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ Torre en {posicionActual} penalizada: -1 PA.");
        }
    }

    public void RecibirPenalizacionTorre()
    {
        var reytorretorre = FindFirstObjectByType<KingController>();
        if (reytorretorre != null)
        {
            reytorretorre.puntosAccionActual -= 1;
            FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♜ Torre en {posicionActual} penalizada: -1 PA.");
        }
    }

    public void RecibirPenalizacionAlfil()
    {
        var reytorrealfil = FindFirstObjectByType<KingController>();
        if (reytorrealfil != null)
        {
            reytorrealfil.puntosAccionActual -= 1;
            FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♝ Torre en {posicionActual} penalizada: -1 PA.");
        }
    }
}
