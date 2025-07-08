using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

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

    private int turnoActual = 0; // 🔥 ahora llevamos un contador global de turnos

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

        turnoActual = 1; // 🔥 arranca el juego en turno 1

        rey.ReiniciarTurno();
        rey.ActivarJuego();
        ActualizarHUD();

        NotificarEfectosTurno(); // 🔥 revisa qué objetos deben activarse en el turno 1
    }

    private void PasarTurno()
    {
        Debug.Log("¡Pasando turno!");

        tiempoRestante = turnoDuration;
        turnoActual++; // 🔥 incrementa el turno global

        rey.ReiniciarTurno();
        rey.RestarTurno();

        NotificarEfectosTurno(); // 🔥 revisa qué objetos deben activarse en este nuevo turno

        ActualizarHUD();

        Debug.Log($"=== Estado global de los objetos en turno {turnoActual} ===");
        foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
        {
            if (efecto is MonoBehaviour mb)
            {
                Debug.Log($"  - Objeto {mb.gameObject.name}, activo: {mb.gameObject.activeSelf}");
            }
        }
    }

    private void NotificarEfectosTurno()
    {
        var efectos = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>().ToList();
        Debug.Log($"🚀 Notificando turno {turnoActual} a {efectos.Count} objetos ITileEffect.");

        foreach (var efecto in efectos)
        {
            Debug.Log($"📦 Notificando objeto {((MonoBehaviour)efecto).gameObject.name}");
            efecto.VerificarTurnoActual(turnoActual);
        }
    }

    public void ActualizarHUD()
    {
        pmText.text = rey.puntosMovimientoActual.ToString();
        turnosText.text = rey.turnosRestantes.ToString();
        paText.text = rey.puntosAccionActual.ToString();
    }
    
    public void DetenerJuego()
    {
    turnoActivo = false;
    Debug.Log("⏸ Juego detenido, reloj pausado.");
    }
}
