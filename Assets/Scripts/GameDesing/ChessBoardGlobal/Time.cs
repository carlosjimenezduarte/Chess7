using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PotionAddTime : MonoBehaviour, ITileEffect
{
    public Vector2Int tileCoords;
    public int turnoAparece = 1;
    public bool visibleDesdeInicio = false;
    public float segundosExtra = 15f; // 🔥 ahora configurable en el Inspector

    private bool activadoEnJuego = false;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError($"🚨 El objeto {gameObject.name} no tiene un componente Image.");
        }
    }

    private void Start()
    {
        if (visibleDesdeInicio && turnoAparece <= 1)
        {
            activadoEnJuego = true;
            image.enabled = true;
            Debug.Log($"🌟 Pocion tiempo en {tileCoords} aparece desde inicio (turnoAparece={turnoAparece})");
        }
        else
        {
            image.enabled = false;
        }
    }

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        if (posicionRey == tileCoords && activadoEnJuego)
        {
            OtorgarTiempo();
        }
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        Debug.Log($"🔍 Pocion tiempo en {tileCoords}: turnoActual={turnoActual}, turnoAparece={turnoAparece}, ActivadoJuego={activadoEnJuego}");

        if (!activadoEnJuego && turnoActual >= turnoAparece)
        {
            activadoEnJuego = true;
            image.enabled = true;
            Debug.Log($"✅ Pocion tiempo en {tileCoords} SE ACTIVÓ en el turno {turnoActual}");

            KingController rey = FindFirstObjectByType<KingController>();
            if (rey != null && rey.GetPosicionActual() == tileCoords)
            {
                OtorgarTiempo(1f);
            }
        }
    }

    private void OtorgarTiempo(float delayDestroy = 0f)
    {
        Debug.Log($"⏳ El Rey recogió una poción tiempo en {tileCoords} y ganó +{segundosExtra}s al reloj actual.");

        ChessGameManager gameManager = FindFirstObjectByType<ChessGameManager>();
        if (gameManager != null)
        {
            gameManager.AgregarTiempoAlTurno(segundosExtra);
        }

        if (delayDestroy > 0f)
            StartCoroutine(DesaparecerDespuesDe(delayDestroy));
        else
            Destroy(gameObject);
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
    // Por ahora no hace nada si el Peón llega a este tile.
    }

    private IEnumerator DesaparecerDespuesDe(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        Destroy(gameObject);
    }
}
