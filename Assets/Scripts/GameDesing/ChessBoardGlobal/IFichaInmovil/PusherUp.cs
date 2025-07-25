using UnityEngine;
using System.Collections.Generic;

public class PusherUp : MonoBehaviour, ITileEffect, IFicha, IFichaInmovil, IPieceWithPosition
{
    public Vector2Int tileCoords;
    public bool esInamovible = false;
    public int distanciaEmpuje = 3;

    private void Start()
    {
        var posicionador = GetComponent<PiecePositioner>();
        if (posicionador != null)
        {
            tileCoords = posicionador.tileCoords;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚀 PusherUp inició en {tileCoords}");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"⚠️ PusherUp sin PiecePositioner.");
        }

        BoardManagerGlobal.Instance?.ReportarFichaInamovible(this);
    }

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        if (posicionRey == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♔ Rey activó PusherUp en {tileCoords}.");
            IntentarEmpujar(rey);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        if (posicionPeon == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♙ Peón activó PusherUp en {tileCoords}.");
            IntentarEmpujar(peon);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RevisarSiFichaLlegó(Vector2Int posicionFicha, IFicha ficha)
    {
        string nombre = ((MonoBehaviour)ficha).name;
        if (posicionFicha == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔼 PusherUp activado por {nombre} en {tileCoords}.");
            IntentarEmpujar((MonoBehaviour)ficha);
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"❌ {nombre} no activó PusherUp: estaba en {posicionFicha}.");
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    private void IntentarEmpujar(MonoBehaviour ficha)
    {
        var fichaMovible = ficha.GetComponent<MovableTileObject>();
        if (fichaMovible == null)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"⚠️ {ficha.name} no tiene MovableTileObject. No se puede empujar.");
            return;
        }

        Vector2Int direccion = new Vector2Int(0, 1); // Dirección fija: arriba
        Vector2Int paso = tileCoords;
        List<Vector2Int> caminoLibre = new List<Vector2Int>();

        for (int i = 1; i <= distanciaEmpuje; i++)
        {
            Vector2Int siguiente = tileCoords + direccion * i;
            if (!EsDentroTablero(siguiente)) break;
            if (BoardManagerGlobal.Instance.EstaCasillaOcupada(siguiente)) break;

            caminoLibre.Add(siguiente);
        }

        if (caminoLibre.Count == 0)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🪨 PusherUp no pudo empujar a {ficha.name}. No hay camino libre.");
            return;
        }

        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🎯 {ficha.name} será empujado {caminoLibre.Count} casilla(s) hacia arriba.");

        foreach (var destino in caminoLibre)
        {
            fichaMovible.MoverA(destino);
        }
    }

    private bool EsDentroTablero(Vector2Int p)
        => p.x >= 0 && p.x <= 7 && p.y >= 0 && p.y <= 7;

    public bool EsInamovible() => esInamovible;

    public void SetPosicionActual(Vector2Int nuevaPos) => tileCoords = nuevaPos;

    public Vector2Int GetPosicionActual() => tileCoords;

    public void VerificarTurnoActual(int turnoActual) { }
}
