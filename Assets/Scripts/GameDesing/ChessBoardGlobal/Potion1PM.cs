using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class Potion1PM : MonoBehaviour, ITileEffect, IObjetoRecoleccionable
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

    // 🚫 Siempre inicia invisible
    image.enabled = false;

    if (visibleDesdeInicio && turnoAparece <= 1)
    {
        Debug.Log($"🌟 Poción planea aparecer en {tileCoords}");
        if (PuedeAparecerEn(tileCoords))
        {
            ActivarVisual();
            Debug.Log($"✅ Poción apareció normalmente en {tileCoords}");
        }
        else
        {
            Debug.Log($"❌ Poción decidió NO aparecer en {tileCoords} y se destruye.");
            Destroy(gameObject, 0);
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
            Debug.Log($"⏳ Pocion en {tileCoords} evalúa si puede aparecer en turno {turnoActual}.");

            if (PuedeAparecerEn(tileCoords))
            {
                ActivarVisual();
                Debug.Log($"✅ Poción en {tileCoords} se activó normalmente en el turno {turnoActual}.");
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private bool PuedeAparecerEn(Vector2Int coords)
    {
        // 🚫 Primero, verifica si hay otro objeto recoleccionable
        var objetos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
            .Where(obj => obj is IObjetoRecoleccionable);

        foreach (var obj in objetos)
        {
            if (obj == this) continue;

            Vector2Int pos = Vector2Int.zero;
            if (obj.TryGetComponent<PiecePositioner>(out var posr))
                pos = posr.tileCoords;
            else if (obj is IPieceWithPosition pieza)
                pos = pieza.GetPosicionActual();

            if (pos == coords)
            {
                Debug.Log($"🚫 Poción decidió NO aparecer en {coords} porque ya hay otro objeto recoleccionable: {obj.name}");
                return false;
            }
        }

        // 🚀 Ahora, verifica si hay una ficha (Rey o Peón) para otorgarle el efecto de inmediato
        var rey = FindFirstObjectByType<KingController>();
        if (rey != null && coords == rey.GetPosicionActual())
        {
            Debug.Log($"⚡ Poción apareció en {coords} encima del Rey. Activa efecto inmediatamente.");
            RecogerPocion(rey);
            return false;
        }

        var peones = FindObjectsByType<PawnController>(FindObjectsSortMode.None);
        foreach (var peon in peones)
        {
            if (coords == peon.GetPosicionActual())
            {
                Debug.Log($"⚡ Poción apareció en {coords} encima del Peón. Activa efecto silencioso.");
                peon.GanarPuntoMovimientoSilencioso(1);
                Destroy(gameObject);
                return false;
            }
        }

        // ✅ Si no hay conflictos, puede aparecer normalmente
        return true;
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
    public bool IsVisible()
    {
    return activadoEnJuego;
    }


    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey) => VerificarAutoChequeoGeneral();
    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon) => VerificarAutoChequeoGeneral();
}
