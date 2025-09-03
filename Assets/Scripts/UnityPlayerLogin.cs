using UnityEngine;
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
