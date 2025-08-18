using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; // Para volver al menú u otra escena
using UnityEngine.Video; // Para reproducir la cinemática

public class GardenGameManager : MonoBehaviour
{
    public static GardenGameManager Instance { get; private set; }

    [Header("UI")]
    public GameObject panelMensaje;
    public Text textoMensaje;
    public Button botonAceptarMensaje;

    public GameObject panelImagenEspejo;
    public Image imagenEspejo;
    public Button botonCerrarEspejo;

    [Header("Botones generales")]
    public Button botonExit;

    [Header("Cinemática y Créditos")]
    public VideoPlayer videoFinal; // Asignar en el inspector
    public GameObject panelCreditos; // Créditos de 4 min

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (panelMensaje != null) panelMensaje.SetActive(false);
        if (panelImagenEspejo != null) panelImagenEspejo.SetActive(false);
        if (panelCreditos != null) panelCreditos.SetActive(false);
    }

    private void Start()
    {
        if (botonAceptarMensaje != null)
            botonAceptarMensaje.onClick.AddListener(CerrarMensaje);

        if (botonCerrarEspejo != null)
            botonCerrarEspejo.onClick.AddListener(CerrarEspejo);

        if (botonExit != null)
            botonExit.onClick.AddListener(VolverAlMenu);
    }

    // ========================
    // MENSAJES GENERALES
    // ========================
    public void MostrarMensaje(string mensaje)
    {
        if (panelMensaje != null && textoMensaje != null)
        {
            textoMensaje.text = mensaje;
            panelMensaje.SetActive(true);
        }
    }

    public void CerrarMensaje()
    {
        if (panelMensaje != null)
            panelMensaje.SetActive(false);
    }

    // ========================
    // ESPEJOS
    // ========================
    public void MostrarEspejo(Sprite sprite)
    {
        if (panelImagenEspejo != null && imagenEspejo != null)
        {
            imagenEspejo.sprite = sprite;
            panelImagenEspejo.SetActive(true);
        }
    }

    public void CerrarEspejo()
    {
        if (panelImagenEspejo != null)
            panelImagenEspejo.SetActive(false);
    }

    // ========================
    // COFRE
    // ========================
    public void IntentarAbrirCofre()
    {
        if (!GardenManagerGlobal.Instance.TieneLlave())
        {
            MostrarMensaje("No tienes la llave para abrir el cofre.");
            return;
        }

        // Preguntar confirmación
        MostrarMensaje("¿Estás seguro que quieres abrir este cofre?");
        // Aquí podrías enlazar el botón "Aceptar" a AbrirCofreFinal()
        botonAceptarMensaje.onClick.RemoveAllListeners();
        botonAceptarMensaje.onClick.AddListener(() => {
            CerrarMensaje();
            AbrirCofreFinal();
        });
    }

    private void AbrirCofreFinal()
    {
        // Mostrar cinemática de 1 minuto
        if (videoFinal != null)
        {
            videoFinal.gameObject.SetActive(true);
            videoFinal.Play();
            Invoke(nameof(MostrarCreditos), 60f); // 1 minuto después mostrar créditos
        }
        else
        {
            MostrarCreditos();
        }
    }

    private void MostrarCreditos()
    {
        if (panelCreditos != null)
            panelCreditos.SetActive(true);

        // Después de 4 min terminar juego o ir a escena final
        Invoke(nameof(FinDelJuego), 240f);
    }

    private void FinDelJuego()
    {
        // Cargar escena final o volver al menú
        SceneManager.LoadScene("MainMenu"); // Cambia por tu escena final
    }

    // ========================
    // SALIDA DEL JARDÍN
    // ========================
    private void VolverAlMenu()
    {
        SceneManager.LoadScene("MainMenu"); // Ajusta a tu menú real
    }
}
