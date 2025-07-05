using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChessGameManager : MonoBehaviour
{
    public KingController rey;

    public Button startButton;
    public Button passTurnButton;
    public TMP_Text timerText;
    public TMP_Text pmText;
    public TMP_Text turnosText;
    public TMP_Text paText;

    private float turnoDuration = 30f;
    private float tiempoRestante;
    private bool turnoActivo = false;

    private void Start()
    {
        passTurnButton.gameObject.SetActive(false);
        startButton.onClick.AddListener(IniciarJuego);
        passTurnButton.onClick.AddListener(PasarTurno);

        timerText.text = "";
        pmText.text = "";
        turnosText.text = "";
        paText.text = "";

        ActualizarHUD();
    }

    private void Update()
    {
        if (turnoActivo)
        {
            tiempoRestante -= Time.deltaTime;
            timerText.text = Mathf.CeilToInt(tiempoRestante).ToString();

            if (tiempoRestante <= 0f)
            {
                PasarTurno();
            }
        }
    }

    private void IniciarJuego()
    {
        Debug.Log("¡Inicio del juego!");
        startButton.gameObject.SetActive(false);
        passTurnButton.gameObject.SetActive(true);

        turnoActivo = true;
        tiempoRestante = turnoDuration;

        rey.ReiniciarTurno();
        ActualizarHUD();
    }

    private void PasarTurno()
    {
        Debug.Log("¡Pasando turno!");
        tiempoRestante = turnoDuration;
        rey.ReiniciarTurno();

        // Aquí podrías reducir el número de turnos (corazones)
        rey.RestarTurno();

        ActualizarHUD();
    }

    public void ActualizarHUD()
    {
        pmText.text = rey.puntosMovimientoActual.ToString();
        turnosText.text = rey.turnosRestantes.ToString();
        paText.text = rey.puntosAccionActual.ToString();
    }
}
