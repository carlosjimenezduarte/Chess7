using UnityEngine;

public class CodexManagerSimple : MonoBehaviour
{
    [Header("Portada del Codex")]
    public GameObject book;

    [Header("Global Items (imágenes principales)")]
    public GameObject fichasAliadas;
    public GameObject fichasEnemigas;
    public GameObject score;
    public GameObject antiScore;
    public GameObject helpItems;
    public GameObject helpItemsLess;
    public GameObject iFichaInmovil;
    public GameObject fichasEnemigasNegras;
    public GameObject awards;

    [Header("Paneles de categorías")]
    public GameObject panelAliadas;
    public GameObject panelEnemigasNegras;        
    public GameObject panelEnemigasRojas; 
    public GameObject panelScore; 
    public GameObject panelAntiScore; 
    public GameObject panelHelpItems; 
    public GameObject panelHelpItemsLess;
    public GameObject panelInmovil; 
    public GameObject panelAwards; 


    [Header("Páginas completas de Aliadas")]
    public GameObject kingPanel;
    public GameObject queenPanel;
    public GameObject rookPanel;
    public GameObject bishopPanel;
    public GameObject knightPanel;
    public GameObject pawnPanel;

    [Header("Páginas completas de Enemigas Negras")]
    public GameObject ReinaNegraPanel;
    public GameObject TorreNegraPanel;
    public GameObject AlfilNegroPanel;
    public GameObject CaballoNegroPanel;
    
    [Header("Páginas completas de Enemigas Rojas")]
    public GameObject peonRojoPanel;
    public GameObject alfilRojoPanel;
    public GameObject torreRojaPanel;
    public GameObject reinaRojaPanel;
    public GameObject caballoRojoPanel;

    [Header("Páginas completas de Score")]
    public GameObject DiamondPanel;
    public GameObject KeyPanel;
    public GameObject KingdomCoinPanel;
    public GameObject KingdomBagPanel;
    public GameObject KingdomChestPanel;
    public GameObject KingdomCrownPanel;

    [Header("Páginas completas de AntiScore")]
    public GameObject PadlockPanel;
    public GameObject TalismanPanel;
    public GameObject FalseCoinPanel;
    public GameObject EfritsRingPanel;
    public GameObject MagicLampPanel;
    public GameObject SwordPanel;

    [Header("Páginas completas de HelpItems")]
    public GameObject ClockPanel;
    public GameObject BlueStarPanel;
    public GameObject HeartPanel;
    public GameObject Potion1PMPanel;
    public GameObject Potion3PMPanel;
    public GameObject Potion5PMPanel;
    public GameObject Potion15PMPanel;
    public GameObject ShieldPanel;

    [Header("Páginas completas de HelpItemsLess")]
    public GameObject ClockLessTimePanel;
    public GameObject BlackStarPanel;
    public GameObject Less1PMPanel;
    public GameObject Less3PMPanel;
    public GameObject Less5PMPanel;
    public GameObject Less15PMPanel;
    public GameObject HeartLessPanel; 

    
    [Header("Páginas completas de IFichaInmovil")]
    public GameObject PusherUpPanel;
    public GameObject PusherDownPanel;
    public GameObject PusherLeftPanel;
    public GameObject PusherRightPanel;
    public GameObject ExpansionPanel;
    public GameObject AttractionPanel;
    public GameObject InterruptionPanel;
    public GameObject VortexPanel;
    public GameObject WallPanel;

    [Header("Páginas completas de Awards")]
    public GameObject ParchmentPanel;
    public GameObject TrophyPanel;
    public GameObject MedalPanel;

    private GameObject currentPage;

    void Start()
    {
        MostrarPortada();
    }

    public void MostrarPortada()
    {
        if (book) book.SetActive(true);

        if (panelAliadas) panelAliadas.SetActive(false);
        OcultarPaginas();

        currentPage = null;
    }

    // ===== Acciones de los GlobalItems =====
    public void OnClickFichasAliadas()
    {
       // if (book) book.SetActive(false);
        if (panelAliadas) panelAliadas.SetActive(true);

        OcultarPaginas();
        currentPage = null;
        Debug.Log("📘 Codex: Abierto panel IFichaAliada");
    }

    public void OnClickFichasEnemigasNegras()
    {
    // Apagamos otras categorías
    if (panelAliadas) panelAliadas.SetActive(false);
    OcultarPaginas();

    // Encendemos esta categoría
    if (panelEnemigasNegras) panelEnemigasNegras.SetActive(true);

    currentPage = null;
    Debug.Log("📘 Codex: Abierto panel IFichasEnemigasNegras");
    }

    public void OnClickFichasEnemigasRojas()
    {
        // Apagamos otras categorías
        if (panelAliadas) panelAliadas.SetActive(false);
        if (panelEnemigasNegras) panelEnemigasNegras.SetActive(false);
        OcultarPaginas();

        // Encendemos esta categoría
        if (panelEnemigasRojas) panelEnemigasRojas.SetActive(true);

        currentPage = null;
        Debug.Log("📘 Codex: Abierto panel IFichasEnemigasRojas");

    }

    public void OnClickScore()
    {
        // Apagamos otras categorías
        if (panelAliadas) panelAliadas.SetActive(false);
        if (panelEnemigasNegras) panelEnemigasNegras.SetActive(false);
        if (panelEnemigasRojas) panelEnemigasRojas.SetActive(false);
        OcultarPaginas();

        // Encendemos esta categoría
        if (panelScore) panelScore.SetActive(true);

        currentPage = null;
        Debug.Log("📘 Codex: Abierto panel Score");
    }


    public void OnClickAntiScore()
    {
        // Apagamos otras categorías
        if (panelAliadas) panelAliadas.SetActive(false);
        if (panelEnemigasNegras) panelEnemigasNegras.SetActive(false);
        if (panelEnemigasRojas) panelEnemigasRojas.SetActive(false);
        if (panelScore) panelScore.SetActive(false);
        OcultarPaginas();

        // Encendemos esta categoría
        if (panelAntiScore) panelAntiScore.SetActive(true);

        currentPage = null;
        Debug.Log("📘 Codex: Abierto panel AntiScore");
    }


    public void OnClickHelpItems()
    {
        // Apagamos otras categorías
        if (panelAliadas) panelAliadas.SetActive(false);
        if (panelEnemigasNegras) panelEnemigasNegras.SetActive(false);
        if (panelEnemigasRojas) panelEnemigasRojas.SetActive(false);
        if (panelScore) panelScore.SetActive(false);
        if (panelAntiScore) panelAntiScore.SetActive(false);
        OcultarPaginas();

        // Encendemos esta categoría
        if (panelHelpItems) panelHelpItems.SetActive(true);

        currentPage = null;
        Debug.Log("📘 Codex: Abierto panel HelpItems");
    }



    public void OnClickHelpItemsLess()
    {
        // Apagamos otras categorías
        if (panelAliadas) panelAliadas.SetActive(false);
        if (panelEnemigasNegras) panelEnemigasNegras.SetActive(false);
        if (panelEnemigasRojas) panelEnemigasRojas.SetActive(false);
        if (panelScore) panelScore.SetActive(false);
        if (panelAntiScore) panelAntiScore.SetActive(false);
        if (panelHelpItems) panelHelpItems.SetActive(false);
        OcultarPaginas();

        // Encendemos esta categoría
        if (panelHelpItemsLess) panelHelpItemsLess.SetActive(true);

        currentPage = null;
        Debug.Log("📘 Codex: Abierto panel HelpItemsLess");
    }

    public void OnClickIFichaInmovil()
    {
        // Apagamos otras categorías
        if (panelAliadas) panelAliadas.SetActive(false);
        if (panelEnemigasNegras) panelEnemigasNegras.SetActive(false);
        if (panelEnemigasRojas) panelEnemigasRojas.SetActive(false);
        if (panelScore) panelScore.SetActive(false);
        if (panelAntiScore) panelAntiScore.SetActive(false);
        if (panelHelpItems) panelHelpItems.SetActive(false);
        if (panelHelpItemsLess) panelHelpItemsLess.SetActive(false);
        OcultarPaginas();

        // Encendemos esta categoría
        if (panelInmovil) panelInmovil.SetActive(true);

        currentPage = null;
        Debug.Log("📘 Codex: Abierto panel IFichaInmovil");
    }



    public void OnClickAwards()
    {
        // Apagamos otras categorías
        if (panelAliadas) panelAliadas.SetActive(false);
        if (panelEnemigasNegras) panelEnemigasNegras.SetActive(false);
        if (panelEnemigasRojas) panelEnemigasRojas.SetActive(false);
        if (panelScore) panelScore.SetActive(false);
        if (panelAntiScore) panelAntiScore.SetActive(false);
        if (panelHelpItems) panelHelpItems.SetActive(false);
        if (panelHelpItemsLess) panelHelpItemsLess.SetActive(false);
        if (panelInmovil) panelInmovil.SetActive(false);
        OcultarPaginas();

        // Encendemos esta categoría
        if (panelAwards) panelAwards.SetActive(true);

        currentPage = null;
        Debug.Log("📘 Codex: Abierto panel Awards");
    }


    // ===== Acciones de los íconos dentro de IFichaAliada =====
    public void TogglePagina(GameObject pagina)
    {
        // Si ya estaba activa, solo la ocultamos (pero dejamos visible IFichaAliadas)
        if (currentPage == pagina && pagina.activeSelf)
        {
            pagina.SetActive(false);
            currentPage = null;
            return;
        }

        // Apaga cualquier otra página activa
        OcultarPaginas();

        // Enciende la página seleccionada
        pagina.SetActive(true);

        // 👇 muy importante: mantenemos visible el panel de IFichaAliadas
        if (panelAliadas) panelAliadas.SetActive(true);

        currentPage = pagina;
        Debug.Log($"📘 Codex: Mostrando {pagina.name}");
    }
    
    public void TogglePaginaEnemiga(GameObject pagina)
    {
    // Si ya estaba activa, la apagamos (pero el panelEnemigasNegras sigue visible)
    if (currentPage == pagina && pagina.activeSelf)
    {
        pagina.SetActive(false);
        currentPage = null;
        return;
    }

    // Apaga todas las páginas activas primero
    OcultarPaginas();

    // Enciende la página seleccionada
    pagina.SetActive(true);

    // Mantiene visible el panel de EnemigasNegras
    if (panelEnemigasNegras) panelEnemigasNegras.SetActive(true);

    currentPage = pagina;
    Debug.Log($"📘 Codex: Mostrando {pagina.name}");
    }

    public void TogglePaginaEnemigaRoja(GameObject pagina)
    {
        if (currentPage == pagina && pagina.activeSelf)
        {
            pagina.SetActive(false);
            currentPage = null;
            return;
        }

        OcultarPaginas();

        pagina.SetActive(true);

        // 👇 mantener visible el panel de enemigas rojas
        if (panelEnemigasRojas) panelEnemigasRojas.SetActive(true);

        currentPage = pagina;
        Debug.Log($"📘 Codex: Mostrando {pagina.name}");
    }

    public void TogglePaginaScore(GameObject pagina)
    {
        if (currentPage == pagina && pagina.activeSelf)
        {
            pagina.SetActive(false);
            currentPage = null;
            return;
        }

        OcultarPaginas();

        pagina.SetActive(true);

        // 👇 mantener visible el panel de Score
        if (panelScore) panelScore.SetActive(true);

        currentPage = pagina;
        Debug.Log($"📘 Codex: Mostrando {pagina.name}");
    }

    public void TogglePaginaAntiScore(GameObject pagina)
    {
        if (currentPage == pagina && pagina.activeSelf)
        {
            pagina.SetActive(false);
            currentPage = null;
            return;
        }

        OcultarPaginas();

        pagina.SetActive(true);

        // 👇 mantener visible el panel de AntiScore
        if (panelAntiScore) panelAntiScore.SetActive(true);

        currentPage = pagina;
        Debug.Log($"📘 Codex: Mostrando {pagina.name}");
    }

    public void TogglePaginaHelpItems(GameObject pagina)
    {
        if (currentPage == pagina && pagina.activeSelf)
        {
            pagina.SetActive(false);
            currentPage = null;
            return;
        }

        OcultarPaginas();

        pagina.SetActive(true);

        // 👇 mantener visible el panel de HelpItems
        if (panelHelpItems) panelHelpItems.SetActive(true);

        currentPage = pagina;
        Debug.Log($"📘 Codex: Mostrando {pagina.name}");

    }

    public void TogglePaginaHelpItemsLess(GameObject pagina)
    {
        if (currentPage == pagina && pagina.activeSelf)
        {
            pagina.SetActive(false);
            currentPage = null;
            return;
        }

        OcultarPaginas();

        pagina.SetActive(true);

        // 👇 mantener visible el panel de HelpItemsLess
        if (panelHelpItemsLess) panelHelpItemsLess.SetActive(true);

        currentPage = pagina;
        Debug.Log($"📘 Codex: Mostrando {pagina.name}");
    }

    public void TogglePaginaInmovil(GameObject pagina)
    {
        if (currentPage == pagina && pagina.activeSelf)
        {
            pagina.SetActive(false);
            currentPage = null;
            return;
        }

        OcultarPaginas();

        pagina.SetActive(true);

        // 👇 mantener visible el panel Inmovil
        if (panelInmovil) panelInmovil.SetActive(true);

        currentPage = pagina;
        Debug.Log($"📘 Codex: Mostrando {pagina.name}");

    }

    public void TogglePaginaAwards(GameObject pagina)
    {
        if (currentPage == pagina && pagina.activeSelf)
        {
            pagina.SetActive(false);
            currentPage = null;
            return;
        }

        OcultarPaginas();

        pagina.SetActive(true);

        // 👇 mantener visible el panel de Awards
        if (panelAwards) panelAwards.SetActive(true);

        currentPage = pagina;
        Debug.Log($"📘 Codex: Mostrando {pagina.name}");

    }



    private void OcultarPaginas()
    {
        if (kingPanel) kingPanel.SetActive(false);
        if (queenPanel) queenPanel.SetActive(false);
        if (rookPanel) rookPanel.SetActive(false);
        if (bishopPanel) bishopPanel.SetActive(false);
        if (knightPanel) knightPanel.SetActive(false);
        if (pawnPanel) pawnPanel.SetActive(false);

        if (ReinaNegraPanel) ReinaNegraPanel.SetActive(false);
        if (TorreNegraPanel) TorreNegraPanel.SetActive(false);
        if (AlfilNegroPanel) AlfilNegroPanel.SetActive(false);
        if (CaballoNegroPanel) CaballoNegroPanel.SetActive(false);

        if (peonRojoPanel) peonRojoPanel.SetActive(false);
        if (alfilRojoPanel) alfilRojoPanel.SetActive(false);
        if (torreRojaPanel) torreRojaPanel.SetActive(false);
        if (reinaRojaPanel) reinaRojaPanel.SetActive(false);
        if (caballoRojoPanel) caballoRojoPanel.SetActive(false);

        if (DiamondPanel) DiamondPanel.SetActive(false);
        if (KeyPanel) KeyPanel.SetActive(false);
        if (KingdomCoinPanel) KingdomCoinPanel.SetActive(false);
        if (KingdomBagPanel) KingdomBagPanel.SetActive(false);
        if (KingdomChestPanel) KingdomChestPanel.SetActive(false);
        if (KingdomCrownPanel) KingdomCrownPanel.SetActive(false);

        if (PadlockPanel) PadlockPanel.SetActive(false);
        if (TalismanPanel) TalismanPanel.SetActive(false);
        if (FalseCoinPanel) FalseCoinPanel.SetActive(false);
        if (EfritsRingPanel) EfritsRingPanel.SetActive(false);
        if (MagicLampPanel) MagicLampPanel.SetActive(false);
        if (SwordPanel) SwordPanel.SetActive(false);

        if (ClockPanel) ClockPanel.SetActive(false);
        if (BlueStarPanel) BlueStarPanel.SetActive(false);
        if (HeartPanel) HeartPanel.SetActive(false);
        if (Potion1PMPanel) Potion1PMPanel.SetActive(false);
        if (Potion3PMPanel) Potion3PMPanel.SetActive(false);
        if (Potion5PMPanel) Potion5PMPanel.SetActive(false);
        if (Potion15PMPanel) Potion15PMPanel.SetActive(false);
        if (ShieldPanel) ShieldPanel.SetActive(false);

        if (ClockLessTimePanel) ClockLessTimePanel.SetActive(false);
        if (BlackStarPanel) BlackStarPanel.SetActive(false);
        if (Less1PMPanel) Less1PMPanel.SetActive(false);
        if (Less3PMPanel) Less3PMPanel.SetActive(false);
        if (Less5PMPanel) Less5PMPanel.SetActive(false);
        if (Less15PMPanel) Less15PMPanel.SetActive(false);
        if (HeartLessPanel) HeartLessPanel.SetActive(false);

        if (PusherUpPanel) PusherUpPanel.SetActive(false);
        if (PusherDownPanel) PusherDownPanel.SetActive(false);
        if (PusherLeftPanel) PusherLeftPanel.SetActive(false);
        if (PusherRightPanel) PusherRightPanel.SetActive(false);
        if (ExpansionPanel) ExpansionPanel.SetActive(false);
        if (AttractionPanel) AttractionPanel.SetActive(false);
        if (InterruptionPanel) InterruptionPanel.SetActive(false);
        if (VortexPanel) VortexPanel.SetActive(false);
        if (WallPanel) WallPanel.SetActive(false);

        if (ParchmentPanel) ParchmentPanel.SetActive(false);
        if (TrophyPanel) TrophyPanel.SetActive(false);
        if (MedalPanel) MedalPanel.SetActive(false);
    

    }
}
