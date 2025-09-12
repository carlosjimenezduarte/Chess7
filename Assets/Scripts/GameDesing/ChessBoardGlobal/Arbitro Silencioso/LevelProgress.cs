using UnityEngine;

public class LevelProgress : MonoBehaviour
{
    public static LevelProgress Instance { get; private set; }

    [Header("Tipo de nivel")]
    public bool esNivelMasterKey = false;
    public bool esNivelPergamino = false;
    public bool esNivelTrofeo = false;
    public bool esNivelMedalla = false;
    public int bagsCollected = 0;
    public int crownsCollected = 0;
    public int chestsCollected = 0;
    public int coinsCollected = 0;

    [Header("Config de nivel")]
    public int maxKeys = 3;        // por defecto 3 llaves
    public int maxDiamonds = 1;    // por defecto 1 diamante

     [Header("Estado del nivel (runtime, no editable)")]
    [HideInInspector] public int keysCollected = 0;
    [HideInInspector] public int diamondsCollected = 0;
    [HideInInspector] public bool hasParchment = false;
    [HideInInspector] public bool hasTrophy = false;
    [HideInInspector] public bool hasMedal = false;
    [HideInInspector] public bool hasMasterKey3 = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 🔹 Métodos de recolección
    public void Key()
    {
        int before = keysCollected;
        keysCollected = Mathf.Clamp(keysCollected + 1, 0, maxKeys);
        Debug.Log($"🗝️ Llaves recogidas: {keysCollected}/{maxKeys} (antes {before})");
    }

    public void Diamond()
    {
        int before = diamondsCollected;
        diamondsCollected = Mathf.Clamp(diamondsCollected + 1, 0, maxDiamonds);
        Debug.Log($"💎 Diamantes recogidos: {diamondsCollected}/{maxDiamonds} (antes {before})");
    }

    // 🔹 Penalizaciones
    public void Padlock()
    {
        int before = keysCollected;
        keysCollected = Mathf.Clamp(keysCollected - 1, 0, maxKeys);
        Debug.Log($"🔒 Candado -> Llaves ahora: {keysCollected}/{maxKeys} (antes {before})");
    }

    public void Talisman()
    {
        int before = diamondsCollected;
        diamondsCollected = Mathf.Clamp(diamondsCollected - 1, 0, maxDiamonds);
        Debug.Log($"🔮 Talismán -> Diamantes ahora: {diamondsCollected}/{maxDiamonds} (antes {before})");
    }

    // 🔹 Objetos especiales
    public void Parchment()
    {
        if (!hasParchment)
        {
            hasParchment = true;
            Debug.Log($"📜 ¡Pergamino obtenido!");
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
            Debug.Log($"🗝️ Fragmento de llave recogido.");
        }
    }
     public void AddBag()
    {
        bagsCollected++;
        Debug.Log($"💰 Bolsa recogida. Total en este nivel: {bagsCollected}");
    }

    public void AddCrown()
    {
        crownsCollected++;
        Debug.Log($"👑 Corona recogida. Total en este nivel: {crownsCollected}");
    }

    public void AddChest()
    {
        chestsCollected++;
        Debug.Log($"📦 Cofre recogido. Total en este nivel: {chestsCollected}");
    }

    public void AddCoin()
    {
        coinsCollected++;
        Debug.Log($"🪙 Moneda real recogida. Total en este nivel: {coinsCollected}");
    }


    // 🔹 Reset al empezar/reintentar nivel
    public void ResetProgress()
    {
        keysCollected = 0;
        diamondsCollected = 0;
        hasParchment = false;
        hasTrophy = false;
        hasMedal = false;
        hasMasterKey3 = false;

        // ✅ Reiniciar contadores de score-objetos por intento
        bagsCollected = 0;
        crownsCollected = 0;
        chestsCollected = 0;
        coinsCollected = 0;

        Debug.Log("🔄 Progreso del nivel reiniciado.");
    }


}
