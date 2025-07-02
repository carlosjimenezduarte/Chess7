using UnityEngine;
using UnityEngine.UI;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using Unity.Services.Core;
using System.Collections.Generic;
using TMPro;
using System.Threading.Tasks;

[System.Serializable]
public class AvatarButtonData
{
    public Button button;
    public Image avatarImage;
    public string avatarId;
}

public class UsernameCloudSave : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField usernameInput;
    public Button confirmButton;
    public TMP_Text feedbackText;
    public Image selectedAvatar;

    [Header("Avatars")]
    public List<AvatarButtonData> avatarButtons;

    private string currentAvatarId = "";

    // Contador de usuarios para asignar un número único
    private static int userCounter = 1; // Puede guardarse en Cloud Save si se quiere persistente

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
        feedbackText.text = "";

        foreach (var avatar in avatarButtons)
        {
            AvatarButtonData localAvatar = avatar;
            avatar.button.onClick.AddListener(() =>
            {
                SetSelectedAvatar(localAvatar.avatarImage.sprite, localAvatar.avatarId);
            });
        }

        // Valor por defecto: primer avatar
        if (string.IsNullOrEmpty(currentAvatarId) && avatarButtons.Count > 0)
        {
            SetSelectedAvatar(avatarButtons[0].avatarImage.sprite, avatarButtons[0].avatarId);
        }

         usernameInput.onSelect.AddListener((eventData) =>
    {
        usernameInput.ActivateInputField(); // Esto activa el campo solo cuando se selecciona
    });

        // Verificar si el usuario ya está registrado
        CheckIfUserIsRegistered();
    }

    public void SetSelectedAvatar(Sprite sprite, string avatarId)
    {
        if (selectedAvatar != null)
            selectedAvatar.sprite = sprite;

        currentAvatarId = avatarId;
    }

    private async void CheckIfUserIsRegistered()
    {
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            feedbackText.text = "Error: user not authenticated.";
            return;
        }

        try
        {
            // Usamos LoadAsync para verificar si el nombre de usuario ya está guardado
            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "username" });

            // Si encuentra la clave "username", significa que el usuario ya está registrado
            if (playerData.ContainsKey("username"))
            {
                feedbackText.text = "User already registered.";
                UnityEngine.SceneManagement.SceneManager.LoadScene("Home"); // Redirigir a la escena Home
            }
        }
        catch (System.Exception e)
        {
            // Si no se encuentra la clave "username" o hay algún error, continuar con el registro
            Debug.LogError("Error checking registration: " + e.Message);
        }
    }

    private async void OnConfirmClicked()
    {
        string username = Capitalize(usernameInput.text.Trim());

        // Validación: básico
        if (string.IsNullOrEmpty(username))
        {
            feedbackText.text = "The name cannot be empty.";
            return;
        }
        if (username.Length > 12)
        {
            feedbackText.text = "Maximum 12 characters.";
            return;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
        {
            feedbackText.text = "Use only letters and numbers (no spaces or symbols).";
            return;
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            feedbackText.text = "Error: user not authenticated.";
            return;
        }

        try
        {
            feedbackText.text = "Saving username...";

            // Asignar un número único al usuario
            int userNumber = userCounter++;
            string userIdKey = "user_number_" + username.ToLower(); // Usamos el nombre como base para la clave global

            // Guardar los datos del usuario en Player Data
            var playerData = new Dictionary<string, object>
            {
                { "username", username },
                { "avatarId", currentAvatarId },
                { "joined_at", System.DateTime.UtcNow.ToString("o") },
                { "userNumber", userNumber } // Guardamos el número único aquí
            };

            // Guardar en Player Data en Cloud Save
            await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);

            feedbackText.text = "Saved successfully.";
            UnityEngine.SceneManagement.SceneManager.LoadScene("Home"); // Redirigir a la escena Home
        }
        catch (System.Exception e)
        {
            feedbackText.text = "An unexpected error occurred.";
            Debug.LogError("Username Save Error: " + e.Message);
        }
    }

    private string Capitalize(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input.Substring(1);
    }
}
