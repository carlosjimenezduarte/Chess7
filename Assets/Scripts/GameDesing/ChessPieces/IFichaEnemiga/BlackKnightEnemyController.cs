using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BlackKnightEnemyController : MonoBehaviour, IPointerClickHandler, ITileEffect, IPieceWithPosition, IFicha, IFichaEnemiga
{
    [Header("Jerarquía de ataque")]
    public int rangoKillZone { get; set; } = 1; // Caballo solo ataca en su salto (1 salto = 1 ataque)
    public int rangoRangeZone { get; set; } = 2; // Solo revisa casillas en L

    public bool esInamovible = false;

    private Vector2Int posicionActual;
    private bool mostrandoRango = false;

    // Movimientos posibles del caballo (en L)
    private readonly Vector2Int[] movimientosL = new Vector2Int[]
    {
        new Vector2Int(1,2), new Vector2Int(2,1),
        new Vector2Int(-1,2), new Vector2Int(-2,1),
        new Vector2Int(1,-2), new Vector2Int(2,-1),
        new Vector2Int(-1,-2), new Vector2Int(-2,-1)
    };

    private void Start()
    {
        PiecePositioner piecePositioner = GetComponent<PiecePositioner>();
        if (piecePositioner != null)
        {
            posicionActual = piecePositioner.tileCoords;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♞ Caballo Negro inició en {posicionActual}");
        }
        else
        {
            posicionActual = new Vector2Int(0, 0);
            BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ No hay PiecePositioner en el Caballo Negro. Usando (0,0).");
        }

        BoardManagerGlobal.Instance.RegistrarMovimiento(this, posicionActual);
        BoardManagerGlobal.Instance.RegistrarFichaEnemiga(this);
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
            efecto.RevisarSiCaballoNegroEnemigoLlegó(posicionActual, null);

        BoardManagerGlobal.Instance.RegistrarMovimiento(this, nuevaPos);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♞ Caballo Negro actualizó su posición lógica a {nuevaPos}");
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public Vector2Int GetPosicionActual() => posicionActual;

    public void OnPointerClick(PointerEventData eventData)
    {
        var manager = FindFirstObjectByType<ChessGameManager>();
        if (manager == null || !manager.IsJuegoActivo())
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("♞ No se puede mostrar rango: juego no activo.");
            BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
            return;
        }

        mostrandoRango = !mostrandoRango;

        if (mostrandoRango)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("♞ Mostrando rango de ataque (L)");
            MostrarRangoDeAtaque();
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("♞ Ocultando rango de ataque");
            OcultarRangoDeAtaque();
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    private void RevisarAmenazaAPieza(Vector2Int posicionPieza, System.Action efectoSobrePieza)
    {
        // Solo ataca si la pieza está exactamente a un salto de caballo
        foreach (var delta in movimientosL)
        {
            if (posicionActual + delta == posicionPieza)
            {
                efectoSobrePieza.Invoke();
                return;
            }
        }
    }

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        RevisarAmenazaAPieza(posicionRey, () =>
        {
            StartCoroutine(MatarPiezaDespuesDelay(rey, posicionRey));
        });
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        RevisarAmenazaAPieza(posicionPeon, () =>
        {
            StartCoroutine(MatarPiezaDespuesDelay(peon, posicionPeon));
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
            piezaVictima.SetPosicionActual(BoardManagerGlobal.DimensionDivina);

        if (pieza is PawnController peon)
        {
            peon.OcultarMovimientos();
            peon.mostrandoMovimientos = false;
        }

        Destroy(pieza.gameObject);

        BoardManagerGlobal.Instance.AgregarMensajeInterno(
            $"💀 {pieza.name} ejecutado por el Caballo Negro en {posicion}"
        );

        // 🔹 Marcar ataque para jerarquía
        BoardManagerGlobal.Instance.reinaNegraAtaco = true;

        yield return new WaitForSeconds(1f);
    }

    public void MostrarRangoDeAtaque()
    {
        OcultarRangoDeAtaque();

        Tile tileCentral = BoardManagerGlobal.Instance.GetTileAt(posicionActual);
        if (tileCentral != null)
            tileCentral.HighlightEnemyKillZone(true);

        foreach (var delta in movimientosL)
        {
            Vector2Int coord = posicionActual + delta;
            if (coord.x < 0 || coord.y < 0 || coord.x > 7 || coord.y > 7)
                continue;

            Tile tile = BoardManagerGlobal.Instance.GetTileAt(coord);
            if (tile != null)
                tile.HighlightEnemyKillZone(true); // Siempre letal en su salto
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

    private IEnumerator ProcesarAmenazasDesdeArbitro()
    {
    int asesinatos = 0;

    // 1️⃣ Revisar todos los saltos posibles
    foreach (var delta in movimientosL)
    {
        Vector2Int destino = posicionActual + delta;
        if (destino.x < 0 || destino.y < 0 || destino.x > 7 || destino.y > 7)
            continue;

        var objetos = BoardManagerGlobal.Instance.ObtenerObjetosEn(destino);
        var fichaAliada = objetos.OfType<IFichaAliada>().FirstOrDefault();

        if (fichaAliada != null)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"💥 Caballo Negro mata a {((MonoBehaviour)fichaAliada).name} en {destino}");

            yield return StartCoroutine(MatarPiezaDespuesDelay((MonoBehaviour)fichaAliada, destino));

            asesinatos++;
            

            // 🔹 Si ya mató 7 fichas en un mismo barrido, se detiene
            if (asesinatos >= 3)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno("♞ Caballo Negro alcanzó su límite de 7 ejecuciones en este barrido.");
                break;
            }

            // 🔹 Pausa ligera entre asesinatos (opcional, para que no sea instantáneo)
            yield return new WaitForSeconds(0.2f);
        }
    }

    yield break;
    }

    public void ProcesarMovimientoAliado(Vector2Int posAliada, int idMovimiento)
    {
        RevisarAmenazasEnZona();
    }

    public void MostrarRango() => MostrarRangoDeAtaque();
    public void OcultarRango() => OcultarRangoDeAtaque();
    public bool EsInamovible() => esInamovible;
    public void ReiniciarTurno() { rangoKillZone = 1; rangoRangeZone = 2; }

    public void RevisarSiFichaLlegó(Vector2Int posicionFicha, IFicha ficha)
    {
        //
    }

    public void RevisarSiReinaEnemigaLlegó(Vector2Int posicion, QueenEnemyController reinaenemiga)
    {
        //
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

    public void RevisarSiReinaNegraEnemigaLlegó(Vector2Int posicion, BlackQueenEnemyController reinanegraenemiga)
    {
        //
    }

}
