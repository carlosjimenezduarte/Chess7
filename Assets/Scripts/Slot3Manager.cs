using UnityEngine;
using UnityEngine.SceneManagement;

public class Slot3Manager : MonoBehaviour
{
    [Header("Paneles del Slot 3")]
    public GameObject panelPlayFirst;
    public GameObject panelPlay;
    public GameObject panelReset;

    private string slotKey = "slot3_state";

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
        PlayerPrefs.SetString("slotActivo", "slot3");        
        PlayerPrefs.SetString(slotKey, "active");
        PlayerPrefs.Save();
        SceneManager.LoadScene(4); // GameHome
    }

    public void OnReiniciar()
    {
        PlayerPrefs.SetString(slotKey, "reset_pending");
        PlayerPrefs.Save();
        UpdateUI();
    }

    public void OnConfirmYes()
    {
        PlayerPrefs.DeleteKey("slot3_progress");
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
