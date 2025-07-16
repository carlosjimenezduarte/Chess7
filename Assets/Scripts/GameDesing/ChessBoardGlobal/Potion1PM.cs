using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Potion1PM : MonoBehaviour, ITileEffect, IObjetoRecoleccionable
{
    [Header("Configuración general")]
    public bool vieneDelFuturo = false;

    [Header("Turno y posiciones")]
    public int turnoAparece = 1;
    public Vector2Int posicionReal = new Vector2Int(0, 0);
    public Vector2Int tileCoordsFuturosInciertos = new Vector2Int(100, 100);

    private Vector2Int tileCoords;
    private bool activadoEnJuego = false;

    private Image image;
    private PiecePositioner piecePositioner;
    private MovableTileObject movable;

    private void Awake()
{
    image = GetComponent<Image>();
    piecePositioner = GetComponent<PiecePositioner>();
    movable = GetComponent<MovableTileObject>();

    if (piecePositioner == null)
        Debug.LogWarning($"⚠️ {name} no tiene PiecePositioner.");
    if (movable == null)
        Debug.LogWarning($"⚠️ {name} no tiene MovableTileObject.");

    if (vieneDelFuturo)
    {
        ColocarEn(tileCoordsFuturosInciertos);
        if (movable != null) movable.activoEnTablero = false;
        image.enabled = false;
        activadoEnJuego = false;
    }
    else
    {
        // ⚡ ahora deja al prefab en su posición, sin reubicar con posicionReal
        tileCoords = piecePositioner != null ? piecePositioner.tileCoords : tileCoords;
        if (movable != null) movable.activoEnTablero = true;
        image.enabled = true;
        activadoEnJuego = true;
        Debug.Log($"✅ {name} inicializado en tablero en {tileCoords}.");
    }
}


    private void Update()
    {
        if (!activadoEnJuego) return;
        VerificarAutoChequeoGeneral();
    }

    private void ColocarEn(Vector2Int coords)
    {
        tileCoords = coords;
        if (piecePositioner != null)
            piecePositioner.tileCoords = coords;
        if (movable != null)
            movable.tileCoords = coords;

        if (BoardManagerGlobal.Instance != null)
            transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(coords);

        Debug.Log($"🧩 {name} colocado en {coords}");
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        if (!activadoEnJuego && turnoActual >= turnoAparece)
        {
            Debug.Log($"⏳ Turno {turnoActual}. {name} programado para aparecer en {posicionReal}.");

            TeletransportarAlTablero();
        }
    }

    private void TeletransportarAlTablero()
{
    if (PuedeAparecerEn(posicionReal))
    {
        ColocarEn(posicionReal);
        if (movable != null) movable.ActivarEnTablero(posicionReal);
        activadoEnJuego = true;
        image.enabled = true;
        Debug.Log($"✅ {name} se materializó en {posicionReal}.");
    }
    else
    {
        ExiliarADimensionDivina();
    }
}

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        if (tileCoords == posicionRey)
        {
            Debug.Log($"🧪 {name} detecta al Rey encima. Se activa.");
            rey.GanarPuntoMovimiento(1);
            Destroy(gameObject);
        }
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        if (tileCoords == posicionPeon)
        {
            Debug.Log($"🤫 {name} detecta Peón encima. Bonus silencioso.");
            peon.GanarPuntoMovimientoSilencioso(1);
            Destroy(gameObject);
        }
    }

    public void VerificarAutoChequeoGeneral()
    {
        var rey = FindFirstObjectByType<KingController>();
        if (rey != null)
            RevisarSiReyLlegó(rey.GetPosicionActual(), rey);

        var peones = FindObjectsByType<PawnController>(FindObjectsSortMode.None);
        foreach (var peon in peones)
            RevisarSiPeonLlegó(peon.GetPosicionActual(), peon);
    }

    public void ExiliarADimensionDivina()
    {
        Vector2Int coordsDivinos = DimensionDivina.ObtenerProximaPosicion();
        ColocarEn(coordsDivinos);

        if (movable != null)
            movable.activoEnTablero = false;

        gameObject.SetActive(false);
        Debug.Log($"🚀 {name} exiliado a Dimensión Divina en {coordsDivinos}.");
    }

    private bool PuedeAparecerEn(Vector2Int coords)
    {
    var objetos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
        .Where(obj => obj is IObjetoRecoleccionable && obj != this);

    foreach (var obj in objetos)
    {
        Vector2Int pos = Vector2Int.zero;
        int turnoOtro = 0;
        bool estaActivo = false;

        // obtiene su posición actual
        if (obj.TryGetComponent<PiecePositioner>(out var posr))
            pos = posr.tileCoords;
        else if (obj is IPieceWithPosition pieza)
            pos = pieza.GetPosicionActual();

        // obtiene sus datos si es un objeto recoleccionable con turno
        if (obj.TryGetComponent<Potion1PM>(out var otro))
        {
            turnoOtro = otro.turnoAparece;
            estaActivo = otro.IsVisible();
        }

        // Si hay algo allí
        if (pos == coords)
        {
            if (estaActivo || turnoOtro <= turnoAparece)
            {
                Debug.Log($"💥 {name} destruye a {obj.name} en {coords}");
                if (otro != null)
                    otro.ExiliarADimensionDivina();
                else
                    obj.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"🕊 {name} NO puede aparecer en {coords} por futura más temprana (turno {turnoOtro})");
                return false;
            }
        }
    }
    return true;
    }


    public bool IsVisible() => activadoEnJuego;

    public bool EstaRealmenteEnTablero()
        => tileCoords.x >= 0 && tileCoords.y >= 0 && tileCoords.x <= 7 && tileCoords.y <= 7;
}
