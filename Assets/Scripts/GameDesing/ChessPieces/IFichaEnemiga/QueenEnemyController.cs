using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class QueenEnemyController : MonoBehaviour, IPointerClickHandler, ITileEffect, IPieceWithPosition, IFicha, IFichaEnemiga
{
    [Header("Alcances tipo Reina")]
    public int rangoKillZone { get; set; } = 3;

    public bool esInamovible = false;
    public int rangoRangeZone { get; set; } = 5;


    public bool ataquesConcatenados = false;

    [Header("Prefab para zonas peligrosas")]
    public GameObject prefabRojo;

    [Header("Padre para overlays")]
    public Transform dangerOverlayParent;

    private Vector2Int posicionActual;
    private bool mostrandoRango = false;

    private List<GameObject> overlaysInstanciados = new List<GameObject>();
    private Vector2Int ultimaPosicionAmenaza = new Vector2Int(-99, -99);

    private void Start()
    {
        PiecePositioner piecePositioner = GetComponent<PiecePositioner>();
        if (piecePositioner != null)
        {
            posicionActual = piecePositioner.tileCoords;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ Reina inició en {posicionActual}");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ No hay PiecePositioner en la Reina. Usando (0,0).");
            posicionActual = new Vector2Int(0, 0);
        }

        // ✅ Registrar manualmente si aún no está registrada
        var objetosEnCasilla = BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual);
        bool yaRegistrada = objetosEnCasilla.Contains(this);

        if (!yaRegistrada)
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
            efecto.RevisarSiReinaEnemigaLlegó(posicionActual, this);
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

        bool esDireccionValida = (dx == 0 || dy == 0 || Mathf.Abs(dx) == Mathf.Abs(dy));
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
            else
            {
                rey.GanarPuntoMovimiento(-2);
                rey.puntosAccionActual -= 2;
                rey.turnosRestantes -= 1;
                BoardManagerGlobal.Instance.AgregarMensajeInterno("♛ Reina aplicó penalización al Rey por estar en zona de amenaza");
            }
        });
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        RevisarAmenazaAPieza(posicionPeon, () =>
        {
            if (Vector2Int.Distance(posicionActual, posicionPeon) <= rangoKillZone)
                StartCoroutine(MatarPiezaDespuesDelay(peon, posicionPeon));
            else
            {
                peon.AumentarRangoMovimiento(-2);
                BoardManagerGlobal.Instance.AgregarMensajeInterno("♛ Reina aplicó penalización al Peón por estar en zona de amenaza");
            }
        });
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    private IEnumerator MatarPiezaDespuesDelay(MonoBehaviour pieza, Vector2Int posicion)
    {
        SetPosicionActual(posicion);
        if (TryGetComponent<MovableTileObject>(out var movable))
            movable.tileCoords = posicion;

        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(posicion);

        if (pieza is IPieceWithPosition piezaVictima)
            piezaVictima.SetPosicionActual(new Vector2Int(-1, -1));

        if (pieza is PawnController peon)
        {
            peon.OcultarMovimientos();
            peon.mostrandoMovimientos = false;
        }

        Destroy(pieza.gameObject);

        BoardManagerGlobal.Instance.AgregarMensajeInterno($"💀 {pieza.name} ejecutado por la Reina en {posicion}");

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

    public void VerificarAmenazaSobre(Vector2Int posicionPieza)
    {
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ [DEBUG] Iniciando VerificarAmenazaSobre hacia {posicionPieza}");

        // Limpieza de overlays anteriores
        foreach (var obj in overlaysInstanciados)
            Destroy(obj);
        overlaysInstanciados.Clear();

        int dx = posicionPieza.x - posicionActual.x;
        int dy = posicionPieza.y - posicionActual.y;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ [DEBUG] Diferencia dx: {dx}, dy: {dy}");

        bool esDireccionValida = dx == 0 || dy == 0 || Mathf.Abs(dx) == Mathf.Abs(dy);
        if (!esDireccionValida)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("♛ [ABORTADO] Dirección no válida para ataque (no es línea recta ni diagonal)");
            ultimaPosicionAmenaza = new Vector2Int(-99, -99);
            return;
        }

        Vector2Int direccion = new Vector2Int(
            dx == 0 ? 0 : (dx > 0 ? 1 : -1),
            dy == 0 ? 0 : (dy > 0 ? 1 : -1)
        );

        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ [DEBUG] Dirección calculada: {direccion}");

        Vector2Int paso = posicionActual;
        int pasosContados = 0;
        List<Vector2Int> lineaDeAtaque = new List<Vector2Int>();

        while (pasosContados <= rangoRangeZone && paso != posicionPieza)
        {
            paso += direccion;
            pasosContados++;

            if (paso.x < 0 || paso.y < 0 || paso.x > 7 || paso.y > 7)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ [ABORTADO] Paso fuera del tablero en {paso}");
                break;
            }

            var objetosEnPaso = BoardManagerGlobal.Instance.ObtenerObjetosEn(paso, incluirRecolectables: false);
            bool hayObstaculo = objetosEnPaso.Any(obj => (obj is IFicha && obj != (object)this) || obj is IObjetoRecoleccionable);

            if (hayObstaculo)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ [BLOQUEADO] Objeto detectado en {paso}, línea interrumpida.");
                break;
            }

            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ [DEBUG] Añadiendo paso a línea: {paso}");
            lineaDeAtaque.Add(paso);
        }

        if (paso == posicionPieza)
        {
            if (HayObstaculoEntre(posicionActual, posicionPieza))
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ [BLOQUEO] Prefabs no instanciados. Obstáculo entre Reina y {posicionPieza}");
                ultimaPosicionAmenaza = new Vector2Int(-99, -99);
                return;
            }

            lineaDeAtaque.Insert(0, posicionActual);
            lineaDeAtaque.Add(posicionPieza);

            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ [ÉXITO] Se alcanzó {posicionPieza}. Instanciando prefabs de peligro.");

            foreach (Vector2Int coord in lineaDeAtaque)
            {
                GameObject overlay = Instantiate(prefabRojo, dangerOverlayParent);
                overlay.GetComponent<RectTransform>().anchoredPosition =
                    BoardManagerGlobal.Instance.GetTileAnchoredPosition(coord);
                overlaysInstanciados.Add(overlay);
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ [PREFAB] Overlay rojo en {coord}");
            }

            ultimaPosicionAmenaza = posicionPieza;
        }


        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
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

        rangoKillZone = 3;
        rangoRangeZone = 5;
    }


    public void RevisarSiReinaEnemigaLlegó(Vector2Int posicion, QueenEnemyController reinaenemiga)
    {
        if (posicion != posicionActual) return;

        foreach (var objeto in BoardManagerGlobal.Instance.ObtenerObjetosEn(posicionActual))
        {
            if (objeto is ITileEffect efecto)
            {
                // Llamada a sí mismo o a otros efectos
                efecto.RevisarSiReinaEnemigaLlegó(posicion, this);
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ TileEffect activado por llegada de la Reina a {posicion}");
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
    for (int x = 0; x <= 7; x++)
    {
        for (int y = 0; y <= 7; y++)
        {
            Vector2Int posObjetivo = new Vector2Int(x, y);
            if (posObjetivo == posicionActual)
                continue;

            int dx = posObjetivo.x - posicionActual.x;
            int dy = posObjetivo.y - posicionActual.y;

            bool esDireccionValida = dx == 0 || dy == 0 || Mathf.Abs(dx) == Mathf.Abs(dy);
            if (!esDireccionValida)
                continue;

            int distancia = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
            if (distancia > rangoRangeZone)
                continue;

            if (HayObstaculoEntre(posicionActual, posObjetivo))
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧱 Obstáculo entre Reina y {posObjetivo}, se cancela ataque.");
                continue;
            }

            var objetos = BoardManagerGlobal.Instance.ObtenerObjetosEn(posObjetivo);

            foreach (var obj in objetos)
            {
                if (obj is IFichaAliada && obj != (object)this)
                {
                    bool asesinatoEjecutado = false;

                    RevisarAmenazaAPieza(posObjetivo, () =>
                    {
                        if (distancia <= rangoKillZone)
                        {
                            StartCoroutine(MatarPiezaDespuesDelay((MonoBehaviour)obj, posObjetivo));
                            asesinatoEjecutado = true;
                        }
                    });

                    if (asesinatoEjecutado)
                    {
                        if (ataquesConcatenados)
                        {
                            BoardManagerGlobal.Instance.AgregarMensajeInterno("⏳ Reina Negra pausa tras asesinato.");
                            yield return new WaitForSeconds(1f);
                        }
                        else
                        {
                            BoardManagerGlobal.Instance.AgregarMensajeInterno("🛑 Reina Roja detiene su cacería tras el primer asesinato.");
                            yield break;
                        }
                    }
                }
            }
        }
    }

    RevisarObjetosRecoleccionablesEnCasilla();
    }


}