using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LevelTile))]
public class TileClickHandlerGameHome : MonoBehaviour, IPointerClickHandler
{
    [Header("Identificador del nivel")]
    public int levelId; // 👈 asigna desde el inspector (ej: 1, 2, 3...)

    private LevelTile levelTile;

    private void Awake()
    {
        levelTile = GetComponent<LevelTile>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (levelTile.state == LevelTile.TileState.Locked)
        {
            Debug.Log($"⛔ Nivel {levelId} está bloqueado.");
            return;
        }

        // Guardar nivel activo
        PlayerPrefs.SetInt("nivelActivo", levelId);
        PlayerPrefs.Save();

        // Cargar escena (ajusta nombres a tu esquema real)
        string sceneName = $"Level {levelId} - A{levelId}";
        Debug.Log($"▶️ Entrando a {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}
