using UnityEngine;

public class ProgressSaver : MonoBehaviour
{
    // 👉 Llamar a este método al finalizar un nivel
    public static void GuardarResultado(string slotId, int levelId, int score, int keys, bool diamond, bool parchment, bool trophy, bool medal, bool masterKey)
    {
        string levelCompletedKey = slotId + "_level_" + levelId + "_completed";

        // ✅ Evitar farmeo
        if (PlayerPrefs.GetInt(levelCompletedKey, 0) == 1)
        {
            Debug.Log($"Nivel {levelId} ya completado en {slotId}, no se suman recompensas otra vez.");
            return;
        }

        // Marcar nivel como completado
        PlayerPrefs.SetInt(levelCompletedKey, 1);

        // Guardar datos de este nivel (opcional, por si quieres consultar después)
        PlayerPrefs.SetInt(slotId + "_level_" + levelId + "_score", score);
        PlayerPrefs.SetInt(slotId + "_level_" + levelId + "_keys", keys);
        PlayerPrefs.SetInt(slotId + "_level_" + levelId + "_diamond", diamond ? 1 : 0);
        PlayerPrefs.SetInt(slotId + "_level_" + levelId + "_parchment", parchment ? 1 : 0);
        PlayerPrefs.SetInt(slotId + "_level_" + levelId + "_trophy", trophy ? 1 : 0);
        PlayerPrefs.SetInt(slotId + "_level_" + levelId + "_medal", medal ? 1 : 0);
        PlayerPrefs.SetInt(slotId + "_level_" + levelId + "_masterKey", masterKey ? 1 : 0);

        // Acumular totales globales del slot
        SumarAcumulados(slotId, score, keys, diamond, parchment, trophy, medal, masterKey);

        PlayerPrefs.Save();
        Debug.Log($"✅ Progreso guardado: Slot={slotId}, Nivel={levelId}, Score={score}, Keys={keys}, Diamond={diamond}");
    }

    private static void SumarAcumulados(string slotId, int score, int keys, bool diamond, bool parchment, bool trophy, bool medal, bool masterKey)
    {
        // Score total
        int totalScore = PlayerPrefs.GetInt(slotId + "_scoreTotal", 0) + score;
        PlayerPrefs.SetInt(slotId + "_scoreTotal", totalScore);

        // Llaves
        int totalKeys = PlayerPrefs.GetInt(slotId + "_keysTotal", 0) + keys;
        PlayerPrefs.SetInt(slotId + "_keysTotal", totalKeys);

        // Diamantes
        if (diamond)
        {
            int totalDiamonds = PlayerPrefs.GetInt(slotId + "_diamondsTotal", 0) + 1;
            PlayerPrefs.SetInt(slotId + "_diamondsTotal", totalDiamonds);
        }

        // Pergaminos
        if (parchment)
        {
            int totalParchments = PlayerPrefs.GetInt(slotId + "_parchmentsTotal", 0) + 1;
            PlayerPrefs.SetInt(slotId + "_parchmentsTotal", totalParchments);
        }

        // Trofeos
        if (trophy)
        {
            int totalTrophies = PlayerPrefs.GetInt(slotId + "_trophiesTotal", 0) + 1;
            PlayerPrefs.SetInt(slotId + "_trophiesTotal", totalTrophies);
        }

        // Medallas
        if (medal)
        {
            int totalMedals = PlayerPrefs.GetInt(slotId + "_medalsTotal", 0) + 1;
            PlayerPrefs.SetInt(slotId + "_medalsTotal", totalMedals);
        }

        // Llave legendaria (fragmentos)
        if (masterKey)
        {
            int totalMasterKeys = PlayerPrefs.GetInt(slotId + "_masterKeysTotal", 0) + 1;
            PlayerPrefs.SetInt(slotId + "_masterKeysTotal", totalMasterKeys);
        }
    }
}
