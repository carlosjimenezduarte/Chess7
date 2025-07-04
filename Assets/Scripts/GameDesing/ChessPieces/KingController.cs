using UnityEngine;

public class KingController : MonoBehaviour
{
    public int puntosMovimientoMax = 3; // PM base por turno
    public int puntosMovimientoActual;

    private Vector2Int posicionActual; // (x,y) del tablero, ej: (0,0) = A1

    private void Start()
    {
        puntosMovimientoActual = puntosMovimientoMax;
        posicionActual = new Vector2Int(0,0); // suponemos A1
    }

    public void MostrarMovimientoPosible()
    {
        Debug.Log("Mostrando casillas alcanzables con " + puntosMovimientoActual + " PM.");
        // Aquí recorrerás el tablero y pintarás casillas alcanzables en verde
    }

    public void MoverA(Vector2Int nuevaPos)
    {
        int distancia = Mathf.Abs(posicionActual.x - nuevaPos.x) + Mathf.Abs(posicionActual.y - nuevaPos.y);

        if (distancia <= puntosMovimientoActual)
        {
            Debug.Log($"Moviendo al Rey de {posicionActual} a {nuevaPos}, consumiendo {distancia} PM.");
            
            // Ajustado a localPosition para UI Panel
            transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(nuevaPos);

            puntosMovimientoActual -= distancia;
            posicionActual = nuevaPos;

            MostrarMovimientoPosible(); // recalcular malla verde
        }
        else
        {
            Debug.Log("Movimiento no permitido, no hay suficientes PM.");
        }
    }

    public void ReiniciarTurno()
    {
        puntosMovimientoActual = puntosMovimientoMax;
        MostrarMovimientoPosible();
    }

    // Método de prueba para botón
    public void TestMoverRey()
    {
        Debug.Log("Botón test presionado. Moviendo Rey a (2,2).");
        MoverA(new Vector2Int(2,2)); // Ejemplo: mover a C3
    }
}
