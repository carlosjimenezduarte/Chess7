using UnityEngine;

public static class FuturosInciertos
{
    private static int offsetX = 0;

    /// <summary>
    /// Retorna la próxima posición "en el limbo del futuro", 
    /// para que cada objeto pendiente tenga su propio espacio.
    /// Ejemplo: (100,100), (101,100), (102,100), ...
    /// </summary>
    public static Vector2Int ObtenerProximaPosicion()
    {
        offsetX++;
        return new Vector2Int(100 + offsetX, 100);
    }
}
