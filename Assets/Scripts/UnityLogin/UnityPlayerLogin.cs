using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Core.Environments; // si quieres SetEnvironmentName
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine.SceneManagement;
using System;
using System.Threading.Tasks;

public class UnityPlayerLogin : MonoBehaviour
{
    [Header("Escena a cargar tras login")]
    public int escenaDespuesDeLogin = 1; // tu Username/Home

    private bool servicesInitialized;
    private bool subscribed;

    private async void Start()
    {
        try
        {
            //var opts = new InitializationOptions()
            //.SetEnvironmentName("production")            
            //.SetProfile("webgl-" + SystemInfo.deviceUniqueIdentifier); // evita sesiones “raras”
            //await UnityServices.InitializeAsync(opts);

            await UnityServices.InitializeAsync();
            servicesInitialized = true;

            if (!subscribed)
            {
                PlayerAccountService.Instance.SignedIn += OnPlayerAccountSignedIn;
                subscribed = true;
            }
        }
        catch (Exception e)
        {
            servicesInitialized = false;
            Debug.LogError($"[Login] InitializeAsync falló: {e.Message}");
        }

        if (servicesInitialized && AuthenticationService.Instance.IsSignedIn)
        {
            SceneManager.LoadScene(escenaDespuesDeLogin);
        }
    }

    // Asigna este método al botón "Sign in with Unity"
    public async void StartPlayerAccountsSignInAsync()
    {
        if (!servicesInitialized)
        {
            Debug.LogWarning("[Login] Servicios aún inicializando…");
            return;
        }

        if (PlayerAccountService.Instance == null)
        {
            Debug.LogError("[Login] PlayerAccountService.Instance == null (revisa paquetes y Project Link).");
            return;
        }

        if (PlayerAccountService.Instance.IsSignedIn)
        {
            await CompleteUnityAuthAsync();
            return;
        }

        try
        {
            Debug.Log("[Login] Llamando StartSignInAsync…");
            await PlayerAccountService.Instance.StartSignInAsync();
            // Al terminar en navegador → OnPlayerAccountSignedIn()
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private async void OnPlayerAccountSignedIn()
    {
        await CompleteUnityAuthAsync();
    }

    private async Task CompleteUnityAuthAsync()
    {
        try
        {
            var accessToken = PlayerAccountService.Instance.AccessToken;
            await AuthenticationService.Instance.SignInWithUnityAsync(accessToken);

            PlayerPrefs.SetString("userType", "unity");
            PlayerPrefs.SetString("playerId", AuthenticationService.Instance.PlayerId);
            PlayerPrefs.Save();

            SceneManager.LoadScene(escenaDespuesDeLogin);
        }
        catch (AuthenticationException ex) { Debug.LogException(ex); }
        catch (RequestFailedException ex)   { Debug.LogException(ex); }
        catch (Exception ex)                { Debug.LogException(ex); }
    }

    private void OnDestroy()
    {
        if (subscribed)
        {
            try { PlayerAccountService.Instance.SignedIn -= OnPlayerAccountSignedIn; }
            catch { }
        }
    }
}
