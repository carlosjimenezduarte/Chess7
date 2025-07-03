using UnityEngine;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance;  // 👈 Añadimos el singleton aquí

    public List<LevelTile> tiles = new List<LevelTile>();
    private int currentLevelIndex = 0;

    private void Awake()
    {
        Instance = this; // 👈 Se inicializa aquí
    }

    private void Start()
    {
        // Solo desbloquea la primera casilla
        for (int i = 0; i < tiles.Count; i++)
        {
            if (i == 0)
                tiles[i].SetState(LevelTile.TileState.Unlocked);
            else
                tiles[i].SetState(LevelTile.TileState.Locked);
        }
    }

    public void CompleteLevel()
    {
        if (currentLevelIndex < tiles.Count)
        {
            Debug.Log("Marcando completado el índice: " + currentLevelIndex);
            tiles[currentLevelIndex].SetState(LevelTile.TileState.Completed);

            currentLevelIndex++;

            if (currentLevelIndex < tiles.Count)
            {
                Debug.Log("Desbloqueando índice: " + currentLevelIndex);
                tiles[currentLevelIndex].SetState(LevelTile.TileState.Unlocked);
            }
        }
    }

    public Vector3 GetTileWorldPosition(Vector2Int tileCoords)
    {
        float tileSize = 135f;
        float offsetX = -540f + tileSize / 2f; // = -472.5
        float offsetY = -191.8f; // tu posición exacta medida en el Editor

        float x = offsetX + tileCoords.x * tileSize;
        float y = offsetY + tileCoords.y * tileSize;

        return new Vector3(x, y, 0f);
    }
}
