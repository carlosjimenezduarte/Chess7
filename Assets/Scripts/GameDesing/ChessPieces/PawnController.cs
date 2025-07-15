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

        // 🔥 Ahora revisa diagonales inmediatas para posibles ataques
        Vector2Int[] diagonales = new Vector2Int[]
        {
        new Vector2Int(1,1), new Vector2Int(-1,1),
        new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };

        foreach (var delta in diagonales)
        {
            Vector2Int diagonal = posicionActual + delta;
            if (diagonal.x < 0 || diagonal.y < 0 || diagonal.x > 7 || diagonal.y > 7) continue;

            var objetivo = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .FirstOrDefault(obj =>
                    obj is IFichaEnemiga &&
                    obj.TryGetComponent<IPieceWithPosition>(out var pos) &&
                    pos.GetPosicionActual() == diagonal);

            if (objetivo != null)
            {
                Tile tile = BoardManagerGlobal.Instance.GetTileAt(diagonal);
                if (tile != null)
                {
                    tile.HighlightEnemyAttack(true); // fucsia fuerte
                    Debug.Log($"🔪 Peón puede atacar en diagonal a {objetivo.name} en {diagonal}");
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

        // 🚀 PRIMERO: revisar si es un ataque diagonal inmediato
        if (Mathf.Abs(nuevaPos.x - posicionActual.x) == 1 && Mathf.Abs(nuevaPos.y - posicionActual.y) == 1)
        {
            var fichaEnDiagonal = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Where(obj => obj is IFichaEnemiga)
                .FirstOrDefault(obj =>
                {
                    if (!obj.TryGetComponent<IPieceWithPosition>(out var pos)) return false;
                    return pos.GetPosicionActual() == nuevaPos;
                });

            if (fichaEnDiagonal != null)
            {
                Debug.Log($"💥 Peón salta en diagonal para eliminar a {fichaEnDiagonal.name} en {nuevaPos}");

                SetPosicionActual(nuevaPos);
                transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

                if (fichaEnDiagonal is IPieceWithPosition enemigo)
                    enemigo.SetPosicionActual(new Vector2Int(-1, -1));
                Destroy(fichaEnDiagonal.gameObject);

                rey.puntosAccionActual -= 1;

                MostrarMovimientoPosible();
                FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

                RevisarAmenazasGlobal(); // 🔥 ahora revisa todas las fichas enemigas (no solo reinas)
                return;
            }
        }

        // 🚫 Movimiento Manhattan normal
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

            foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
                efecto.RevisarSiPeonLlegó(paso, this);
        }

        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

        puntosMovimientoActual -= distancia;
        rey.puntosAccionActual -= 1;

        MostrarMovimientoPosible();
        mostrandoMovimientos = true;

        RevisarObjetosEnCasilla();

        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

        RevisarAmenazasGlobal(); // 🔥 importante: después del movimiento completo

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
    private void RevisarAmenazasGlobal()
    {
        var fichasEnemigas = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .OfType<IFichaEnemiga>();

        foreach (var ficha in fichasEnemigas)
        {
            ficha.RevisarAmenazasEnZona();
        }
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
