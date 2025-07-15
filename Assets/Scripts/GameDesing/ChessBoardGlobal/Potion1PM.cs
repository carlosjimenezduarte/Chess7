using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Potion1PM : MonoBehaviour, ITileEffect, IObjetoRecoleccionable
{
    [Header("Turno y posición planificada")]
    public int turnoAparece = 1;
    public Vector2Int posicionReal = new Vector2Int(0, 0);

    [Header("Posición en Futuros Inciertos")]
    public Vector2Int tileCoordsFuturosInciertos = new Vector2Int(100, 100);

    [Header("Estado actual en el juego")]
    public Vector2Int tileCoords;
    public bool visibleDesdeInicio = false;

    private bool activadoEnJuego = false;
    private Image image;
    private PiecePositioner piecePositioner;

    private void Awake()
    {
        image = GetComponent<Image>();
        piecePositioner = GetComponent<PiecePositioner>();

        // Siempre inicia escondida en Futuros Inciertos
        tileCoords = tileCoordsFuturosInciertos;
        if (piecePositioner != null)
            piecePositioner.tileCoords = tileCoordsFuturosInciertos;

        image.enabled = false;
    }

    private void Start()
    {
        // Caso para debug o niveles muy simples que tienen la poción activa desde el inicio
        if (visibleDesdeInicio && turnoAparece <= 1)
        {
            Debug.Log($"🌟 Poción planea aparecer ya mismo en {posicionReal}");
            if (PuedeAparecerEn(posicionReal))
            {
                TeletransportarAlTablero();
            }
            else
            {
                Debug.Log($"❌ Poción en {posicionReal} NO pudo aparecer y se va a la Dimensión Divina.");
                ExiliarADimensionDivina();
            }
        }
    }

    private void Update()
    {
        if (piecePositioner != null)
            tileCoords = piecePositioner.tileCoords;

        if (!activadoEnJuego) return;

        VerificarAutoChequeoGeneral();
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        if (!activadoEnJuego && turnoActual >= turnoAparece)
        {
            Debug.Log($"⏳ Turno {turnoActual}. Poción programada para aparecer en {posicionReal} desde Futuros Inciertos ({tileCoordsFuturosInciertos}).");

            // Antes de aparecer, limpia la casilla si hay una poción rezagada
            var objetos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
                .Where(obj => obj is IObjetoRecoleccionable && obj != this);

            foreach (var obj in objetos)
            {
                Vector2Int pos = Vector2Int.zero;
                if (obj.TryGetComponent<PiecePositioner>(out var posr))
                    pos = posr.tileCoords;
                else if (obj is IPieceWithPosition pieza)
                    pos = pieza.GetPosicionActual();

                if (pos == posicionReal)
                {
                    Debug.Log($"💥 Poción en turno {turnoActual} reemplaza a {obj.name} que estaba en {pos}.");
                    if (obj.TryGetComponent<Potion1PM>(out var otraPocion))
                        otraPocion.ExiliarADimensionDivina();
                    else
                        obj.gameObject.SetActive(false);
                }
            }

            // Verifica si hay una ficha (rey o peón)
            var rey = FindFirstObjectByType<KingController>();
            if (rey != null && posicionReal == rey.GetPosicionActual())
            {
                Debug.Log($"⚡ Poción aparece en {posicionReal} justo sobre el Rey. Otorga bonus inmediato.");
                RecogerPocion(rey);
                return;
            }

            var peones = FindObjectsByType<PawnController>(FindObjectsSortMode.None);
            foreach (var peon in peones)
            {
                if (posicionReal == peon.GetPosicionActual())
                {
                    Debug.Log($"⚡ Poción aparece en {posicionReal} justo sobre el Peón. Bonus silencioso.");
                    peon.GanarPuntoMovimientoSilencioso(1);
                    Destroy(gameObject);
                    return;
                }
            }

            // Si nada lo impide, ahora sí se materializa
            TeletransportarAlTablero();
        }
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

        if (obj.TryGetComponent<PiecePositioner>(out var posr))
            pos = posr.tileCoords;
        else if (obj is IPieceWithPosition pieza)
            pos = pieza.GetPosicionActual();

        if (obj.TryGetComponent<Potion1PM>(out var otraPocion))
        {
            turnoOtro = otraPocion.turnoAparece;
            estaActivo = otraPocion.activadoEnJuego;
        }

        if (pos == coords)
        {
            if (estaActivo || turnoOtro <= turnoAparece)
            {
                Debug.Log($"💥 Poción destruye a {obj.name} que estaba en {coords} (turno {turnoOtro}).");
                if (otraPocion != null)
                    otraPocion.ExiliarADimensionDivina();
                else
                    obj.gameObject.SetActive(false);
            }
            else
            {
                Debug.Log($"🕊 Poción NO aparece en {coords} porque hay futura más temprana (turno {turnoOtro}).");
                return false;
            }
        }
    }

    // Además chequea si hay Rey o Peón encima
    var rey = FindFirstObjectByType<KingController>();
    if (rey != null && coords == rey.GetPosicionActual())
    {
        Debug.Log($"⚡ Poción aparece en {coords} encima del Rey. Se activa inmediatamente.");
        RecogerPocion(rey);
        return false;
    }

    var peones = FindObjectsByType<PawnController>(FindObjectsSortMode.None);
    foreach (var peon in peones)
    {
        if (coords == peon.GetPosicionActual())
        {
            Debug.Log($"⚡ Poción aparece en {coords} encima del Peón. Se activa silenciosamente.");
            peon.GanarPuntoMovimientoSilencioso(1);
            Destroy(gameObject);
            return false;
        }
    }

    return true;
    }


    private void TeletransportarAlTablero()
    {
        tileCoords = posicionReal;
        if (piecePositioner != null)
            piecePositioner.tileCoords = posicionReal;

        if (TryGetComponent<MovableTileObject>(out var movable))
        {
            movable.ActivarEnTablero(posicionReal);            
        }

        activadoEnJuego = true;
        image.enabled = true;

        Debug.Log($"✅ Poción se materializa en {tileCoords} (Turno {turnoAparece}).");
    }

    private void ExiliarADimensionDivina()
    {
        tileCoords = DimensionDivina.ObtenerProximaPosicion();
        if (piecePositioner != null)
            piecePositioner.tileCoords = tileCoords;

        if (TryGetComponent<MovableTileObject>(out var movable))
        {
            movable.tileCoords = tileCoords;
            transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(tileCoords);
        }

        gameObject.SetActive(false);
        Debug.Log($"🚀 {gameObject.name} enviado a Dimensión Divina en {tileCoords}.");
    }

    private void RecogerPocion(KingController rey)
    {
        Debug.Log($"🧪 El Rey recoge la poción en {tileCoords} (+1 PM).");
        rey.GanarPuntoMovimiento(1);
        Destroy(gameObject);
    }

    public void VerificarAutoChequeoGeneral()
    {
        var rey = FindFirstObjectByType<KingController>();
        if (rey != null && tileCoords == rey.GetPosicionActual())
        {
            Debug.Log($"🧲 Poción en {tileCoords} detecta al Rey encima. Se activa.");
            RecogerPocion(rey);
            return;
        }

        var peones = FindObjectsByType<PawnController>(FindObjectsSortMode.None);
        foreach (var peon in peones)
        {
            if (tileCoords == peon.GetPosicionActual())
            {
                Debug.Log($"🤫 Poción en {tileCoords} detecta Peón encima. Bonus silencioso.");
                peon.GanarPuntoMovimientoSilencioso(1);
                Destroy(gameObject);
                return;
            }
        }
    }

    public bool IsVisible() => activadoEnJuego;

    public bool EstaRealmenteEnTablero()
        => tileCoords.x >= 0 && tileCoords.y >= 0 && tileCoords.x <= 7 && tileCoords.y <= 7;

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey) => VerificarAutoChequeoGeneral();
    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon) => VerificarAutoChequeoGeneral();
}
