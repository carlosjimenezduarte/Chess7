using UnityEngine;
using UnityEngine.UI;

public class CollectibleUP : MonoBehaviour, ITileEffect
{
    public Vector2Int tileCoords;
    public int turnoAparece = 1;
    public bool visibleDesdeInicio = false;

    private bool activadoEnJuego = false;
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError($"🚨 El objeto {gameObject.name} no tiene un componente Image.");
        }
    }

    private void Start()
    {
        if (visibleDesdeInicio && turnoAparece <= 1)
        {
            activadoEnJuego = true;
            image.enabled = true;
            Debug.Log($"❤️ Corazón en {tileCoords} aparece desde el inicio (turnoAparece={turnoAparece})");
        }
        else
        {
            image.enabled = false;
        }
    }

    public void RevisarSiReyLlegó(Vector2Int posicionRey, KingController rey)
    {
        if (posicionRey == tileCoords && activadoEnJuego)
        {
            Debug.Log($"❤️ El Rey recogió un corazón en {tileCoords} y ganó +1 vida.");
            rey.GanarVida(1);
            Destroy(gameObject);
        }
    }

    public void VerificarTurnoActual(int turnoActual)
    {
        Debug.Log($"🔍 Corazón en {tileCoords}: turnoActual={turnoActual}, turnoAparece={turnoAparece}, ActivadoJuego={activadoEnJuego}");

        if (!activadoEnJuego && turnoActual >= turnoAparece)
        {
            activadoEnJuego = true;
            image.enabled = true;
            Debug.Log($"✅ Corazón en {tileCoords} SE ACTIVÓ en el turno {turnoActual}");

            KingController rey = FindFirstObjectByType<KingController>();
            if (rey != null && rey.GetPosicionActual() == tileCoords)
            {
                Debug.Log($"❤️ El Rey ya estaba sobre el corazón en {tileCoords}. Mostrando 1 seg antes de desaparecer y sumar vida.");
                rey.GanarVida(1);
                StartCoroutine(DesaparecerDespuesDe(1f));
            }
        }
    }

    private System.Collections.IEnumerator DesaparecerDespuesDe(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        Destroy(gameObject);
    }
}
