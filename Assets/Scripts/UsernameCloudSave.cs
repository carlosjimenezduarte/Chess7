using UnityEngine;
using UnityEngine.UI;
using Unity.Services.CloudSave;
using Unity.Services.Authentication;
using TMPro;
using System.Collections.Generic;
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
    public Image selectedAvatar;

    [Header("Feedback Texts")]
    public TMP_Text msgEmptyName;
    public TMP_Text msgTooLong;
    public TMP_Text msgInvalidChars;
    public TMP_Text msgNotAuthenticated;
    public TMP_Text msgSaving;

    [Header("Avatars")]
    public List<AvatarButtonData> avatarButtons;

    private string currentAvatarId = "";
    private static int userCounter = 1;

    private void Start()
    {
        confirmButton.onClick.AddListener(OnConfirmClicked);
        HideAllMessages();

        foreach (var avatar in avatarButtons)
        {
            AvatarButtonData localAvatar = avatar;
            avatar.button.onClick.AddListener(() =>
            {
                SetSelectedAvatar(localAvatar.avatarImage.sprite, localAvatar.avatarId);
            });
        }

        if (string.IsNullOrEmpty(currentAvatarId) && avatarButtons.Count > 0)
        {
            SetSelectedAvatar(avatarButtons[0].avatarImage.sprite, avatarButtons[0].avatarId);
        }

        usernameInput.onSelect.AddListener((eventData) =>
        {
            usernameInput.ActivateInputField();
        });

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
        HideAllMessages();
        
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            ShowMessage(msgNotAuthenticated);
            return;
        }

        try
        {
            var playerData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string> { "username", "avatarId" });

            if (playerData.ContainsKey("username"))
            {
                string savedUsername = playerData["username"].Value.GetAs<string>();
                string savedAvatarId = playerData["avatarId"].Value.GetAs<string>();

                PlayerPrefs.SetString("username", savedUsername);
                PlayerPrefs.SetString("avatarId", savedAvatarId);
                PlayerPrefs.Save();

                UnityEngine.SceneManagement.SceneManager.LoadScene(2); // Home
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error checking registration: " + e.Message);
        }
    }

    public async void OnConfirmClicked()
    {
        HideAllMessages();

        string username = Capitalize(usernameInput.text.Trim());

        if (string.IsNullOrEmpty(username))
        {
            ShowMessage(msgEmptyName);
            return;
        }
        if (username.Length > 12)
        {
            ShowMessage(msgTooLong);
            return;
        }
        if (!System.Text.RegularExpressions.Regex.IsMatch(username, @"^[a-zA-Z0-9]+$"))
        {
            ShowMessage(msgInvalidChars);
            return;
        }
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            ShowMessage(msgNotAuthenticated);
            return;
        }

        try
        {
            ShowMessage(msgSaving);

            int userNumber = userCounter++;
            var playerData = new Dictionary<string, object>
            {
                { "username", username },
                { "avatarId", currentAvatarId },
                { "joined_at", System.DateTime.UtcNow.ToString("o") },
                { "userNumber", userNumber }
            };

            await CloudSaveService.Instance.Data.Player.SaveAsync(playerData);

            PlayerPrefs.SetString("username", username);
            PlayerPrefs.SetString("avatarId", currentAvatarId);
            PlayerPrefs.SetString("joined_at", System.DateTime.UtcNow.ToString("o"));
            PlayerPrefs.SetInt("userNumber", userNumber);
            PlayerPrefs.Save();

            UnityEngine.SceneManagement.SceneManager.LoadScene(2); // Home
        }
        catch (System.Exception e)
        {
            Debug.LogError("Username Save Error: " + e.Message);
        }
    }

    private void HideAllMessages()
    {
        if (msgEmptyName) msgEmptyName.gameObject.SetActive(false);
        if (msgTooLong) msgTooLong.gameObject.SetActive(false);
        if (msgInvalidChars) msgInvalidChars.gameObject.SetActive(false);
        if (msgNotAuthenticated) msgNotAuthenticated.gameObject.SetActive(false);
        if (msgSaving) msgSaving.gameObject.SetActive(false);
    }

    private void ShowMessage(TMP_Text msg)
    {
        HideAllMessages();
        if (msg != null) msg.gameObject.SetActive(true);
    }

    private string Capitalize(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpper(input[0]) + input.Substring(1);
    }
}
