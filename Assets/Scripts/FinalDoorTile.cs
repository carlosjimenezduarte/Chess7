using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(LevelTile))]
public class FinalDoorTile : MonoBehaviour, IPointerClickHandler
{
    [Header("Identificadores")]
    public const int BUILD_OFFSET = 14; // mismo offset que usas
    [Tooltip("BuildIndex real de la escena 204. Si no lo pones, se calcula como 204 + BUILD_OFFSET.")]
    public int buildIndex = -1;
    [Tooltip("Número lógico del nivel final (204).")]
    public int nivelLogico = 204;

    [Header("Iconos de la casilla")]
    public GameObject doorOpenIcon;   // “Door”
    public GameObject doorClosedIcon; // “CloseDoor”

    [Header("Panel que aparece cuando falta algo")]
    public GameObject panelDoorClosed;      // MasterKeyComplete (o contenedor del panel)
    public TMP_Text panelMessage;          // opcional (el texto grande del panel)

    [Header("Íconos del panel: se activan solo si el requisito está completo")]
    // Trofeos (4)
    public GameObject trophyGreen;
    public GameObject trophyBlue;
    public GameObject trophyRed;
    public GameObject trophyGold;

    // Condecoraciones (6)
    public GameObject medal1;
    public GameObject medal2;
    public GameObject medal3;
    public GameObject medal4;
    public GameObject medal5;
    public GameObject medal6;

    // Master Keys (3)
    public GameObject crownOfTheKey;
    public GameObject soulColumn;
    public GameObject toothOfTheKingdom;

    // Totales


    private LevelTile tile;
    private string slotId;

    void Awake()
    {
        tile = GetComponent<LevelTile>();
        slotId = PlayerPrefs.GetString("slotActivo", "slot1");
        if (buildIndex < 0) buildIndex = nivelLogico + BUILD_OFFSET;
    }

    void Start()
    {
        RefreshVisual();
    }

    // Llama GameHomeManager al cargar progreso (ver abajo)
    public void RefreshVisual()
    {
        bool hasAll = HasAllRequirements();

        // Estado base de la casilla: ya vienes al mapa 8 tras pasar 203, así que dejamos Unlocked
        //tile.SetState(LevelTile.TileState.Unlocked);

        // Iconos de la casilla (solo Door/CloseDoor; el LevelTile.keyIcon no se usa aquí)
        if (doorOpenIcon) doorOpenIcon.SetActive(hasAll);
        if (doorClosedIcon) doorClosedIcon.SetActive(!hasAll);

        // Cerrar panel por si quedó abierto
        if (panelDoorClosed) panelDoorClosed.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (HasAllRequirements())
        {
            // Guarda el nivel lógico activo y entra
            PlayerPrefs.SetInt("nivelActivo", nivelLogico);
            PlayerPrefs.Save();
            SceneManager.LoadScene(buildIndex);
        }
        else
        {
            // Abre panel con los íconos marcados/apagados
            MarcarPanel();
            if (panelDoorClosed) panelDoorClosed.SetActive(true);
            if (panelMessage)
                panelMessage.text = "No podrás acceder a esta sala hasta no tener las 3 partes de la Llave Legendaria, " +
                                    "todas las condecoraciones, todos los trofeos, todos los pergaminos, " +
                                    "todos los diamantes y todas las llaves del juego.";
        }
    }

    private bool HasAllRequirements()
    {
        // Totales
        int trophiesTotal = PlayerPrefs.GetInt($"{slotId}_trophiesTotal", 0); // 4
        int medalsTotal = PlayerPrefs.GetInt($"{slotId}_medalsTotal", 0); // 6
        int parchmentsTotal = PlayerPrefs.GetInt($"{slotId}_parchmentsTotal", 0); // 10
        int masterKeysTotal = PlayerPrefs.GetInt($"{slotId}_masterKeysTotal", 0); // 3
        int keysTotal = PlayerPrefs.GetInt($"{slotId}_keysTotal", 0); // 500
        int diamondsTotal = PlayerPrefs.GetInt($"{slotId}_diamondsTotal", 0); // 200

        // Fallback por logros de MasterKey si (por lo que sea) el total no está actualizado:
        bool mk1 = AchievementsManager.IsUnlocked(slotId, AchievementId.MasterKey1);
        bool mk2 = AchievementsManager.IsUnlocked(slotId, AchievementId.MasterKey2);
        bool mk3 = AchievementsManager.IsUnlocked(slotId, AchievementId.MasterKey3);
        if (masterKeysTotal < 3 && mk1 && mk2 && mk3) masterKeysTotal = 3;

        bool okTrophies = trophiesTotal >= 4;
        bool okMedals = medalsTotal >= 6;
        bool okParchments = parchmentsTotal >= 10;
        bool okMasterKeys = masterKeysTotal >= 3;
        bool okKeys = keysTotal >= 500;
        bool okDiamonds = diamondsTotal >= 200;

        return okTrophies && okMedals && okParchments && okMasterKeys && okKeys && okDiamonds;
    }

    private void MarcarPanel()
    {
        // Lee totales
        int trophiesTotal = PlayerPrefs.GetInt($"{slotId}_trophiesTotal", 0);
        int medalsTotal = PlayerPrefs.GetInt($"{slotId}_medalsTotal", 0);
        int parchmentsTotal = PlayerPrefs.GetInt($"{slotId}_parchmentsTotal", 0);
        int masterKeysTotal = PlayerPrefs.GetInt($"{slotId}_masterKeysTotal", 0);
        int keysTotal = PlayerPrefs.GetInt($"{slotId}_keysTotal", 0);
        int diamondsTotal = PlayerPrefs.GetInt($"{slotId}_diamondsTotal", 0);

        bool mk1 = AchievementsManager.IsUnlocked(slotId, AchievementId.MasterKey1);
        bool mk2 = AchievementsManager.IsUnlocked(slotId, AchievementId.MasterKey2);
        bool mk3 = AchievementsManager.IsUnlocked(slotId, AchievementId.MasterKey3);

        // Trofeos (activa solo los “principales”; las versiones Enabled/Opacas quedan apagadas)
        SetActiveSafe(trophyGreen, trophiesTotal >= 1);
        SetActiveSafe(trophyBlue, trophiesTotal >= 2);
        SetActiveSafe(trophyRed, trophiesTotal >= 3);
        SetActiveSafe(trophyGold, trophiesTotal >= 4);

        // Condecoraciones (6)
        SetActiveSafe(medal1, medalsTotal >= 1);
        SetActiveSafe(medal2, medalsTotal >= 2);
        SetActiveSafe(medal3, medalsTotal >= 3);
        SetActiveSafe(medal4, medalsTotal >= 4);
        SetActiveSafe(medal5, medalsTotal >= 5);
        SetActiveSafe(medal6, medalsTotal >= 6);

        // Master Keys
        SetActiveSafe(crownOfTheKey, mk1 || masterKeysTotal >= 1);
        SetActiveSafe(soulColumn, mk2 || masterKeysTotal >= 2);
        SetActiveSafe(toothOfTheKingdom, mk3 || masterKeysTotal >= 3);


    }

    private void SetActiveSafe(GameObject go, bool on)
    {
        if (go) go.SetActive(on);
    }

    // Puedes llamar este método desde un botón [X] del panel
    public void ClosePanel()
    {
        if (panelDoorClosed) panelDoorClosed.SetActive(false);
    }
    public void OnAllRightClick() => ClosePanel();
}
