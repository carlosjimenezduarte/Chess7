using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Potion1PM : MonoBehaviour, ITileEffect
{
    public Vector2Int tileCoords;
    public int turnoAparece = 1;
    public bool visibleDesdeInicio = false;

    private bool activadoEnJuego = false;
    private Image image;
    private PiecePositioner piecePositioner;

    private void Awake()
    {
        image = GetComponent<Image>();
        piecePositioner = GetComponent<PiecePositioner>();

        if (piecePositioner != null)
            piecePositioner.tileCoords = tileCoords;
    }

    private void Start()
    {
        if (piecePositioner != null)
            tileCoords = piecePositioner.tileCoords;

        if (visibleDesdeInicio && turnoAparece <= 1)
        {
            ActivarVisual();
            Debug.Log($"🌟 Poción inicia visible en {tileCoords}");
        }
        else
        {
            image.enabled = false;
        }
    }

    private void Update()
    {
        // 🚀 Sincroniza siempre con PiecePositioner
        if (piecePositioner != null)
            tileCoords = piecePositioner.tileCoords;

        if (!activadoEnJuego) return;

        VerificarAutoChequeoGeneral();
    }

    public void VerificarAutoChequeoGeneral()
    {
        var rey = FindFirstObjectByType<KingController>();
        if (rey != null && tileCoords == rey.GetPosicionActual())
        {
            Debug.Log($"🧲 Poción en {tileCoords} detectó al Rey. Se activará.");
            RecogerPocion(rey);
            return;
        }

        var peones = FindObjectsByType<PawnController>(FindObjectsSortMode.None);
        foreach (var peon in peones)
        {
            if (tileCoords == peon.GetPosicionActual())
            {
                Debug.Log($"🤫 Poción en {tileCoords} detectó Peón encima tras ser empujada. Se activará silenciosa.");
                peon.GanarPuntoMovimientoSilencioso(1);
                Destroy(gameObject);
                return;
            }
        }
    }

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        VerificarAutoChequeoGeneral();
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        VerificarAutoChequeoGeneral();
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        if (!activadoEnJuego && turnoActual >= turnoAparece)
        {
            ActivarVisual();
            Debug.Log($"✅ Poción en {tileCoords} se activó en el turno {turnoActual}.");
        }
    }

    private void ActivarVisual()
    {
        activadoEnJuego = true;
        image.enabled = true;
    }

    private void RecogerPocion(KingController rey)
    {
        Debug.Log($"🧪 El Rey recogió la poción en {tileCoords} (+1 PM).");
        rey.GanarPuntoMovimiento(1);
        Destroy(gameObject);
    }

    // 🚀 NUEVO: verificación si la casilla está ocupada
    public bool EstaCasillaOcupada(Vector2Int coords)
    {
        var piezas = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .Where(obj => 
                obj is KingController ||
                obj is PawnController ||
                obj.GetComponent<Potion1PM>() != null
            );

        foreach (var pieza in piezas)
        {
            Vector2Int pos = Vector2Int.zero;

            if (pieza is KingController rey) pos = rey.GetPosicionActual();
            else if (pieza is PawnController peon) pos = peon.GetPosicionActual();
            else if (pieza.GetComponent<Potion1PM>() != null) pos = pieza.GetComponent<Potion1PM>().tileCoords;

            // ⚠️ Evita considerarse a sí misma
            if (pos == coords && pieza.gameObject != this.gameObject)
            {
                Debug.Log($"🚫 La casilla {coords} está ocupada por {pieza.gameObject.name}");
                return true;
            }
        }
        return false;
    }

    // 🚀 NUEVO: SetPosicionActual inteligente que regresa si hay colisión
    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        Vector2Int posicionPrev = tileCoords;

        tileCoords = nuevaPos;
        if (piecePositioner != null)
            piecePositioner.tileCoords = nuevaPos;

        if (EstaCasillaOcupada(nuevaPos))
        {
            Debug.Log($"⛔ {gameObject.name} encontró la casilla {nuevaPos} ocupada. Regresará a {posicionPrev}.");
            tileCoords = posicionPrev;
            if (piecePositioner != null)
                piecePositioner.tileCoords = posicionPrev;
        }
    }
}
