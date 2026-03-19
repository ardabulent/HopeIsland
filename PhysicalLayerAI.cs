using UnityEngine;
using System.Collections.Generic;
using System.Linq; // Matematik işlemleri için gerekli

// ARDA'NIN 'physical_layer.py' DOSYASININ C# PORTU
// Bu script, dokunmatik hareketlerdeki titremeyi (Jitter) ve hızı analiz eder.
public class PhysicalLayerAI : MonoBehaviour
{
    public static PhysicalLayerAI Instance; // Diğer scriptlerden ulaşmak için

    [Header("Arda'nın Yapay Zeka Parametreleri")]
    // physical_layer.py dosyasındaki eşik değer (jitter > 50.0 ise Tremor)
    public float tremorThreshold = 50.0f;

    // physical_layer.py dosyasındaki yavaşlık eşiği (avg_speed < 2.0 ise Yavaş)
    public float slowThreshold = 2.0f;

    [Header("Analiz Durumu")]
    public string currentStatus = "Bekleniyor...";
    public float currentJitter = 0f;

    // Hareket hafızası (Son 10 kareyi tutar)
    private List<Vector2> touchHistory = new List<Vector2>();
    private int maxSampleCount = 10;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Update()
    {
        // 1. Dokunma veya Mouse Tıklaması Var mı?
        // (Input.GetMouseButton(0) hem telefonda dokunmayı hem PC'de mouse'u algılar)
        if (Input.GetMouseButton(0))
        {
            Vector2 currentPos = Input.mousePosition;
            touchHistory.Add(currentPos);

            // Listeyi temiz tut (Sadece son hareketlere odaklan)
            if (touchHistory.Count > maxSampleCount)
            {
                touchHistory.RemoveAt(0);
            }

            // Yeterli veri toplandıysa analizi çalıştır
            if (touchHistory.Count >= 5)
            {
                AnalyzeMotorSkills();
            }
        }
        else
        {
            // Elini çektiyse analizi sıfırla
            touchHistory.Clear();
            currentStatus = "Normal";
        }
    }

    // Arda'nın 'analyze_motor_skills' fonksiyonunun C# karşılığı
    void AnalyzeMotorSkills()
    {
        // A. Hız ve Mesafe Hesapla (Python: np.diff ve np.linalg.norm)
        List<float> distances = new List<float>();
        for (int i = 1; i < touchHistory.Count; i++)
        {
            float dist = Vector2.Distance(touchHistory[i], touchHistory[i - 1]);
            distances.Add(dist);
        }

        // B. Titreme (Jitter) Hesabı: Standart Sapma (Python: np.std)
        float avgSpeed = distances.Average();
        float sumOfSquares = 0f;

        foreach (float d in distances)
        {
            sumOfSquares += Mathf.Pow(d - avgSpeed, 2);
        }

        // Standart Sapma Formülü: Karekök(Varyans)
        float jitter = Mathf.Sqrt(sumOfSquares / distances.Count);

        // Değerleri Unity Editörde görelim diye değişkene atıyoruz
        currentJitter = jitter;

        // C. KARAR MEKANİZMASI (Arda'nın Mantığı)
        if (jitter > tremorThreshold)
        {
            currentStatus = "Titreme Tespit Edildi (Tremor)";
            // Konsola sadece durum değişince yazmak iyi olur, sürekli spam yapmasın diye
            // Debug.Log($"⚠️ AI UYARISI: Titreme Var! Skor: {jitter:F2}");

            // İLERİDE BURAYA OYUNU KOLAYLAŞTIRAN KODU BAĞLAYACAĞIZ
            // Örn: GameManager.Instance.MakeButtonsBigger();
        }
        else if (avgSpeed < slowThreshold)
        {
            currentStatus = "Refleks Yavaş";
            // Debug.Log($"🐢 AI UYARISI: Hareket Çok Yavaş. Hız: {avgSpeed:F2}");
        }
        else
        {
            currentStatus = "Normal";
        }
    }
}