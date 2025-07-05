using UnityEngine;

public class Potion2PM : MonoBehaviour, ITileEffect
{
    public Vector2Int tileCoords;

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        if (posicionRey == tileCoords)
        {
            Debug.Log($"El Rey recogió una poción en {tileCoords} y ganó +2 PM.");
            rey.GanarPuntoMovimiento(2);
            Destroy(gameObject);
        }
    }
}