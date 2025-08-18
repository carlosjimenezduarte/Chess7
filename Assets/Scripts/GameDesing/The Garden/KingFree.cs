using UnityEngine;

public class KingFree : MonoBehaviour, IPieceWithPosition
{
    public Vector2Int tileCoords;   // Posición actual en el jardín
    public float moveCooldown = 0.15f; // Tiempo mínimo entre movimientos

    private float lastMoveTime = 0f;

    private void Start()
    {
        // Si quieres que arranque en una posición específica
        transform.localPosition = GardenManagerGlobal.Instance.GetTileWorldPosition(tileCoords);
    }

    private void Update()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        if (Time.time - lastMoveTime < moveCooldown) return;

        Vector2Int nuevaPos = tileCoords;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            nuevaPos += Vector2Int.up;
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            nuevaPos += Vector2Int.down;
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            nuevaPos += Vector2Int.left;
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            nuevaPos += Vector2Int.right;

        // Validar límites del jardín
        if (nuevaPos.x >= 0 && nuevaPos.x <= 8 && nuevaPos.y >= 0 && nuevaPos.y <= 8)
        {
            MoverA(nuevaPos);
            lastMoveTime = Time.time;
        }
    }

    public void MoverA(Vector2Int nuevaPos)
    {
        SetPosicionActual(nuevaPos);
        GardenManagerGlobal.Instance.RevisarInteraccion(tileCoords);
    }

    public Vector2Int GetPosicionActual()
    {
        return tileCoords;
    }

    public void SetPosicionActual(Vector2Int nuevaPos)
    {
        tileCoords = nuevaPos;
        transform.localPosition = GardenManagerGlobal.Instance.GetTileWorldPosition(tileCoords);
    }
}
