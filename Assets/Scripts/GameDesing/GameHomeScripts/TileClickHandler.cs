using UnityEngine;
using UnityEngine.EventSystems;

public class TileClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("Coordenadas de esta casilla")]
    public Vector2Int tileCoords;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Click o toque detectado en casilla: {tileCoords}");

        KingController rey = FindFirstObjectByType<KingController>();
        if (rey != null)
        {
            rey.MoverA(tileCoords);
        }
        else
        {
            Debug.LogWarning("No se encontró ningún KingController en la escena.");
        }
    }
}
