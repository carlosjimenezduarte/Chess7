using UnityEngine;
using UnityEngine.UI;

public class Potion1PM : MonoBehaviour, ITileEffect, IPieceWithPosition
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

        if (image == null)
            Debug.LogError($"🚨 {gameObject.name} no tiene componente Image.");

        if (piecePositioner != null)
            piecePositioner.tileCoords = tileCoords;  // inicial sincronía
    }

    private void Start()
    {
        if (piecePositioner != null)
            tileCoords = piecePositioner.tileCoords;  // inicialización por PiecePositioner

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
        if (!activadoEnJuego) return;

        var rey = FindFirstObjectByType<KingController>();
        if (rey != null)
            VerificarAutoChequeo(rey);
    }

    public void VerificarAutoChequeo(KingController rey)
    {
        if (activadoEnJuego && tileCoords == rey.GetPosicionActual())
        {
            Debug.Log($"🧲 Poción en {tileCoords} detectó al Rey. Se activará.");
            RecogerPocion(rey);
        }
    }

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        VerificarAutoChequeo(rey);
    }

    public void RecogerPocion(KingController rey)
    {
        Debug.Log($"🧪 El Rey recogió la poción en {tileCoords} (+1 PM).");
        rey.GanarPuntoMovimiento(1);
        Destroy(gameObject);
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        if (!activadoEnJuego && turnoActual >= turnoAparece)
        {
            ActivarVisual();
            Debug.Log($"✅ Poción en {tileCoords} se activó en el turno {turnoActual}.");

            var rey = FindFirstObjectByType<KingController>();
            if (rey != null)
                VerificarAutoChequeo(rey);
        }
    }

    public void ForzarActivacion()
    {
        if (!activadoEnJuego)
        {
            ActivarVisual();
            Debug.Log($"🚀 Poción en {tileCoords} activada manualmente.");

            var rey = FindFirstObjectByType<KingController>();
            if (rey != null)
                VerificarAutoChequeo(rey);
        }
    }

    private void ActivarVisual()
    {
        activadoEnJuego = true;
        image.enabled = true;
    }

    // === INTERFAZ IPieceWithPosition ===
    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        tileCoords = nuevaPos;
        if (piecePositioner != null)
            piecePositioner.tileCoords = nuevaPos;
    }

    public Vector2Int GetPosicionActual()
    {
        return tileCoords;
    }
    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
   
    }
}

