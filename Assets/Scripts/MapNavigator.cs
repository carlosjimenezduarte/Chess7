using UnityEngine;

public class MapNavigator : MonoBehaviour
{
    [Header("Mapas en orden")]
    public GameObject[] mapas; // arrastras aquí los 8 mapas en orden

    [Header("Botones de navegación")]
    public GameObject botonPrevious;
    public GameObject botonNext;

    private int currentIndex = 0;

    void Start()
    {
        MostrarMapa(currentIndex);
    }

    public void OnNext()
    {
        if (currentIndex < mapas.Length - 1)
        {
            currentIndex++;
            MostrarMapa(currentIndex);
        }
    }

    public void OnPrevious()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            MostrarMapa(currentIndex);
        }
    }

    private void MostrarMapa(int index)
    {
        // Apagar todos
        for (int i = 0; i < mapas.Length; i++)
        {
            mapas[i].SetActive(i == index);
        }

        // Actualizar botones según posición
        if (botonPrevious) botonPrevious.SetActive(index > 0);
        if (botonNext) botonNext.SetActive(index < mapas.Length - 1);
    }
}
