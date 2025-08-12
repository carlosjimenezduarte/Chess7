using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class QueenController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition, IFicha, IFichaAliada
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
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ Reina inició en {posicionActual}.");

        var objetosEnCasilla = BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual);
        if (!objetosEnCasilla.Contains(this))
        {
            BoardManagerGlobal.Instance.RegistrarMovimiento(this, posicionActual);
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"✅ ♛ Reina registrada manualmente en {posicionActual}.");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"ℹ️ ♛ Reina ya estaba registrada.");
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        posicionActual = nuevaPos;
        GetComponent<MovableTileObject>().tileCoords = nuevaPos;
        GetComponent<PiecePositioner>().tileCoords = nuevaPos;
        BoardManagerGlobal.Instance?.RegistrarMovimiento(this, nuevaPos);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ Reina movida a {nuevaPos}.");
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
            BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ Rey sin PA, Reina no puede moverse.");
            OcultarMovimientos();
            mostrandoMovimientos = false;
            return;
        }

        BoardManagerGlobal.Instance.AgregarMensajeInterno("🔍 Mostrando posibles movimientos de la Reina (diagonales y ortogonales):");

        Vector2Int[] direcciones = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
            new Vector2Int(1,1), new Vector2Int(-1,1), new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };

        var casillaActual = BoardManagerGlobal.Instance.GetTileAt(posicionActual);
        if (casillaActual != null)
        {
            casillaActual.HighlightMove(true);
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔵 Casilla central de la Reina ({posicionActual}) marcada como centro.");
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
                    BoardManagerGlobal.Instance.AgregarMensajeInterno($"⛔ Casilla {destino} no accesible.");
                    break;
                }

                var tile = BoardManagerGlobal.Instance.GetTileAt(destino);
                if (tile == null) break;

                tile.HighlightMove(true);
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🟦 Casilla {destino} marcada como movimiento válido.");

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
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🟢 Mostrando previsualización de la Reina.");
        }
        else
        {
            mostrandoMovimientos = false;
            OcultarMovimientos();
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🔴 Ocultando previsualización de la Reina.");
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void MoverA(Vector2Int nuevaPos, KingController rey)
    {
    if (!juegoActivo) return;

    Vector2Int delta = nuevaPos - posicionActual;
    bool esDiagonal = Mathf.Abs(delta.x) == Mathf.Abs(delta.y);
    bool esOrtogonales = delta.x == 0 || delta.y == 0;
    int distancia = Mathf.Max(Mathf.Abs(delta.x), Mathf.Abs(delta.y));

    // 1) Validación de patrón y rango
    if (!(esDiagonal || esOrtogonales) || distancia > RangoMovimientoActual)
    {
        BoardManagerGlobal.Instance.AgregarMensajeInterno(
            $"🚫 Movimiento inválido para la Reina desde {posicionActual} a {nuevaPos} (patrón/rango).");
        return;
    }

    // 2) Bloquear si hay obstáculos entre origen y destino (incluye recoleccionables)
    if (BoardManagerGlobal.Instance.HayObstaculoEntreAliado(posicionActual, nuevaPos, this))
    {
        BoardManagerGlobal.Instance.AgregarMensajeInterno(
            $"🛑 Movimiento bloqueado: hay un obstáculo entre {posicionActual} y {nuevaPos}.");
        return;
    }
    
    if (rey.puntosAccionActual <= 0)
    {
        BoardManagerGlobal.Instance.AgregarMensajeInterno("🚫 Movimiento inválido. Rey sin PA.");
        return;
    }

    // 3) Evaluar destino (enemigo o recoleccionable es válido)
        var objetosEnDestino = BoardManagerGlobal.Instance.ObtenerObjetosEn(nuevaPos);
    var enemigo = objetosEnDestino.FirstOrDefault(o => o is IFichaEnemiga);

    // 4) Mover
    SetPosicionActual(nuevaPos);
    transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

    // 5) Resolver combate si hay enemigo en destino
    if (enemigo != null)
    {
        if (enemigo is IPieceWithPosition enemigoPos)
            enemigoPos.SetPosicionActual(BoardManagerGlobal.DimensionDivina);

        Destroy(((MonoBehaviour)enemigo).gameObject);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"💀 Reina eliminó a un enemigo en {nuevaPos}.");
    }

    // 6) Disparar efectos (incluye recoger pociones/llaves si tu ITileEffect lo maneja)
    foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
        efecto.RevisarSiReinaLlegó(nuevaPos, null);

    // 7) Coste y refrescos
    rey.puntosAccionActual--;
    rangoMovimientoBase -= 1;
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

    BoardManagerGlobal.Instance.AgregarMensajeInterno("🔁 Evaluando casillas de ataque reales de la Reina...");

    Vector2Int[] direcciones = new Vector2Int[]
    {
        Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right,
        new Vector2Int(1,1), new Vector2Int(-1,1), new Vector2Int(1,-1), new Vector2Int(-1,-1)
    };

    foreach (var dir in direcciones)
    {
        for (int i = 1; i <= rangoAtaque; i++)
        {
            Vector2Int destino = posicionActual + dir * i;

            // ⛔ Bordes del tablero
            if (destino.x < 0 || destino.y < 0 || destino.x > 7 || destino.y > 7)
                break;

            // 🧱 Si hay obstáculo ENTRE origen y destino, no seguimos en esta dirección
            if (BoardManagerGlobal.Instance.HayObstaculoEntreAliado(posicionActual, destino, this))
                break;

            var tile = BoardManagerGlobal.Instance.GetTileAt(destino);
            if (tile == null) break;

            var objetos = BoardManagerGlobal.Instance.ObtenerObjetosEn(destino);

            // 👥 Aliado o inmóvil en destino bloquea (no es casilla de ataque)
            if (objetos.Any(obj => obj is IFichaAliada || obj is IFichaInmovil))
                break;

            // 🎯 Enemigo en destino: marcar y cortar la línea
            if (objetos.Any(obj => obj is IFichaEnemiga))
            {
                tile.HighlightEnemyAttack(true);
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🎯 Casilla {destino} marcada como zona de ataque.");
                break;
            }

            // Si no hay enemigo, seguimos buscando hasta rangoAtaque (sin pintar)
        }
    }
    }

    public void AumentarRangoMovimiento(int cantidad)
    {
        rangoMovimientoExtra += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"📏 Reina ganó +{cantidad} de rango temporal. Total: {RangoMovimientoActual}.");
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
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🤫 Reina ganó +{cantidad} de rango en silencio. Total: {RangoMovimientoActual}.");
    }

    public bool EstaActivo() => juegoActivo;
    public bool EsInamovible() => esInamovible;

    public void RecibirPenalizacionReina()
    {
        var reyreina = FindFirstObjectByType<KingController>();
        if (reyreina != null)
        {
            reyreina.puntosAccionActual -= 1;
            FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ Reina en {posicionActual} penalizada: -1 PA.");
        }
    }

    public void RecibirPenalizacionTorre()
    {
        var reyreinatorre = FindFirstObjectByType<KingController>();
        if (reyreinatorre != null)
        {
            reyreinatorre.puntosAccionActual -= 1;
            FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♜ Reina en {posicionActual} penalizada por Torre: -1 PA.");
        }
    }

    public void RecibirPenalizacionAlfil()
    {
        var reyreinaalfil = FindFirstObjectByType<KingController>();
        if (reyreinaalfil != null)
        {
            reyreinaalfil.puntosAccionActual -= 1;
            FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♝ Reina en {posicionActual} penalizada por Alfil: -1 PA.");
        }
    }

    
}
