using UnityEngine;

public static class DimensionDivina
{
    private static int offsetY = 0;

    /// <summary>
    /// Retorna la próxima posición "sagrada" para un objeto exiliado,
    /// garantizando que cada uno tenga su propia casilla en la Dimensión Divina.
    /// </summary>
    public static Vector2Int ObtenerProximaPosicion()
    {
        offsetY++;
        return new Vector2Int(-1, -offsetY);
    }
}
