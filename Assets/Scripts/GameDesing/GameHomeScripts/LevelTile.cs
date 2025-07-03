using UnityEngine;
using UnityEngine.UI;

public class LevelTile : MonoBehaviour
{
    public Image backgroundImage;
    public GameObject keyIcon;

    public enum TileState
    {
        Locked,
        Unlocked,
        Completed
    }

    public TileState state = TileState.Locked;

    private void Start()
    {
        UpdateVisual();
    }

    public void SetState(TileState newState)
    {
        state = newState;
        UpdateVisual();
    }

    private void UpdateVisual()
{
    Color originalColor = backgroundImage.color;

    switch (state)
    {
        case TileState.Locked:
            backgroundImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            keyIcon.SetActive(false);
            break;

        case TileState.Unlocked:
            backgroundImage.color = new Color(1f, 1f, 1f, 1f);
            keyIcon.SetActive(false);
            break;

        case TileState.Completed:
            backgroundImage.color = new Color(1f, 1f, 1f, 1f);
            keyIcon.SetActive(true);
            break;
    }
}
}