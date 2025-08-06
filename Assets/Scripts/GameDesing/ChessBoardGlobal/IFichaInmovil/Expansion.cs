using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Expansion : MonoBehaviour, ITileEffect, IFicha, IFichaInmovil, IPieceWithPosition
{
    public Vector2Int tileCoords;

    private bool esInamovible = false;

    private void Start()
    {
        var posicionador = GetComponent<PiecePositioner>();
        if (posicionador != null)
        {
            tileCoords = posicionador.tileCoords;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧩 Expansion inició en {tileCoords}");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"⚠️ Expansion sin PiecePositioner. tileCoords no inicializado.");
        }
        if (BoardManagerGlobal.Instance != null)
        {
            // ✅ Reporte manual al Árbitro Silencioso
            BoardManagerGlobal.Instance.ReportarFichaInamovible(this);
        }

    }
    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        if (posicionRey == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♔ Rey llegó a casilla con Expansion en {tileCoords}.");
            ActivarExpansion(rey);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RevisarSiReinaEnemigaLlegó(Vector2Int posicion, QueenEnemyController reinaenemiga)
    {
        if (posicion == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♔ Rey llegó a casilla con Expansion en {tileCoords}.");
            ActivarExpansion(reinaenemiga);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();

    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        if (posicionPeon == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♙ Peón llegó a casilla con Expansion en {tileCoords}.");
            ActivarExpansion(peon);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RevisarSiFichaLlegó(Vector2Int posicionFicha, IFicha ficha)
    {
        var nombreFicha = ((MonoBehaviour)ficha).name;
        if (posicionFicha == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧩 Expansion activado por {nombreFicha} en {tileCoords}.");
            ActivarExpansion((MonoBehaviour)ficha);
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"❌ Ficha {nombreFicha} no activó Expansion: estaba en {posicionFicha}, no en {tileCoords}.");
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void VerificarTurnoActual(int turnoActual) { }

    private void ActivarExpansion(MonoBehaviour activador)
    {
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"💢 Expansion en {tileCoords} se activa por {activador.name}.");

        List<MovableTileObject> todos = BoardManagerGlobal.Instance.GetObjetosMoviblesOrdenadosDesde(tileCoords);

        foreach (var obj in todos)
        {
            if (EsIgnorable(obj, activador))
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🛑 {obj.name} fue ignorado por Expansion.");
                continue;
            }

            if (!EsDentroTablero(obj.tileCoords))
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🌌 {obj.name} está fuera del tablero en {obj.tileCoords}.");
                continue;
            }

            Vector2Int dir = CalcularDireccion(obj.tileCoords - tileCoords);
            Vector2Int destino = obj.tileCoords + dir;

            if (!EsDentroTablero(destino))
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚫 {obj.name} no puede empujarse fuera del tablero hacia {destino}.");
                continue;
            }

            BoardManagerGlobal.Instance.AgregarMensajeInterno($"💥 {obj.name} empujado de {obj.tileCoords} a {destino}.");
            obj.MoverA(destino);
        }
    }

    private bool EsIgnorable(MovableTileObject obj, MonoBehaviour activador)
    {
        if (obj.tileCoords == tileCoords) return true;

        if (activador != null && ReferenceEquals(obj.gameObject, activador.gameObject))
            return true;

        return false;
    }

    private bool EstaVisibleYRecolectable(MovableTileObject obj)
    {
        // Aquí puedes filtrar por clase si necesitas más adelante.
        return true;
    }

    private Vector2Int CalcularDireccion(Vector2Int delta)
    {
        if (delta.x == 0 && delta.y == 0) return Vector2Int.zero;

        if (Mathf.Abs(delta.x) == Mathf.Abs(delta.y))
            return new Vector2Int((int)Mathf.Sign(delta.x), (int)Mathf.Sign(delta.y));
        if (delta.y == 0)
            return new Vector2Int((int)Mathf.Sign(delta.x), 0);
        if (delta.x == 0)
            return new Vector2Int(0, (int)Mathf.Sign(delta.y));

        return Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
            ? new Vector2Int((int)Mathf.Sign(delta.x), 0)
            : new Vector2Int(0, (int)Mathf.Sign(delta.y));
    }

    private bool EsDentroTablero(Vector2Int p)
        => p.x >= 0 && p.x <= 7 && p.y >= 0 && p.y <= 7;

    public bool EsInamovible()
    {
        return esInamovible;
    }

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        tileCoords = nuevaPos;
    }

    public Vector2Int GetPosicionActual()
    {
        return tileCoords;
    }

    public void RevisarSiReinaNegraEnemigaLlegó(Vector2Int posicion, BlackQueenEnemyController reinaenemiga)
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

    public void RevisarSiTorreEnemigaLlegó(Vector2Int posicion, RookEnemyController torreenemiga)
    {
        //
    }

    public void RevisarSiAlfilEnemigoLlegó(Vector2Int posicion, BishopEnemyController alfilenemigo)
    {
        //
    }

    public void RevisarSiCaballoEnemigoLlegó(Vector2Int posicion, KnightEnemyController caballoenemigo)
    {
        //
    }
    public void RevisarSiPeonEnemigoLlegó(Vector2Int posicion, PawnEnemyController peonenemigo)
    {
        
    }
} 