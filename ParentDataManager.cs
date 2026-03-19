using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq; // List işlemleri için lazım

public class ParentDataManager : MonoBehaviour
{
    public static ParentDataManager Instance;

    private void Awake()
    {
        // Singleton: Sahne değişse de yok olmasın
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- VERİ KAYDETME (Sentis Burayı Çağıracak) ---
    public void SaveDailyMood(int score)
    {
        // Bugünün tarihi (Örn: "28-01-2026")
        string todayKey = DateTime.Now.ToString("dd-MM-yyyy");

        // Mevcut veriyi çek
        string currentData = PlayerPrefs.GetString(todayKey, "");

        // Yeni skoru ekle (Virgülle ayırarak: "100,50,25")
        if (string.IsNullOrEmpty(currentData))
        {
            currentData = score.ToString();
        }
        else
        {
            currentData += "," + score.ToString();
        }

        // Kaydet
        PlayerPrefs.SetString(todayKey, currentData);
        PlayerPrefs.Save();

        Debug.Log("Veri Kaydedildi: " + score);
    }

    // --- VERİ ÇEKME (Grafik Burayı Çağıracak) ---
    public List<int> GetScoresForDate(DateTime date)
    {
        string dateKey = date.ToString("dd-MM-yyyy");
        string dataString = PlayerPrefs.GetString(dateKey, "");

        List<int> scoreList = new List<int>();

        if (!string.IsNullOrEmpty(dataString))
        {
            // Virgülleri ayırıp listeye çevir
            string[] scores = dataString.Split(',');
            foreach (var s in scores)
            {
                if (int.TryParse(s, out int result))
                {
                    scoreList.Add(result);
                }
            }
        }

        return scoreList;
    }
}