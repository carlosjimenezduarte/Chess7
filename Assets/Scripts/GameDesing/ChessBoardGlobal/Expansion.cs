using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Expansión: activa un empuje en 4 direcciones cardinales sobre objetos movibles.
/// No se autoafecta, no se mueve, y actúa como ficha inamovible con efecto visual sincronizado.
/// </summary>
public class Expansion : MonoBehaviour, ITileEffect, IFicha, IPieceWithPosition
{
    public Vector2Int tileCoords;
    public bool activoEnTablero = true;

    private void Start()
    {
        var posicionador = GetComponent<PiecePositioner>();
        if (posicionador != null)
        {
            tileCoords = posicionador.tileCoords;
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧩 Expansion inició en {tileCoords}");
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"⚠️ Expansion sin PiecePositioner. tileCoords no inicializado.");
        }
    }

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        if (posicionRey == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♔ Rey llegó a casilla con Expansion en {tileCoords}.");
            ActivarExpansion(rey);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        if (posicionPeon == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"♙ Peón llegó a casilla con Expansion en {tileCoords}.");
            ActivarExpansion(peon);
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void RevisarSiFichaLlegó(Vector2Int posicionFicha, IFicha ficha)
    {
        var nombreFicha = ((MonoBehaviour)ficha).name;
        if (posicionFicha == tileCoords)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🧩 Expansion activado por {nombreFicha} en {tileCoords}.");
            ActivarExpansion((MonoBehaviour)ficha);
        }
        else
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"❌ Ficha {nombreFicha} no activó Expansion: estaba en {posicionFicha}, no en {tileCoords}.");
        }
        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    public void VerificarTurnoActual(int turnoActual) { }

    public void SetPosicionActual(Vector2Int nuevaPos) => tileCoords = nuevaPos;
    public Vector2Int GetPosicionActual() => tileCoords;
    public bool EsInamovible() => true;

    private void ActivarExpansion(MonoBehaviour activador)
    {
        if (!activoEnTablero)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"❌ Expansion no activa en tablero.");
            return;
        }

        if (activador == this)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno("🔁 Expansion se ignoró a sí misma para evitar autoactivación.");
            return;
        }

        BoardManagerGlobal.Instance.AgregarMensajeInterno($"💢 Expansion en {tileCoords} se activa por {activador.name}.");

        Vector2Int[] direcciones = new Vector2Int[]
        {
            Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right
        };

        // Recolectamos primero todos los empujables agrupados por dirección
        List<(MovableTileObject objeto, Vector2Int destino)> empujesPendientes = new();

        foreach (var direccion in direcciones)
        {
            Vector2Int pos = tileCoords + direccion;

            while (EsCasillaDentroDelTablero(pos))
            {
                var objetos = BoardManagerGlobal.Instance.ObtenerObjetosEn(pos);
                foreach (var obj in objetos)
                {
                    if (obj is MovableTileObject m && m != this && m.activoEnTablero)
                    {
                        if (m is IFicha ficha && ficha.EsInamovible())
                        {
                            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚫 {m.name} es inamovible y no será empujado.");
                            continue;
                        }

                        Vector2Int nuevaPos = m.tileCoords + direccion;

                        if (!EsCasillaDentroDelTablero(nuevaPos))
                        {
                            BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚫 {m.name} no puede empujarse fuera del tablero hacia {nuevaPos}.");
                            continue;
                        }

                        if (EstaCasillaOcupada(nuevaPos))
                        {
                            BoardManagerGlobal.Instance.AgregarMensajeInterno($"⛔ {m.name} no puede ser empujado a {nuevaPos} porque está ocupada.");
                            continue;
                        }

                        empujesPendientes.Add((m, nuevaPos));
                    }
                }

                pos += direccion;
            }
        }

        // Ejecutamos todos los empujes en una sola acción tipo "¡AHORA!"
        foreach (var (objeto, destino) in empujesPendientes)
        {
            BoardManagerGlobal.Instance.AgregarMensajeInterno($"💥 {objeto.name} empujado de {objeto.tileCoords} a {destino}.");
            objeto.MoverA(destino);
        }

        BoardManagerGlobal.Instance.ReportarEstadoActualDelTablero();
    }

    private bool EsCasillaDentroDelTablero(Vector2Int coords)
    {
        return coords.x >= 0 && coords.x < BoardManagerGlobal.Instance.Ancho &&
               coords.y >= 0 && coords.y < BoardManagerGlobal.Instance.Alto;
    }

    private bool EstaCasillaOcupada(Vector2Int coords)
    {
        var objetosEnTile = BoardManagerGlobal.Instance.ObtenerObjetosEn(coords);

        foreach (var obj in objetosEnTile)
        {
            if ((object)obj == this) continue;

            if (obj is MovableTileObject mov && !mov.activoEnTablero)
                continue;

            if (obj is IObjetoRecoleccionable || obj is IFicha)
            {
                BoardManagerGlobal.Instance.AgregarMensajeInterno($"🚫 La casilla {coords} está ocupada por {((MonoBehaviour)obj).name}");
                return true;
            }
        }

        return false;
    }
}
