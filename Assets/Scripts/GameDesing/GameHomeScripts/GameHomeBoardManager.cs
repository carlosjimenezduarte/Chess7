using UnityEngine;
using System.Collections.Generic;

public class GameHomeBoardManager : MonoBehaviour
{
    public List<LevelTile> tiles = new List<LevelTile>();
    private int currentLevelIndex = 0;

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
            // Marcar casilla actual como completada
            tiles[currentLevelIndex].SetState(LevelTile.TileState.Completed);

            // Desbloquear la siguiente casilla
            currentLevelIndex++;
            if (currentLevelIndex < tiles.Count)
            {
                tiles[currentLevelIndex].SetState(LevelTile.TileState.Unlocked);
            }
        }
    }
}