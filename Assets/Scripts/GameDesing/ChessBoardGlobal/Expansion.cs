using UnityEngine;

public class Expansion : MonoBehaviour, ITileEffect
{
    public Vector2Int tileCoords;

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        if (posicionRey == tileCoords)
        {
            Debug.Log($"💥 Expansion activado en {tileCoords}. Empujará todos los objetos cercanos excepto al Rey.");

            var allMovables = FindObjectsByType<MovableTileObject>(FindObjectsSortMode.None);
            foreach (var movable in allMovables)
            {
                // 🚫 No afectar la propia casilla ni al Rey
                if (movable.tileCoords == tileCoords || movable.GetComponent<KingController>() != null)
                    continue;

                Vector2Int delta = movable.tileCoords - tileCoords;
                Vector2Int dir = Vector2Int.zero;

                // Caso diagonal perfecta
                if (Mathf.Abs(delta.x) == Mathf.Abs(delta.y))
                {
                    dir = new Vector2Int(delta.x > 0 ? 1 : -1, delta.y > 0 ? 1 : -1);
                }
                // Caso misma fila
                else if (delta.y == 0)
                {
                    dir = new Vector2Int(delta.x > 0 ? 1 : -1, 0);
                }
                // Caso misma columna
                else if (delta.x == 0)
                {
                    dir = new Vector2Int(0, delta.y > 0 ? 1 : -1);
                }
                // Caso no alineado: empuja en el eje dominante
                else
                {
                    if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
                        dir = new Vector2Int(delta.x > 0 ? 1 : -1, 0);
                    else
                        dir = new Vector2Int(0, delta.y > 0 ? 1 : -1);
                }

                Vector2Int nuevaPos = movable.tileCoords + dir;

                // 🔥 Verificar que no salga del tablero
                if (nuevaPos.x < 0 || nuevaPos.y < 0 || nuevaPos.x > 7 || nuevaPos.y > 7)
                {
                    Debug.Log($"🚫 {movable.gameObject.name} no puede salir del tablero hacia {nuevaPos}.");
                    continue;
                }

                Debug.Log($"💥 {movable.gameObject.name} expulsado de {movable.tileCoords} a {nuevaPos}.");
                movable.MoverA(nuevaPos);
            }
        }
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
    // Por ahora no hace nada si el Peón llega a este tile.
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        // sin efecto por turno
    }
}
