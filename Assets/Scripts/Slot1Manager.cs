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
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsPawn"); // 👈 per-level best (anti-farmeo)
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsKnight"); // NUEVO
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsBishop");
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsRook");   // 👈 NUEVO
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsQueen");  // 👈 NUEVO
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsRookBlack");   // 👈 NUEVO
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsQueenBlack");  // 👈 NUEVO
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsBishopBlack");  // NUEVO
            PlayerPrefs.DeleteKey($"{slotId}_level_{lvl}_killsKnightBlack");  // NUEVO


            // 🧹 Achievements y Notificaciones
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.PawnIsGold}"); // y futuros
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Liberador}");

            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Keys10}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Keys50}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Keys100}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Keys250}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Keys500}");


            PlayerPrefs.DeleteKey($"{slotId}_scoreFromAchievements");

            PlayerPrefs.DeleteKey($"{slotId}_notif_queue");

            // 🔹 Rango máximo alcanzado (stadistics)            
            PlayerPrefs.DeleteKey($"{slotId}_rankMaxIndex");

            // 🔹 Achievements de rango (17)
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_CaminanteMisterio}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_CaballeroVerdad}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_ElegidoEstrellas}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_AlmaVictoriosa}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_VencedorTiempo}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_CaballeroPaz}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_MaestroTableros}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_MaestroSilencio}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_MaestroIndomable}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_GuardianReino}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_GuardianUmbral}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_GuardianLuz}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_AprendizRey}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_ReyEstrellas}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_ReyMisterio}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_ReyLibre}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Rank_ReyCoronado}");

            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.FirstTrophy}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.SecondTrophy}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.ThirdTrophy}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.FourthTrophy}");

            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Trophy1}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Trophy2}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Trophy3}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Trophy4}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Diamonds10}");
            // 🔹 Achievements de DIAMANTES (acumulados)
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Diamonds25}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Diamonds50}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Diamonds100}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Diamonds200}");

            // (Opcional) Si usas dismiss persistente para estos paneles en Notificaciones:
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Diamonds10}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Diamonds25}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Diamonds50}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Diamonds100}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Diamonds200}_dismissed");

            // 🔹 Achievements de MONEDA REAL (acumulados)
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RealCoin10}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RealCoin100}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RealCoin250}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RealCoin500}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RealCoin1000}");

            // (Opcional) flags de dismiss en Notificaciones
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RealCoin10}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RealCoin100}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RealCoin250}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RealCoin500}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RealCoin1000}_dismissed");

            // 🔹 Achievements de BOLSA (acumulados)
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Bag10}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Bag50}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Bag100}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Bag200}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Bag300}");

            // (Opcional) flags de dismiss en Notificaciones
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Bag10}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Bag50}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Bag100}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Bag200}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Bag300}_dismissed");
            // 🔹 Achievements de COFRE (acumulados)
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Chest10}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Chest25}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Chest50}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Chest75}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Chest100}");

            // (Opcional) flags de dismiss en Notificaciones
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Chest10}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Chest25}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Chest50}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Chest75}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Chest100}_dismissed");

            // 🔹 Achievements de CORONA (acumulados)
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Crown10}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Crown20}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Crown30}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Crown40}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.Crown50}");

            // (Opcional) flags de dismiss en Notificaciones
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Crown10}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Crown20}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Crown30}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Crown40}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Crown50}_dismissed");

            // 🔹 Achievements de KILLS Peón Rojo (acumulados)
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedPawn1}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedPawn25}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedPawn50}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedPawn75}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedPawn100}");

            // (Opcional) flags de dismiss en Notificaciones
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedPawn1}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedPawn25}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedPawn50}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedPawn75}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedPawn100}_dismissed");

            // 🔹 Achievements de KILLS Caballo Rojo (acumulados)
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedKnight1}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedKnight5}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedKnight15}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedKnight20}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedKnight30}");

            // (Opcional) flags de dismiss en Notificaciones
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedKnight1}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedKnight5}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedKnight15}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedKnight20}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedKnight30}_dismissed");

            // 🔹 Achievements de KILLS Alfil Rojo (acumulados)
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedBishop1}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedBishop5}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedBishop15}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedBishop20}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedBishop30}");

            // (Opcional) flags de dismiss en Notificaciones
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedBishop1}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedBishop5}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedBishop15}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedBishop20}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedBishop30}_dismissed");

            // 🔹 Achievements de KILLS Torre Roja
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedRook1}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedRook10}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedRook20}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedRook30}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedRook50}");

            // (Opcional) dismiss
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedRook1}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedRook10}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedRook20}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedRook30}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedRook50}_dismissed");

            // 🔹 Achievements de KILLS Reina Roja
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedQueen1}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedQueen15}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedQueen30}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedQueen40}");
            PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.RedQueen50}");

            // (Opcional) dismiss
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedQueen1}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedQueen15}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedQueen30}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedQueen40}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.RedQueen50}_dismissed");


            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.FirstTrophy}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.SecondTrophy}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.ThirdTrophy}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.FourthTrophy}_dismissed");

            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Trophy1}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Trophy2}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Trophy3}_dismissed");
            PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.Trophy4}_dismissed");

// 🔹 Achievements de KILLS Negras (Torre/Alfil/Caballo/Reina) — 1,3,5,7,10
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackRook1}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackRook3}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackRook5}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackRook7}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackRook10}");

PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackBishop1}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackBishop3}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackBishop5}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackBishop7}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackBishop10}");

PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackKnight1}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackKnight3}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackKnight5}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackKnight7}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackKnight10}");

PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackQueen1}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackQueen3}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackQueen5}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackQueen7}");
PlayerPrefs.DeleteKey($"{slotId}_ach_{AchievementId.BlackQueen10}");

// (Opcional) flags de dismiss en Notificaciones
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackRook1}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackRook3}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackRook5}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackRook7}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackRook10}_dismissed");

PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackBishop1}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackBishop3}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackBishop5}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackBishop7}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackBishop10}_dismissed");

PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackKnight1}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackKnight3}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackKnight5}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackKnight7}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackKnight10}_dismissed");

PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackQueen1}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackQueen3}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackQueen5}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackQueen7}_dismissed");
PlayerPrefs.DeleteKey($"{slotId}_notif_{AchievementId.BlackQueen10}_dismissed");





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

        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_PawnRed"); // 👈 global visible en Stadistics
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_KnightRed"); // NUEVO
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_BishopRed"); 
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_RookRed");   // 👈 NUEVO
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_QueenRed");  // 👈 NUEVO
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_RookBlack");   // 👈 NUEVO
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_QueenBlack");  // 👈 NUEVO
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_BishopBlack");  // NUEVO
        PlayerPrefs.DeleteKey($"{slotId}_stats_kill_KnightBlack");  // NUEVO

        PlayerPrefs.DeleteKey($"{slotId}_lastRunDebt");

        
        PlayerPrefs.Save();
    }

}
