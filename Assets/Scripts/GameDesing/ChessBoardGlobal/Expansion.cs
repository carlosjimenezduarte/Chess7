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

    public void RevisarSiFichaLlegó(Vector2Int posicionFicha, IFicha ficha)
    {
        Debug.Log($"🧩 Expansion compara ficha en {posicionFicha} con su posición {tileCoords}");
        if (posicionFicha == tileCoords)
        {
            Debug.Log($"💥 Expansion activado por ficha universal: {((MonoBehaviour)ficha).name}");
            ActivarExpansion((MonoBehaviour)ficha);
        }
         else
        {
        Debug.Log($"❌ Posición no coincide: Expansion en {tileCoords}, ficha en {posicionFicha}");
        }
    }

    public void VerificarTurnoActual(int turnoActual) { }

    private void ActivarExpansion(MonoBehaviour activador)
    {
        Debug.Log($"💢 Expansion en {tileCoords} activado por {activador.name}.");

        List<MovableTileObject> todos = BoardManagerGlobal.Instance.GetObjetosMoviblesOrdenadosDesde(tileCoords);

        foreach (var obj in todos)
        {
            if (EsIgnorable(obj, activador)) continue;

            if (!EsDentroTablero(obj.tileCoords))
            {
                Debug.Log($"🛡 {obj.name} está fuera del tablero en {obj.tileCoords}, ignorado por Expansion.");
                continue;
            }

            if (!EstaVisibleYRecolectable(obj)) continue;

            Vector2Int dir = CalcularDireccion(obj.tileCoords - tileCoords);
            Vector2Int destino = obj.tileCoords + dir;

            if (!EsDentroTablero(destino))
            {
                Debug.Log($"🚫 {obj.name} no puede salir hacia {destino}.");
                continue;
            }

            Debug.Log($"💥 {obj.name} empujado de {obj.tileCoords} a {destino}");
            obj.MoverA(destino);
        }
    }

    private bool EsIgnorable(MovableTileObject obj, MonoBehaviour activador)
    {
        if (obj.tileCoords == tileCoords) return true;

        // Ignora si es la misma ficha que activó la expansión
        if (activador != null && ReferenceEquals(obj.gameObject, activador.gameObject))
        {
            Debug.Log($"🛑 Ignorando {obj.name} porque es el activador.");
            return true;
        }

        return false;
    }

    private bool EstaVisibleYRecolectable(MovableTileObject obj)
    {
        var pocion = obj.GetComponent<Potion1PM>();
        // Si quieres agregar lógica de visibilidad, lo haces aquí

        return true; // Se puede mover por defecto
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
}
