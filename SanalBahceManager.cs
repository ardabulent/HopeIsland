using UnityEngine;
using TMPro;

public class SanalBahceManager : MonoBehaviour
{
    public TextMeshProUGUI yildizText; // Txt_BahceYildiz objesini buraya sürükleyeceðiz
    private int totalStars = 0;

    void OnEnable()
    {
        // Panel açýldýðýnda güncel yýldýzlarý çek
        totalStars = PlayerPrefs.GetInt("Global_TotalStars", 0);
        UpdateUI();
    }

    // Aðaç satýn alma fonksiyonu
    public bool AgacSatinAl(int maliyet)
    {
        if (totalStars >= maliyet)
        {
            totalStars -= maliyet;
            PlayerPrefs.SetInt("Global_TotalStars", totalStars);
            PlayerPrefs.Save();
            UpdateUI();
            return true; // Parasý yetti, aðacý dikebilir!
        }
        else
        {
            Debug.Log("Yetersiz Yýldýz! Minik kahraman biraz daha oyun oynamalý.");
            // Ýstersen buraya "Hata Sesi" ekleyebiliriz ileride.
            return false; // Parasý yetmedi!
        }
    }

    void UpdateUI()
    {
        if (yildizText != null) yildizText.text = totalStars.ToString();
    }
}