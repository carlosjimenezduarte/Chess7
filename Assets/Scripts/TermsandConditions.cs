using UnityEngine;
using System.Collections.Generic;

public class TermsAndConditionsOpener : MonoBehaviour
{
    private Dictionary<SystemLanguage, string> enlacesPorIdioma = new Dictionary<SystemLanguage, string>()
    {
        { SystemLanguage.Spanish,  "https://chessescape.com/es/terms-es.html" },
        { SystemLanguage.English,  "https://chessescape.com/en/terms-en.html" },
        { SystemLanguage.Japanese, "https://chessescape.com/ja/terms-ja.html" },
        { SystemLanguage.Korean,   "https://chessescape.com/ko/terms-ko.html" },
        { SystemLanguage.Greek,    "https://chessescape.com/el/terms-el.html" },
        { SystemLanguage.Russian,  "https://chessescape.com/ru/terms-ru.html" },
        { SystemLanguage.French,   "https://chessescape.com/fr/terms-fr.html" },
        { SystemLanguage.German,   "https://chessescape.com/de/terms-de.html" },
        { SystemLanguage.Italian,  "https://chessescape.com/it/terms-it.html" },
        { SystemLanguage.Portuguese,"https://chessescape.com/pt/terms-pt.html" }
    };

    [SerializeField] private string urlDefault = "https://chessescape.com/en/terms-en.html"; // fallback

    public void AbrirWeb()
    {
        SystemLanguage idioma = Application.systemLanguage;
        Debug.Log("🌍 Idioma detectado: " + idioma);

        string url;
        if (!enlacesPorIdioma.TryGetValue(idioma, out url))
        {
            Debug.Log("⚠️ Idioma no soportado, usando inglés por defecto.");
            url = urlDefault;
        }

        Debug.Log("🔗 Abriendo URL: " + url);
        Application.OpenURL(url);
    }
}
