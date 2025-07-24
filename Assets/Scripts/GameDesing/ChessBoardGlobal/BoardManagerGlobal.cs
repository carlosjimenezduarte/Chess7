using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class BoardManagerGlobal : MonoBehaviour
{
    public static BoardManagerGlobal Instance;

    public int Ancho => 8;
    public int Alto => 8;

    [Header("Lista de todas las casillas del tablero")]
    public List<Tile> tiles = new List<Tile>();

    private Dictionary<Vector2Int, List<IPieceWithPosition>> tableroOcupacion
        = new Dictionary<Vector2Int, List<IPieceWithPosition>>();

    private List<string> mensajesInternos = new List<string>();

    private void Awake()
    {
        Instance = this;

        foreach (Tile tile in tiles)
        {
            tableroOcupacion[tile.tileCoords] = new List<IPieceWithPosition>();
            AgregarMensajeInterno($"📋 Tile inicializado en {tile.tileCoords}");
        }
    }

    private void Start()
    {
        InicializarRegistroDeFichas();
    }

    public void RegistrarMovimiento(IPieceWithPosition pieza, Vector2Int nuevaPos)
    {
        foreach (var lista in tableroOcupacion.Values)
            lista.Remove(pieza);

        if (!tableroOcupacion.ContainsKey(nuevaPos))
            tableroOcupacion[nuevaPos] = new List<IPieceWithPosition>();

        tableroOcupacion[nuevaPos].Add(pieza);
        AgregarMensajeInterno($"📌 {pieza} registrado en {nuevaPos}");
    }

    public List<IPieceWithPosition> ObtenerObjetosEn(Vector2Int pos, bool incluirRecolectables = true)
    {
        if (tableroOcupacion.TryGetValue(pos, out var lista))
        {
            return lista
                .Where(obj => obj != null && ((MonoBehaviour)obj) != null)
                .Where(obj => incluirRecolectables || !(obj is IObjetoRecoleccionable))
                .ToList();
        }

        return new List<IPieceWithPosition>();
    }

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
                AgregarMensajeInterno($"🚫 Casilla {pos} ocupada por {obj}");
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

        AgregarMensajeInterno($"No se encontró tile en {tileCoords}");
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

    public List<MovableTileObject> GetObjetosMoviblesOrdenadosDesde(Vector2Int origen)
    {
        var todos = FindObjectsByType<MovableTileObject>(FindObjectsSortMode.None);

        var movibles = new List<MovableTileObject>();

        foreach (var obj in todos)
        {
            if (!obj.activoEnTablero)
            {
                AgregarMensajeInterno($"🕳 {obj.name} ignorado (no activo en tablero).");
                continue;
            }

            if (obj.tileCoords.x < 0 || obj.tileCoords.x > 7 || obj.tileCoords.y < 0 || obj.tileCoords.y > 7)
            {
                AgregarMensajeInterno($"🌌 {obj.name} ignorado (fuera del tablero en {obj.tileCoords}).");
                continue;
            }

            movibles.Add(obj);
        }

        movibles.Sort((a, b) =>
            Vector2Int.Distance(b.tileCoords, origen).CompareTo(Vector2Int.Distance(a.tileCoords, origen)));

        return movibles;
    }

    private void InicializarRegistroDeFichas()
    {
        AgregarMensajeInterno("📜 Iniciando registro global de fichas...");

        var componentes = Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

        // 🔹 Paso 1: Registrar todas las fichas (IFicha)
        foreach (var ficha in componentes.OfType<IFicha>())
        {
            var pieza = ficha as MonoBehaviour;

            if (pieza is Expansion || pieza is Interruption)
            {
                AgregarMensajeInterno($"⛔ {pieza.name} es un Expansion. No se registrará como ficha.");
                continue;
            }

            if (ficha is IPieceWithPosition piezaConPos)
            {
                var posicion = piezaConPos.GetPosicionActual();
                if (posicion.x >= 0)
                {
                    AgregarMensajeInterno($"📍 Registrando ficha inicial: {pieza.name} en {posicion}");
                    RegistrarMovimiento(piezaConPos, posicion);
                }
            }
            else
            {
                Debug.LogWarning($"⚠️ {pieza.name} no implementa IPieceWithPosition. No registrada.");
            }
        }

        // 🔹 Paso 2: Registrar objetos recoleccionables (especiales y normales)
        foreach (var objeto in componentes.OfType<IObjetoRecoleccionable>())
        {
            if (objeto is MonoBehaviour mono && objeto is IPieceWithPosition objetoConPos)
            {
                // Ya fue registrado como ficha (no repetir)
                if (mono is IFicha) continue;

                var pos = objetoConPos.GetPosicionActual();
                if (pos.x >= 0)
                {
                    AgregarMensajeInterno($"📌 {mono.name} ({mono.GetType().Name}) registrado en {pos}");
                    RegistrarMovimiento(objetoConPos, pos);
                }
            }
        }
    }

    // ✅ Recolección de mensajes internos para el Árbitro Silencioso
    public void AgregarMensajeInterno(string mensaje)
    {
        mensajesInternos.Add(mensaje);
    }

    public void ReportarEstadoActualDelTablero()
    {
        StringBuilder reporte = new StringBuilder();

        reporte.AppendLine("🧠 [Árbitro Silencioso] Estado actual del tablero:");

        // 📝 Mensajes personalizados antes del reporte de casillas
        if (mensajesInternos.Count > 0)
        {
            reporte.AppendLine("📝 Mensajes recientes:");
            foreach (var mensaje in mensajesInternos)
                reporte.AppendLine("   " + mensaje);
            reporte.AppendLine();
        }

        mensajesInternos.Clear(); // Limpiar después de imprimir

        foreach (var par in tableroOcupacion)
        {
            Vector2Int coords = par.Key;
            var lista = par.Value;

            if (lista.Count == 0)
            {
                reporte.AppendLine($"📭 Casilla {coords}: vacía.");
                continue;
            }

            reporte.AppendLine($"📍 Casilla {coords}: contiene {lista.Count} objeto(s).");

            foreach (var obj in lista)
            {
                if (obj == null || ((MonoBehaviour)obj) == null) continue;

                string nombre = ((MonoBehaviour)obj).name;
                string tipo = obj.GetType().Name;

                string interfaces = "";
                if (obj is IFicha) interfaces += "IFicha ";
                if (obj is IFichaAliada) interfaces += "IFichaAliada ";
                if (obj is IFichaEnemiga) interfaces += "IFichaEnemiga ";
                if (obj is ITileEffect) interfaces += "ITileEffect ";
                if (obj is IObjetoRecoleccionable) interfaces += "IObjetoRecoleccionable ";

                Vector2Int posicionReportada = obj.GetPosicionActual();
                bool activo = true;
                string estatus = "";

                if (obj is MovableTileObject mto)
                {
                    activo = mto.activoEnTablero;
                    if (mto.esInamovible)
                        estatus = "🪨 Inamovible";
                    else
                        estatus = "Movible";
                }

                reporte.AppendLine($"   🔹 {nombre} ({tipo}) -> Pos: {posicionReportada}, Interfaces: [{interfaces}], Activo: {activo}, {estatus}");

            }
        }

        reporte.AppendLine("✅ Fin del reporte del Árbitro Silencioso.\n");

        Debug.Log(reporte.ToString());
    }
    public void FinalizarTurno()
    {
        // Aquí puedes agregar otras tareas del fin de turno si las hay
        VerificarEfectosTemporales();
    }

    private void VerificarEfectosTemporales()
    {
        foreach (var obj in FindObjectsByType<Potion1PM>(FindObjectsSortMode.None))
        {
            if (obj == null) continue;
            obj.VerificarAutoChequeoGeneral();
        }
    }
    public void ReportarFichaInamovible(IPieceWithPosition pieza)
    {
        if (pieza == null || ((MonoBehaviour)pieza) == null) return;

        string nombre = ((MonoBehaviour)pieza).name;
        string tipo = pieza.GetType().Name;

        string interfaces = "";
        if (pieza is IFicha) interfaces += "IFicha ";
        if (pieza is IFichaAliada) interfaces += "IFichaAliada ";
        if (pieza is IFichaEnemiga) interfaces += "IFichaEnemiga ";
        if (pieza is ITileEffect) interfaces += "ITileEffect ";
        if (pieza is IObjetoRecoleccionable) interfaces += "IObjetoRecoleccionable ";
        if (pieza is IFichaInmovil) interfaces += "IFichaInmovil ";

        Vector2Int posicion = pieza.GetPosicionActual();
        bool activo = true;
        string estatus = "🪨 Inamovible";

        if (pieza is MovableTileObject mto)
        {
            activo = mto.activoEnTablero;
        }

        string reporte = $"🪨 Reporte manual: {nombre} ({tipo}) -> Pos: {posicion}, Interfaces: [{interfaces}], Activo: {activo}, {estatus}";
        AgregarMensajeInterno(reporte);
    }

    public int GetTurnoActual()
    {
        var gm = FindFirstObjectByType<ChessGameManager>();
        if (gm != null)
            return (int)gm
                .GetType()
                .GetField("turnoActual", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                ?.GetValue(gm);

        return -1; // Si no se encuentra, se devuelve un valor inválido
    }
    
    
}