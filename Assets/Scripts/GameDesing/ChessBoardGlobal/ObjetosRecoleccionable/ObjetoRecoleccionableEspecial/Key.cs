using UnityEngine;
using UnityEngine.UI;
using System.Linq;


public class Key : MonoBehaviour, ITileEffect, IObjetoRecoleccionable, IObjetoRecoleccionableEspecial, IPieceWithPosition
{
    public Vector2Int tileCoords;
    public bool esInamovible = false;

    private Image image;
    private PiecePositioner positioner;
    private MovableTileObject movable;
    private bool yaRecolectado = false;

    private void Update()
    {
        if (yaRecolectado) return;
        VerificarAutoChequeoGeneral();
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        positioner = GetComponent<PiecePositioner>();
        movable = GetComponent<MovableTileObject>();

        tileCoords = positioner != null ? positioner.tileCoords : new Vector2Int(-1, -1);
        if (movable != null) movable.activoEnTablero = true;

        BoardManagerGlobal.Instance?.RegistrarMovimiento(this, tileCoords);
        BoardManagerGlobal.Instance?.AgregarMensajeInterno($"🗝️ Llave posicionada en {tileCoords}.");
    }

    public void RevisarSiFichaAliadaLlegó(Vector2Int posicion, IFichaAliada ficha)
    {
        if (yaRecolectado || tileCoords != posicion) return;

        yaRecolectado = true;
        BoardManagerGlobal.Instance?.AgregarMensajeInterno($"🔑 Llave recolectada por {ficha.GetType().Name} en {posicion}.");

        // Puntaje
        var score = FindFirstObjectByType<PlayerScore>();
        if (score != null)
            score.AgregarPuntaje(ObtenerValorPuntaje());

        // Recolectar y destruir
        Destroy(gameObject);
    }

    public void RevisarSiPeonLlegó(Vector2Int posicion, PawnController peon) =>
        RevisarSiFichaAliadaLlegó(posicion, peon);

    public void RevisarSiReyLlegó(Vector2Int posicion, KingController rey) =>
        RevisarSiFichaAliadaLlegó(posicion, rey);

    public void RevisarSiReinaEnemigaLlegó(Vector2Int posicion, QueenEnemyController reinaenemiga)
    {
               //RevisarSiReinaEnemigaLlegó(posicion, rey);
    }

    public void RevisarSiFichaLlegó(Vector2Int posicion, IFicha ficha)
    {
        if (ficha is IFichaAliada aliada)
            RevisarSiFichaAliadaLlegó(posicion, aliada);
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        // Nada que hacer: la llave no viene del futuro
    }

    public bool EsInamovible() => esInamovible;

    public int ObtenerValorPuntaje() => 100; // Valor configurable

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        tileCoords = nuevaPos;
        if (positioner != null) positioner.tileCoords = nuevaPos;
        if (movable != null) movable.tileCoords = nuevaPos;
    }

    private void VerificarAutoChequeoGeneral()
{
    var fichasAliadas = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
        .OfType<IFichaAliada>();

    foreach (var ficha in fichasAliadas)
    {
        if (ficha is KingController rey)
            RevisarSiReyLlegó(rey.GetPosicionActual(), rey);
        else
            RevisarSiFichaAliadaLlegó(ficha.GetPosicionActual(), ficha);
    }
}

    public Vector2Int GetPosicionActual() => tileCoords;

    public bool EstaRealmenteEnTablero() =>
        tileCoords.x >= 0 && tileCoords.y >= 0 && tileCoords.x <= 7 && tileCoords.y <= 7;
}