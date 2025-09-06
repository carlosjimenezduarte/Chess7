using UnityEngine;
using System.Collections.Generic;

public class GameHomeManager : MonoBehaviour
{
    public static GameHomeManager Instance { get; private set; }

    [Header("Lista de casillas (niveles) en este mapa")]
    public List<LevelTile> tiles; // 👈 arrastra los prefabs de las casillas en orden

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        CargarProgreso();
    }

    public void CargarProgreso()
    {
        // 🔎 Leemos qué slot está activo
        string slotActivo = PlayerPrefs.GetString("slotActivo", "slot1");

        // Cargamos progreso del slot (último nivel desbloqueado)
        int nivelMax = PlayerPrefs.GetInt(slotActivo + "_nivelMax", 1);

        for (int i = 0; i < tiles.Count; i++)
        {
            if (i + 1 < nivelMax)
                tiles[i].SetState(LevelTile.TileState.Completed); // niveles terminados
            else if (i + 1 == nivelMax)
                tiles[i].SetState(LevelTile.TileState.Unlocked);  // nivel disponible
            else
                tiles[i].SetState(LevelTile.TileState.Locked);    // niveles bloqueados
        }
    }

    public void MarcarNivelCompletado(int nivelId, bool obtuvoObjetoClave)
    {
        if (nivelId - 1 < 0 || nivelId - 1 >= tiles.Count) return;

        tiles[nivelId - 1].SetState(LevelTile.TileState.Completed, obtuvoObjetoClave);

        // 🔓 Desbloquear el siguiente nivel si existe
        if (nivelId < tiles.Count)
            tiles[nivelId].SetState(LevelTile.TileState.Unlocked);

        // Guardar en PlayerPrefs
        string slotActivo = PlayerPrefs.GetString("slotActivo", "slot1");
        int nivelMax = PlayerPrefs.GetInt(slotActivo + "_nivelMax", 1);
        if (nivelId + 1 > nivelMax)
        {
            PlayerPrefs.SetInt(slotActivo + "_nivelMax", nivelId + 1);
            PlayerPrefs.Save();
        }
    }
}
