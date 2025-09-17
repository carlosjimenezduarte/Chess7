using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public enum AchievementId
{
    PawnIsGold,
    Liberador,
    Keys10, Keys50, Keys100, Keys250, Keys500,
    Diamonds10, Diamonds25, Diamonds50, Diamonds100, Diamonds200,
    Crown10, Crown20, Crown30, Crown40, Crown50,
    Chest10, Chest25, Chest50, Chest75, Chest100,
    Bag10, Bag50, Bag100, Bag200, Bag300,
    RealCoin10, RealCoin100, RealCoin250, RealCoin500, RealCoin1000, 
    BlackQueen1, BlackQueen3, BlackQueen5, BlackQueen7, BlackQueen10,
    BlackRook1, BlackRook3, BlackRook5, BlackRook7, BlackRook10,
    BlackBishop1, BlackBishop3, BlackBishop5, BlackBishop7, BlackBishop10,
    BlackKnight1, BlackKnight3, BlackKnight5, BlackKnight7, BlackKnight10,
    RedQueen1, RedQueen15, RedQueen30, RedQueen40, RedQueen50,
    RedRook1, RedRook10, RedRook20, RedRook30, RedRook40,
    RedBishop1, RedBishop5, RedBishop15, RedBishop20, RedBishop30,
    RedKnight1, RedKnight5,RedKnight15, RedKnight20, RedKnight30,
    RedPawn1, RedPawn25, RedPawn50, RedPawn75, RedPawn100,
    FirstTrophy, SecondTrophy, ThirdTrophy, FourthTrophy,
    All100,

    LegendaryChest,    
    
    Trophy1, Trophy2, Trophy3, Trophy4,
    Score1, Score2, Score3, Score4, Score5,
    Score6, Score7, Score8, Score9, Score10,
    Score11, Score12, Score13, Score14, Score15,
    Score16,
  

   
    Rank_CaminanteMisterio,
    Rank_CaballeroVerdad,
    Rank_ElegidoEstrellas,
    Rank_AlmaVictoriosa,
    Rank_VencedorTiempo,
    Rank_CaballeroPaz,
    Rank_MaestroTableros,
    Rank_MaestroSilencio,
    Rank_MaestroIndomable,
    Rank_GuardianReino,
    Rank_GuardianUmbral,
    Rank_GuardianLuz,
    Rank_AprendizRey,
    Rank_ReyEstrellas,
    Rank_ReyMisterio,
    Rank_ReyLibre,
    Rank_ReyCoronado
}

public static class AchievementsManager
{
    // Config
    private const int SCORE_PAWN_IS_GOLD = 1000;

    private const int SCORE_LIBERADOR = 5000;
    private const int OFFSET_NIVELES = 8;
    public static int Liberador_LevelGate = -1;

    // Si quieres limitar el logro a un nivel lógico concreto, pon su id aquí.
    // -1 => válido en cualquier nivel.
    public static int PawnIsGold_LevelGate = -1;

    // --- API pública ---
    public static bool IsUnlocked(string slotId, AchievementId id)
    {
        return PlayerPrefs.GetInt(KeyAch(slotId, id), 0) == 1;
    }

    /// <summary>Intento de desbloqueo: aplica gating, “solo una vez”, suma score, dispara notificación, refresca Honors.</summary>
    public static bool TryUnlock(string slotId, AchievementId id, int scoreReward = 0, string notifTitle = null, string notifBody = null)
    {
        if (IsUnlocked(slotId, id))
        {
            Debug.Log($"[Ach] {id} ya estaba desbloqueado para {slotId}.");
            return false;
        }

        PlayerPrefs.SetInt(KeyAch(slotId, id), 1);

        // Score de logro (si aplica)
        if (scoreReward != 0)
        {
            int totalScore = PlayerPrefs.GetInt($"{slotId}_scoreTotal", 0) + scoreReward;
            PlayerPrefs.SetInt($"{slotId}_scoreTotal", totalScore);
            // (Opcional) tracking específico de score por logros
            int achScore = PlayerPrefs.GetInt($"{slotId}_scoreFromAchievements", 0) + scoreReward;
            PlayerPrefs.SetInt($"{slotId}_scoreFromAchievements", achScore);
        }

        PlayerPrefs.Save();

        // Notificación persistente (queda hasta que el usuario la cierre)
        if (!string.IsNullOrEmpty(notifTitle))
        {
            NotificationCenter.Instance?.OnAchievementUnlocked(
                slotId,
                id,
                notifTitle,
                notifBody ?? string.Empty
            );
        }


        // Aviso opcional para paneles de Honors que “escuchen”
        OnAchievementUnlocked?.Invoke(slotId, id);

        Debug.Log($"[Ach] ✅ Desbloqueado {id} para {slotId}. Score+={scoreReward}");
        return true;
    }

    // --- Logro específico: El Peón vale Oro ---
    public static void ReportPawnCoronation()
    {
        string slotId = PlayerPrefs.GetString("slotActivo", "slot1");

        // Gate por nivel, si lo quieres activo
        if (PawnIsGold_LevelGate >= 0)
        {
            int sceneIndex = SceneManager.GetActiveScene().buildIndex;
            int nivelLogico = sceneIndex - OFFSET_NIVELES;
            if (nivelLogico != PawnIsGold_LevelGate)
            {
                Debug.Log($"[Ach] Coronación detectada pero nivel {nivelLogico} != gate {PawnIsGold_LevelGate}. No desbloquea.");
                return;
            }
        }

        // Desbloquear (solo una vez)
        TryUnlock(
            slotId,
            AchievementId.PawnIsGold,
            SCORE_PAWN_IS_GOLD,
            "Logro desbloqueado: El Peón vale Oro",
            $"+{SCORE_PAWN_IS_GOLD} puntos por coronar al Peón."
        );
    }

    public static void ReportLiberador()
    {
        string slotId = PlayerPrefs.GetString("slotActivo", "slot1");

        // (Opcional) Gateo por nivel — actualmente desactivado:
        if (Liberador_LevelGate >= 0)
        {
            int sceneIndex = UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex;
            const int OFFSET_NIVELES = 8;
            int nivelLogico = sceneIndex - OFFSET_NIVELES;
            if (nivelLogico != Liberador_LevelGate)
            {
                Debug.Log($"[Ach] Liberador detectado pero nivel {nivelLogico} != gate {Liberador_LevelGate}. No desbloquea.");
                return;
            }
        }

        TryUnlock(
            slotId,
            AchievementId.Liberador,
            SCORE_LIBERADOR,
            "Logro desbloqueado: Liberador",
            $"+{SCORE_LIBERADOR} puntos por liberar a todas las fichas aliadas en un mapa."
        );
    }

    // --- Evento para UIs (Honors) que quieran reaccionar al vuelo ---
    public static event Action<string, AchievementId> OnAchievementUnlocked;

    // --- Helpers ---
    private static string KeyAch(string slotId, AchievementId id) => $"{slotId}_ach_{id}";

    // ----- Hitos de llaves -----
    static readonly (AchievementId id, int threshold, int reward, string title, string body)[] KEY_MILESTONES =
    {
    (AchievementId.Keys10,   10,   0, "Logro desbloqueado: 10 llaves",   "+0 puntos por alcanzar 10 llaves."),
    (AchievementId.Keys50,   50,   0, "Logro desbloqueado: 50 llaves",   "+0 puntos por alcanzar 50 llaves."),
    (AchievementId.Keys100,  100,  0, "Logro desbloqueado: 100 llaves",  "+0 puntos por alcanzar 100 llaves."),
    (AchievementId.Keys250,  250,  0, "Logro desbloqueado: 250 llaves",  "+0 puntos por alcanzar 250 llaves."),
    (AchievementId.Keys500,  500,  0, "Logro desbloqueado: 500 llaves",  "+0 puntos por alcanzar 500 llaves."),
    };


    // ----- Verificador único: llámalo pasándole el total global de llaves -----
    public static void ReportKeysProgress(int totalKeys)
    {
        string slotId = PlayerPrefs.GetString("slotActivo", "slot1");
        foreach (var m in KEY_MILESTONES)
        {
            if (totalKeys >= m.threshold && !IsUnlocked(slotId, m.id))
            {
                TryUnlock(slotId, m.id, m.reward, m.title, m.body);
            }
        }

    }

}
