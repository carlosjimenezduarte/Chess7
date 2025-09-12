using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class Slot1Manager : MonoBehaviour
{
    [Header("Paneles del Slot 1")]
    public GameObject panelPlayFirst;
    public GameObject panelPlay;
    public GameObject panelReset;

    [Header("Texto de progreso global (opcional)")]
    public TMP_Text levelsProgressText; // 👈 arrastra aquí el mismo TMP del GameHome si quieres

    private string slotKey = "slot1_state";
    private const int TOTAL_NIVELES = 204;

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
        PlayerPrefs.SetString("slotActivo", "slot1");
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
        BorrarProgresoSlot("slot1");

        PlayerPrefs.SetString(slotKey, "empty");
        PlayerPrefs.Save();

        Debug.Log("🧹 Slot1 completamente reiniciado.");
        UpdateUI();

        // 🔹 Refrescar visualmente el contador de progreso
        if (levelsProgressText != null)
            levelsProgressText.text = $"0/{TOTAL_NIVELES}";
    }

    public void OnConfirmNo()
    {
        PlayerPrefs.SetString(slotKey, "active");
        PlayerPrefs.Save();
        UpdateUI();
    }

    private void BorrarProgresoSlot(string slotId)
    {
        string[] claves = {
        "_nivelMax",
        "_scoreTotal",
        "_keysTotal",
        "_diamondsTotal",
        "_parchmentsTotal",
        "_trophiesTotal",
        "_medalsTotal",
        "_masterKeysTotal"
    };

        for (int lvl = 1; lvl <= 300; lvl++)
        {
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_completed");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_score");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_keys");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_diamond");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_diamonds");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_parchment");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_trophy");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_medal");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_masterKey");

            // ✅ Claves nuevas que sí usa ProgressSaver
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_bags");
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_crowns");
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_chests");
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_coins");
        }


        foreach (var c in claves)
        {
            PlayerPrefs.DeleteKey(slotId + c);
        }

        // 🔹 Borrar estadísticas de Stadistics (los 4 objetos básicos)
        PlayerPrefs.DeleteKey(slotId + "_stats_" + TipoObjetoScore.Bag.ToString());
        PlayerPrefs.DeleteKey(slotId + "_stats_" + TipoObjetoScore.Chest.ToString());
        PlayerPrefs.DeleteKey(slotId + "_stats_" + TipoObjetoScore.RealCoin.ToString());
        PlayerPrefs.DeleteKey(slotId + "_stats_" + TipoObjetoScore.Crown.ToString());

        PlayerPrefs.Save();
    }

}
