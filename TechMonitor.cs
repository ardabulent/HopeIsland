using UnityEngine;
using TMPro; // TextMeshPro kullanmak için şart
using System.Collections;
using UnityEngine.Profiling; // RAM kullanımı için

public class TechMonitor : MonoBehaviour
{
    [Header("Ayarlar")]
    public TextMeshProUGUI debugText; // Ekrandaki yazı kutusu
    public float updateInterval = 0.5f; // Yarım saniyede bir güncelle (Göz yormasın)

    // --- SAHTE AI VERİLERİ (Sonra gerçeğiyle değişecek) ---
    private string[] fakeMoods = { "Mutlu 😊", "Sakin 😐", "Odaklı 🧐", "Enerjik ⚡", "Analiz Ediliyor... 🔄" };
    private string currentMood = "Bekleniyor...";
    private string cloudStatus = "Huawei Cloud: Connected 🟢";

    private float accum = 0; // FPS hesaplama yardımcıları
    private int frames = 0;
    private float timeLeft;

    void Start()
    {
        timeLeft = updateInterval;
        // Yapay zeka simülasyonunu başlat
        StartCoroutine(SimulateAI());
    }

    void Update()
    {
        // 1. FPS ve MS HESAPLAMA
        timeLeft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // Süre dolduysa ekrana yaz
        if (timeLeft <= 0.0)
        {
            // FPS Değeri
            float fps = accum / frames;
            // MS Değeri (1 kare ne kadar sürede çiziliyor)
            float ms = 1000.0f / fps;

            // RAM Kullanımı (Megabyte cinsinden)
            long memory = Profiler.GetTotalAllocatedMemoryLong() / 1048576;

            // Rengi FPS'e göre ayarla (30 altıysa Kırmızı yap)
            string colorHex = (fps < 30) ? "FF0000" : "00FF00";

            // EKRANA YAZDIRMA (Formatlama)
            // \n alt satıra geçer. <b> kalın yapar.
            string textToShow = string.Format(
                "<color=#{4}>FPS: {0:F1}</color> | MS: {1:F1}ms\n" +
                "RAM: {2} MB\n" +
                "<b>AI STATUS:</b> {3}\n" +
                "<size=80%>{5}</size>", // Cloud yazısı biraz küçük olsun
                fps, ms, memory, currentMood, colorHex, cloudStatus
            );

            if (debugText != null)
                debugText.text = textToShow;

            // Değerleri sıfırla
            timeLeft = updateInterval;
            accum = 0.0f;
            frames = 0;
        }
    }

    // --- YAPAY ZEKA SİMÜLASYONU ---
    // Bu kısım 3-5 saniyede bir rastgele duygu değiştirir.
    // Gerçek AI gelince burayı silip gerçek veriyi bağlayacağız.
    IEnumerator SimulateAI()
    {
        while (true)
        {
            // Rastgele bir duygu seç
            int randomIndex = Random.Range(0, fakeMoods.Length);
            currentMood = fakeMoods[randomIndex];

            // 3 ile 6 saniye arasında bekle
            yield return new WaitForSeconds(Random.Range(3f, 6f));
        }
    }
}
