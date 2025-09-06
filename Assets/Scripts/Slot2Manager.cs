using UnityEngine;
using UnityEngine.SceneManagement;

public class Slot2Manager : MonoBehaviour
{
    [Header("Paneles del Slot 2")]
    public GameObject panelPlayFirst;
    public GameObject panelPlay;
    public GameObject panelReset;

    private string slotKey = "slot2_state";

    private void Start()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        string state = PlayerPrefs.GetString(slotKey, "empty");

        panelPlayFirst.SetActive(state == "empty");
        panelPlay.SetActive(state == "active");
        panelReset.SetActive(state == "reset_pending");
    }

    public void OnPlay()
    {
        // 👉 Guardamos que este es el slot activo
        PlayerPrefs.SetString("slotActivo", "slot2");

        // Activamos el slot si estaba vacío
        PlayerPrefs.SetString(slotKey, "active");
        PlayerPrefs.Save();

        SceneManager.LoadScene(4); // Ir a GameHome
    }

    public void OnReiniciar()
    {
        PlayerPrefs.SetString(slotKey, "reset_pending");
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void OnConfirmYes()
    {
        // 🔄 Borrar progreso del slot 2
        PlayerPrefs.DeleteKey("slot2_progress"); 
        PlayerPrefs.SetString(slotKey, "empty");
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void OnConfirmNo()
    {
        PlayerPrefs.SetString(slotKey, "active");
        PlayerPrefs.Save();
        UpdateUI();
    }
}
