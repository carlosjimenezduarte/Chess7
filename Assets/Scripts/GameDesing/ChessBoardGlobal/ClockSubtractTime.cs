using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ClockSubtractTime : MonoBehaviour
{
    public Vector2Int tileCoords;
    public int turnoAparece = 1;
    public bool visibleDesdeInicio = false;
    public float segundosRestar = 15f;

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
            Debug.Log($"🌑 Reloj oscuro en {tileCoords} aparece desde inicio (turnoAparece={turnoAparece})");
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
            RestarTiempoSeguro();
        }
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        Debug.Log($"🔍 Reloj oscuro en {tileCoords}: turnoActual={turnoActual}, turnoAparece={turnoAparece}, ActivadoJuego={activadoEnJuego}");

        if (!activadoEnJuego && turnoActual >= turnoAparece)
        {
            activadoEnJuego = true;
            image.enabled = true;
            Debug.Log($"✅ Reloj oscuro en {tileCoords} SE ACTIVÓ en el turno {turnoActual}");

            KingController rey = FindFirstObjectByType<KingController>();
            if (rey != null && rey.GetPosicionActual() == tileCoords)
            {
                RestarTiempoSeguro(1f);
            }
        }
    }

    private void RestarTiempoSeguro(float delayDestroy = 0f)
    {
        ChessGameManager gameManager = FindFirstObjectByType<ChessGameManager>();
        if (gameManager != null)
        {
            // Calcula el tiempo que realmente puede restar
            float tiempoActual = gameManager.GetTiempoRestante();
            float tiempoARestar = Mathf.Min(segundosRestar, tiempoActual);

            gameManager.AgregarTiempoAlTurno(-tiempoARestar);
            Debug.Log($"⏱️ El Rey recogió un reloj oscuro en {tileCoords} y perdió -{tiempoARestar}s. Tiempo restante ahora: {gameManager.GetTiempoRestante():F1}s");
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
