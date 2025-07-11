using UnityEngine;

public interface ITileEffect
{
    void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey);
    void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon);
    void VerificarTurnoActual(int turnoActual);
}