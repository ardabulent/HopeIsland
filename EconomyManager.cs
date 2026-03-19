using UnityEngine;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance; // Her yerden ulaþmak için

    public TextMeshProUGUI starText; // Ekrandaki sayý
    public int totalStars = 0;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        totalStars = PlayerPrefs.GetInt("TotalStars", 0);
        UpdateUI();
    }

    // Puan Ekleme
    public void AddStars(int amount)
    {
        totalStars += amount;
        PlayerPrefs.SetInt("TotalStars", totalStars);
        PlayerPrefs.Save();
        UpdateUI();
    }

    // --- ÝÞTE EKSÝK OLAN FONKSÝYON BU ---
    public bool SpendStars(int amount)
    {
        if (totalStars >= amount)
        {
            totalStars -= amount;
            PlayerPrefs.SetInt("TotalStars", totalStars);
            PlayerPrefs.Save();
            UpdateUI();
            return true; // Satýn alma baþarýlý
        }
        else
        {
            return false; // Para yetmedi
        }
    }
    // -------------------------------------

    void UpdateUI()
    {
        if (starText != null)
            starText.text = totalStars.ToString();
    }
}