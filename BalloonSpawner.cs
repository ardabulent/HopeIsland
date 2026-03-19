using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BalloonSpawner : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject balloonPrefab;
    public Transform spawnPointContainer;

    [Header("Görseller")]
    public Sprite[] balloonSprites;

    // --- SAYAÇLAR ---
    private int comboCount = 0;           // Doğru vuruş serisi
    private int consecutiveMissCount = 0; // ÜST ÜSTE hatalı vuruş serisi

    private float minSpawnTime = 1.0f;
    private float maxSpawnTime = 2.5f;
    private float currentBalloonSpeed = 250f;
    private float baseBalloonSpeed = 250f;
    private float xRange = 350f;

    private bool isGameActive = false;
    private GameAdaptationEngine adaptationEngine;

    void OnEnable()
    {
        adaptationEngine = new GameAdaptationEngine();

        // --- ZORLUK AYARI (MOOD) ---
        if (GameManager.Instance != null)
        {
            int mood = GameManager.Instance.currentMoodLevel;
            if (mood == 1)
            { // YORGUN
                minSpawnTime = 1.5f; maxSpawnTime = 3.0f;
                baseBalloonSpeed = 150f;
            }
            else if (mood == 2)
            { // ENERJİK
                minSpawnTime = 0.5f; maxSpawnTime = 1.2f;
                baseBalloonSpeed = 400f;
            }
            else
            { // NORMAL
                minSpawnTime = 1.0f; maxSpawnTime = 2.5f;
                baseBalloonSpeed = 250f;
            }
        }
        else { baseBalloonSpeed = 250f; }

        currentBalloonSpeed = baseBalloonSpeed;
        isGameActive = true;
        StartCoroutine(SpawnRoutine());
        StartCoroutine(UpdateAdaptationRoutine());
    }

    /// <summary>
    /// Balon tıklandığında veya panele dokunulduğunda çalışır
    /// </summary>
    public void OnBalloonClicked(bool isCorrect)
    {
        if (adaptationEngine == null) adaptationEngine = new GameAdaptationEngine();

        // --- YENİLENMİŞ KOMBO VE TOLERANS MANTIĞI ---
        if (isCorrect)
        {
            comboCount++;            // Doğru vuruşu artır
            consecutiveMissCount = 0; // HATA SERİSİNİ SIFIRLA (Çünkü bir tane vurdu!)

            Debug.Log("🔥 Kombo: " + comboCount + "/5");

            if (comboCount >= 5)
            {
                GameControl.Difficulty = "Zor"; // Hızlandır
                comboCount = 0;
                Debug.Log("🚀 MÜKEMMEL SERİ! Oyun hızlandı.");
            }
        }
        else
        {
            // ÇOCUK BOŞLUĞA (PANELE) BASTI
            comboCount = 0;           // Doğru seriyi anında boz
            consecutiveMissCount++;    // HATA SERİSİNİ ARTIR

            Debug.Log("⚠️ Boşluğa basıldı! Hata Serisi: " + consecutiveMissCount + "/2");

            if (consecutiveMissCount >= 2)
            {
                GameControl.Difficulty = "Kolay"; // ÜST ÜSTE 2 HATA: Yavaşlat
                consecutiveMissCount = 0;          // Ceza verildi, seriyi sıfırla
                Debug.Log("🐢 ÜST ÜSTE 2 HATA! Oyun yavaşlatıldı.");
            }
        }

        // Adaptasyon motoruna bildir
        ClickData clickData = new ClickData { isCorrect = isCorrect, timestamp = Time.time };
        adaptationEngine.AnalyzeClick(clickData);
    }

    // ... Diğer SpawnRoutine ve UpdateAdaptationRoutine kodların aynı kalabilir ...
    IEnumerator SpawnRoutine()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(Random.Range(minSpawnTime, maxSpawnTime));
            SpawnBalloon();
        }
    }

    void SpawnBalloon()
    {
        if (balloonPrefab == null) return;
        float randomX = Random.Range(-xRange, xRange);
        GameObject newBalloon = Instantiate(balloonPrefab, spawnPointContainer);
        newBalloon.transform.localPosition = new Vector3(randomX, -700, 0);
        newBalloon.transform.localScale = Vector3.one;

        Balloon balloonScript = newBalloon.GetComponent<Balloon>();
        if (balloonScript != null) balloonScript.speed = currentBalloonSpeed;

        if (balloonSprites.Length > 0)
        {
            Image img = newBalloon.GetComponent<Image>();
            if (img != null) img.sprite = balloonSprites[Random.Range(0, balloonSprites.Length)];
        }
    }

    IEnumerator UpdateAdaptationRoutine()
    {
        while (isGameActive)
        {
            yield return new WaitForSeconds(0.5f);
            if (adaptationEngine != null)
            {
                currentBalloonSpeed = baseBalloonSpeed * adaptationEngine.GetCurrentSpeedMultiplier();
            }
        }
    }
}