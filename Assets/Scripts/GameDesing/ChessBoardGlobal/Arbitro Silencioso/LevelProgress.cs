using UnityEngine;

public class LevelProgress : MonoBehaviour
{
    public static LevelProgress Instance { get; private set; }

    [Header("Tipo de nivel")]

    public bool esNivelMasterKey = false;
    public bool esNivelPergamino = false;

    public bool esNivelTrofeo = false;

    public bool esNivelMedalla = false;


    [Header("Estado del nivel")]
    public int keysCollected = 0;
    public bool hasDiamond = false;
    
    public bool hasParchment = false;

    public bool hasTrophy = false;

    public bool hasMedal = false;

    public bool hasMasterKey3 = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Key()
    {
        if (keysCollected < 3)
        {
            keysCollected++;
            Debug.Log($"🗝️ Llaves recogidas: {keysCollected}");
        }
    }

    public void Diamond()
    {
        if (!hasDiamond)
        {
            hasDiamond = true;
            Debug.Log($"💎 ¡Diamante recogido!");
        }
    }
    public void Padlock()
    {
        keysCollected--; // Permitir valores negativos
        Debug.Log($"🔒 Llaves después de candado: {keysCollected}");
    }

    public void Talisman()
    {
        hasDiamond = false;
        Debug.Log($"💎 ¡Diamante perdido!");
    }

    public void Parchment()
    {
        if (!hasParchment)
        {
            hasParchment = true;
            Debug.Log($"💎 ¡Pergamino obtenido!");
        }
    }

    public void Trophy()
    {
        if (!hasTrophy)
        {
            hasTrophy = true;
            Debug.Log($"🏆 ¡Trofeo obtenido!");
        }
    }

    public void Medal()
    {
        if (!hasMedal)
        {
            hasMedal = true;
            Debug.Log($"🎖️ ¡Medalla obtenida!");
        }
    }

    public void MasterKey()
    {
        if (!hasMasterKey3)
        {
            hasMasterKey3 = true;
            Debug.Log($"🗝️ Fragmento de llave recogido: {hasMasterKey3}");
        }
    }

    public void ResetProgress()
    {
        keysCollected = 0;
        hasDiamond = false;
        hasParchment = false;
        hasTrophy = false;
        hasMedal = false;    
    }

    public void FinalizarNivel(int vidas, int score)
    {
        if (esNivelPergamino)
        {
            LevelResultUI.Instance.ShowParchmentResult(score, vidas, hasDiamond, hasParchment);
        }
        else if (esNivelTrofeo)
        {
            LevelResultUI.Instance.ShowTrophyResult(score, vidas, hasDiamond, hasTrophy);
        }
        else if (esNivelMedalla)
        {
            LevelResultUI.Instance.ShowMedalResult(score, vidas, hasDiamond, hasMedal);
        }
        else if (esNivelMasterKey)
        {
            LevelResultUI.Instance.ShowMasterKeyResult(score, vidas, hasMasterKey3);
        }
        else
        {
            // Nivel normal
            LevelResultUI.Instance.ShowResults(keysCollected, hasDiamond, vidas, score);
        }
    
}





}
