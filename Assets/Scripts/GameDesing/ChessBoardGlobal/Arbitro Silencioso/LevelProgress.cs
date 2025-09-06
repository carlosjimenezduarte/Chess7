using UnityEngine;
using UnityEngine.SceneManagement;

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
    // 💾 Guardar progreso automáticamente en el slot activo
    string slotActivo = PlayerPrefs.GetString("slotActivo", "slot1");
    int nivelActual = SceneManager.GetActiveScene().buildIndex;

    // Guardamos progreso en PlayerPrefs con protección anti-farmeo
    ProgressSaver.GuardarResultado(
        slotActivo,
        nivelActual,
        score,
        keysCollected,
        hasDiamond,
        hasParchment,
        hasTrophy,
        hasMedal,
        hasMasterKey3
    );

    Debug.Log($"💾 Progreso guardado en {slotActivo} -> Nivel {nivelActual}");

    // 📌 Determinar si este nivel debe mostrar ícono en el mapa (objeto clave recogido)
    bool obtuvoObjetoClave =
        (keysCollected >= 3) || // todas las llaves
        hasParchment ||         // pergamino
        hasTrophy ||            // trofeo
        hasMedal ||             // medalla
        hasMasterKey3;          // fragmento de llave

    // ✅ Avisar al GameHomeManager para actualizar visual del tablero
    if (GameHomeManager.Instance != null)
    {
        GameHomeManager.Instance.MarcarNivelCompletado(nivelActual, obtuvoObjetoClave);
    }

    // 📊 Mostrar resultados en UI según el tipo de nivel
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
        // Nivel normal (3 llaves + diamante, o 1 llave + diamante)
        LevelResultUI.Instance.ShowResults(keysCollected, hasDiamond, vidas, score);
    }
}


    
}


