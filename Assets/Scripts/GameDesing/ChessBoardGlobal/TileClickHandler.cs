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
                Debug.Log($"✅ Seleccionando pieza {((MonoBehaviour)pieza).name} en {tileCoords}");
                gameManager.fichaSeleccionadaActual = (MonoBehaviour)pieza;

                // Desactivar todas las demás
                foreach (var otra in piezas)
                {
                    if (otra != pieza)
                    {
                        if (otra is KingController rey)
                        {
                            rey.OcultarMovimientos();
                            rey.mostrandoMovimientos = false;
                            rey.DesactivarJuego();
                        }
                        else if (otra is PawnController peon)
                        {
                            peon.OcultarMovimientos();
                            peon.mostrandoMovimientos = false;
                            peon.DesactivarJuego();
                        }
                    }
                }

                // 🚀 Activar el juegoActivo solo de esta pieza seleccionada
                if (pieza is KingController reyPieza)
                {
                    reyPieza.ActivarJuego();
                }
                else if (pieza is PawnController peonPieza)
                {
                    peonPieza.ActivarJuego();
                }

                return; // terminamos, ya se seleccionó
            }
        }

        // Si no hay pieza, intentar mover ficha seleccionada
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
