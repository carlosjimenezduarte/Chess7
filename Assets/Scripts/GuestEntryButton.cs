using UnityEngine;
using UnityEngine.SceneManagement;

public class GuestEntryButton : MonoBehaviour
{
    // Escena destino tras pulsar "Continuar como invitado"
    [SerializeField] private int homeGuestBuildIndex = 13;

    private const string SLOT_ID = "guest";
    private const string SLOT_STATE_KEY = "guest_state";
    private const string USER_TYPE_KEY = "userType";

    // Ajustes globales (si aún no tienes llaves, estas son sugeridas)
    private const string MUSIC_VOL = "settings_musicVolume";
    private const string SFX_VOL   = "settings_sfxVolume";
    private const string LANG_KEY  = "settings_language";
    private const string MUSIC_ON  = "settings_musicOn";
    private const string SFX_ON    = "settings_sfxOn";

    public void OnContinueAsGuest()
    {
        // Tipo de usuario
        PlayerPrefs.SetString(USER_TYPE_KEY, "guest");

        // Marcar slot activo = guest
        PlayerPrefs.SetString("slotActivo", SLOT_ID);

        // Si es primera vez, inicializa estructura mínima del slot guest
        if (!PlayerPrefs.HasKey(SLOT_STATE_KEY))
        {
            // Estado del pseudo-slot de invitado
            PlayerPrefs.SetString(SLOT_STATE_KEY, "active");

            // Progreso base (lo mínimo que GameHomeManager espera)
            PlayerPrefs.SetInt($"{SLOT_ID}_nivelMax", 1);
            PlayerPrefs.SetInt($"{SLOT_ID}_scoreTotal", 0);
            PlayerPrefs.SetInt($"{SLOT_ID}_keysTotal", 0);
            PlayerPrefs.SetInt($"{SLOT_ID}_diamondsTotal", 0);
            PlayerPrefs.SetInt($"{SLOT_ID}_parchmentsTotal", 0);
            PlayerPrefs.SetInt($"{SLOT_ID}_trophiesTotal", 0);
            PlayerPrefs.SetInt($"{SLOT_ID}_medalsTotal", 0);
            PlayerPrefs.SetInt($"{SLOT_ID}_masterKeysTotal", 0);
        }

        // Asegurar ajustes globales para que Settings/Tutorial funcionen en HomeGuest
        if (!PlayerPrefs.HasKey(MUSIC_VOL)) PlayerPrefs.SetFloat(MUSIC_VOL, 0.6f);
        if (!PlayerPrefs.HasKey(SFX_VOL))   PlayerPrefs.SetFloat(SFX_VOL,   0.8f);
        if (!PlayerPrefs.HasKey(LANG_KEY))  PlayerPrefs.SetString(LANG_KEY, "es"); // o "en"
        if (!PlayerPrefs.HasKey(MUSIC_ON))  PlayerPrefs.SetInt(MUSIC_ON, 1);
        if (!PlayerPrefs.HasKey(SFX_ON))    PlayerPrefs.SetInt(SFX_ON,   1);

        PlayerPrefs.Save();

        // Ir a HomeGuest (réplica de Home)
        SceneManager.LoadScene(homeGuestBuildIndex);
    }
}
