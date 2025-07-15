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

        foreach (var pieza in piezas)
        {
            Vector2Int posPieza = pieza.GetPosicionActual();
            Debug.Log($"🔍 Revisando pieza {((MonoBehaviour)pieza).name} en {posPieza}");

            if (posPieza == tileCoords)
            {
                
                if (pieza is IFichaEnemiga enemiga)
                {
                    if (gameManager.fichaSeleccionadaActual == null)
                    {
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
                                {
                                    otraEnemiga.OcultarRango();
                                }
                            }

                            enemiga.MostrarRango();
                            Debug.Log($"🔴 Mostrando rango de ataque de {((MonoBehaviour)pieza).name}");
                            gameManager.fichaSeleccionadaActual = (MonoBehaviour)pieza;
                        }
                        return; // solo regresa si NO hay ficha seleccionada
                    }
                    // si hay ficha seleccionada, no retorna y sigue a intentar mover abajo
                }


                // 🚀 Si es ficha aliada
                if (pieza is IFichaAliada aliada)
                {
                    // Toggle si clickeas la misma pieza ya seleccionada
                    if ((MonoBehaviour)pieza == gameManager.fichaSeleccionadaActual)
                    {
                        aliada.OcultarRango();
                        Debug.Log($"⚪ Desactivando rango de {((MonoBehaviour)pieza).name}");
                        gameManager.fichaSeleccionadaActual = null;
                    }
                    else
                    {
                        // Desactiva todas las demás aliadas
                        foreach (var otra in piezas)
                        {
                            if (otra != pieza && otra is IFichaAliada otraAliada)
                            {
                                otraAliada.OcultarRango();
                            }
                        }

                        aliada.MostrarRango();
                        Debug.Log($"🟢 Mostrando rango de {((MonoBehaviour)pieza).name}");
                        gameManager.fichaSeleccionadaActual = (MonoBehaviour)pieza;
                    }
                    return;
                }
            }
        }

        // 🚀 Si no hay pieza, intenta mover la ficha seleccionada
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
