using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using System;
using System.Threading.Tasks;

public class UnityPlayerLogin : MonoBehaviour
{
    private bool servicesInitialized = false;

    private async void Start()
    {
        await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();
            servicesInitialized = true;

            Debug.Log("Unity Services inicializados correctamente");

            // Suscribimos el evento solo si está disponible
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

            // Redirigir a escena de username
            UnityEngine.SceneManagement.SceneManager.LoadScene("Username");
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
}
