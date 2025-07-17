using UnityEngine;
using UnityEngine.UI;

public class AP : MonoBehaviour, IPieceWithPosition
{
    [Header("Coordenadas y aparición")]
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
            ActivarVisual();
        else
            image.enabled = false;
    }

    private void Update()
    {
        if (!activadoEnJuego) return;

        var rey = FindFirstObjectByType<KingController>();
        if (rey != null && tileCoords == rey.GetPosicionActual())
            RecogerAP(rey);
    }

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        if (activadoEnJuego && posicionRey == tileCoords)
            RecogerAP(rey);
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        if (activadoEnJuego && posicionPeon == tileCoords)
        {
            Debug.Log($"⚡ Peón recogió AP en {tileCoords}, lo transfiere al Rey (+1 PA).");
            var rey = FindFirstObjectByType<KingController>();
            if (rey != null)
            {
                rey.puntosAccionActual += 1;
                FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
            }
            Destroy(gameObject);
        }
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        if (!activadoEnJuego && turnoActual >= turnoAparece)
            ActivarVisual();
    }

    private void ActivarVisual()
    {
        activadoEnJuego = true;
        image.enabled = true;
    }

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

    private void RecogerAP(KingController rey)
    {
        Debug.Log($"⚡ El Rey recogió AP en {tileCoords} (+1 PA).");
        rey.puntosAccionActual += 1;
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
        Destroy(gameObject);
    }
}
