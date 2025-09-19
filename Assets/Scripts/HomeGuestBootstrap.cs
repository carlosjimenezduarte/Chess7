using UnityEngine;

public class HomeGuestBootstrap : MonoBehaviour
{
    private const string SLOT_ID = "guest";

    void Awake()
    {
        // Forzar que el invitado siga activo (por si venimos de otros flujos)
        PlayerPrefs.SetString("slotActivo", SLOT_ID);

        // Asegurar que el modo invitado existe
        if (!PlayerPrefs.HasKey("guest_state"))
        {
            PlayerPrefs.SetString("guest_state", "active");
            if (!PlayerPrefs.HasKey($"{SLOT_ID}_nivelMax"))
                PlayerPrefs.SetInt($"{SLOT_ID}_nivelMax", 1);
        }

        PlayerPrefs.Save();
    }
}
