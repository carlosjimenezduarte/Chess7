using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class BoardManagerGlobal : MonoBehaviour
{
    public static BoardManagerGlobal Instance;

    [Header("Lista de todas las casillas del tablero")]
    public List<Tile> tiles = new List<Tile>();

    private Dictionary<Vector2Int, List<IPieceWithPosition>> tableroOcupacion
        = new Dictionary<Vector2Int, List<IPieceWithPosition>>();

    private void Awake()
    {
        Instance = this;

        foreach (Tile tile in tiles)
        {
            tableroOcupacion[tile.tileCoords] = new List<IPieceWithPosition>();
            Debug.Log($"📋 Tile inicializado en {tile.tileCoords}");
        }


    }

    private void Start()
    {
        InicializarRegistroDeFichas();
    }

    // ✅ Registrar o mover una ficha
    public void RegistrarMovimiento(IPieceWithPosition pieza, Vector2Int nuevaPos)
    {
        foreach (var lista in tableroOcupacion.Values)
            lista.Remove(pieza);

        if (!tableroOcupacion.ContainsKey(nuevaPos))
            tableroOcupacion[nuevaPos] = new List<IPieceWithPosition>();

        tableroOcupacion[nuevaPos].Add(pieza);
        Debug.Log($"📌 {pieza} registrado en {nuevaPos}");
    }

    // ✅ Obtener fichas en una casilla
    public List<IPieceWithPosition> ObtenerObjetosEn(Vector2Int pos)
    {
        if (tableroOcupacion.TryGetValue(pos, out var lista))
            return lista;

        return new List<IPieceWithPosition>();
    }

    // ✅ Ver si hay ficha o recolectable en casilla (opcionalmente ignora alguna)
    public bool EstaCasillaOcupada(Vector2Int pos, IPieceWithPosition ignorar = null)
    {
        var objetos = ObtenerObjetosEn(pos);
        foreach (var obj in objetos)
        {
            if (obj == ignorar) continue;

            bool esFicha = obj is IFicha;
            bool esRecolectable = obj is IObjetoRecoleccionable;

            if (esFicha || esRecolectable)
            {
                Debug.Log($"🚫 Casilla {pos} ocupada por {obj}");
                return true;
            }
        }
        return false;
    }

    public Vector3 GetTileWorldPosition(Vector2Int tileCoords)
    {
        if (tileCoords.x < 0 || tileCoords.y < 0 || tileCoords.x > 7 || tileCoords.y > 7)
            return new Vector3(10000, 10000, 0);

        foreach (Tile tile in tiles)
        {
            if (tile.tileCoords == tileCoords)
                return tile.transform.localPosition;
        }

        Debug.LogWarning($"No se encontró tile en {tileCoords}");
        return Vector3.zero;
    }

    public Tile GetTileAt(Vector2Int coords)
    {
        foreach (Tile tile in tiles)
        {
            if (tile.tileCoords == coords)
                return tile;
        }
        Debug.LogWarning($"No se encontró Tile en {coords}");
        return null;
    }

    public Vector2 GetTileAnchoredPosition(Vector2Int tileCoords)
    {
        Tile tile = GetTileAt(tileCoords);
        if (tile != null)
            return tile.GetComponent<RectTransform>().anchoredPosition;

        Debug.LogWarning($"No se encontró tile en {tileCoords}");
        return Vector2.zero;
    }

    public Vector2Int GetClosestTileCoords(Vector3 worldPos)
    {
        Tile closest = null;
        float minDist = Mathf.Infinity;

        foreach (Tile tile in tiles)
        {
            float dist = Vector3.Distance(tile.transform.localPosition, worldPos);
            if (dist < minDist)
            {
                minDist = dist;
                closest = tile;
            }
        }

        if (closest != null)
            return closest.tileCoords;

        Debug.LogWarning($"No se encontró tile cercano a {worldPos}");
        return Vector2Int.zero;
    }

    public static Vector2Int FuturoIncierto = new Vector2Int(100, 100);
    public static Vector2Int DimensionDivina = new Vector2Int(-1, -9999);

    /// 🔍 Devuelve todos los objetos Movables dentro del tablero, ordenados desde un origen
    public List<MovableTileObject> GetObjetosMoviblesOrdenadosDesde(Vector2Int origen)
    {
        var todos = FindObjectsByType<MovableTileObject>(FindObjectsSortMode.None);

        // Solo los que estén activos en tablero y dentro de límites válidos
        var movibles = new List<MovableTileObject>();

        foreach (var obj in todos)
        {
            if (!obj.activoEnTablero)
            {
                Debug.Log($"🕳 {obj.name} ignorado (no activo en tablero).");
                continue;
            }

            if (obj.tileCoords.x < 0 || obj.tileCoords.x > 7 || obj.tileCoords.y < 0 || obj.tileCoords.y > 7)
            {
                Debug.Log($"🌌 {obj.name} ignorado (fuera del tablero en {obj.tileCoords}).");
                continue;
            }

            movibles.Add(obj);
        }

        // Ordenamos de más lejos a más cerca desde el origen
        movibles.Sort((a, b) =>
            Vector2Int.Distance(b.tileCoords, origen).CompareTo(Vector2Int.Distance(a.tileCoords, origen)));

        return movibles;
    }
    
    private void InicializarRegistroDeFichas()
{
    Debug.Log("📜 Iniciando registro global de fichas...");

    var componentes = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

    var fichas = componentes.OfType<IFicha>();
    foreach (var ficha in fichas)
    {
        var pieza = ficha as MonoBehaviour;
        var posicion = pieza.GetComponent<MovableTileObject>()?.tileCoords ?? new Vector2Int(-1, -1);

        if (posicion.x >= 0)
        {
            if (ficha is IPieceWithPosition piezaConPos)
            {
                Debug.Log($"📍 Registrando ficha inicial: {pieza.name} en {posicion}");
                RegistrarMovimiento(piezaConPos, posicion);
            }
            else
            {
                Debug.LogWarning($"⚠️ {pieza.name} no implementa IPieceWithPosition. No registrada.");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ {pieza.name} no tiene coordenadas válidas. No registrada.");
        }
    }
}

}
