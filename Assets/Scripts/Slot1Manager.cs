using UnityEngine;
using UnityEngine.SceneManagement;

public class Slot1Manager : MonoBehaviour
{
    [Header("Paneles del Slot 1")]
    public GameObject panelPlayFirst;
    public GameObject panelPlay;
    public GameObject panelReset;

    private string slotKey = "slot1_state";

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
        // Guardar que este es el slot activo
        PlayerPrefs.SetString("slotActivo", "slot1");

        // Activar slot si es la primera vez
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
        // 🔄 Borrar progreso del slot
        PlayerPrefs.DeleteKey("slot1_progress"); 
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
