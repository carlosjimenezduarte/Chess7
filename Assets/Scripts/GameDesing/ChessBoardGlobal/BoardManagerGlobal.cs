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
}
