using UnityEngine;
using System.Collections.Generic;

public class Donaciones : MonoBehaviour
{
    private Dictionary<SystemLanguage, string> enlacesPorIdioma = new Dictionary<SystemLanguage, string>()
    {
        { SystemLanguage.Spanish, "https://chessescape.com/donations/donations-es.html" },
        { SystemLanguage.English, "https://chessescape.com/donations/donations-en.html" },
        { SystemLanguage.French,  "https://chessescape.com/donations/donations-fr.html" },
        { SystemLanguage.German,  "https://chessescape.com/donations/donations-de.html" },
        { SystemLanguage.Portuguese, "https://chessescape.com/donations/donations-pt.html" },
        { SystemLanguage.Korean,  "https://chessescape.com/donations/donations-ko.html" },
        { SystemLanguage.Greek,   "https://chessescape.com/donations/donations-el.html" },
        { SystemLanguage.Italian, "https://chessescape.com/donations/donations-it.html" },
        { SystemLanguage.Japanese,"https://chessescape.com/donations/donations-ja.html" },
        { SystemLanguage.Russian, "https://chessescape.com/donations/donations-ru.html" }
    };

    [SerializeField] private string urlDefault = "https://chessescape.com/donations/donations-en.html"; // fallback

    public void AbrirWeb()
    {
        SystemLanguage idioma = Application.systemLanguage;
        Debug.Log("🌍 Idioma detectado: " + idioma);

        string url;
        if (!enlacesPorIdioma.TryGetValue(idioma, out url))
        {
            Debug.Log("⚠️ Idioma no soportado, usando default");
            url = urlDefault;
        }

        Debug.Log("🔗 Abriendo URL: " + url);
        Application.OpenURL(url);
    }
}
