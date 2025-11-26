using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using VisQuizDesktop.Models;

namespace VisQuizDesktop.Services
{
    public class SettingsService
    {
        private const string SettingsFileName = "Ustawienia.json";
        private static AppSettings? _cachedSettings;

        public static AppSettings LoadSettings()
        {
            // Jeœli ustawienia s¹ ju¿ w cache, zwróæ je
            if (_cachedSettings != null)
            {
                return _cachedSettings;
            }

            try
            {
                // SprawdŸ czy plik istnieje
                if (File.Exists(SettingsFileName))
                {
                    string json = File.ReadAllText(SettingsFileName);
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true,
                        WriteIndented = true
                    };

                    var settings = JsonSerializer.Deserialize<AppSettings>(json, options);
                    
                    if (settings != null)
                    {
                        _cachedSettings = settings;
                        System.Diagnostics.Debug.WriteLine($"Za³adowano ustawienia: LiczbaPytan={settings.LiczbaPytan}");
                        return settings;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"B³¹d podczas wczytywania ustawieñ: {ex.Message}");
            }

            // Jeœli nie uda³o siê wczytaæ, utwórz domyœlne ustawienia
            System.Diagnostics.Debug.WriteLine("Tworzê domyœlny plik ustawieñ...");
            var defaultSettings = new AppSettings();
            SaveSettings(defaultSettings);
            _cachedSettings = defaultSettings;
            return defaultSettings;
        }

        public static void SaveSettings(AppSettings settings)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(SettingsFileName, json);
                _cachedSettings = settings;
                System.Diagnostics.Debug.WriteLine($"Zapisano ustawienia: LiczbaPytan={settings.LiczbaPytan}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"B³¹d podczas zapisywania ustawieñ: {ex.Message}");
            }
        }

        public static void ReloadSettings()
        {
            _cachedSettings = null;
            LoadSettings();
        }
    }
}
