using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class BlackQueenEnemyController : MonoBehaviour, IPointerClickHandler, ITileEffect, IPieceWithPosition, IFicha, IFichaEnemiga
{
    [Header("Jerarquía de ataque")]

    public int rangoKillZone { get; set; } = 7;

    public bool esInamovible = false;
    public int rangoRangeZone { get; set; } = 7;


    private Vector2Int posicionActual;
    private bool mostrandoRango = false;

    private void Start()
    {
        // 1️⃣ Determinar posición inicial
        PiecePositioner piecePositioner = GetComponent<PiecePositioner>();
        if (piecePositioner != null)
        {
            posicionActual = piecePositioner.tileCoords;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ Reina inició en {posicionActual}");
        }
        else
        {
            posicionActual = new Vector2Int(0, 0);
            BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ No hay PiecePositioner en la Reina. Usando (0,0).");
        }

        // 2️⃣ Registrar posición en el tablero global
        BoardManagerGlobal.Instance.RegistrarMovimiento(this, posicionActual);

        // 3️⃣ Registrar como ficha enemiga para el Árbitro Silencioso
        BoardManagerGlobal.Instance.RegistrarFichaEnemiga(this);

        // 4️⃣ Reporte final
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
        if (TryGetComponent<PiecePositioner>(out var piecePositioner))
            piecePositioner.tileCoords = nuevaPos;
        foreach (var efecto in BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual).OfType<ITileEffect>())
        {
            efecto.RevisarSiReinaNegraEnemigaLlegó(posicionActual, this);
        }

        BoardManagerGlobal.Instance?.RegistrarMovimiento(this, nuevaPos);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ Reina actualizó su posición lógica a {nuevaPos}");
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();

    }

    public Vector2Int GetPosicionActual() => posicionActual;

    public void OnPointerClick(PointerEventData eventData)
    {
        var manager = FindFirstObjectByType<ChessGameManager>();


        if (manager == null || !manager.IsJuegoActivo())
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("♛ No se puede mostrar rango: juego no activo.");
            BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
            return;
        }

        mostrandoRango = !mostrandoRango;

        if (mostrandoRango)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("♛ Mostrando rango de ataque (Tiles)");
            MostrarRangoDeAtaque();
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("♛ Ocultando rango de ataque (Tiles)");
            OcultarRangoDeAtaque();
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    private void RevisarAmenazaAPieza(Vector2Int posicionPieza, System.Action efectoSobrePieza)
    {
        int dx = posicionPieza.x - posicionActual.x;
        int dy = posicionPieza.y - posicionActual.y;

        bool esDireccionValida = dx == 0 || dy == 0 || Mathf.Abs(dx) == Mathf.Abs(dy);
        if (!esDireccionValida) return;

        // 🛑 NUEVO: cancelamos amenaza si hay obstáculo entre Reina y la ficha
        if (HayObstaculoEntre(posicionActual, posicionPieza))
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🛡️ Amenaza bloqueada por obstáculo entre Reina {posicionActual} y pieza {posicionPieza}");
            return;
        }

        // Si no hay obstáculo, aplicamos efecto
        if (Vector2Int.Distance(posicionActual, posicionPieza) <= rangoKillZone)
        {
            efectoSobrePieza.Invoke();
        }
        else
        {
            efectoSobrePieza.Invoke();
        }
    }


    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        RevisarAmenazaAPieza(posicionRey, () =>
        {
            if (Vector2Int.Distance(posicionActual, posicionRey) <= rangoKillZone)
                StartCoroutine(MatarPiezaDespuesDelay(rey, posicionRey));

        });
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        RevisarAmenazaAPieza(posicionPeon, () =>
        {
            if (Vector2Int.Distance(posicionActual, posicionPeon) <= rangoKillZone)
                StartCoroutine(MatarPiezaDespuesDelay(peon, posicionPeon));

        });
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }



    private IEnumerator MatarPiezaDespuesDelay(MonoBehaviour pieza, Vector2Int posicion)
    {
        // Movimiento lógico
        SetPosicionActual(posicion);

        if (TryGetComponent<MovableTileObject>(out var movable))
            movable.tileCoords = posicion;

        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(posicion);

        // Exiliar pieza víctima
        if (pieza is IPieceWithPosition piezaVictima)
            piezaVictima.SetPosicionActual(new Vector2Int(-7, -7));

        if (pieza is PawnController peon)
        {
            peon.OcultarMovimientos();
            peon.mostrandoMovimientos = false;
        }

        Destroy(pieza.gameObject);

        BoardManagerGlobal.Instance.AgregarMensajeInterno(
            $"💀 {pieza.name} ejecutado por la Reina Negra en {posicion}"
        );

        // 🔹 Marcar que la ReinaNegra atacó
        BoardManagerGlobal.Instance.reinaNegraAtaco = true;

        yield return new WaitForSeconds(1f);
    }



    public void MostrarRangoDeAtaque()
    {
        OcultarRangoDeAtaque();

        Tile tileCentral = BoardManagerGlobal.Instance.GetTileAt(posicionActual);
        if (tileCentral != null)
            tileCentral.HighlightEnemyKillZone(true); // Casilla de la Reina

        Vector2Int[] direcciones = {
        new Vector2Int(1,0), new Vector2Int(-1,0),
        new Vector2Int(0,1), new Vector2Int(0,-1),
        new Vector2Int(1,1), new Vector2Int(-1,1),
        new Vector2Int(1,-1), new Vector2Int(-1,-1)
    };

        foreach (var dir in direcciones)
        {
            for (int i = 1; i <= rangoRangeZone; i++)
            {
                Vector2Int coord = posicionActual + dir * i;
                if (coord.x < 0 || coord.y < 0 || coord.x > 7 || coord.y > 7)
                    break;

                var objetos = BoardManagerGlobal.Instance.ObtenerObjetosEn(coord);

                // 💡 NUEVO: Bloqueo visual si hay cualquier otra ficha enemiga
                bool bloqueVisual = objetos.Any(obj =>
                obj is IFichaEnemiga && (Object)obj != this);

                if (bloqueVisual)
                {
                    BoardManagerGlobal.Instance.AgregarMensajeInterno($"👁️ Reina no colorea {coord} (ocupado por otra enemiga)");
                    break; // 🔺 No pinta ni sigue la línea
                }

                Tile tile = BoardManagerGlobal.Instance.GetTileAt(coord);
                if (tile == null) break;

                if (i <= rangoKillZone)
                    tile.HighlightEnemyKillZone(true);
                else
                    tile.HighlightEnemyRangeZone(true);

                // 🛑 Obstáculo que detiene visión (fichas o recolectables)
                bool hayObstaculo = objetos.Any(obj =>
                    (obj is IFicha && obj != (object)this) || obj is IObjetoRecoleccionable);

                if (hayObstaculo)
                {
                    BoardManagerGlobal.Instance.AgregarMensajeInterno($"🛑 Visión bloqueada por {objetos.First()} en {coord}");
                    break;
                }
            }
        }
    }



    public void OcultarRangoDeAtaque()
    {
        foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
            tile.ResetColor();
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        RevisarAmenazasEnZona();
    }

    public void RevisarAmenazasEnZona()
    {
        StartCoroutine(ProcesarAmenazasDesdeArbitro());
    }


    private void RevisarObjetosRecoleccionablesEnCasilla()
    {
        // ✅ Ahora revisa con el árbitro silencioso
        foreach (var objeto in BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual))
        {
            if (objeto is IObjetoRecoleccionable)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"💥 Reina destruye objeto {objeto} en {posicionActual}");
                if (objeto is Potion1PM pocion)
                    pocion.ExiliarADimensionDivina();

                Destroy(((MonoBehaviour)objeto).gameObject);
            }
        }
    }

    public void MostrarRango()
    {
        MostrarRangoDeAtaque();
    }

    public void OcultarRango()
    {
        OcultarRangoDeAtaque();
    }

    public void RevisarSiFichaLlegó(Vector2Int posicionFicha, IFicha ficha)
    {
        if (posicionFicha != posicionActual) return;

        foreach (var objeto in BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual))
        {
            if (objeto is IObjetoRecoleccionable)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ Reina destruye objeto {objeto} porque ficha {ficha} lo trajo encima");
                if (objeto is Potion1PM pocion)
                    pocion.ExiliarADimensionDivina();

                Destroy(((MonoBehaviour)objeto).gameObject);
            }
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public bool EsInamovible()
    {
        return esInamovible;
    }

    public void ReiniciarTurno()
    {

        rangoKillZone = 7;
        rangoRangeZone = 7;
    }


    public void RevisarSiReinaNegraEnemigaLlegó(Vector2Int posicion, BlackQueenEnemyController reinanegraenemiga)
    {
        // 🚫 Evitar bucles infinitos
        if (reinanegraenemiga == this) return; // No procesar a sí misma


        // ✅ Solo activar efectos de la casilla (no otras Reinas)
        foreach (var objeto in BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual))
        {
            if (objeto is ITileEffect efecto && !(objeto is IFichaEnemiga))
            {
                efecto.RevisarSiReinaNegraEnemigaLlegó(posicion, this);
                BoardManagerGlobal.Instance.AgregarMensajeInterno(
                    $"♛ TileEffect activado por llegada de la Reina a {posicion}"
                );
            }
        }
    }

    private bool HayObstaculoEntre(Vector2Int origen, Vector2Int destino)
    {
        int dx = destino.x - origen.x;
        int dy = destino.y - origen.y;

        if (!(dx == 0 || dy == 0 || Mathf.Abs(dx) == Mathf.Abs(dy)))
            return false;

        Vector2Int direccion = new Vector2Int(
            dx == 0 ? 0 : (dx > 0 ? 1 : -1),
            dy == 0 ? 0 : (dy > 0 ? 1 : -1)
        );

        Vector2Int paso = origen + direccion;
        while (paso != destino)
        {
            if (paso.x < 0 || paso.y < 0 || paso.x > 7 || paso.y > 7)
                break;

            var objetos = BoardManagerGlobal.Instance.ObtenerObjetosEn(paso);
            bool hayObstaculo = objetos.Any(obj =>
            (obj is IFicha && obj != (object)this) || obj is IObjetoRecoleccionable
            );

            if (hayObstaculo)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔰 Obstáculo detectado en {paso}. Línea bloqueada.");
                return true;
            }

            paso += direccion;
        }

        return false;
    }


    private IEnumerator ProcesarAmenazasDesdeArbitro()
    {

        // 🛑 Comprobación de autorización global del Árbitro
        int idMovimiento = BoardManagerGlobal.Instance.idMovimientoActual;


        // Si ya no tiene rango letal, no hace nada
        if (rangoKillZone <= 0)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("♛ Reina Roja no tiene energía letal este turno.");
            yield break;
        }

        // Direcciones absolutas de ajedrez real
        Vector2Int[] direcciones = new Vector2Int[]
        {
        new Vector2Int(1,0),   // Este
        new Vector2Int(-1,0),  // Oeste
        new Vector2Int(0,1),   // Norte
        new Vector2Int(0,-1),  // Sur
        new Vector2Int(1,1),   // NE
        new Vector2Int(-1,1),  // NO
        new Vector2Int(1,-1),  // SE
        new Vector2Int(-1,-1), // SO
        };

        MonoBehaviour objetivoElegido = null;
        Vector2Int posicionObjetivo = new Vector2Int(-1, -1);

        // 1️⃣ Buscar primer objetivo válido
        foreach (var dir in direcciones)
        {
            Vector2Int paso = posicionActual;

            for (int i = 1; i <= rangoRangeZone; i++)
            {
                paso += dir;

                if (paso.x < 0 || paso.y < 0 || paso.x > 7 || paso.y > 7)
                    break;

                var objetos = BoardManagerGlobal.Instance.ObtenerObjetosEn(paso);

                var fichaAliada = objetos.OfType<IFichaAliada>().FirstOrDefault();
                if (fichaAliada != null)
                {
                    objetivoElegido = (MonoBehaviour)fichaAliada;
                    posicionObjetivo = paso;
                    break;
                }

                bool hayObstaculo = objetos.Any(obj =>
                    (obj is IFicha && obj != (object)this) || obj is IObjetoRecoleccionable
                );
                if (hayObstaculo)
                    break;
            }

            if (objetivoElegido != null)
                break;
        }

        // 2️⃣ Si no hay objetivo, solo limpia casilla
        if (objetivoElegido == null)
        {
            RevisarObjetosRecoleccionablesEnCasilla();
            yield break;
        }

        // 3️⃣ Ataque o penalización
        float distancia = Vector2Int.Distance(posicionActual, posicionObjetivo);

        if (distancia <= rangoKillZone)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"💥 Reina Roja mata a {objetivoElegido.name} en {posicionObjetivo}");
            yield return StartCoroutine(MatarPiezaDespuesDelay(objetivoElegido, posicionObjetivo));

            //rangoKillZone = 0;
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🩸 Reina Roja ejecutó su presa y se detiene.");

        }
        else
        {
            if (objetivoElegido is PawnController peon)
                peon.AumentarRangoMovimiento(-2);
            else if (objetivoElegido is KingController rey)
            {
                rey.GanarPuntoMovimiento(-2);
                rey.puntosAccionActual -= 2;
            }

            BoardManagerGlobal.Instance.AgregarMensajeInterno(
                $"⚡ Reina Roja penaliza a {objetivoElegido.name} en {posicionObjetivo}"
            );
        }

        RevisarObjetosRecoleccionablesEnCasilla();
        yield break;
    }


    public void ProcesarMovimientoAliado(Vector2Int posAliada, int idMovimiento)
    {
        // ✅ Si llega aquí, es la atacante autorizada
        RevisarAmenazasEnZona();
    }
    public void RevisarSiReinaEnemigaLlegó(Vector2Int posicion, QueenEnemyController reinaenemiga)
    {
        //RevisarSiReinaEnemigaLlegó(posicion, rey);
    }
    public void RevisarSiTorreNegraEnemigaLlegó(Vector2Int posicion, BlackRookEnemyController torrenegraenemiga)
    {
        //
    }
    public void RevisarSiAlfilNegroEnemigoLlegó(Vector2Int posicion, BlackBishopEnemyController alfilnegroenemigo)
    {
        //
    }
    public void RevisarSiCaballoNegroEnemigoLlegó(Vector2Int posicion, BlackKnightEnemyController caballonegroenemigo)
    {
        //
    }
}