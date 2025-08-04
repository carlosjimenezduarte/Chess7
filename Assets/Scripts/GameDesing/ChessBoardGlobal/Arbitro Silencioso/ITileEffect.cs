using UnityEngine;

public interface ITileEffect
{
    void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey);
    void RevisarSiPeonLlegó(Vector2Int posicionPeon, PawnController peon);

    void RevisarSiFichaLlegó(Vector2Int posicionFicha, IFicha ficha);

    void RevisarSiReinaEnemigaLlegó(Vector2Int posicion, QueenEnemyController reinaenemiga);

    void RevisarSiReinaNegraEnemigaLlegó(Vector2Int posicion, BlackQueenEnemyController reinanegraenemiga);
    void VerificarTurnoActual(int turnoActual);
    
}