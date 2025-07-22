using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class QueenEnemyController : MonoBehaviour, IPointerClickHandler, ITileEffect, IPieceWithPosition, IFicha, IFichaEnemiga
{
    [Header("Alcances tipo Reina")]
    public int rangoKillZone = 3;

    public bool esInamovible = false;
    public int rangoRangeZone = 5;
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
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        posicionActual = nuevaPos;
        if (TryGetComponent<PiecePositioner>(out var piecePositioner))
            piecePositioner.tileCoords = nuevaPos;

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

        Vector2Int direccion = new Vector2Int(
            dx == 0 ? 0 : (dx > 0 ? 1 : -1),
            dy == 0 ? 0 : (dy > 0 ? 1 : -1)
        );

        Vector2Int paso = posicionActual + direccion;
        int pasosContados = 1;

        while (pasosContados <= rangoRangeZone && paso != posicionPieza)
        {
            if (paso.x < 0 || paso.y < 0 || paso.x > 7 || paso.y > 7)
                break;

            // ✅ NUEVO: la Reina barre su trayectoria destruyendo objetos recoleccionables
            foreach (var obj in BoardManagerGlobal.Instance.ObtenerObjetosEn(paso))
            {
                if (obj is IObjetoRecoleccionable)
                {
                    BoardManagerGlobal.Instance.AgregarMensajeInterno($"💥 Reina destruye {obj} en su trayectoria por {paso}");
                    if (obj is Potion1PM pocion) pocion.ExiliarADimensionDivina();
                    Destroy(((MonoBehaviour)obj).gameObject);
                }
            }

            paso += direccion;
            pasosContados++;
        }

        if (paso == posicionPieza)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"💀 Pieza alcanzada en {posicionPieza}");
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
            tileCentral.HighlightEnemyKillZone(true);

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

            // ⚠️ Revisión: si hay un Wall, se interrumpe
            var objetos = BoardManagerGlobal.Instance.ObtenerObjetosEn(coord);
            bool hayWall = objetos.Any(obj => obj is IFichaInmovil);

            if (hayWall)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧱 Rango de la Reina interrumpido por Wall en {coord}");
                break;
            }

            Tile tile = BoardManagerGlobal.Instance.GetTileAt(coord);
            if (tile == null) break;

            if (i <= rangoKillZone)
                tile.HighlightEnemyKillZone(true);
            else
                tile.HighlightEnemyRangeZone(true);
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
        StartCoroutine(ProcesarAmenazas());
    }

    private IEnumerator ProcesarAmenazas()
    {
        // ✅ NUEVO: recorre solo su rango en el tablero, con ayuda del árbitro
        Vector2Int[] direcciones = {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1),
            new Vector2Int(1,1), new Vector2Int(-1,1),
            new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };

        foreach (var dir in direcciones)
        {
            Vector2Int paso = posicionActual + dir;
            int pasosContados = 1;

            while (pasosContados <= rangoRangeZone)
            {
                if (paso.x < 0 || paso.y < 0 || paso.x > 7 || paso.y > 7)
                    break;

                foreach (var pieza in BoardManagerGlobal.Instance.ObtenerObjetosEn(paso))
                {
                    if (pieza is IFichaAliada && pieza != (object)this)
                    {
                        Vector2Int pos = pieza.GetPosicionActual();
                        bool asesinatoEjecutado = false;

                        RevisarAmenazaAPieza(pos, () =>
                        {
                            if (Vector2Int.Distance(posicionActual, pos) <= rangoKillZone)
                            {
                                StartCoroutine(MatarPiezaDespuesDelay(((MonoBehaviour)pieza), pos));
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

                paso += dir;
                pasosContados++;
            }
        }

        RevisarObjetosRecoleccionablesEnCasilla();
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
        foreach (var obj in overlaysInstanciados)
            Destroy(obj);
        overlaysInstanciados.Clear();

        int dx = posicionPieza.x - posicionActual.x;
        int dy = posicionPieza.y - posicionActual.y;

        bool esDireccionValida = (dx == 0 || dy == 0 || Mathf.Abs(dx) == Mathf.Abs(dy));
        if (!esDireccionValida)
        {
            ultimaPosicionAmenaza = new Vector2Int(-99, -99);
            return;
        }

        Vector2Int direccion = new Vector2Int(
            dx == 0 ? 0 : (dx > 0 ? 1 : -1),
            dy == 0 ? 0 : (dy > 0 ? 1 : -1)
        );

        Vector2Int paso = posicionActual;
        int pasosContados = 0;
        List<Vector2Int> lineaDeAtaque = new List<Vector2Int>();

        while (pasosContados <= rangoRangeZone && paso != posicionPieza)
        {
            if (paso.x < 0 || paso.y < 0 || paso.x > 7 || paso.y > 7)
                break;

            lineaDeAtaque.Add(paso);
            paso += direccion;
            pasosContados++;
        }

        if (paso == posicionPieza)
        {
            lineaDeAtaque.Add(posicionPieza);

            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♛ Pintando línea de prefabs hacia pieza en {posicionPieza}");

            foreach (Vector2Int coord in lineaDeAtaque)
            {
                GameObject overlay = Instantiate(prefabRojo, dangerOverlayParent);
                overlay.GetComponent<RectTransform>().anchoredPosition =
                    BoardManagerGlobal.Instance.GetTileAnchoredPosition(coord);
                overlaysInstanciados.Add(overlay);
            }

            ultimaPosicionAmenaza = posicionPieza;
        }
        else
        {
            ultimaPosicionAmenaza = new Vector2Int(-99, -99);
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

}
