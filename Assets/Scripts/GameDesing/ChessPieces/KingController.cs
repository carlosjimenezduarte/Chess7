using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class KingController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition
{
    public int puntosMovimientoMax = 3;
    public int puntosMovimientoActual;
    public int puntosAccionActual = 5;
    public int turnosRestantes = 7;

    private Vector2Int posicionActual;
    private bool juegoActivo = false;
    private bool mostrandoMovimientos = false;

    private void Start()
    {
        PiecePositioner piecePositioner = GetComponent<PiecePositioner>();
        if (piecePositioner != null)
        {
            posicionActual = piecePositioner.tileCoords;
            Debug.Log($"♔ Rey inició en {posicionActual}");
        }
        else
        {
            Debug.LogWarning("⚠️ No hay PiecePositioner en el Rey. Usando (0,0).");
            posicionActual = new Vector2Int(0, 0);
        }
    }

    // 👑 interfaz
    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        posicionActual = nuevaPos;
    }

    public Vector2Int GetPosicionActual()
    {
        return posicionActual;
    }

    public void ActivarJuego()
    {
        juegoActivo = true;
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
    }

    public void MostrarMovimientoPosible()
    {
        if (!juegoActivo) return;

        Debug.Log($"Mostrando casillas alcanzables con {puntosMovimientoActual} PM.");

        foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
        {
            int distancia = Mathf.Abs(tile.tileCoords.x - posicionActual.x) + Mathf.Abs(tile.tileCoords.y - posicionActual.y);
            bool puedeAlcanzar = distancia <= puntosMovimientoActual;
            tile.HighlightMove(puedeAlcanzar);
        }
    }

    private void OcultarMovimientos()
    {
        foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
            tile.HighlightMove(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!juegoActivo) return;

        mostrandoMovimientos = !mostrandoMovimientos;

        if (mostrandoMovimientos)
        {
            Debug.Log("🟢 Mostrando previsualización de movimientos del Rey.");
            MostrarMovimientoPosible();
        }
        else
        {
            Debug.Log("🔴 Ocultando previsualización de movimientos del Rey.");
            OcultarMovimientos();
        }
    }

    public void MoverA(Vector2Int nuevaPos)
    {
        if (!juegoActivo) return;

        int distancia = Mathf.Abs(posicionActual.x - nuevaPos.x) + Mathf.Abs(posicionActual.y - nuevaPos.y);

        if (distancia <= puntosMovimientoActual)
        {
            Debug.Log($"Moviendo al Rey desde {posicionActual} a {nuevaPos}, consumiendo {distancia} PM.");

            Vector2Int paso = posicionActual;

            // 🔥 Recorre casilla por casilla
            while (paso != nuevaPos)
            {
                if (paso.x < nuevaPos.x) paso.x++;
                else if (paso.x > nuevaPos.x) paso.x--;

                if (paso.y < nuevaPos.y) paso.y++;
                else if (paso.y > nuevaPos.y) paso.y--;

                // 🚀 Actualiza posición lógica en cada paso
                SetPosicionActual(paso);

                Debug.Log($"🚶 El Rey pasa por {paso}");

                foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
                {
                    efecto.RevisarSiReyLlegó(paso, this);
                }
            }

            transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);
            puntosMovimientoActual -= distancia;

            MostrarMovimientoPosible();
            mostrandoMovimientos = true;

            // ⚔ actualiza amenaza de la Reina
            var reina = FindFirstObjectByType<QueenEnemyController>();
            if (reina != null)
                reina.VerificarSiReyEstaAmenazado(posicionActual);

            FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

            // 🏁 Meta
            if (posicionActual == new Vector2Int(7, 7))
            {
                Debug.Log("🚀 El Rey llegó a la meta (H8). Calculando bonus.");

                int bonus = turnosRestantes * 50;
                PlayerScore.Instance.AddScore(bonus);

                LevelResultUI.Instance.ShowResults(
                    LevelProgress.Instance.keysCollected,
                    LevelProgress.Instance.hasDiamond,
                    turnosRestantes,
                    PlayerScore.Instance.GetTotalScore()
                );

                juegoActivo = false;
                FindFirstObjectByType<ChessGameManager>()?.DetenerJuego();
            }

            // 💀 Derrota
            if (turnosRestantes <= 0)
            {
                Debug.Log("💀 El Rey sin vidas.");

                LevelResultUI.Instance.ShowResults(
                    LevelProgress.Instance.keysCollected,
                    LevelProgress.Instance.hasDiamond,
                    0,
                    PlayerScore.Instance.GetTotalScore()
                );

                juegoActivo = false;
                FindFirstObjectByType<ChessGameManager>()?.DetenerJuego();
            }
        }
        else
        {
            Debug.Log("🚫 Movimiento no permitido, no hay suficientes PM.");
        }
    }

    public void ReiniciarTurno()
    {
        puntosMovimientoActual = puntosMovimientoMax;
        Debug.Log($"Nuevo turno: {puntosMovimientoActual} PM.");
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
    }

    public void GanarPuntoMovimiento(int cantidad)
    {
        puntosMovimientoActual += cantidad;
        Debug.Log($"El Rey gana +{cantidad} PM. Total: {puntosMovimientoActual}.");
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
    }

    public void GanarVida(int cantidad)
    {
        turnosRestantes += cantidad;
        Debug.Log($"❤️ El Rey gana +{cantidad} vida(s). Ahora tiene {turnosRestantes}.");
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
    }

    //    public void TestMoverRey()
    //    {
    //        Debug.Log("Botón test presionado. Moviendo Rey a (2,2).");
    //        MoverA(new Vector2Int(2, 2));
    //    }

    public void RestarTurno()
    {
        turnosRestantes--;
        Debug.Log($"Turnos restantes: {turnosRestantes}");
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

        if (turnosRestantes <= 0)
        {
            Debug.Log("💀 Sin vidas tras pasar turno.");

            LevelResultUI.Instance.ShowResults(
                LevelProgress.Instance.keysCollected,
                LevelProgress.Instance.hasDiamond,
                0,
                PlayerScore.Instance.GetTotalScore()
            );
            juegoActivo = false;
            FindFirstObjectByType<ChessGameManager>()?.DetenerJuego();
        }
    }
    public void ConsumirPA(int cantidad)
    {
    puntosAccionActual -= cantidad;
    Debug.Log($"♔ El Rey consume {cantidad} PA. Quedan: {puntosAccionActual}");
    FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
    }

}
