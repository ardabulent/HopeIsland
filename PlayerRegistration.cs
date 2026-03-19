using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerRegistration : MonoBehaviour
{
    [Header("--- ARAYÜZ BAÐLANTILARI ---")]
    public TMP_InputField nameInput;
    public TMP_InputField ageInput;
    public Toggle privacyToggle;
    public Button submitButton;

    [Header("--- CÝNSÝYET KUTUCUKLARI (TOGGLES) ---")]
    public Toggle boyToggle;
    public Toggle girlToggle;
    private string selectedGender = "";

    [Header("--- GÝZLÝLÝK POPUP ---")]
    public GameObject privacyPopupPanel;
    public Button openPrivacyButton;
    public Button closePrivacyButton;

    [Header("--- SONRAKÝ SAHNE ---")]
    public string nextSceneName = "WorldSelectionScene";

    void Start()
    {
        // Baþlangýçta Butonu ve Popup'ý kapat
        submitButton.interactable = false;
        if (privacyPopupPanel != null) privacyPopupPanel.SetActive(false);

        // Veri giriþlerini dinle (Herhangi biri deðiþirse formu kontrol et)
        nameInput.onValueChanged.AddListener(delegate { CheckForm(); });
        ageInput.onValueChanged.AddListener(delegate { CheckForm(); });
        privacyToggle.onValueChanged.AddListener(delegate { CheckForm(); });

        // --- CÝNSÝYET KUTUCUKLARI MANTIÐI ---
        // Erkek seçilirse Kýzý kapat
        boyToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                girlToggle.isOn = false;
                selectedGender = "Erkek";
                CheckForm();
            }
        });

        // Kýz seçilirse Erkeði kapat
        girlToggle.onValueChanged.AddListener((isOn) => {
            if (isOn)
            {
                boyToggle.isOn = false;
                selectedGender = "Kýz";
                CheckForm();
            }
        });

        // Popup'ý açýp kapatma butonlarý
        openPrivacyButton.onClick.AddListener(OpenPrivacyPopup);
        closePrivacyButton.onClick.AddListener(ClosePrivacyPopup);

        // Kayýt butonu
        submitButton.onClick.AddListener(SaveAndContinue);
    }

    public void OpenPrivacyPopup()
    {
        privacyPopupPanel.SetActive(true);
    }

    public void ClosePrivacyPopup()
    {
        privacyPopupPanel.SetActive(false);
    }

    public void CheckForm()
    {
        // 4 Þart Aranýyor: Ýsim var mý? + Yaþ var mý? + Cinsiyetten biri iþaretli mi? + Kutu iþaretli mi?
        bool isGenderSelected = boyToggle.isOn || girlToggle.isOn;

        if (!string.IsNullOrEmpty(nameInput.text) &&
            !string.IsNullOrEmpty(ageInput.text) &&
            isGenderSelected &&
            privacyToggle.isOn)
        {
            submitButton.interactable = true; // Her þey tamamsa Submit yanar!
        }
        else
        {
            submitButton.interactable = false;
        }
    }

    public void SaveAndContinue()
    {
        // Tüm verileri telefon hafýzasýna kaydet
        PlayerPrefs.SetString("PlayerName", nameInput.text);
        PlayerPrefs.SetInt("PlayerAge", int.Parse(ageInput.text));
        PlayerPrefs.SetString("PlayerGender", selectedGender);
        PlayerPrefs.SetInt("IsRegistered", 1);
        PlayerPrefs.Save();

        Debug.Log($"Kayýt Baþarýlý! Ýsim: {nameInput.text} | Yaþ: {ageInput.text} | Cinsiyet: {selectedGender}");

        SceneManager.LoadScene(nextSceneName);
    }
}