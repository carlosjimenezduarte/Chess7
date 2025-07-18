using UnityEngine;
public interface IFichaAliada : IFicha
{
    // Solo un marker interface, no necesita métodos

    void MostrarRango();
    void OcultarRango();
    void ActivarJuego();
    Vector2Int GetPosicionActual();
}