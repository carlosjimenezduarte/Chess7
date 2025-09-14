using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LevelTile))]
public class TileClickHandlerGameHome : MonoBehaviour, IPointerClickHandler
{
    [Header("Identificadores del nivel")]
    [Tooltip("Número que corresponde al Build Index en File -> Build Settings")]

    public const int BUILD_OFFSET = 14;
    public int buildIndex;   // 👈 índice real de escena
    public int nivelLogico;  // 👈 número visible (1, 2, 3...)

    private LevelTile levelTile;

    private void Awake()
    {
        levelTile = GetComponent<LevelTile>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (levelTile.state == LevelTile.TileState.Locked)
        {
            Debug.Log($"⛔ Nivel {nivelLogico} está bloqueado.");
            return;
        }

        // Guardar el nivel lógico activo
        PlayerPrefs.SetInt("nivelActivo", nivelLogico);
        PlayerPrefs.Save();

        Debug.Log($"▶️ Entrando al nivel lógico {nivelLogico} (BuildIndex={buildIndex})");
        SceneManager.LoadScene(buildIndex); // 👈 aquí usamos el índice real de escena
    }
} 