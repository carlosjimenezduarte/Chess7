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
    private float tiempoNivelAcumulado = 0f; // 🔥 nuevo acumulador del tiempo jugado

    private bool turnoActivo = false;
    private int turnoActual = 0;

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
            tiempoNivelAcumulado += Time.deltaTime; // 🔥 suma el tiempo efectivo jugado
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
        tiempoNivelAcumulado = 0f; // 🔥 reinicia acumulador al iniciar el juego

        turnoActual = 1;

        rey.ReiniciarTurno();
        rey.ActivarJuego();
        ActualizarHUD();

        NotificarEfectosTurno();
    }

    private void PasarTurno()
    {
        Debug.Log("¡Pasando turno!");

        tiempoRestante = turnoDuration;
        turnoActual++;

        rey.ReiniciarTurno();
        rey.RestarTurno();

        NotificarEfectosTurno();

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

    public void AgregarTiempoAlTurno(float segundos)
    {
        tiempoRestante += segundos;
        tiempoRestante = Mathf.Max(tiempoRestante, 0f); // 🔥 nunca menos de 0
        Debug.Log($"⏰ Tiempo ajustado: {segundos:+0.##;-0.##}s -> Tiempo restante: {tiempoRestante:F1}s");
    }

    public float GetTiempoRestante()
    {
        return tiempoRestante;
    }

    public float GetTiempoNivelAcumulado()
    {
        return tiempoNivelAcumulado;
    }
    public bool IsJuegoActivo()
    {
    return turnoActivo;
    }
}
