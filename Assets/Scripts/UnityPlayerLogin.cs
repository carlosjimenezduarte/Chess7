/*using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class UnityPlayerLogin : MonoBehaviour
{
    private bool servicesInitialized = false;

    [Header("Referencias UI")]
    public GameObject panelLogin;   // Panel con botones
    public Button[] botonesLogin;   // Arrastra los botones de Login/Invitado

    private async void Start()
    {
         if (PlayerPrefs.GetString("userType", "") == "guest")
    {
        // Estamos en flujo de invitado: no muestres ni habilites login
        if (panelLogin) panelLogin.SetActive(false);
        ActivarBotones(false);
        return;
    }
    

        await InitializeUnityServices();

        if (AuthenticationService.Instance.IsSignedIn)
        {
            Debug.Log("Usuario ya autenticado. Deshabilitando botones un instante...");
            StartCoroutine(EsperarYRedirigir());
        }
        else
        {
            panelLogin.SetActive(true);
            ActivarBotones(true);
        }
    }

    private IEnumerator EsperarYRedirigir()
    {
        ActivarBotones(false);
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(1); // Username siempre decide si Home o quedarse
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            servicesInitialized = true;

            Debug.Log("Unity Services inicializados correctamente");
            PlayerAccountService.Instance.SignedIn += OnPlayerAccountSignedIn;
        }
        catch (Exception e)
        {
            Debug.LogError("Error al inicializar Unity Services: " + e.Message);
        }
    }

    public async void OnLoginButtonClicked()
    {
        if (!servicesInitialized)
        {
            Debug.LogWarning("Los servicios aún no están inicializados.");
            return;
        }

        try
        {
            Debug.Log("Iniciando login con Unity Player Accounts...");
            await PlayerAccountService.Instance.StartSignInAsync();
        }
        catch (Exception e)
        {
            Debug.LogError("Error al iniciar el flujo de login: " + e.Message);
        }
    }

    private async void OnPlayerAccountSignedIn()
    {
        try
        {
            Debug.Log("Cuenta Unity Player detectada. Accediendo al AccessToken...");
            string accessToken = PlayerAccountService.Instance.AccessToken;

            await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);
            Debug.Log("Inicio de sesión exitoso. Player ID: " + AuthenticationService.Instance.PlayerId);

            // Guardar tipo de usuario y PlayerId
            PlayerPrefs.SetString("userType", "unity");
            PlayerPrefs.SetString("playerId", AuthenticationService.Instance.PlayerId);
            PlayerPrefs.Save();

            StartCoroutine(EsperarYRedirigir());
        }
        catch (AuthenticationException e)
        {
            Debug.LogError("Error de autenticación: " + e.Message);
        }
        catch (RequestFailedException e)
        {
            Debug.LogError("Error en la solicitud: " + e.Message);
        }
    }

    private void ActivarBotones(bool estado)
    {
        foreach (var b in botonesLogin)
        {
            b.interactable = estado;
        }
    }
}
*/
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using System;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UnityPlayerLogin : MonoBehaviour
{
    [Header("UI (no se apagan)")]
    public GameObject panelLogin;          // Déjalo visible desde el Editor
    public Button loginButton;             // Botón "Iniciar sesión"
    public Button guestButton;             // (Opcional) "Entrar como invitado"

    [Header("Escena siguiente")]
    public int escenaDespuesDeLogin = 1;   // Ej: Username/Home

    // Interno (solo para evitar doble clic)
    private bool servicesInitialized = false;
    private bool loginInProgress = false;

    private async void Start()
    {
        // (Opcional) Si quieres mantener el botón de invitado:
        if (guestButton != null)
        {
            guestButton.onClick.AddListener(() =>
            {
                PlayerPrefs.SetString("userType", "guest");
                PlayerPrefs.SetString("slotActivo", "guest");
                PlayerPrefs.Save();
                // Si quieres ir a HomeGuest directamente:
                // SceneManager.LoadScene(13);
            });
        }

        // En ningún momento apagamos panelLogin ni botones
        if (panelLogin && !panelLogin.activeSelf) panelLogin.SetActive(true);

        // Inicializa servicios, pero sin bloquear UI
        await InitializeUnityServices();

        // Si ya estaba autenticado, pasa a la siguiente escena
        if (servicesInitialized && AuthenticationService.Instance.IsSignedIn)
        {
            SceneManager.LoadScene(escenaDespuesDeLogin);
            return;
        }

        // Listener del botón de login
        if (loginButton != null)
        {
            loginButton.onClick.RemoveAllListeners();
            loginButton.onClick.AddListener(OnLoginButtonClicked);
        }
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            servicesInitialized = true;

            // Suscribir después de init
            PlayerAccountService.Instance.SignedIn += OnPlayerAccountSignedIn;

            Debug.Log("[Login] Unity Services OK");
        }
        catch (Exception e)
        {
            // Importante: NO apagar la UI aquí
            servicesInitialized = false;
            Debug.LogError("[Login] Falló InitializeAsync: " + e.Message);
        }
    }

    private async void OnLoginButtonClicked()
    {
        if (loginInProgress) return;              // evita doble-disparo
        if (!servicesInitialized)
        {
            Debug.LogWarning("[Login] Servicios no listos. Reintentando init…");
            await InitializeUnityServices();
            if (!servicesInitialized)
            {
                Debug.LogError("[Login] No fue posible inicializar servicios.");
                return; // UI sigue visible y usable
            }
        }

        try
        {
            loginInProgress = true;
            Debug.Log("[Login] Iniciando Player Accounts…");
            await PlayerAccountService.Instance.StartSignInAsync();
            // Continuación llega a OnPlayerAccountSignedIn()
        }
        catch (Exception e)
        {
            loginInProgress = false;
            Debug.LogError("[Login] Error StartSignInAsync: " + e.Message);
        }
    }

    private async void OnPlayerAccountSignedIn()
    {
        try
        {
            Debug.Log("[Login] Player Accounts OK. Haciendo SignInWithUnityAsync…");
            string accessToken = PlayerAccountService.Instance.AccessToken;
            await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);

            PlayerPrefs.SetString("userType", "unity");
            PlayerPrefs.SetString("playerId", AuthenticationService.Instance.PlayerId);
            PlayerPrefs.Save();

            SceneManager.LoadScene(escenaDespuesDeLogin);
        }
        catch (AuthenticationException e)
        {
            loginInProgress = false;
            Debug.LogError("[Login] AuthException: " + e.Message);
        }
        catch (RequestFailedException e)
        {
            loginInProgress = false;
            Debug.LogError("[Login] RequestFailed: " + e.Message);
        }
        catch (Exception e)
        {
            loginInProgress = false;
            Debug.LogError("[Login] Excepción inesperada: " + e.Message);
        }
    }

    private void OnDestroy()
    {
        if (servicesInitialized)
        {
            try { PlayerAccountService.Instance.SignedIn -= OnPlayerAccountSignedIn; }
            catch { /* no-op */ }
        }
    }
}
