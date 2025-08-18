using UnityEngine;

public class GardenManagerGlobal : MonoBehaviour
{
    public static GardenManagerGlobal Instance { get; private set; }

    [Header("Configuración del Jardín")]
    public float tileSize = 100f; // tamaño de cada casilla en unidades de mundo
    public Vector2 origenTablero = Vector2.zero; // posición en mundo de la casilla (0,0)

    private bool llaveObtenida = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Convierte coordenadas de casilla (tileCoords) a posición en mundo
    /// </summary>
    public Vector3 GetTileWorldPosition(Vector2Int tileCoords)
    {
        float worldX = origenTablero.x + (tileCoords.x * tileSize);
        float worldY = origenTablero.y + (tileCoords.y * tileSize);
        return new Vector3(worldX, worldY, 0f);
    }

    /// <summary>
    /// Marca que el Rey ya tiene la llave maestra
    /// </summary>
    public void MarcarLlaveObtenida()
    {
        llaveObtenida = true;
        Debug.Log("🔑 Llave Maestra obtenida.");
    }

    /// <summary>
    /// Consulta si el Rey tiene la llave
    /// </summary>
    public bool TieneLlave()
    {
        return llaveObtenida;
    }

    public void RevisarInteraccion(Vector2Int posicionRey)
    {
    // Aquí detectas si el Rey está sobre algún objeto especial
    // Ej: llave, cofre, espejo
    }
}
