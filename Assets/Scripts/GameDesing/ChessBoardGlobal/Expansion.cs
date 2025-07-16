using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public class Expansion : MonoBehaviour, ITileEffect
{
    public Vector2Int tileCoords;

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        if (posicionRey == tileCoords) ActivarExpansion(rey);
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        if (posicionPeon == tileCoords) ActivarExpansion(peon);
    }

    private void ActivarExpansion(MonoBehaviour activador)
    {
        Debug.Log($"💥 Expansion en {tileCoords} activado por {activador.name}.");

        // Tomamos todos los MovableTileObject, ordenados de más lejos a más cerca
        List<MovableTileObject> todos = FindObjectsByType<MovableTileObject>(FindObjectsSortMode.None)
            .OrderByDescending(m => Vector2Int.Distance(m.tileCoords, tileCoords))
            .ToList();

        foreach (var obj in todos)
            {
            // ⛔ Ignora el activador (Rey o Peón que activó la expansión) y la propia casilla origen
            if (EsIgnorable(obj, activador)) continue;

            // ⛔ Ignora directamente todo lo que está fuera del tablero (ej: en Futuros Inciertos o Dimensión Divina)
            if (!EsDentroTablero(obj.tileCoords))
            {
                Debug.Log($"🛡 {obj.name} está fuera del tablero en {obj.tileCoords}, ignorado por Expansion.");
                continue;
            }

            // ⛔ Ignora recolectables no activos (pociones que aún no han aparecido)
            if (!EstaVisibleYRecolectable(obj)) continue;

            // Calcula la dirección de empuje y casilla destino
            Vector2Int dir = CalcularDireccion(obj.tileCoords - tileCoords);
            Vector2Int destino = obj.tileCoords + dir;

            // Si el destino está fuera del tablero, no mueve
            if (!EsDentroTablero(destino))
            {
                Debug.Log($"🚫 {obj.name} no puede salir hacia {destino}.");
                continue;
            }

            // ✅ Mueve el objeto al destino calculado
            Debug.Log($"💥 {obj.name} de {obj.tileCoords} a {destino}.");
            obj.MoverA(destino);
            }
    }

    private bool EsIgnorable(MovableTileObject obj, MonoBehaviour activador)
    {
        if (obj.tileCoords == tileCoords) return true;
        return obj.GetComponent<KingController>() == activador || obj.GetComponent<PawnController>() == activador;
    }

    private bool EstaVisibleYRecolectable(MovableTileObject obj)
    {
        // Si es pocima (u otro recolectable) y aún no está activada, la ignoramos
        var pocion = obj.GetComponent<Potion1PM>();
     //   if (pocion != null && !pocion.IsVisible()) return false;

        // Aquí podrías agregar lógica para otros recolectables similares

        return true;
    }

    private Vector2Int CalcularDireccion(Vector2Int delta)
    {
    if (delta.x == 0 && delta.y == 0) return Vector2Int.zero;  
    if (Mathf.Abs(delta.x) == Mathf.Abs(delta.y)) 
        return new Vector2Int((int)Mathf.Sign(delta.x), (int)Mathf.Sign(delta.y));
    if (delta.y == 0) 
        return new Vector2Int((int)Mathf.Sign(delta.x), 0);
    if (delta.x == 0) 
        return new Vector2Int(0, (int)Mathf.Sign(delta.y));
    return Mathf.Abs(delta.x) > Mathf.Abs(delta.y)
        ? new Vector2Int((int)Mathf.Sign(delta.x), 0)
        : new Vector2Int(0, (int)Mathf.Sign(delta.y));
    }


    private bool EsDentroTablero(Vector2Int p)
        => p.x >= 0 && p.x <= 7 && p.y >= 0 && p.y <= 7;

    public void VerificarTurnoActual(int turnoActual) { }
}
