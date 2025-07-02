using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyboardUIAdjuster : MonoBehaviour
{
    public RectTransform contentToMove;
    public TMP_InputField inputField;
    public float moveAmount = 500f; // Ajusta según el tamaño de tu teclado/UI

    private Vector2 originalPosition;

    private void Start()
    {
        if (contentToMove != null)
            originalPosition = contentToMove.anchoredPosition;

        // Conectamos eventos
        inputField.onSelect.AddListener(OnInputSelected);
        inputField.onEndEdit.AddListener(OnInputDeselected);
    }

    private void OnInputSelected(string text)
    {
        MoveUIUp();
    }

    private void OnInputDeselected(string text)
    {
        MoveUIDown();
    }

    private void MoveUIUp()
    {
        if (contentToMove != null)
            contentToMove.anchoredPosition = new Vector2(originalPosition.x, originalPosition.y + moveAmount);
    }

    private void MoveUIDown()
    {
        if (contentToMove != null)
            contentToMove.anchoredPosition = originalPosition;
    }
}
