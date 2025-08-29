using UnityEngine;

public class LevelProgress : MonoBehaviour
{
    public static LevelProgress Instance { get; private set; }

    [Header("Tipo de nivel")]
    public bool esNivelPergamino = false;

    public bool esNivelTrofeo = false;

    public bool esNivelMedalla = false;


    [Header("Estado del nivel")]
    public int keysCollected = 0;
    public bool hasDiamond = false;

    public bool hasParchment = false;

    public bool hasTrophy = false;

    public bool hasMedal = false;

    public bool MasterKey3 = false;

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
        hasTrophy = true;
        Debug.Log($"💎 ¡Pergamino obtenido!");
    }

    public void Medal()
    {
        hasMedal = true;
        Debug.Log($"💎 ¡Pergamino obtenido!");
    }

    public void MasterKey()
    {
        MasterKey3 = true;
        Debug.Log($"🗝️ Fragmento de llave recogido: {MasterKey3}");

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
        //LevelResultUI.Instance.ShowTrophyResult(score, vidas, hasDiamond, hasTrophy);
    }
    else if (esNivelMedalla)
    {
    //    LevelResultUI.Instance.ShowMedalResult(score, vidas, hasDiamond, hasMedal);
    }
    else
    {
        // Nivel normal
        LevelResultUI.Instance.ShowResults(keysCollected, hasDiamond, vidas, score);
    }
}





}
