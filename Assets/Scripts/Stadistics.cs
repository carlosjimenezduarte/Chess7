using UnityEngine;
using TMPro;

public class Stadistics : MonoBehaviour
{
    [Header("Referencias UI (arrastrar en Inspector)")]
    public TMP_Text realCoinText;
    public TMP_Text bagText;
    public TMP_Text chestText;
    public TMP_Text crownText;

    public TMP_Text parchmentText;
    public TMP_Text trophyText;
    public TMP_Text medalText;

    // 🔹 Actualizar estadísticas globales
    public static void RegistrarObjeto(string slotId, TipoObjetoScore tipo, int cantidad)
    {
        string key = slotId + "_stats_" + tipo.ToString();
        int previo = PlayerPrefs.GetInt(key, 0);
        int nuevo = previo + cantidad;

        PlayerPrefs.SetInt(key, nuevo);
        PlayerPrefs.Save();

        Debug.Log($"📊 Estadística {tipo}: {previo} -> {nuevo}");
    }

    public static int ObtenerConteo(string slotId, TipoObjetoScore tipo)
    {
        return PlayerPrefs.GetInt(slotId + "_stats_" + tipo.ToString(), 0);
    }

    private void Start()
    {
        string slotActivo = PlayerPrefs.GetString("slotActivo", "slot1");

        // 🔹 Mostrar acumulados (ejemplo con metas fijas)
        if (realCoinText != null)
            realCoinText.text = $"{ObtenerConteo(slotActivo, TipoObjetoScore.RealCoin)}/1000";
        if (bagText != null)
            bagText.text = $"{ObtenerConteo(slotActivo, TipoObjetoScore.Bag)}/300";
        if (chestText != null)
            chestText.text = $"{ObtenerConteo(slotActivo, TipoObjetoScore.Chest)}/100";
        if (crownText != null)
            crownText.text = $"{ObtenerConteo(slotActivo, TipoObjetoScore.Crown)}/50";

        if (parchmentText != null)
            parchmentText.text = $"0/10"; // 🔹 Slot para futuro
        if (trophyText != null)
            trophyText.text = $"0/4";    // 🔹 Slot para futuro
        if (medalText != null)
            medalText.text = $"0/6";     // 🔹 Slot para futuro
    }
}
