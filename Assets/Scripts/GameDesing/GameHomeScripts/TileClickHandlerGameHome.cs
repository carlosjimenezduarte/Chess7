using UnityEngine;
using UnityEngine.EventSystems;

public class TileClickHandlerGameHome : MonoBehaviour, IPointerClickHandler
{
    [Header("Coordenadas de esta casilla")]
    public Vector2Int tileCoords;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"🖱 Click detectado en casilla {tileCoords} en GameHome.");
    }
}
