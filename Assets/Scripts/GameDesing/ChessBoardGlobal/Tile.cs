using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [Header("Coordenadas en el tablero (x,y)")]
    public Vector2Int tileCoords;

    private Image myImage;

    private void Awake()
    {
        // Automáticamente encuentra el Image del propio GameObject
        myImage = GetComponent<Image>();
    }

    public void HighlightMove(bool isActive)
    {
        if (myImage == null)
        {
            Debug.LogWarning($"{name} no tiene un componente Image adjunto.");
            return;
        }

        myImage.color = isActive 
            ? new Color(0.5f, 1f, 0.5f, 1f) // verde claro
            : Color.white;                  // color normal
    }
}