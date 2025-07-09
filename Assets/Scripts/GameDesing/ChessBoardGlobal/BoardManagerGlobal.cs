using UnityEngine;
using System.Collections.Generic;

public class BoardManagerGlobal : MonoBehaviour
{
    public static BoardManagerGlobal Instance;

    [Header("Lista de todas las casillas del tablero")]
    public List<Tile> tiles = new List<Tile>();

    private void Awake()
    {
        // Asignar el singleton
        Instance = this;

        // Opcional: imprimir el tablero para depuración
        foreach (Tile tile in tiles)
        {
            Debug.Log($"Tile en {tile.tileCoords} inicializado.");
        }
    }

    public Vector3 GetTileWorldPosition(Vector2Int tileCoords)
    {
        foreach (Tile tile in tiles)
        {
            if (tile.tileCoords == tileCoords)
            {
                return tile.transform.localPosition;
            }
        }

        Debug.LogWarning($"No se encontró tile en coordenadas {tileCoords}");
        return Vector3.zero;
    }

    public Tile GetTileAt(Vector2Int coords)
    {
        foreach (Tile tile in tiles)
        {
            if (tile.tileCoords == coords)
                return tile;
        }
        Debug.LogWarning($"No se encontró Tile en posición {coords}");
        return null;
    }
    
    public Vector2Int GetClosestTileCoords(Vector3 worldPos)
    {
    Tile closest = null;
    float minDist = Mathf.Infinity;

    foreach (Tile tile in tiles)
    {
        float dist = Vector3.Distance(tile.transform.localPosition, worldPos);
        if (dist < minDist)
        {
            minDist = dist;
            closest = tile;
        }
    }

    if (closest != null)
        return closest.tileCoords;

    Debug.LogWarning($"No se encontró tile cercano a posición {worldPos}");
    return Vector2Int.zero;
    }
}
