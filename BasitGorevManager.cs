using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro; // YAZI GÜNCELLEMESÝ ÝÇÝN BU ÞART KANKA

public class BasitGorevManager : MonoBehaviour
{
    [Header("--- UI BAÐLANTILARI ---")]
    public TextMeshProUGUI starText; // YENÝ EKLENDÝ: Ekrandaki yýldýz yazýmýz

    [Header("--- TIKLANABÝLÝR BUTONLAR ---")]
    public Button[] gorevButonlari;

    [Header("--- TÝK GÖRSELLERÝ ---")]
    public GameObject[] tikGorselleri;

    private const string GlobalStarsKey = "Global_TotalStars";
    private const string LastResetDateKey = "TaskList_LastResetDate";

    void OnEnable()
    {
        CheckDailyReset();
        LoadTaskStates();
        UpdateStarUI(); // YENÝ EKLENDÝ: Ekran açýldýðý an güncel yýldýzý yaz!
    }

    // =========================================================
    // --- GÜNLÜK SIFIRLAMA MANTIÐI ---
    // =========================================================
    void CheckDailyReset()
    {
        string today = DateTime.Now.ToString("dd/MM/yyyy");
        string lastResetDate = PlayerPrefs.GetString(LastResetDateKey, "");

        if (today != lastResetDate)
        {
            SifirlaButunGorevleri();
            PlayerPrefs.SetString(LastResetDateKey, today);
            PlayerPrefs.Save();
        }
    }

    public void Test_ManualResetButton()
    {
        SifirlaButunGorevleri();
        PlayerPrefs.SetString(LastResetDateKey, "01/01/2000");
        PlayerPrefs.Save();
        LoadTaskStates();
        Debug.Log("Test: Görevler sýfýrlandý!");
    }

    void SifirlaButunGorevleri()
    {
        for (int i = 0; i < 3; i++)
        {
            PlayerPrefs.SetInt("BasitGorev_" + i, 0);
        }
    }

    // =========================================================
    // --- DURUMLARI YÜKLEME VE TIKLAMA MANTIÐI ---
    // =========================================================
    void LoadTaskStates()
    {
        for (int i = 0; i < 3; i++)
        {
            int isDone = PlayerPrefs.GetInt("BasitGorev_" + i, 0);

            if (isDone == 1)
            {
                tikGorselleri[i].SetActive(true);
                gorevButonlari[i].interactable = false;
            }
            else
            {
                tikGorselleri[i].SetActive(false);
                gorevButonlari[i].interactable = true;
            }
        }
    }

    public void GoreveTikla(int gorevIndex)
    {
        // 1. Görevi yapýldý olarak kaydet
        PlayerPrefs.SetInt("BasitGorev_" + gorevIndex, 1);

        // 2. Tiki göster ve butonu kilitle
        tikGorselleri[gorevIndex].SetActive(true);
        gorevButonlari[gorevIndex].interactable = false;

        // 3. ORTAK KASAYA 1 YILDIZ EKLE!
        int currentStars = PlayerPrefs.GetInt(GlobalStarsKey, 0);
        currentStars++;
        PlayerPrefs.SetInt(GlobalStarsKey, currentStars);
        PlayerPrefs.Save();

        // YENÝ EKLENDÝ: Yýldýz kazanýldýðý an ekrandaki yazýyý þlak diye güncelle!
        UpdateStarUI();

        Debug.Log("Aferin minik kahraman! Bir yýldýz kazandýn!");
    }

    // YENÝ EKLENDÝ: Ekrandaki yazýyý güncelleyen özel fonksiyon
    void UpdateStarUI()
    {
        if (starText != null)
        {
            int currentStars = PlayerPrefs.GetInt(GlobalStarsKey, 0);
            starText.text = currentStars.ToString();
        }
    }
}