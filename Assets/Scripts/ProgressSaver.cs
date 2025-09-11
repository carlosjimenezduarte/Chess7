using UnityEngine;

public class ProgressSaver : MonoBehaviour
{
    // 🔹 Método central
    private static void GuardarResultado(
     string slotId, int levelId, int score, int keys,
     int diamonds, bool parchment, bool trophy, bool medal, bool masterKey)
    {
        string levelCompletedKey = slotId + "_level_" + levelId + "_completed";
        string scoreKey = slotId + "_level_" + levelId + "_score";
        string keysKey = slotId + "_level_" + levelId + "_keys";
        string diamondKey = slotId + "_level_" + levelId + "_diamonds"; // plural

        int prevCompleted = PlayerPrefs.GetInt(levelCompletedKey, 0);
        int prevScore = PlayerPrefs.GetInt(scoreKey, 0);
        int prevKeys = PlayerPrefs.GetInt(keysKey, 0);
        int prevDiamonds = PlayerPrefs.GetInt(diamondKey, 0);

        int finalKeys = Mathf.Max(prevKeys, keys);
        int finalDiamonds = Mathf.Max(prevDiamonds, diamonds);

        int deltaKeys = finalKeys - prevKeys;
        int deltaDiamonds = finalDiamonds - prevDiamonds;
        Debug.Log($"[ProgressSaver] prevK={prevKeys}, newK={keys}, finalK={finalKeys}, ΔK={deltaKeys} | " +
                $"prevD={prevDiamonds}, newD={diamonds}, finalD={finalDiamonds}, ΔD={deltaDiamonds}");

        if (prevCompleted == 1 &&
            finalKeys == prevKeys &&
            finalDiamonds == prevDiamonds &&
            score <= prevScore)
        {
            Debug.Log($"⚠️ Nivel {levelId} ya completado en {slotId}. No hay mejora.");
            return;
        }

        // ✅ Guardar
        PlayerPrefs.SetInt(levelCompletedKey, 1);
        PlayerPrefs.SetInt(scoreKey, Mathf.Max(prevScore, score));
        PlayerPrefs.SetInt(keysKey, finalKeys);
        PlayerPrefs.SetInt(diamondKey, finalDiamonds);
        PlayerPrefs.SetInt(slotId + "_level_" + levelId + "_parchment", parchment ? 1 : 0);
        PlayerPrefs.SetInt(slotId + "_level_" + levelId + "_trophy", trophy ? 1 : 0);
        PlayerPrefs.SetInt(slotId + "_level_" + levelId + "_medal", medal ? 1 : 0);
        PlayerPrefs.SetInt(slotId + "_level_" + levelId + "_masterKey", masterKey ? 1 : 0);

        if (finalKeys > prevKeys || finalDiamonds > prevDiamonds || parchment || trophy || medal || masterKey)
        {
            SumarAcumulados(
                slotId,
                score,
                finalKeys - prevKeys,
                finalDiamonds - prevDiamonds,
                parchment, trophy, medal, masterKey
            );
        }

        PlayerPrefs.Save();
        Debug.Log($"✅ Guardado nivel {levelId}: Keys={finalKeys}, Diamonds={finalDiamonds}, Score={Mathf.Max(prevScore, score)}");
    }



    // 🔹 Métodos especializados
    public static void GuardarNivelNormal(string slotId, int levelId, int score, int keys, int diamonds)
    {
        GuardarResultado(slotId, levelId, score, keys, diamonds, false, false, false, false);
    }


    public static void GuardarNivelPergamino(string slotId, int levelId, int score, bool parchment)
    {
        GuardarResultado(slotId, levelId, score, 0, 0, parchment, false, false, false);
    }

    public static void GuardarNivelTrofeo(string slotId, int levelId, int score, bool trophy)
    {
        GuardarResultado(slotId, levelId, score, 0, 0, false, trophy, false, false);
    }

    public static void GuardarNivelMedalla(string slotId, int levelId, int score, bool medal)
    {
        GuardarResultado(slotId, levelId, score, 0, 0, false, false, medal, false);
    }

    public static void GuardarNivelMasterKey(string slotId, int levelId, int score, bool masterKey)
    {
        GuardarResultado(slotId, levelId, score, 0, 0, false, false, false, masterKey);
    }

    // 🔹 Totales acumulados
    private static void SumarAcumulados(
        string slotId, int score, int keysToAdd,
        int diamondsToAdd, bool parchment, bool trophy, bool medal, bool masterKey)
    {
        int totalScore = PlayerPrefs.GetInt(slotId + "_scoreTotal", 0) + score;
        PlayerPrefs.SetInt(slotId + "_scoreTotal", totalScore);

        if (keysToAdd > 0)
        {
            int totalKeys = PlayerPrefs.GetInt(slotId + "_keysTotal", 0) + keysToAdd;
            PlayerPrefs.SetInt(slotId + "_keysTotal", totalKeys);
        }

        if (diamondsToAdd > 0)
        {
            int totalDiamonds = PlayerPrefs.GetInt(slotId + "_diamondsTotal", 0) + diamondsToAdd;
            PlayerPrefs.SetInt(slotId + "_diamondsTotal", totalDiamonds);
        }

        if (parchment)
        {
            int totalParchments = PlayerPrefs.GetInt(slotId + "_parchmentsTotal", 0) + 1;
            PlayerPrefs.SetInt(slotId + "_parchmentsTotal", totalParchments);
        }

        if (trophy)
        {
            int totalTrophies = PlayerPrefs.GetInt(slotId + "_trophiesTotal", 0) + 1;
            PlayerPrefs.SetInt(slotId + "_trophiesTotal", totalTrophies);
        }

        if (medal)
        {
            int totalMedals = PlayerPrefs.GetInt(slotId + "_medalsTotal", 0) + 1;
            PlayerPrefs.SetInt(slotId + "_medalsTotal", totalMedals);
        }

        if (masterKey)
        {
            int totalMasterKeys = PlayerPrefs.GetInt(slotId + "_masterKeysTotal", 0) + 1;
            PlayerPrefs.SetInt(slotId + "_masterKeysTotal", totalMasterKeys);
        }
    }
}
