using UnityEngine;
using System.Collections.Generic;

public class PusherUp : MonoBehaviour, ITileEffect, IFichaInmovil, IFicha, IPieceWithPosition
{
    public Vector2Int tileCoords;
    public bool esInamovible = true;

    private void Start()
    {
        var posicionador = GetComponent<PiecePositioner>();
        if (posicionador != null)
        {
            tileCoords = posicionador.tileCoords;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"📦 PusherUp activo en {tileCoords}");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"⚠️ PusherUp sin PiecePositioner en escena.");
        }

        if (BoardManagerGlobal.Instance != null)
            BoardManagerGlobal.Instance.ReportarFichaInamovible(this);
    }

    public void RevisarSiFichaLlegó(Vector2Int posicion, IFicha ficha)
    {
        if (posicion != tileCoords)
            return;

        var nombreFicha = ((MonoBehaviour)ficha).name;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚀 {nombreFicha} activó PusherUp en {tileCoords}.");
        ActivarEmpuje((MonoBehaviour)ficha);
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    private void ActivarEmpuje(MonoBehaviour activador)
    {
        if (activador.TryGetComponent<MovableTileObject>(out var movible))
        {
            Vector2Int origen = movible.tileCoords;
            Vector2Int delta = origen - tileCoords;
            Vector2Int direccion = CalcularDireccion(delta);

            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧭 Dirección de empuje: {direccion}.");

            for (int i = 0; i < 3; i++)
            {
                Vector2Int siguiente = movible.tileCoords + direccion;

                if (!EsDentroTablero(siguiente))
                {
                    BoardManagerGlobal.Instance.AgregarMensajeInterno($"⛔ {movible.name} no puede moverse fuera del tablero.");
                    break;
                }

                movible.MoverA(siguiente);
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"➡️ {movible.name} empujado a {siguiente}.");
            }
        }
    }

    private Vector2Int CalcularDireccion(Vector2Int delta)
    {
        if (delta == Vector2Int.zero) return Vector2Int.up; // Por defecto hacia arriba

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

    public bool EsInamovible() => esInamovible;

    public void SetPosicionActual(Vector2Int nuevaPos) => tileCoords = nuevaPos;

    public Vector2Int GetPosicionActual() => tileCoords;

    public void VerificarTurnoActual(int turnoActual) { }

     public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        if (posicionRey == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♔ Rey llegó a casilla con Expansion en {tileCoords}.");
            ActivarEmpuje(rey);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RevisarSiReinaEnemigaLlegó(Vector2Int posicion, QueenEnemyController reinaenemiga)
    {
         if (posicion == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♔ Rey llegó a casilla con Expansion en {tileCoords}.");
            ActivarEmpuje(reinaenemiga);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
               
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        if (posicionPeon == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♙ Peón llegó a casilla con Expansion en {tileCoords}.");
            ActivarEmpuje(peon);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

}
