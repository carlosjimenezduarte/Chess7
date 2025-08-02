using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.SceneManagement; // para usar SceneManager

public class ChessGameManager : MonoBehaviour
{
    public KingController rey;

    public Button startButton;
    public Button passTurnButton;

    [Header("Ficha seleccionada actualmente (Rey o Peón)")]
    public MonoBehaviour fichaSeleccionadaActual;

    // 🔥 NUEVOS BOTONES
    public Button restartButton;
    public Button exitButton;

    public TMP_Text timerText;
    public TMP_Text pmText;
    public TMP_Text turnosText;
    public TMP_Text paText;

    private float turnoDuration = 30f;
    private float tiempoRestante;
    private float tiempoNivelAcumulado = 0f;

    private bool turnoActivo = false;
    private int turnoActual = 0;

    private void Start()
    {
        passTurnButton.gameObject.SetActive(false);
        startButton.onClick.AddListener(IniciarJuego);
        passTurnButton.onClick.AddListener(PasarTurno);

        // 🔥 NUEVOS listeners para los botones
        if (restartButton != null)
            restartButton.onClick.AddListener(Restart);
        if (exitButton != null)
            exitButton.onClick.AddListener(Exit);

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
            tiempoNivelAcumulado += Time.deltaTime;
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
        tiempoNivelAcumulado = 0f;
        turnoActual = 1;

        fichaSeleccionadaActual = rey;

        rey.ActivarJuego();
        Invoke(nameof(MostrarRangoInicialRey), 0.02f);

    var aliadas = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
    .OfType<IFichaAliada>();

    foreach (var ficha in aliadas)
    {
    ficha.ActivarJuego();

    if (ficha is IPieceWithPosition pieza)
    {
        var tile = BoardManagerGlobal.Instance.GetTileAt(pieza.GetPosicionActual());
        if (tile != null)
        {
            tile.HighlightMove(false); // Opcional: oculta movimientos al inicio
        }
    }

    if (ficha is MonoBehaviour mb)
    {
        var mostrarFlag = mb.GetType().GetField("mostrandoMovimientos");
        if (mostrarFlag != null)
            mostrarFlag.SetValue(mb, false);
    }
    }

        ActualizarHUD();
        NotificarEfectosTurno();
    }

    private void MostrarRangoInicialRey()
    {
        rey.mostrandoMovimientos = true;
        rey.MostrarMovimientoPosible();
    }


    private void PasarTurno()
{
    Debug.Log("¡Pasando turno!");
    tiempoRestante = turnoDuration;
    turnoActual++;

    // 🔁 Reiniciar fichas aliadas
    var aliadas = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
        .OfType<IFichaAliada>();
    foreach (var aliada in aliadas)
    {
        aliada.ReiniciarTurno();
    }

    // 🔁 Reiniciar fichas enemigas
    var enemigas = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
        .OfType<IFichaEnemiga>();
    foreach (var enemiga in enemigas)
    {
        enemiga.ReiniciarTurno();
    }

    // ♔ El Rey también reinicia su turno
    rey.ReiniciarTurno();
    rey.RestarTurno();

    // 🔄 Rango visible si una ficha sigue seleccionada
    if (fichaSeleccionadaActual is IFichaAliada fichaAliada)
    {
        fichaAliada.MostrarRango();
    }

    NotificarEfectosTurno();
    ActualizarHUD();
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
        tiempoRestante = Mathf.Max(tiempoRestante, 0f);
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

    // === NUEVAS FUNCIONES PARA LOS BOTONES ===
    public void Restart()
    {
        Debug.Log("🔄 Reiniciando nivel...");
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Exit()
    {
        Debug.Log("🏠 Saliendo al GameHome...");
        SceneManager.LoadScene("GameHome");
    }
    public void NotifyBoardChanged()
{
    var reinas = FindObjectsByType<QueenEnemyController>(FindObjectsSortMode.None);
    foreach (var reina in reinas)
    {
        reina.RevisarAmenazasEnZona();
    }
    }
    
}
