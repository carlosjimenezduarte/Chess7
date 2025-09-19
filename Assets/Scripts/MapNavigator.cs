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
        for (int i = 0; i < mapas.Length; i++)
            mapas[i].SetActive(i == index);

        if (botonPrevious) botonPrevious.SetActive(index > 0);
        if (botonNext) botonNext.SetActive(index < mapas.Length - 1);

        // 🔄 Refresca visual de los tiles del mapa que acaban de activarse
        var ghm = FindFirstObjectByType<GameHomeManager>();
        ghm?.RefrescarTilesActuales();

        // (opcional) si este mapa tiene puerta final, refresca solo sus iconos
        foreach (var door in mapas[index].GetComponentsInChildren<FinalDoorTile>(true))
            door.RefreshVisual(); // esto NO toca LevelTile (ver punto 2)
    }


    public void IrAlMapa(int index)
    {
        if (index < 0 || index >= mapas.Length) return;
        currentIndex = index;
        MostrarMapa(currentIndex);
    }
    

}
