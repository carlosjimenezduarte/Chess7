using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

/// <summary>
/// Gestor único de notificaciones internas basadas en paneles preexistentes.
/// - En la escena "Notificaciones": asigna los paneles (inactivos por defecto) y el contenedor del ScrollView.
/// - En la escena "GameHome": asigna el badge (texto y burbuja roja).
/// - Persistencia por slot: cada panel se cierra una sola vez (PlayerPrefs).
/// - Se integra con AchievementsManager: llámalo desde TryUnlock (ver parche abajo).
/// </summary>
public class NotificationCenter : MonoBehaviour
{
    [Serializable]
    public struct PanelBinding
    {
        [Tooltip("ID de la notificación (usa el mismo nombre que AchievementId, ej: PawnIsGold)")]
        public string id;
        [Tooltip("Panel ya diseñado dentro del ScrollView. Déjalo INACTIVO por defecto.")]
        public GameObject panel;
        [Tooltip("Botón de cerrar del panel (opcional si ya lo conectas en el Inspector)")]
        public Button closeButton;
    }

    public static NotificationCenter Instance { get; private set; }

    [Header("Escena: Notificaciones (opcional)")]
    [Tooltip("Contenedor del ScrollView (no obligatorio, solo informativo)")]
    public Transform scrollContent;
    [Tooltip("Lista de paneles preexistentes a controlar")]
    public List<PanelBinding> paneles = new List<PanelBinding>();

    [Header("Escena: GameHome (opcional)")]
    [Tooltip("Burbuja roja del badge (se oculta si count=0)")]
    public GameObject badgeBubble;
    [Tooltip("Texto del badge (ej: 3)")]
    public TMP_Text badgeText;

    [Header("Config")]
    [Tooltip("Si no hay slotActivo, usar este por defecto")]
    public string fallbackSlotId = "slot1";
    [Tooltip("Imprime logs útiles")]
    public bool verbose = false;

    // Mapa rápido id->panel
    private readonly Dictionary<string, PanelBinding> _map = new Dictionary<string, PanelBinding>(StringComparer.Ordinal);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            // Reusar el singleton y pasarle referencias de la escena actual (bindings y badge),
            // luego destruir el duplicado.
            if (verbose) Debug.Log("[NC] Ya existe instancia. Actualizando referencias de escena…");
            Instance.AbsorbSceneRefsFrom(this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        BuildMap();
        WireCloseButtonsIfNeeded();
        RefreshPanelsFromPrefs();
        RefreshBadge();
    }

    private void Start()
    {
        // Por si los references se asignan tarde en el ciclo de vida (no es obligatorio)
        RefreshBadge();
        RefreshPanelsFromPrefs();
    }

    /// <summary>
    /// Copia referencias de la escena actual a la instancia persistente.
    /// Útil cuando pones este componente en varias escenas (GameHome y Notificaciones).
    /// </summary>
    public void AbsorbSceneRefsFrom(NotificationCenter other)
    {
        if (other == null) return;

        // Mezclar/actualizar bindings (prioridad a los que trae la escena actual si no existen)
        foreach (var b in other.paneles)
        {
            if (b.panel == null || string.IsNullOrWhiteSpace(b.id)) continue;
            if (!_map.ContainsKey(b.id)) _map[b.id] = b;
        }

        // Actualizar refs de UI de escena
        if (other.scrollContent != null) scrollContent = other.scrollContent;
        if (other.badgeBubble != null) badgeBubble = other.badgeBubble;
        if (other.badgeText != null) badgeText = other.badgeText;

        WireCloseButtonsIfNeeded();
        RefreshPanelsFromPrefs();
        RefreshBadge();
    }

    private void BuildMap()
    {
        _map.Clear();
        foreach (var b in paneles)
        {
            if (b.panel == null || string.IsNullOrWhiteSpace(b.id)) continue;
            if (!_map.ContainsKey(b.id)) _map.Add(b.id, b);
            // Asegura inactivo por defecto
            b.panel.SetActive(false);
        }
    }

    private void WireCloseButtonsIfNeeded()
    {
        foreach (var kv in _map)
        {
            var pb = kv.Value;
            if (pb.closeButton != null)
            {
                string idCopy = pb.id; // capturar
                pb.closeButton.onClick.RemoveListener(() => { });
                pb.closeButton.onClick.AddListener(() => ClosePanelById(idCopy));
            }
        }
    }

    // ==========================
    // API para Achievements / HUD
    // ==========================

    /// <summary>
    /// Llamar cuando se desbloquea un logro (una sola vez por slot).
    /// No muestra pop-up inmediato; activa el panel (si existe en la escena) y actualiza badge.
    /// </summary>
    public void OnAchievementUnlocked(string slotId, AchievementId id, string title = null, string body = null)
    {
        if (string.IsNullOrEmpty(slotId)) slotId = GetActiveSlot();
        string notifId = id.ToString();

        // Marca como "no cerrado" para que aparezca en la pantalla de notificaciones
        SetDismissed(slotId, notifId, false);

        // Si el panel de esta notificación está mapeado en esta escena, activarlo.
        ActivatePanelIfPresent(notifId);

        if (verbose) Debug.Log($"[NC] OnAchievementUnlocked -> {notifId} (slot {slotId}).");

        RefreshBadge(slotId);
    }

    /// <summary>
    /// Cerrar panel y persistir el cierre (no volverá a mostrarse).
    /// Conectar este método al botón [X] del panel (vía Inspector), pasando el ID.
    /// </summary>
    public void ClosePanelById(string id)
    {
        string slotId = GetActiveSlot();
        SetDismissed(slotId, id, true);

        if (_map.TryGetValue(id, out var pb) && pb.panel != null)
            pb.panel.SetActive(false);

        if (verbose) Debug.Log($"[NC] ClosePanel -> {id} (slot {slotId})");

        RefreshBadge(slotId);
    }

    /// <summary>
    /// Actualiza badge en GameHome.
    /// </summary>
    /*public void RefreshBadge(string slotId = null)
    {
        slotId ??= GetActiveSlot();

        // Contar notificaciones desbloqueadas y NO cerradas para IDs que conocemos (paneles registrados)
        int count = 0;
        foreach (var id in _map.Keys)
        {
            if (IsUnlocked(slotId, id) && !IsDismissed(slotId, id))
                count++;
        }

        if (badgeBubble != null) badgeBubble.SetActive(count > 0);
        if (badgeText != null) badgeText.text = count.ToString();

        if (verbose) Debug.Log($"[NC] Badge actualizado => {count}");
    }*/
    public void RefreshBadge(string slotId = null)
    {
        slotId ??= GetActiveSlot();
        int count = 0;
        foreach (var name in Enum.GetNames(typeof(AchievementId)))
            if (IsUnlocked(slotId, name) && !IsDismissed(slotId, name))
                count++;

        if (badgeBubble != null) badgeBubble.SetActive(count > 0);
        if (badgeText != null) badgeText.text = count.ToString();
        if (verbose) Debug.Log($"[NC] Badge actualizado => {count}");

    }


    /// <summary>
    /// Llamar cuando entras a la escena de Notificaciones para reflejar estado actual.
    /// </summary>
    public void RefreshPanelsFromPrefs()
    {
        string slotId = GetActiveSlot();

        foreach (var kv in _map)
        {
            string id = kv.Key;
            var pb = kv.Value;

            bool visible = IsUnlocked(slotId, id) && !IsDismissed(slotId, id);
            if (pb.panel != null) pb.panel.SetActive(visible);
        }

        if (verbose) Debug.Log("[NC] Panels refrescados desde PlayerPrefs.");
    }

    // ==========================
    // Helpers de estado
    // ==========================

    private void ActivatePanelIfPresent(string id)
    {
        if (_map.TryGetValue(id, out var pb) && pb.panel != null)
            pb.panel.SetActive(true);
    }

    private string GetActiveSlot()
    {
        return PlayerPrefs.GetString("slotActivo", fallbackSlotId);
    }

    private static string KeyAch(string slotId, string notifId) => $"{slotId}_ach_{notifId}";
    private static string KeyDismissed(string slotId, string notifId) => $"{slotId}_notif_{notifId}_dismissed";

    // Unlocked lo escribe AchievementsManager (TryUnlock). Aquí solo lo leemos.
    private bool IsUnlocked(string slotId, string notifId)
    {
        return PlayerPrefs.GetInt(KeyAch(slotId, notifId), 0) == 1;
    }

    private bool IsDismissed(string slotId, string notifId)
    {
        return PlayerPrefs.GetInt(KeyDismissed(slotId, notifId), 0) == 1;
    }

    private void SetDismissed(string slotId, string notifId, bool dismissed)
    {
        PlayerPrefs.SetInt(KeyDismissed(slotId, notifId), dismissed ? 1 : 0);
        PlayerPrefs.Save();
    }

    
}
