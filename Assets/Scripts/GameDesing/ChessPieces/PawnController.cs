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

    // ✅ Informamos al BoardManagerGlobal del nuevo posicionamiento
    BoardManagerGlobal.Instance?.RegistrarMovimiento(this, nuevaPos);

    // ✅ También actualizamos el MovableTileObject
    var movible = GetComponent<MovableTileObject>();
    if (movible != null)
        movible.tileCoords = nuevaPos;

    // ✅ También actualizamos el PiecePositioner
    var posicionador = GetComponent<PiecePositioner>();
    if (posicionador != null)
        posicionador.tileCoords = nuevaPos;
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

        // ✅ OPTIMIZADO: ahora revisa diagonales inmediatas con el BoardManagerGlobal
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
                    tile.HighlightEnemyAttack(true); // fucsia fuerte
                    Debug.Log($"🔪 Peón puede atacar en diagonal a {objetivo} en {diagonal}");
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

        // ✅ OPTIMIZADO: ahora usa BoardManagerGlobal para revisar enemigos en diagonal
        if (Mathf.Abs(nuevaPos.x - posicionActual.x) == 1 && Mathf.Abs(nuevaPos.y - posicionActual.y) == 1)
        {
            var fichaEnDiagonal = BoardManagerGlobal.Instance.ObtenerObjetosEn(nuevaPos)
                .FirstOrDefault(obj => obj is IFichaEnemiga);

            if (fichaEnDiagonal != null)
            {
                Debug.Log($"💥 Peón salta en diagonal para eliminar a {fichaEnDiagonal} en {nuevaPos}");

                SetPosicionActual(nuevaPos);
                transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

                if (fichaEnDiagonal is IPieceWithPosition enemigo)
                    enemigo.SetPosicionActual(new Vector2Int(-1, -1));
                Destroy(((MonoBehaviour)fichaEnDiagonal).gameObject);

                rey.puntosAccionActual -= 1;

                MostrarMovimientoPosible();
                FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

                RevisarAmenazasGlobal();
                return;
            }
        }

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
            if (paso.x < nuevaPos.x) paso.x++;
            else if (paso.x > nuevaPos.x) paso.x--;

            if (paso.y < nuevaPos.y) paso.y++;
            else if (paso.y > nuevaPos.y) paso.y--;

            SetPosicionActual(paso);
            Debug.Log($"🚶 El Peón pasa por {paso}");

            // ✅ OPTIMIZADO: usa BoardManagerGlobal para encontrar ITileEffect en el paso
            foreach (var objeto in BoardManagerGlobal.Instance.ObtenerObjetosEn(paso))
            {
                if (objeto is ITileEffect efecto)
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
            Debug.Log("♕ El Peón ha coronado en H8. Otorga bonus al Rey.");
            rey.puntosAccionActual += 7;
            rey.puntosMovimientoActual += 7;
            rey.GanarVida(3);
            FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
            Destroy(gameObject);
        }
    }

    public void RevisarObjetosEnCasilla()
    {
        Debug.Log($"♟️ Peón en {posicionActual} revisa objetos en la casilla.");
        // ✅ OPTIMIZADO: ahora busca objetos directamente en el BoardManagerGlobal
        foreach (var objeto in BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual))
        {
             Debug.Log($"📦 Encontrado objeto: {objeto.GetType().Name} ({((MonoBehaviour)objeto).name})");

            if (objeto is ITileEffect efecto)
            {
                Debug.Log($"♙ Peón en {posicionActual} absorbe efecto {efecto}.");
                efecto.RevisarSiPeonLlegó(posicionActual, this);
                MostrarMovimientoPosible();
            }
        }
    }

    private void RevisarAmenazasGlobal()
    {
        // 🔥 Por ahora sigue recorriendo todo (pues aquí sí queremos revisar todas las fichas enemigas del mapa)
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
}
