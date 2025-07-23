using UnityEngine;
public interface IFichaEnemiga
{
    void RevisarAmenazasEnZona();
    void MostrarRango();
    void OcultarRango();
    Vector2Int GetPosicionActual();
    
    
}