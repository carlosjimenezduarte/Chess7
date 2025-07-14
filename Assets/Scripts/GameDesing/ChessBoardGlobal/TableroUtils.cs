using UnityEngine;

public static class TableroUtils
{
    /// <summary>
    /// Envía un objeto del tablero a la Dimensión Divina:
    /// - Cambia su tileCoords (ya sea en PiecePositioner o IPieceWithPosition)
    /// - Oculta su imagen si tiene componente Image
    /// - Lo coloca visualmente fuera del tablero
    /// </summary>
    public static void EnviarADimensionDivina(GameObject objeto)
    {
        Vector2Int posDivina = DimensionDivina.ObtenerProximaPosicion();

        if (objeto.TryGetComponent<PiecePositioner>(out var piecePositioner))
        {
            piecePositioner.tileCoords = posDivina;
        }

        if (objeto.TryGetComponent<IPieceWithPosition>(out var pieza))
        {
            pieza.SetPosicionActual(posDivina);
        }

        if (objeto.TryGetComponent<UnityEngine.UI.Image>(out var image))
        {
            image.enabled = false;
        }

        // Si tiene un MovableTileObject, también actualiza el world position
        if (objeto.TryGetComponent<MovableTileObject>(out var movable))
        {
            movable.tileCoords = posDivina;
            objeto.transform.localPosition = BoardManagerGlobal.Instance.GetTileWorldPosition(posDivina);
        }

        Debug.Log($"🌀 {objeto.name} fue exiliado a la Dimensión Divina en {posDivina}");
    }
}
