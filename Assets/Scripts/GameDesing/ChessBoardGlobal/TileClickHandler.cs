using UnityEngine;
using UnityEngine.EventSystems;
using System.Linq;

public class TileClickHandler : MonoBehaviour, IPointerClickHandler
{
    [Header("Coordenadas de esta casilla")]
    public Vector2Int tileCoords;

   public void OnPointerClick(PointerEventData eventData)
{
    Debug.Log($"🖱 Click detectado en casilla: {tileCoords}");

    var gameManager = FindFirstObjectByType<ChessGameManager>();
    if (gameManager == null)
    {
        Debug.LogWarning("⚠️ No se encontró ChessGameManager en la escena.");
        return;
    }

    var piezas = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None)
        .Where(obj => obj is IPieceWithPosition)
        .Cast<IPieceWithPosition>();

    // ✅ Nuevo: Si hay una ficha aliada seleccionada, y esta casilla está marcada como ataque...
    if (gameManager.fichaSeleccionadaActual is IFichaAliada fichaActiva)
    {
        Tile tile = BoardManagerGlobal.Instance.GetTileAt(tileCoords);
        if (tile != null && tile.EsCasillaDeAtaque())
        {
            Debug.Log($"⚔️ Casilla {tileCoords} reconocida como zona de ataque para {gameManager.fichaSeleccionadaActual.name}");

            if (gameManager.fichaSeleccionadaActual is PawnController peon)
            {
                peon.MoverA(tileCoords, gameManager.rey);
                return;
            }
            else if (gameManager.fichaSeleccionadaActual is KingController rey)
            {
                rey.IntentarAtacar(tileCoords);
                return;
            }

            // Si agregas más fichas aliadas que pueden atacar, extiende aquí...
        }
    }

    foreach (var pieza in piezas)
    {
        Vector2Int posPieza = pieza.GetPosicionActual();
        Debug.Log($"🔍 Revisando pieza {((MonoBehaviour)pieza).name} en {posPieza}");

        if (posPieza == tileCoords)
        {
            if (pieza is IFichaEnemiga enemiga)
            {
                if (!gameManager.IsJuegoActivo())
                {
                    Debug.Log("🛑 Juego no activo. Ignorando clic sobre ficha enemiga.");
                    return;
                }

                if ((MonoBehaviour)pieza == gameManager.fichaSeleccionadaActual)
                {
                    enemiga.OcultarRango();
                    Debug.Log($"⚪ Ocultando rango de {((MonoBehaviour)pieza).name}");
                    gameManager.fichaSeleccionadaActual = null;
                }
                else
                {
                    foreach (var otra in piezas)
                    {
                        if (otra != pieza && otra is IFichaEnemiga otraEnemiga)
                            otraEnemiga.OcultarRango();
                    }

                    enemiga.MostrarRango();
                    Debug.Log($"🔴 Mostrando rango de ataque de {((MonoBehaviour)pieza).name}");
                    gameManager.fichaSeleccionadaActual = (MonoBehaviour)pieza;
                }

                return;
            }

            if (pieza is IFichaAliada aliada)
            {
                if ((MonoBehaviour)pieza == gameManager.fichaSeleccionadaActual)
                {
                    aliada.OcultarRango();
                    Debug.Log($"⚪ Desactivando rango de {((MonoBehaviour)pieza).name}");
                    gameManager.fichaSeleccionadaActual = null;
                }
                else
                {
                    foreach (var otra in piezas)
                    {
                        if (otra != pieza && otra is IFichaAliada otraAliada)
                            otraAliada.OcultarRango();
                    }

                    aliada.MostrarRango();
                    Debug.Log($"🟢 Mostrando rango de {((MonoBehaviour)pieza).name}");
                    gameManager.fichaSeleccionadaActual = (MonoBehaviour)pieza;
                }
                return;
            }
        }
    }

    // Si no hay pieza en esta casilla pero hay una ficha seleccionada, intenta mover
    if (gameManager.fichaSeleccionadaActual != null)
    {
        var ficha = gameManager.fichaSeleccionadaActual;

        if (ficha is KingController rey)
        {
            rey.MoverA(tileCoords);
        }
        else if (ficha is PawnController peon)
        {
            peon.MoverA(tileCoords, gameManager.rey);
        }
    }
    else
    {
        Debug.Log("🚫 No hay ficha seleccionada actualmente para mover.");
    }
    }

}