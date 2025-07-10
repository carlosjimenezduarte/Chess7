using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

public class QueenEnemyController : MonoBehaviour, IPointerClickHandler, ITileEffect
{
    [Header("Alcances tipo Reina")]
    public int rangoKillZone = 3;
    public int rangoRangeZone = 5;

    [Header("Prefab para zonas peligrosas")]
    public GameObject prefabRojo;

    [Header("Padre para overlays")]
    public Transform dangerOverlayParent;

    private Vector2Int posicionActual;
    private bool mostrandoRango = false;

    private List<GameObject> overlaysInstanciados = new List<GameObject>();
    private Vector2Int ultimaPosicionAmenaza = new Vector2Int(-99, -99);

    private void Start()
    {
        PiecePositioner piecePositioner = GetComponent<PiecePositioner>();
        if (piecePositioner != null)
        {
            posicionActual = piecePositioner.tileCoords;
            Debug.Log($"♛ Reina inició en {posicionActual}");
        }
        else
        {
            Debug.LogWarning("⚠️ No hay PiecePositioner en la Reina. Usando (0,0).");
            posicionActual = new Vector2Int(0, 0);
        }
    }

    // === Click manual para mostrar u ocultar el rango con Tiles ===
    public void OnPointerClick(PointerEventData eventData)
    {
        ChessGameManager manager = FindFirstObjectByType<ChessGameManager>();
        if (manager == null || !manager.IsJuegoActivo())
        {
            Debug.Log("♛ No se puede mostrar rango: juego no activo.");
            return;
        }

        mostrandoRango = !mostrandoRango;

        if (mostrandoRango)
        {
            Debug.Log("♛ Mostrando rango de ataque (Tiles)");
            MostrarRangoDeAtaque();
        }
        else
        {
            Debug.Log("♛ Ocultando rango de ataque (Tiles)");
            OcultarRangoDeAtaque();
        }
    }

    // === Visualización con Prefabs en línea directa al Rey ===
    public void VerificarSiReyEstaAmenazado(Vector2Int posicionRey)
    {
        // Limpia cualquier overlay anterior
        foreach (var obj in overlaysInstanciados)
            Destroy(obj);
        overlaysInstanciados.Clear();

        int dx = posicionRey.x - posicionActual.x;
        int dy = posicionRey.y - posicionActual.y;

        bool esDireccionValida = (dx == 0 || dy == 0 || Mathf.Abs(dx) == Mathf.Abs(dy));
        if (!esDireccionValida)
        {
            ultimaPosicionAmenaza = new Vector2Int(-99, -99);
            return;
        }

        Vector2Int direccionRey = new Vector2Int(
            dx == 0 ? 0 : (dx > 0 ? 1 : -1),
            dy == 0 ? 0 : (dy > 0 ? 1 : -1)
        );

        Vector2Int paso = posicionActual;
        int pasosContados = 0;
        List<Vector2Int> lineaDeAtaque = new List<Vector2Int>();

        while (pasosContados <= rangoRangeZone && paso != posicionRey)
        {
            if (paso.x < 0 || paso.y < 0 || paso.x > 7 || paso.y > 7)
                break;

            lineaDeAtaque.Add(paso);
            paso += direccionRey;
            pasosContados++;
        }

        if (paso == posicionRey)
        {
            lineaDeAtaque.Add(posicionRey);

            Debug.Log($"♛ Pintando línea de prefabs hacia Rey en {posicionRey}");

            foreach (Vector2Int coord in lineaDeAtaque)
            {
                GameObject overlay = Instantiate(prefabRojo, dangerOverlayParent);
                overlay.GetComponent<RectTransform>().anchoredPosition = 
                    BoardManagerGlobal.Instance.GetTileAnchoredPosition(coord);
                overlaysInstanciados.Add(overlay);
            }

            ultimaPosicionAmenaza = posicionRey;
        }
        else
        {
            ultimaPosicionAmenaza = new Vector2Int(-99, -99);
        }
    }

    // === Método ITileEffect: lógica para atacar directamente al Rey ===
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
            if (paso.x < 0 || paso.y < 0 || paso.x > 7 || paso.y > 7)
                break;

            paso += direccionRey;
            pasosContados++;
        }

        if (paso == posicionRey)
        {
            Debug.Log($"💀 El Rey fue alcanzado en {posicionRey}");
            if (pasosContados <= rangoKillZone)
            {
                StartCoroutine(MatarReyDespuesDelay(rey, posicionRey));
            }
            else
            {
                rey.GanarPuntoMovimiento(-2);
                rey.puntosAccionActual -= 2;
                rey.turnosRestantes -= 1;
            }
        }
    }

    private IEnumerator MatarReyDespuesDelay(KingController rey, Vector2Int posicionRey)
    {
        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(posicionRey);

        Destroy(rey.gameObject);
        Debug.Log("💀 Rey destruido visualmente del tablero.");

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

    // === Previsualización con Tiles ===
    public void MostrarRangoDeAtaque()
    {
        OcultarRangoDeAtaque();

        Tile tileCentral = BoardManagerGlobal.Instance.GetTileAt(posicionActual);
        if (tileCentral != null)
            tileCentral.HighlightEnemyKillZone(true);

        Vector2Int[] direcciones = {
            new Vector2Int(1,0), new Vector2Int(-1,0),
            new Vector2Int(0,1), new Vector2Int(0,-1),
            new Vector2Int(1,1), new Vector2Int(-1,1),
            new Vector2Int(1,-1), new Vector2Int(-1,-1)
        };

        foreach (var dir in direcciones)
        {
            for (int i = 1; i <= rangoRangeZone; i++)
            {
                Vector2Int coord = posicionActual + dir * i;
                if (coord.x < 0 || coord.y < 0 || coord.x > 7 || coord.y > 7)
                    break;

                Tile tile = BoardManagerGlobal.Instance.GetTileAt(coord);
                if (tile == null) break;

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
            tile.ResetColor();
    }

    public void SetPosicionActual(Vector2Int nuevaPos) => posicionActual = nuevaPos;
    public Vector2Int GetPosicionActual() => posicionActual;
    public void VerificarTurnoActual(int turnoActual) { /* reservado para el futuro */ }
}
