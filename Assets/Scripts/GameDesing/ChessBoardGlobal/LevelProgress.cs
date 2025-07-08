using UnityEngine;

public class LevelProgress : MonoBehaviour
{
    public static LevelProgress Instance { get; private set; }

    [Header("Estado del nivel")]
    public int keysCollected = 0;
    public bool hasDiamond = false;

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

    public void CollectKey()
    {
        keysCollected++;
        Debug.Log($"🗝️ Llaves recogidas: {keysCollected}");
    }

    public void CollectDiamond()
    {
        hasDiamond = true;
        Debug.Log($"💎 ¡Diamante recogido!");
    }
}
