using UnityEngine;

public class TestOpenURL : MonoBehaviour
{
    public void AbrirWeb()
    {
        Application.OpenURL("https://chessescape.com/legal/termsandconditions.html");
    }
}