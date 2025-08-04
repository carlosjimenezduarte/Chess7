using UnityEngine;

public class PusherUp : MonoBehaviour, ITileEffect, IFichaInmovil, IFicha, IPieceWithPosition
{
    public Vector2Int tileCoords;
    public bool esInamovible = true;

    private void Start()
    {
        var pos = GetComponent<PiecePositioner>();
        if (pos != null)
        {
            tileCoords = pos.tileCoords;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"📦 PusherUp activo en {tileCoords}.");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"⚠️ PusherUp sin PiecePositioner.");
        }

        BoardManagerGlobal.Instance?.ReportarFichaInamovible(this);
    }

    public void RevisarSiFichaLlegó(Vector2Int posicion, IFicha ficha)
    {
        if (posicion == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚀 {((MonoBehaviour)ficha).name} activó PusherUp en {tileCoords}.");
            ActivarEmpuje((MonoBehaviour)ficha);
            BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
        }
    }

    private void ActivarEmpuje(MonoBehaviour activador)
    {
        if (!activador.TryGetComponent<MovableTileObject>(out var movible))
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"❌ {activador.name} no es movible. Empuje cancelado.");
            return;
        }

        Vector2Int origen = movible.tileCoords;
        Vector2Int delta = origen - tileCoords;
        Vector2Int direccion = CalcularDireccion(delta);
        Vector2Int destinoFinal = origen;

        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧭 Dirección de empuje: {direccion}.");

        for (int i = 0; i < 3; i++)
        {
            Vector2Int siguiente = destinoFinal + direccion;

            if (!EsDentroTablero(siguiente))
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚫 {movible.name} no puede empujarse fuera del tablero hacia {siguiente}.");
                break;
            }

            if (BoardManagerGlobal.Instance.EstaCasillaOcupada(siguiente))
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧱 Casilla {siguiente} ocupada. {movible.name} se detiene aquí.");
                break;
            }

            // ACTUALIZACIÓN LÓGICA (ALMA)
            movible.tileCoords = siguiente;

            // ACTUALIZACIÓN VISUAL (CUERPO)
            movible.transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(siguiente);

            // ACTUALIZACIÓN INTERNA (ESPÍRITU)
            if (movible.TryGetComponent<IPieceWithPosition>(out var pieza))
            {
                pieza.SetPosicionActual(siguiente);
            }

            if (movible.TryGetComponent<PawnController>(out var peon))
            {
                peon.SetPosicionActual(siguiente);
            }

            destinoFinal = siguiente;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"➡ {movible.name} empujado a {siguiente}.");
        }
    }

    private Vector2Int CalcularDireccion(Vector2Int delta)
    {
        if (delta == Vector2Int.zero) return Vector2Int.up;

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
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♔ Rey llegó a PusherUp en {tileCoords}.");
            ActivarEmpuje(rey);
        }
    }

    public void RevisarSiReinaEnemigaLlegó(Vector2Int posicion, QueenEnemyController reina)
    {
        if (posicion == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"👑 Reina llegó a PusherUp en {tileCoords}.");
            ActivarEmpuje(reina);
        }
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        if (posicionPeon == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♙ Peón llegó a PusherUp en {tileCoords}.");
            ActivarEmpuje(peon);
        }
    }

    public void RevisarSiReinaNegraEnemigaLlegó(Vector2Int posicion, BlackQueenEnemyController reinaenemiga)
    {
        //RevisarSiReinaEnemigaLlegó(posicion, rey);
    }
    public void RevisarSiFichaAliadaLlegó(Vector2Int posicionFicha, IFichaAliada ficha)
    {
     //StartCoroutine(ProcesarLlegadaFichaAliada(posicionFicha, ficha)); 
    }
}
