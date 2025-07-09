using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class QueenEnemyController : MonoBehaviour, IPointerClickHandler, ITileEffect
{
    [Header("Alcances")]
    public int rangoKillZone = 3;
    public int rangoRangeZone = 5;

    [Header("Daños a distancia")]
    public int quitarPM = 2;
    public int quitarPA = 2;
    public int quitarTurnos = 1;

    private Vector2Int posicionActual;
    private bool mostrandoRango = false;

    private void Start()
    {
        PiecePositioner piecePositioner = GetComponent<PiecePositioner>();
        if (piecePositioner != null)
        {
            posicionActual = piecePositioner.tileCoords;
            Debug.Log($"♛ La Reina inicializó su posición lógica en {posicionActual} según el PiecePositioner.");
        }
        else
        {
            Debug.LogWarning("⚠️ No se encontró PiecePositioner en la Reina. Usando posición por defecto (0,0).");
            posicionActual = new Vector2Int(0, 0);
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ChessGameManager manager = FindFirstObjectByType<ChessGameManager>();
        if (manager == null || !manager.IsJuegoActivo())
        {
            Debug.Log("♛ No se puede mostrar rango: el juego no está activo.");
            return;
        }

        mostrandoRango = !mostrandoRango;

        if (mostrandoRango)
        {
            Debug.Log("♛ Mostrando rango de ataque de la Reina.");
            MostrarRangoDeAtaque();
        }
        else
        {
            Debug.Log("♛ Ocultando rango de ataque de la Reina.");
            OcultarRangoDeAtaque();
        }
    }

    public void MostrarRangoDeAtaque()
    {
        OcultarRangoDeAtaque(); 

        Tile tileCentral = BoardManagerGlobal.Instance.GetTileAt(posicionActual);
        if (tileCentral != null)
            tileCentral.HighlightEnemyKillZone(true);

        Vector2Int[] direcciones = new Vector2Int[]
        {
            new Vector2Int(1, 0),  new Vector2Int(-1, 0),
            new Vector2Int(0, 1),  new Vector2Int(0, -1),
            new Vector2Int(1, 1),  new Vector2Int(-1, 1),
            new Vector2Int(1, -1), new Vector2Int(-1, -1)
        };

        foreach (var dir in direcciones)
        {
            for (int i = 1; i <= rangoRangeZone; i++)
            {
                Vector2Int coord = posicionActual + dir * i;
                Tile tile = BoardManagerGlobal.Instance.GetTileAt(coord);

                if (tile == null)
                    break;

                if (i <= rangoKillZone)
                    tile.HighlightEnemyKillZone(true);
                else
                    tile.HighlightEnemyRangeZone(true);
            }
        }
    }

    public void OcultarRangoDeAtaque()
    {
        foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
        {
            tile.ResetColor();
        }
    }

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        int dx = posicionRey.x - posicionActual.x;
        int dy = posicionRey.y - posicionActual.y;

        bool esDireccionValida = (dx == 0 || dy == 0 || Mathf.Abs(dx) == Mathf.Abs(dy));
        if (!esDireccionValida)
            return;

        Vector2Int direccionRey = new Vector2Int(
            dx == 0 ? 0 : (dx > 0 ? 1 : -1),
            dy == 0 ? 0 : (dy > 0 ? 1 : -1)
        );

        Vector2Int paso = posicionActual + direccionRey;
        int pasosContados = 1;

        while (pasosContados <= rangoRangeZone && paso != posicionRey)
        {
            paso += direccionRey;
            pasosContados++;
        }

        if (paso == posicionRey)
        {
            if (pasosContados <= rangoKillZone)
            {
                Debug.Log("💀 El Rey entró en el alcance LETAL de la Reina.");
                StartCoroutine(MatarReyDespuesDelay(rey, posicionRey));
            }
            else if (pasosContados <= rangoRangeZone)
            {
                Debug.Log("🏹 El Rey entró en el alcance de ATAQUE a distancia de la Reina.");

                rey.GanarPuntoMovimiento(-quitarPM);
                rey.puntosAccionActual -= quitarPA;
                rey.turnosRestantes -= quitarTurnos;

                // 🔥 Dibuja la línea exclusiva en color de ataque real
                Vector2Int pasoLinea = posicionActual + direccionRey;
                while (pasoLinea != posicionRey)
                {
                    Tile tile = BoardManagerGlobal.Instance.GetTileAt(pasoLinea);
                    if (tile == null) break;

                    tile.HighlightEnemyAttack(true);
                    pasoLinea += direccionRey;
                }
            }
        }
    }

    private IEnumerator MatarReyDespuesDelay(KingController rey, Vector2Int posicionRey)
    {
        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(posicionRey);
        yield return new WaitForSeconds(2f);

        rey.turnosRestantes = 0;
        LevelResultUI.Instance.ShowResults(
            LevelProgress.Instance.keysCollected,
            LevelProgress.Instance.hasDiamond,
            0,
            PlayerScore.Instance.GetTotalScore()
        );
        FindFirstObjectByType<ChessGameManager>().DetenerJuego();
    }

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        posicionActual = nuevaPos;
    }

    public Vector2Int GetPosicionActual()
    {
        return posicionActual;
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        // por ahora sin lógica especial por turno
    }
}
