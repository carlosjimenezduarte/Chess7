using UnityEngine;
using TMPro;

public class Stadistics : MonoBehaviour
{
    [Header("Referencias UI (recolectables)")]
    public TMP_Text realCoinText;
    public TMP_Text bagText;
    public TMP_Text chestText;
    public TMP_Text crownText;

    public TMP_Text parchmentText;
    public TMP_Text trophyText;
    public TMP_Text medalText;

    // ----- KILLS ENEMIGOS -----
    public enum EnemyKillType
    {
        PawnRed,
        KnightRed,
        BishopRed,
        RookRed,
        QueenRed,
        KnightBlack,
        BishopBlack,
        RookBlack,
        QueenBlack
    }

    [System.Serializable]
    public struct KillStatUI
    {
        [Tooltip("Clave para PlayerPrefs (debe coincidir). Ej: PawnRed, KnightRed, ...")]
        public EnemyKillType tipo;
        [Tooltip("TMP_Text donde se mostrará el conteo")]
        public TMP_Text text;
        [Tooltip("Meta opcional para mostrar como X/Meta (0 = solo X)")]
        public int goal;
    }

    [Header("Kills de Enemigos (arrastrar 9 TMP_Text)")]
    public KillStatUI[] enemyKillStats; // Tamaño 9

    // ------------------ API recolectables ------------------
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

    // ------------------ API kills (con enum) ------------------
    public static void RegistrarKill(string slotId, EnemyKillType tipo, int cantidad)
    {
        RegistrarKill(slotId, tipo.ToString(), cantidad);
    }

    public static int ObtenerKills(string slotId, EnemyKillType tipo)
    {
        return ObtenerKills(slotId, tipo.ToString());
    }

    // ------------------ API kills (string legacy) ------------------
    public static void RegistrarKill(string slotId, string tipo, int cantidad)
    {
        string key = $"{slotId}_stats_kill_{tipo}";
        int previo = PlayerPrefs.GetInt(key, 0);
        int nuevo = previo + cantidad;
        PlayerPrefs.SetInt(key, nuevo);
        PlayerPrefs.Save();
        Debug.Log($"📊 Kill {tipo}: {previo} -> {nuevo} (+{cantidad})");
    }

    public static int ObtenerKills(string slotId, string tipo)
    {
        return PlayerPrefs.GetInt($"{slotId}_stats_kill_{tipo}", 0);
    }

    // ------------------ UI ------------------
    private void Start()
    {
        RefrescarUI();
    }

    public void RefrescarUI()
    {
        string slotActivo = PlayerPrefs.GetString("slotActivo", "slot1");

        // Recolectables (metas de ejemplo fijas)
        if (realCoinText != null)
            realCoinText.text = $"{ObtenerConteo(slotActivo, TipoObjetoScore.RealCoin)}/1000";
        if (bagText != null)
            bagText.text = $"{ObtenerConteo(slotActivo, TipoObjetoScore.Bag)}/300";
        if (chestText != null)
            chestText.text = $"{ObtenerConteo(slotActivo, TipoObjetoScore.Chest)}/100";
        if (crownText != null)
            crownText.text = $"{ObtenerConteo(slotActivo, TipoObjetoScore.Crown)}/50";

        if (parchmentText != null)
            parchmentText.text = $"0/10";
        if (trophyText != null)
            trophyText.text = $"0/4";
        if (medalText != null)
            medalText.text = $"0/6";

        // Kills enemigos
        if (enemyKillStats != null)
        {
            for (int i = 0; i < enemyKillStats.Length; i++)
            {
                var ui = enemyKillStats[i];
                if (ui.text == null) continue;

                int count = ObtenerKills(slotActivo, ui.tipo);
                ui.text.text = ui.goal > 0 ? $"{count}/{ui.goal}" : $"{count}";
            }
        }
    }
}

