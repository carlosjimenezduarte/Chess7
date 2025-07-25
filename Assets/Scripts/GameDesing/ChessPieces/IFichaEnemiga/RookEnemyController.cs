using UnityEngine;
using UnityEngine.EventSystems;

public class RookEnemyController : MonoBehaviour, IPointerClickHandler, IPieceWithPosition, IFicha, IFichaEnemiga, ITileEffect
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

    public void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon)
    {
        // Lógica vacía por ahora
    }

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        // Lógica vacía por ahora
    }

    public void RevisarSiFichaLlegó(Vector2Int posicionFicha, IFicha ficha)
    {
        // Lógica vacía por ahora
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        // Lógica vacía por ahora
    }

    public void RevisarAmenazasEnZona()
    {
        //
    }
    public bool EsInamovible()
    {
        return esInamovible;
    }

    public int rangoKillZone
    {
        get => 0;
        set => Debug.LogWarning("⚠️ El Rey no usa 'rangoAtaque'");
    }

    public int rangoRangeZone
    {
        get => 0;
        set => Debug.LogWarning("⚠️ El Rey no usa 'rangoAtaque'");
    }

    public void ReiniciarTurno()
    {
        //
    }

    public void RevisarSiReinaEnemigaLlegó(Vector2Int posicion, QueenEnemyController reinaenemiga)
    {
               //RevisarSiReinaEnemigaLlegó(posicion, rey);
    }


}
