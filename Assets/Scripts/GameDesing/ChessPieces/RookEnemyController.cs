using UnityEngine;
using UnityEngine.EventSystems;

public class RookEnemyController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition, IFicha
{
    public bool esInamovible = false;

    public Vector2Int posicionActual;

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        posicionActual = nuevaPos;
    }

    public Vector2Int GetPosicionActual()
    {
        return posicionActual;
    }

    public void VerificarAmenazaSobre(Vector2Int posicionPieza)
    {
        
    }


    public void MostrarRango()
    {
        // Devuelve la posición actual de la torre.
        // Por ahora puedes devolver un valor por defecto

    }

    public void OcultarRango()
    {
        // Devuelve la posición actual de la torre.
        // Por ahora puedes devolver un valor por defecto:

    }

    public void ActivarJuego()
    {
        // Devuelve la posición actual de la torre.
        // Por ahora puedes devolver un valor por defecto:

    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("♜ Torre enemiga clickeada");
    }

    private void VerificarAmenazaSobre()
    {
    
    }
    
    public bool EsInamovible() => esInamovible;


}
