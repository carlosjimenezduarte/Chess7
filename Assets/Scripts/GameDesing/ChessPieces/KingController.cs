using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class KingController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition, IFicha, IFichaAliada
{
    public int puntosMovimientoMax = 3;
    public int puntosAccionMax = 5;

    public bool esInamovible = false;

    [HideInInspector]
    public int puntosMovimientoActual;
    public int puntosAccionActual = 5;
    public int turnosRestantes = 7;

    private Vector2Int posicionActual;
    private bool juegoActivo = false;
    public bool mostrandoMovimientos = false;

    private void Start()
    {
        PiecePositioner piecePositioner = GetComponent<PiecePositioner>();
        if (piecePositioner != null)
        {
            posicionActual = piecePositioner.tileCoords;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♔ Rey inició en {posicionActual}");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("⚠️ No hay PiecePositioner en el Rey. Usando (0,0).");
            posicionActual = new Vector2Int(0, 0);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        posicionActual = nuevaPos;

        var movible = GetComponent<MovableTileObject>();
        if (movible != null) movible.tileCoords = nuevaPos;

        var posicionador = GetComponent<PiecePositioner>();
        if (posicionador != null) posicionador.tileCoords = nuevaPos;

        BoardManagerGlobal.Instance?.RegistrarMovimiento(this, nuevaPos);
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"♔ Rey actualizado a {nuevaPos}.");
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
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void MostrarMovimientoPosible()
    {
        if (!juegoActivo) return;

        BoardManagerGlobal.Instance.AgregarMensajeInterno($"👣 Alcance personal del Rey: {puntosMovimientoActual} PM.");

        foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
        {
            int distancia = Mathf.Abs(tile.tileCoords.x - posicionActual.x) + Mathf.Abs(tile.tileCoords.y - posicionActual.y);
            tile.HighlightMove(distancia <= puntosMovimientoActual);
        }
    }

    public void OcultarMovimientos()
    {
        foreach (Tile tile in BoardManagerGlobal.Instance.tiles)
            tile.HighlightMove(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        var gameManager = FindFirstObjectByType<ChessGameManager>();
        bool esNuevaSeleccion = gameManager.fichaSeleccionadaActual != this;
        gameManager.fichaSeleccionadaActual = this;

        var rey = FindFirstObjectByType<KingController>();
        if (rey != null && rey != this)
            rey.OcultarMovimientos();

        if (esNuevaSeleccion || !mostrandoMovimientos)
        {
            mostrandoMovimientos = true;
            MostrarMovimientoPosible();
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🟢 Mostrando previsualización automática del Rey.");
        }
        else
        {
            mostrandoMovimientos = false;
            OcultarMovimientos();
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🔴 Ocultando previsualización del Rey.");
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

     public void MoverA(Vector2Int nuevaPos)
    {
        if (!juegoActivo) return;

        int distancia = Mathf.Abs(posicionActual.x - nuevaPos.x) + Mathf.Abs(posicionActual.y - nuevaPos.y);

        if (distancia <= puntosMovimientoActual)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♔ Moviendo al Rey desde {posicionActual} a {nuevaPos}, consumiendo {distancia} PM.");

            Vector2Int paso = posicionActual;

            while (paso != nuevaPos)
            {
                if (paso.x < nuevaPos.x) paso.x++;
                else if (paso.x > nuevaPos.x) paso.x--;
                if (paso.y < nuevaPos.y) paso.y++;
                else if (paso.y > nuevaPos.y) paso.y--;

                SetPosicionActual(paso);
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚶 El Rey pasa por {paso}");

                foreach (ITileEffect efecto in FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ITileEffect>())
                {
                    efecto.RevisarSiReyLlegó(paso, this);
                }
            }

            transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);
            puntosMovimientoActual -= distancia;

            MostrarMovimientoPosible();
            mostrandoMovimientos = true;

            var reina = FindFirstObjectByType<QueenEnemyController>();
            if (reina != null)
                reina.VerificarAmenazaSobre(posicionActual);

            FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

            if (posicionActual == new Vector2Int(7, 7))
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno("🚀 El Rey llegó a la meta (H8). Calculando bonus.");

                int bonus = turnosRestantes * 50;
                PlayerScore.Instance.AgregarPuntaje(bonus);

                LevelResultUI.Instance.ShowResults(
                    LevelProgress.Instance.keysCollected,
                    LevelProgress.Instance.hasDiamond,
                    turnosRestantes,
                    PlayerScore.Instance.GetTotalScore()
                );

                juegoActivo = false;
                FindFirstObjectByType<ChessGameManager>()?.DetenerJuego();
            }

            if (turnosRestantes <= 0)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno("💀 El Rey sin vidas.");

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
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🚫 Movimiento no permitido, no hay suficientes PM. Soy el Rey");
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }


    public void ReiniciarTurno()
    {
        puntosMovimientoActual = puntosMovimientoMax;
        puntosAccionActual = puntosAccionMax;
        BoardManagerGlobal.Instance.AgregarMensajeInterno(
        $"♔ Nuevo turno del Rey → 🧭 PM personales: {puntosMovimientoActual}, 🎖️ PA estratégicos: {puntosAccionActual}."
    );
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void GanarPuntoMovimiento(int cantidad)
    {
        puntosMovimientoActual += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧭 El Rey gana {cantidad:+#;-#} PM personales. Total: {puntosMovimientoActual}.");
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void GanarVida(int cantidad)
    {
        turnosRestantes += cantidad;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"❤️ El Rey gana +{cantidad} vida(s). Ahora tiene {turnosRestantes}.");
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RestarTurno()
    {
        turnosRestantes--;
        BoardManagerGlobal.Instance.AgregarMensajeInterno($"⏳ El Rey pierde 1 vida. Turnos restantes: {turnosRestantes}");
        FindFirstObjectByType<ChessGameManager>()?.ActualizarHUD();

        if (turnosRestantes <= 0)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("💀 El Rey ha agotado todos sus turnos.");

            LevelResultUI.Instance.ShowResults(
                LevelProgress.Instance.keysCollected,
                LevelProgress.Instance.hasDiamond,
                0,
                PlayerScore.Instance.GetTotalScore()
            );
            juegoActivo = false;
            FindFirstObjectByType<ChessGameManager>()?.DetenerJuego();
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void DesactivarJuego()
    {
        juegoActivo = false;
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void MostrarRango()
    {
        MostrarMovimientoPosible();
        mostrandoMovimientos = true;
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void OcultarRango()
    {
        OcultarMovimientos();
        mostrandoMovimientos = false;
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }
    public bool EsInamovible()
    {
    return esInamovible;
    }
}
