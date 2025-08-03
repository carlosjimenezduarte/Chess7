using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    [Header("Coordenadas en el tablero (x,y)")]
    public Vector2Int tileCoords;

    private Image myImage;

    private void Awake()
    {
        myImage = GetComponent<Image>();
    }

    public void HighlightMove(bool isActive)
    {
        if (myImage == null) return;

        myImage.color = isActive
            ? new Color(0.5f, 1f, 0.5f, 1f) // verde claro
            : Color.white;
    }

    public void HighlightEnemyKillZone(bool isActive)
    {
        if (myImage == null) return;
        myImage.color = isActive ? new Color(1f, 0.3f, 0.3f, 1f) : Color.white; // rojo claro
    }

    public void HighlightEnemyRangeZone(bool isActive)
    {
        if (myImage == null) return;
        myImage.color = isActive ? new Color(1f, 0.5f, 0.7f, 1f) : Color.white; // rosado
    }

    public void HighlightEnemyAttack(bool state)
{
    if (myImage == null) return;

    Color colorAtaque = new Color(1f, 0.2f, 0.9f, 1f);
    myImage.color = state ? colorAtaque : Color.clear;
}
    public void HighlightSpecific(bool isActive)
    {
    if (myImage != null)
    {
        myImage.color = isActive 
            ? new Color(1f, 0.2f, 0.9f, 1f) // fucsia sólido
            : Color.white;
    }
    }

    public bool EsCasillaDeAtaque()
    {
    if (myImage == null) return false;

    // Este es el color que se usa en HighlightEnemyAttack
    Color colorAtaque = new Color(1f, 0.2f, 0.9f, 1f);

    return myImage.color.Equals(colorAtaque);
    }

    
    public void ResetColor()
    {
        if (myImage != null)
            myImage.color = Color.white;
    }
}