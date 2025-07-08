using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class KingController : MonoBehaviour, IPointerClickHandler
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
        puntosMovimientoActual = puntosMovimientoMax;
        posicionActual = new Vector2Int(0, 0);
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

        Debug.Log("Mostrando casillas alcanzables con " + puntosMovimientoActual + " PM.");

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
        {
            tile.HighlightMove(false);
        }
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
        Debug.Log($"Moviendo al Rey desde {posicionActual} a {nuevaPos} con recorrido paso a paso, consumiendo {distancia} PM.");

        Vector2Int paso = posicionActual;

        // Recorre lógica paso a paso
        while (paso != nuevaPos)
        {
            if (paso.x < nuevaPos.x) paso.x++;
            else if (paso.x > nuevaPos.x) paso.x--;

            if (paso.y < nuevaPos.y) paso.y++;
            else if (paso.y > nuevaPos.y) paso.y--;

            Debug.Log($"🚶 El Rey pasa por {paso}");

            foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
            {
                efecto.RevisarSiReyLlegó(paso, this);
            }
        }

        posicionActual = nuevaPos;
        transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

        puntosMovimientoActual -= distancia;

        MostrarMovimientoPosible();
        mostrandoMovimientos = true;

        FindFirstObjectByType<ChessGameManager>().ActualizarHUD();

            // 🚀 CHEQUEO META
            if (posicionActual == new Vector2Int(7, 7))
            {
                Debug.Log("🚀 El Rey llegó a la meta (H8). Calculando bonus y mostrando resultados.");

                int bonus = turnosRestantes * 50;
                PlayerScore.Instance.AddScore(bonus);
                Debug.Log($"🎉 Bonus por vidas: {turnosRestantes} x 50 = +{bonus}");

                LevelResultUI.Instance.ShowResults(
                    LevelProgress.Instance.keysCollected,
                    LevelProgress.Instance.hasDiamond,
                    turnosRestantes,
                    PlayerScore.Instance.GetTotalScore()
                );

                juegoActivo = false;
                FindFirstObjectByType<ChessGameManager>().DetenerJuego();
        }

            // 🚀 CHEQUEO DERROTA
            if (turnosRestantes <= 0)
            {
                Debug.Log("💀 El Rey se quedó sin vidas. Mostrando panel de derrota.");

                LevelResultUI.Instance.ShowResults(
                    LevelProgress.Instance.keysCollected,
                    LevelProgress.Instance.hasDiamond,
                    0,
                    PlayerScore.Instance.GetTotalScore()
                );

                juegoActivo = false;
                FindFirstObjectByType<ChessGameManager>().DetenerJuego();
        }
    }
    else
    {
        Debug.Log("Movimiento no permitido, no hay suficientes PM.");
    }
    }


    public void ReiniciarTurno()
    {
        puntosMovimientoActual = puntosMovimientoMax;
        Debug.Log($"Nuevo turno. El Rey tiene {puntosMovimientoActual} PM.");
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
    }

    public void GanarPuntoMovimiento(int cantidad)
    {
        puntosMovimientoActual += cantidad;
        Debug.Log($"El Rey ganó +{cantidad} PM y ahora tiene {puntosMovimientoActual} PM.");
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
        FindFirstObjectByType<ChessGameManager>().ActualizarHUD();
    }

    public void GanarVida(int cantidad)
    {
        turnosRestantes += cantidad;
        Debug.Log($"❤️ El Rey ganó +{cantidad} vida(s) y ahora tiene {turnosRestantes} vidas restantes.");
        FindFirstObjectByType<ChessGameManager>().ActualizarHUD();
    }

    public void TestMoverRey()
    {
        Debug.Log("Botón test presionado. Moviendo Rey a (2,2).");
        MoverA(new Vector2Int(2, 2));
    }

    public void RestarTurno()
    {
        turnosRestantes--;
        Debug.Log("Turnos restantes: " + turnosRestantes);
        FindFirstObjectByType<ChessGameManager>().ActualizarHUD();

        if (turnosRestantes <= 0)
        {
            Debug.Log("💀 El Rey se quedó sin vidas tras pasar turno. Mostrando panel de derrota.");

            LevelResultUI.Instance.ShowResults(
                LevelProgress.Instance.keysCollected,
                LevelProgress.Instance.hasDiamond,
                0,
                PlayerScore.Instance.GetTotalScore()
            );

            juegoActivo = false;
            FindFirstObjectByType<ChessGameManager>().DetenerJuego();
        }
    }

    public Vector2Int GetPosicionActual()
    {
        return posicionActual;
    }
}
