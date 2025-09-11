using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

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
        // 🔄 Borrar TODO el progreso asociado al slot1
        BorrarProgresoSlot("slot1");

        // Marcar slot como vacío
        PlayerPrefs.SetString(slotKey, "empty");
        PlayerPrefs.Save();

        Debug.Log("🧹 Slot1 completamente reiniciado.");
        UpdateUI();
    }

    public void OnConfirmNo()
    {
        PlayerPrefs.SetString(slotKey, "active");
        PlayerPrefs.Save();
        UpdateUI();
    }

    // 🔹 Método auxiliar para limpiar todas las claves de un slot específico
    private void BorrarProgresoSlot(string slotId)
    {
        // Lista de claves que sabemos que existen
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

        // Borrar progresos de niveles individuales (hasta 300 como margen)
        for (int lvl = 1; lvl <= 300; lvl++)
        {
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_completed");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_score");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_keys");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_diamond");   // boolean “intento perfecto”
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_diamonds");  // 👈 NUEVA: conteo real (plural)
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_parchment");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_trophy");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_medal");
            PlayerPrefs.DeleteKey(slotId + "_level_" + lvl + "_masterKey");
        }


        // Borrar acumulados globales
        foreach (var c in claves)
        {
            PlayerPrefs.DeleteKey(slotId + c);
        }

        PlayerPrefs.Save();
    }
}
