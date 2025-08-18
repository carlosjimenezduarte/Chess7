using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class PiecePositionerGarden : MonoBehaviour
{
    [Header("Ubicación deseada en el jardín (0,0 a 8,8)")]
    public Vector2Int tileCoords;

    [ContextMenu("Posicionar pieza en el jardín")]
    public void PosicionarEnJardin()
    {
#if UNITY_EDITOR
        GardenManagerGlobal manager = GardenManagerGlobal.Instance;

        if (manager == null)
        {
            manager = FindFirstObjectByType<GardenManagerGlobal>();
        }

        if (manager != null)
        {
            transform.localPosition = manager.GetTileWorldPosition(tileCoords);
            Debug.Log($"{name} posicionado automáticamente en {tileCoords} (Jardín)");
            EditorUtility.SetDirty(gameObject);
        }
        else
        {
            Debug.LogWarning("No se encontró GardenManagerGlobal en escena. Asegúrate que el jardín está activo.");
        }
#endif
    }
}
