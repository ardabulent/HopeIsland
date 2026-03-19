using UnityEngine;
using TMPro;
using System.Text; // StringBuilder için gerekli
using System.Collections.Generic; // List için gerekli
using System; // DateTime için gerekli

public class ParentPanelUI : MonoBehaviour
{
    [Header("UI Elemanları")]
    public GameObject loginPanel;    // Şifre Ekranı
    public GameObject dataPanel;     // Veri Ekranı
    public TMP_InputField passwordInput; // Şifre Kutusu
    public TextMeshProUGUI reportText;   // Raporun yazılacağı yer (Eğer grafik kullanıyorsan bunu boş bırakabilirsin)
    public TextMeshProUGUI errorText;    // "Yanlış Şifre" yazısı

    private string parentPassword = "1234"; // Şimdilik basit şifre

    void Start()
    {
        // ŞUANKİ SAHNENİN ADINI KONTROL ET
        string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        // Eğer Ebeveyn Sahnesindeysek -> Giriş Paneli AÇIK başlasın
        if (currentScene == "ParentScene")
        {
            if (loginPanel != null) loginPanel.SetActive(true);
        }
        else
        {
            // Başka sahnedeysek (Örn: IntroScene'de popup ise) -> KAPALI başlasın
            if (loginPanel != null) loginPanel.SetActive(false);
        }

        // Veri paneli ve hata yazısı her türlü kapalı başlasın
        if (dataPanel != null) dataPanel.SetActive(false);
        if (errorText != null) errorText.text = "";
    }

    // "Ebeveyn Girişi" butonuna basınca bu çalışacak
    public void OpenLoginScreen()
    {
        loginPanel.SetActive(true);
        dataPanel.SetActive(false);
        passwordInput.text = ""; // Kutuyu temizle
    }

    // Şifreyi Girip "Giriş" butonuna basınca
    public void CheckPassword()
    {
        if (passwordInput.text == parentPassword)
        {
            ShowData(); // Şifre doğru, verileri göster
        }
        else
        {
            if (errorText != null) errorText.text = "Hatalı Şifre! ❌";
        }
    }

    void ShowData()
    {
        loginPanel.SetActive(false); // Girişi kapat
        dataPanel.SetActive(true);   // Raporu aç

        // Verileri Çek ve Yazdır
        if (ParentDataManager.Instance != null)
        {
            // --- DEĞİŞİKLİK BURADA: history yerine GetScoresForDate kullanıyoruz ---
            List<int> todayScores = ParentDataManager.Instance.GetScoresForDate(DateTime.Now);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("<b>SAAT | DUYGU DURUMU | PUAN</b>");
            sb.AppendLine("-----------------------------------");

            if (todayScores.Count == 0)
            {
                sb.AppendLine("Bugün henüz veri kaydedilmemiş.");
            }
            else
            {
                // Verileri listele
                for (int i = 0; i < todayScores.Count; i++)
                {
                    int score = todayScores[i];
                    string moodText = ConvertScoreToMood(score);

                    // Saati tahmini yazıyoruz (Veri sırasına göre 10:00'dan başlatıp 2 saat ekleyerek)
                    string time = (10 + (i * 2)).ToString("00") + ":00";

                    sb.AppendLine($"{time} | {moodText} | {score}");
                }
            }

            if (reportText != null) reportText.text = sb.ToString();
        }
        else
        {
            if (reportText != null) reportText.text = "Veri Yöneticisi Bulunamadı!";
        }
    }

    // Puanı tekrar yazıya çeviren yardımcı fonksiyon
    string ConvertScoreToMood(int score)
    {
        if (score >= 75) return "Harika! 😊";
        if (score >= 50) return "Nötr 😐";
        if (score >= 25) return "Üzgün 😔";
        return "Kötü 😢";
    }

    // Geri Dön Butonu için
    public void BackToMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("StartScene");
    }

    public void ClosePanel()
    {
        loginPanel.SetActive(false);
        dataPanel.SetActive(false);
    }
}