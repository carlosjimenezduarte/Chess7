using UnityEngine;
using System.Collections.Generic;

public class PusherUp : MonoBehaviour, IFicha, IFichaInmovil, IPieceWithPosition
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
            IntentarEmpujar(rey.gameObject);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        if (posicionPeon == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♙ Peón activó PusherUp en {tileCoords}.");
            IntentarEmpujar(peon.gameObject);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RevisarSiFichaLlegó(Vector2Int posicionFicha, IFicha ficha)
    {
        string nombre = ((MonoBehaviour)ficha).name;
        if (posicionFicha == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🔼 PusherUp activado por {nombre} en {tileCoords}.");
            IntentarEmpujar(((MonoBehaviour)ficha).gameObject);
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"❌ {nombre} no activó PusherUp: estaba en {posicionFicha}.");
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    private void IntentarEmpujar(GameObject ficha)
    {
        var movible = ficha.GetComponent<MovableTileObject>();
        if (movible == null)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"⚠️ {ficha.name} no tiene MovableTileObject. No se puede empujar.");
            return;
        }

        if (movible.esInamovible)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🪨 {ficha.name} es inamovible. No será empujado.");
            return;
        }

        Vector2Int direccion = new Vector2Int(0, 1); // Dirección fija: arriba
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
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🪨 {ficha.name} no puede ser empujado: sin camino.");
            return;
        }

        Vector2Int destinoFinal = caminoLibre[caminoLibre.Count - 1];

        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🎯 {ficha.name} será empujado desde {tileCoords} hasta {destinoFinal} ({caminoLibre.Count} casillas).");

        movible.MoverA(destinoFinal);
    }

    private bool EsDentroTablero(Vector2Int p)
        => p.x >= 0 && p.x <= 7 && p.y >= 0 && p.y <= 7;

    public bool EsInamovible() => esInamovible;

    public void SetPosicionActual(Vector2Int nuevaPos) => tileCoords = nuevaPos;

    public Vector2Int GetPosicionActual() => tileCoords;

    public void VerificarTurnoActual(int turnoActual) { }
}
