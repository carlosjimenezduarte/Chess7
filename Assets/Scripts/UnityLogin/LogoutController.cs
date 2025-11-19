using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Authentication;
using Unity.Services.Authentication.PlayerAccounts;

public class LogoutController : MonoBehaviour
{
    [Header("Panel de confirmación")]
    public GameObject panelConfirmLogout; // arrastra tu panel de UI aquí

    [Header("Escenas")]
    [Tooltip("Escena a la que volver tras cerrar sesión (ej: 0 = MainMenu)")]
    public int mainMenuBuildIndex = 0;

    // === Botón "Regresar" en Home (abre el panel) ===
    public void OnLogoutRequest()
    {
        if (panelConfirmLogout != null)
            panelConfirmLogout.SetActive(true);
    }

    // === Botón "Sí" ===
    public void OnConfirmYes()
    {
        // 1. Cerrar sesión Unity Authentication
        if (AuthenticationService.Instance != null && AuthenticationService.Instance.IsSignedIn)
        {
            AuthenticationService.Instance.SignOut();
            Debug.Log("✅ AuthenticationService cerrado");
        }

        // 2. Cerrar sesión PlayerAccounts
        PlayerAccountService.Instance.SignOut();
        Debug.Log("✅ PlayerAccountService cerrado");

        // 3. Borrar datos básicos de usuario (opcional)
        PlayerPrefs.DeleteKey("userType");
        PlayerPrefs.DeleteKey("playerId");
        PlayerPrefs.Save();

        // 4. Regresar al MainMenu
        SceneManager.LoadScene(mainMenuBuildIndex);
    }

    // === Botón "No" ===
    public void OnConfirmNo()
    {
        if (panelConfirmLogout != null)
            panelConfirmLogout.SetActive(false);
    }
}
