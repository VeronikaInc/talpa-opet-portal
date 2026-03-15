using System.Text.Json;

namespace OPETPortal.Web.Services;

public class LocalizationService
{
    private readonly Dictionary<string, Dictionary<string, string>> _strings;
    public string CurrentLang { get; private set; } = "tr";

    public LocalizationService(IWebHostEnvironment env)
    {
        _strings = new();
        foreach (var lang in new[] { "tr", "en" })
        {
            var path = Path.Combine(env.ContentRootPath, "Resources", $"{lang}.json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var dict = JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
                _strings[lang] = dict;
            }
            else
            {
                _strings[lang] = new();
            }
        }
    }

    public void SetLang(string lang)
    {
        if (_strings.ContainsKey(lang))
            CurrentLang = lang;
    }

    public string Get(string key)
    {
        if (_strings.TryGetValue(CurrentLang, out var dict) && dict.TryGetValue(key, out var val))
            return val;
        if (_strings.TryGetValue("tr", out var fallback) && fallback.TryGetValue(key, out var fb))
            return fb;
        return key;
    }

    public string this[string key] => Get(key);
}
