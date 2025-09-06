using UnityEngine;
using UnityEngine.UI;

public class LevelTile : MonoBehaviour
{
    [Header("Referencias UI")]
    public Image backgroundImage;
    public GameObject keyIcon; // 👈 arrastra el icono de llave/trofeo/etc.

    public enum TileState { Locked, Unlocked, Completed }
    public TileState state = TileState.Locked;

    private void Start()
    {
        UpdateVisual(false);
    }

    public void SetState(TileState newState, bool mostrarIcono = false)
    {
        state = newState;
        UpdateVisual(mostrarIcono);
    }

    private void UpdateVisual(bool mostrarIcono)
    {
        switch (state)
        {
            case TileState.Locked:
                backgroundImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
                keyIcon.SetActive(false);
                break;

            case TileState.Unlocked:
                backgroundImage.color = Color.white;
                keyIcon.SetActive(false);
                break;

            case TileState.Completed:
                backgroundImage.color = Color.white;
                keyIcon.SetActive(mostrarIcono);
                break;
        }
    }
}
