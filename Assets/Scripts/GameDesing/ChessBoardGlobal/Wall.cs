using UnityEngine;

public class Wall : MonoBehaviour, IFicha, IFichaInmovil, IPieceWithPosition
{
    [Header("Coordenadas del muro")]
    public Vector2Int tileCoords;

    [Header("¿Debe registrarse como inamovible?")]
    public bool esInamovible = true;

    private void Start()
    {
        var posicionador = GetComponent<PiecePositioner>();
        if (posicionador != null)
        {
            tileCoords = posicionador.tileCoords;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧱 Wall iniciado en {tileCoords}");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ Wall sin PiecePositioner. Usando (0,0)");
            tileCoords = Vector2Int.zero;
        }

        // Reportar al Árbitro Silencioso como ficha inamovible
        if (BoardManagerGlobal.Instance != null)
        {
            BoardManagerGlobal.Instance.ReportarFichaInamovible(this);
        }
    }

    public bool EsInamovible()
    {
        return esInamovible;
    }

    public Vector2Int GetPosicionActual()
    {
        return tileCoords;
    }

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        tileCoords = nuevaPos;
    }
}
