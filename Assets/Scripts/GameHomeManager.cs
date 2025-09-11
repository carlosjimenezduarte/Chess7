using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class GameHomeManager : MonoBehaviour
{
    public static GameHomeManager Instance { get; private set; }

    [Header("Lista de casillas (niveles) en este mapa")]
    public List<LevelTile> tiles; // arrastra en orden en el inspector

    [Header("Referencias UI de progreso acumulado")]
    public TMP_Text scoreText;
    public TMP_Text keysText;
    public TMP_Text diamondsText;

    private const int OFFSET_NIVELES = 8;

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
        string slotActivo = PlayerPrefs.GetString("slotActivo", "slot1");
        int nivelMax = PlayerPrefs.GetInt(slotActivo + "_nivelMax", 1);

        // 🔹 Leer totales acumulados
        int totalScore = PlayerPrefs.GetInt(slotActivo + "_scoreTotal", 0);
        int totalKeys = PlayerPrefs.GetInt(slotActivo + "_keysTotal", 0);
        int totalDiamonds = PlayerPrefs.GetInt(slotActivo + "_diamondsTotal", 0);

        // 🔹 Actualizar UI
        if (scoreText != null) scoreText.text = totalScore.ToString();
        if (keysText != null) keysText.text = totalKeys.ToString();
        if (diamondsText != null) diamondsText.text = totalDiamonds.ToString();

        Debug.Log($"[GameHomeManager] CargarProgreso -> Slot={slotActivo}, nivelMax={nivelMax}, Score={totalScore}, Keys={totalKeys}, Diamonds={totalDiamonds}");

        // 🔹 Refrescar tiles
        for (int i = 0; i < tiles.Count; i++)
        {
            var handler = tiles[i].GetComponent<TileClickHandlerGameHome>();
            if (handler == null) continue;

            int completed = PlayerPrefs.GetInt(slotActivo + "_level_" + handler.nivelLogico + "_completed", 0);
            int keys = PlayerPrefs.GetInt(slotActivo + "_level_" + handler.nivelLogico + "_keys", 0);

            // 🔹 Diferencia importante:
            // diamond = flag de "nivel 100% completado"
            bool diamondFlag = PlayerPrefs.GetInt(slotActivo + "_level_" + handler.nivelLogico + "_diamond", 0) == 1;

            // diamondsCollected = conteo real (puede ser 0, 1, 2...)
            int diamondsCollected = PlayerPrefs.GetInt(slotActivo + "_level_" + handler.nivelLogico + "_diamonds", 0);

            bool parchment = PlayerPrefs.GetInt(slotActivo + "_level_" + handler.nivelLogico + "_parchment", 0) == 1;
            bool trophy = PlayerPrefs.GetInt(slotActivo + "_level_" + handler.nivelLogico + "_trophy", 0) == 1;
            bool medal = PlayerPrefs.GetInt(slotActivo + "_level_" + handler.nivelLogico + "_medal", 0) == 1;
            bool masterKey = PlayerPrefs.GetInt(slotActivo + "_level_" + handler.nivelLogico + "_masterKey", 0) == 1;

            // 🔑 Nuevo cálculo de objeto clave:
            // exige 3 llaves + TODOS los diamantes configurados para ese nivel
            int maxDiamonds = 1;
            if (LevelProgress.Instance != null)
                maxDiamonds = LevelProgress.Instance.maxDiamonds;

            bool obtuvoObjetoClave =
                (keys >= 3 && diamondsCollected >= maxDiamonds) ||
                parchment || trophy || medal || masterKey;

            if (completed == 1)
                tiles[i].SetState(LevelTile.TileState.Completed, obtuvoObjetoClave);
            else if (handler.nivelLogico == nivelMax)
                tiles[i].SetState(LevelTile.TileState.Unlocked);
            else
                tiles[i].SetState(LevelTile.TileState.Locked);

            Debug.Log($"   • Tile[{i}] -> nivelLogico={handler.nivelLogico}, completed={completed}, keys={keys}, " +
                      $"diamondsCollected={diamondsCollected}, diamondFlag={diamondFlag}, objetoClave={obtuvoObjetoClave}");
        }
    }

    public void MarcarNivelCompletado(int nivelLogico, bool obtuvoObjetoClave)
    {
        Debug.Log($"[GameHomeManager] ➡️ MarcarNivelCompletado -> nivel={nivelLogico}, ObjetoClave={obtuvoObjetoClave}");

        string slotActivo = PlayerPrefs.GetString("slotActivo", "slot1");
        int nivelMaxAntes = PlayerPrefs.GetInt(slotActivo + "_nivelMax", 1);

        LevelTile tile = tiles.Find(t =>
        {
            var handler = t.GetComponent<TileClickHandlerGameHome>();
            return handler != null && handler.nivelLogico == nivelLogico;
        });

        if (tile == null)
        {
            Debug.LogWarning($"⚠️ No se encontró casilla para Nivel lógico={nivelLogico}");
            return;
        }

        // 🔹 Leer progreso del nivel (para validar 100% completado)
        int keys = PlayerPrefs.GetInt(slotActivo + "_level_" + nivelLogico + "_keys", 0);
        int diamondsCollected = PlayerPrefs.GetInt(slotActivo + "_level_" + nivelLogico + "_diamonds", 0);

        bool parchment = PlayerPrefs.GetInt(slotActivo + "_level_" + nivelLogico + "_parchment", 0) == 1;
        bool trophy = PlayerPrefs.GetInt(slotActivo + "_level_" + nivelLogico + "_trophy", 0) == 1;
        bool medal = PlayerPrefs.GetInt(slotActivo + "_level_" + nivelLogico + "_medal", 0) == 1;
        bool masterKey = PlayerPrefs.GetInt(slotActivo + "_level_" + nivelLogico + "_masterKey", 0) == 1;

        int maxDiamonds = 1;
        if (LevelProgress.Instance != null)
            maxDiamonds = LevelProgress.Instance.maxDiamonds;

        // 🔑 Recalcular con regla unificada
        bool logroPerfecto =
            (keys >= 3 && diamondsCollected >= maxDiamonds) ||
            parchment || trophy || medal || masterKey;

        // 🔹 Actualizar estado visual
        tile.SetState(LevelTile.TileState.Completed, logroPerfecto);

        // 🔹 Desbloquear siguiente nivel
        int currentIndex = tiles.IndexOf(tile);
        if (currentIndex >= 0 && currentIndex + 1 < tiles.Count)
        {
            tiles[currentIndex + 1].SetState(LevelTile.TileState.Unlocked);
            Debug.Log($"   • Nivel lógico {nivelLogico + 1} desbloqueado.");
        }

        // 🔹 Guardar progreso en PlayerPrefs
        PlayerPrefs.SetInt(slotActivo + "_level_" + nivelLogico + "_completed", 1);

        if (logroPerfecto)
            PlayerPrefs.SetInt(slotActivo + "_level_" + nivelLogico + "_diamond", 1); // bandera de nivel 100% completado

        if (nivelLogico + 1 > nivelMaxAntes)
            PlayerPrefs.SetInt(slotActivo + "_nivelMax", nivelLogico + 1);

        PlayerPrefs.Save();

        int nivelMaxDespues = PlayerPrefs.GetInt(slotActivo + "_nivelMax", 1);
        Debug.Log($"[GameHomeManager] ✅ Progreso guardado -> nivel={nivelLogico}, Perfecto={logroPerfecto}, nivelMax={nivelMaxDespues}");
    }


}
