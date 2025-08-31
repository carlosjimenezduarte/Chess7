using UnityEngine;

public class TestOpenURL : MonoBehaviour
{
    public void AbrirWeb()
    {
        Application.OpenURL("https://chessescapeauth.web.app/legal.html");
    }
}