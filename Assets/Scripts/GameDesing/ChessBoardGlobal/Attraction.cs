using UnityEngine;

public class Attraction : MonoBehaviour
{
    public Vector2Int tileCoords;

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        if (posicionRey != tileCoords) return;

        Debug.Log($"🧲 Attraction activado en {tileCoords}. Atraerá todos los objetos inteligentemente excepto al Rey.");

        var allMovables = FindObjectsByType<MovableTileObject>(FindObjectsSortMode.None);
        foreach (var movable in allMovables)
        {
            // 🚫 Evita atraer el propio Attraction y al Rey
            if (movable.tileCoords == tileCoords || movable.GetComponent<KingController>() != null)
                continue;

            Vector2Int direccion;

            // Caso alineado en X
            if (movable.tileCoords.x == tileCoords.x)
            {
                direccion = new Vector2Int(0, tileCoords.y < movable.tileCoords.y ? -1 : 1);
            }
            // Caso alineado en Y
            else if (movable.tileCoords.y == tileCoords.y)
            {
                direccion = new Vector2Int(tileCoords.x < movable.tileCoords.x ? -1 : 1, 0);
            }
            // Caso diagonal exacta
            else if (Mathf.Abs(movable.tileCoords.x - tileCoords.x) == Mathf.Abs(movable.tileCoords.y - tileCoords.y))
            {
                direccion = new Vector2Int(tileCoords.x < movable.tileCoords.x ? -1 : 1,
                                           tileCoords.y < movable.tileCoords.y ? -1 : 1);
            }
            else
            {
                // 🔥 Caso no alineado ni diagonal exacta: mover hacia el eje dominante
                int dx = movable.tileCoords.x - tileCoords.x;
                int dy = movable.tileCoords.y - tileCoords.y;

                if (Mathf.Abs(dx) > Mathf.Abs(dy))
                    direccion = new Vector2Int(dx > 0 ? -1 : 1, 0);
                else
                    direccion = new Vector2Int(0, dy > 0 ? -1 : 1);
            }

            Vector2Int nuevaPos = movable.tileCoords + direccion;

            if (nuevaPos.x < 0 || nuevaPos.y < 0 || nuevaPos.x > 7 || nuevaPos.y > 7)
            {
                Debug.Log($"🚫 {movable.gameObject.name} no puede moverse fuera del tablero hacia {nuevaPos}.");
                continue;
            }

            Debug.Log($"🧲 {movable.gameObject.name} movido de {movable.tileCoords} a {nuevaPos}.");
            movable.MoverA(nuevaPos);
        }
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        // Por ahora no hace nada si un Peón llega a este tile.
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        // sin efecto por turno
    }
}
