using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class KingController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition, IFicha, IFichaAliada
{
    public int puntosMovimientoMax = 3;

    public int rangoAtaqueKing = 1; // 🔺 Rango de ataque fijo del Rey (igual que el Peón)
    public int puntosAccionMax = 5;

    public bool esInamovible = false;

    [HideInInspector]
    public int puntosMovimientoActual;
    public int puntosAccionActual = 5;
    public int turnosRestantes = 7;

    private Vector2Int posicionActual;
    private bool juegoActivo = false;
    public bool mostrandoMovimientos = false;

    private void Start()
    {

        puntosMovimientoActual = 3;
        PiecePositioner piecePositioner = GetComponent<PiecePositioner>();
        if (piecePositioner != null)
        {
            posicionActual = piecePositioner.tileCoords;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♔ Rey inició en {posicionActual}");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ No hay PiecePositioner en el Rey. Usando (0,0).");
            posicionActual = new Vector2Int(0, 0);
        }

        // 🔍 Verificar si el Rey ya fue registrado en el tablero
        var objetosEnCasilla = BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual);
        bool yaRegistrado = objetosEnCasilla.Contains(this);

        if (!yaRegistrado)
        {
            BoardManagerGlobal.Instance.RegistrarMovimiento(this, posicionActual);
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"✅ ♔ Rey registrado manualmente en {posicionActual}.");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"ℹ️ ♔ Rey ya estaba registrado.");
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
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♔ Rey actualizado a {nuevaPos}.");
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

    BoardManagerGlobal.Instance.AgregarMensajeInterno($"👣 Alcance personal del Rey: {puntosMovimientoActual} PM.");

    // (Opcional) limpiar antes
    foreach (Tile t in BoardManagerGlobal.Instance.tiles)
        t.HighlightMove(false);

    foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
    {
        int distancia = Mathf.Abs(tile.tileCoords.x - posicionActual.x) + Mathf.Abs(tile.tileCoords.y - posicionActual.y);

        // ✅ Solo pinta si está dentro de PM y la casilla NO está ocupada por aliada/inmóvil
        bool puedeMover = distancia <= puntosMovimientoActual
                  && (tile.tileCoords == posicionActual 
                      || BoardManagerGlobal.Instance.EsCasillaAccesiblePorAliado(tile.tileCoords));

        tile.HighlightMove(puedeMover);
    }

    // --- tu bloque de ataque adyacente queda igual ---
    Vector2Int[] direcciones = new Vector2Int[]
    {
        new Vector2Int(1,0), new Vector2Int(-1,0),
        new Vector2Int(0,1), new Vector2Int(0,-1),
        new Vector2Int(1,1), new Vector2Int(-1,1),
        new Vector2Int(1,-1), new Vector2Int(-1,-1)
    };

    BoardManagerGlobal.Instance.AgregarMensajeInterno("🧠 Revisando casillas adyacentes para posibles ataques del Rey...");

    foreach (var delta in direcciones)
    {
        Vector2Int destino = posicionActual + delta;

        if (destino.x < 0 || destino.y < 0 || destino.x > 7 || destino.y > 7)
            continue;

        var objetosEnDestino = BoardManagerGlobal.Instance.ObtenerObjetosEn(destino).ToList();
        if (objetosEnDestino.Count == 0)
            continue;

        var enemigo = objetosEnDestino.FirstOrDefault(obj =>
            obj is IFichaEnemiga && obj is IPieceWithPosition pwp && pwp.GetPosicionActual() == destino);

        if (enemigo != null)
        {
            int distanciaX = Mathf.Abs(destino.x - posicionActual.x);
            int distanciaY = Mathf.Abs(destino.y - posicionActual.y);

            if (distanciaX <= rangoAtaqueKing && distanciaY <= rangoAtaqueKing)
            {
                Tile tile = BoardManagerGlobal.Instance.GetTileAt(destino);
                if (tile != null)
                {
                    tile.HighlightEnemyAttack(true);
                    BoardManagerGlobal.Instance.AgregarMensajeInterno($"🎯 Casilla {destino} marcada como ataque posible del Rey.");
                }
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
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🟢 Mostrando previsualización automática del Rey.");
        }
        else
        {
            mostrandoMovimientos = false;
            OcultarMovimientos();
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🔴 Ocultando previsualización del Rey.");
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void MoverA(Vector2Int nuevaPos)
{
    if (!juegoActivo) return;

    // 1) Chequeo de alcance Manhattan (diamante)
    int manhattan = Mathf.Abs(posicionActual.x - nuevaPos.x) + Mathf.Abs(posicionActual.y - nuevaPos.y);
    if (manhattan > puntosMovimientoActual)
    {
        BoardManagerGlobal.Instance.AgregarMensajeInterno("🚫 Movimiento no permitido, no hay suficientes PM.");
        return;
    }

    // 2) Avance paso a paso ORTOGONAL (nunca diagonal) hacia el destino
    Vector2Int paso = posicionActual;
    int pasosDados = 0;

    while (paso != nuevaPos)
    {
        Vector2Int siguiente = paso;

        // Elegimos mover en el eje con mayor diferencia primero (o X primero, como prefieras).
        int dx = nuevaPos.x - paso.x;
        int dy = nuevaPos.y - paso.y;

        if (Mathf.Abs(dx) >= Mathf.Abs(dy))
        {
            // Mueve en X si hay distancia en X; si no, en Y
            if (dx != 0)      siguiente.x += dx > 0 ? 1 : -1;
            else if (dy != 0) siguiente.y += dy > 0 ? 1 : -1;
        }
        else
        {
            // Mueve en Y si hay más distancia en Y; si no, en X
            if (dy != 0)      siguiente.y += dy > 0 ? 1 : -1;
            else if (dx != 0) siguiente.x += dx > 0 ? 1 : -1;
        }

        // 2.1) Validar tablero
        if (siguiente.x < 0 || siguiente.y < 0 || siguiente.x > 7 || siguiente.y > 7)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🛑 Movimiento cancelado: {siguiente} fuera del tablero.");
            return;
        }

        // 2.2) Revisar obstáculos en la casilla "siguiente"
        var objetos = BoardManagerGlobal.Instance.ObtenerObjetosEn(siguiente);

        // Aliado o inmóvil → bloquea
        if (objetos.Any(o => o is IFichaAliada))
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🛑 Movimiento bloqueado por aliado/obstáculo en {siguiente}.");
            return;
        }

        // Enemigo → el movimiento no puede pasar por encima (ataque es otra acción)
        if (objetos.Any(o => o is IFichaEnemiga))
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🛑 Movimiento bloqueado por enemigo en {siguiente}.");
            return;
        }

        // Recoleccionable → puedes caer aquí, pero NO seguir más allá
        bool hayReco = objetos.Any(o => o is IObjetoRecoleccionable);
        bool esUltimoPaso = siguiente == nuevaPos;

        if (hayReco && !esUltimoPaso)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🛑 Hay un objeto en {siguiente}. Debes caer aquí primero.");
            return;
        }

        // 2.3) Avanzar un paso (y disparar efectos)
        paso = siguiente;
        pasosDados++;

        SetPosicionActual(paso);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚶 El Rey pasa por {paso}");

        foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
            efecto.RevisarSiReyLlegó(paso, this);
    }

    // 3) Sincroniza visual + descuenta PM reales
    transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);
    puntosMovimientoActual -= pasosDados;

    // 4) Refrescos y lógica existente
    MostrarMovimientoPosible();
    mostrandoMovimientos = true;
    BoardManagerGlobal.Instance.NotificarMovimientoAliado(posicionActual);

    var reina = FindFirstObjectByType<QueenEnemyController>(); if (reina != null) reina.VerificarAmenazaSobre(posicionActual);
    var torre = FindFirstObjectByType<RookEnemyController>();  if (torre != null) torre.VerificarAmenazaSobre(posicionActual);
    var alfil = FindFirstObjectByType<BishopEnemyController>();if (alfil != null) alfil.VerificarAmenazaSobre(posicionActual);

    FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

    if (posicionActual == new Vector2Int(7, 7))
    {
        BoardManagerGlobal.Instance.AgregarMensajeInterno("🚀 El Rey llegó a la meta (H8). Calculando bonus.");
        int bonus = turnosRestantes * 50;
        PlayerScore.Instance.AgregarPuntaje(bonus);
        LevelResultUI.Instance.ShowResults(
            LevelProgress.Instance.keysCollected,
            LevelProgress.Instance.hasDiamond,
            turnosRestantes,
            PlayerScore.Instance.GetTotalScore()
        );
        juegoActivo = false;
        FindFirstObjectByType<ChessGameManager>()?.DetenerJuego();
    }

    if (turnosRestantes <= 0)
    {
        BoardManagerGlobal.Instance.AgregarMensajeInterno("💀 El Rey sin vidas.");
        LevelResultUI.Instance.ShowResults(
            LevelProgress.Instance.keysCollected,
            LevelProgress.Instance.hasDiamond,
            0,
            PlayerScore.Instance.GetTotalScore()
        );
        juegoActivo = false;
        FindFirstObjectByType<ChessGameManager>()?.DetenerJuego();
    }

    BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }




    public void ReiniciarTurno()
    {
        puntosMovimientoActual = puntosMovimientoMax;
        puntosAccionActual = puntosAccionMax;
        rangoAtaqueKing = 1;
        BoardManagerGlobal.Instance.AgregarMensajeInterno(
        $"♔ Nuevo turno del Rey → 🧭 PM personales: {puntosMovimientoActual}, 🎖️ PA estratégicos: {puntosAccionActual}.");
        //MostrarMovimientoPosible();
        mostrandoMovimientos = true;
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
        BoardManagerGlobal.Instance.ResetearAtaquesEnemigos();
    }

    public void GanarPuntoMovimiento(int cantidad)
    {
        puntosMovimientoActual += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧭 El Rey gana {cantidad:+#;-#} PM personales. Total: {puntosMovimientoActual}.");
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void GanarVida(int cantidad)
    {
        turnosRestantes += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"❤️ El Rey gana +{cantidad} vida(s). Ahora tiene {turnosRestantes}.");
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RestarTurno()
    {
        turnosRestantes--;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"⏳ El Rey pierde 1 vida. Turnos restantes: {turnosRestantes}");
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

        if (turnosRestantes <= 0)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("💀 El Rey ha agotado todos sus turnos.");

            LevelResultUI.Instance.ShowResults(
                LevelProgress.Instance.keysCollected,
                LevelProgress.Instance.hasDiamond,
                0,
                PlayerScore.Instance.GetTotalScore()
            );
            juegoActivo = false;
            FindFirstObjectByType<ChessGameManager>()?.DetenerJuego();
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
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
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void OcultarRango()
    {
        OcultarMovimientos();
        mostrandoMovimientos = false;
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }
    public bool EsInamovible()
    {
        return esInamovible;
    }

    public void IntentarAtacar(Vector2Int destino)
    {
        int distanciaX = Mathf.Abs(destino.x - posicionActual.x);
        int distanciaY = Mathf.Abs(destino.y - posicionActual.y);

        bool dentroDelRango = distanciaX <= rangoAtaqueKing && distanciaY <= rangoAtaqueKing;

        if (!dentroDelRango)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"❌ Casilla {destino} fuera del rango de ataque del Rey.");
            return;
        }

        var objetivo = BoardManagerGlobal.Instance.ObtenerObjetosEn(destino)
            .FirstOrDefault(obj => obj is IFichaEnemiga);

        if (objetivo == null)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🕊️ No hay enemigo en {destino}. Nada que atacar.");
            return;
        }

        if (puntosAccionActual <= 0)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ El Rey no tiene PA suficientes para atacar.");
            return;
        }

        // 🔥 Eliminar ficha enemiga y marcarla fuera del tablero
        if (objetivo is IPieceWithPosition enemigo)
        {
            if (enemigo.GetPosicionActual() != destino)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"❌ El enemigo {objetivo} no está realmente en {destino}, está en {enemigo.GetPosicionActual()}. No se ejecuta el ataque.");
                return;
            }

            enemigo.SetPosicionActual(BoardManagerGlobal.DimensionDivina);
        }

        Destroy(((MonoBehaviour)objetivo).gameObject);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"💀 Rey eliminó al enemigo en {destino}.");

        // 🔄 Actualizar posición lógica y visual del Rey
        SetPosicionActual(destino);
        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(destino);

        // ✨ Activar efectos especiales de casilla
        foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
        {
            efecto.RevisarSiReyLlegó(destino, this);
        }

        // 📉 Consumir 1 PA
        puntosAccionActual--;

        // 🔎 Verificar amenazas después del ataque
        foreach (var ficha in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<IFichaEnemiga>())
            ficha.RevisarAmenazasEnZona();

        // 🎯 Refrescar HUD y estado del tablero
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"⚔️ Rey atacó y se desplazó a {destino}. PA restantes: {puntosAccionActual}");
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
        MostrarMovimientoPosible();
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }
    public int rangoMovimientoBase
    {
        get => 0; // El Rey no usa esta propiedad
        set { }   // Ignora cualquier intento de modificarla
    }
    public int rangoAtaque
    {
        get => 0; // El Rey no usa esta propiedad
        set { }   // Ignora cualquier intento de modificarla
    }
    
    


}