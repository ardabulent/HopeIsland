using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ConstellationGame : MonoBehaviour
{
    [Header("Ayarlar")]
    public List<GameObject> starButtons; // Yıldız Listesi
    public GameObject linePrefab; // Çizgi Resmi
    public Transform lineContainer; // Çizgilerin kutusu
    public int reward = 300;

    [Header("Efekt Ayarları")]
    public float pulseSpeed = 2.0f; // Yanıp sönme hızı
    public float pulseSize = 0.2f; // Ne kadar büyüsün?

    private int currentIndex = 0;
    private bool isGameFinished = false;

    // --- BURASI GÜNCELLENDİ ---
    void OnEnable()
    {
        // GameManager'a sor: Çocuk nasıl hissediyor?
        if (GameManager.Instance != null)
        {
            int mood = GameManager.Instance.currentMoodLevel;

            if (mood == 1) // Yorgun/Sakin Mod
            {
                pulseSpeed = 1.0f; // Çok yavaş ve sakin yanıp sön (Ninnili gibi)
                reward = 500;      // Moral olsun diye ödülü artırdık!
                Debug.Log("Mod: Sakin. Yıldızlar yavaşladı. 🌙");
            }
            else if (mood == 2) // Enerjik Mod
            {
                pulseSpeed = 5.0f; // Hızlı ve canlı yanıp sön
                reward = 300;      // Normal ödül
                Debug.Log("Mod: Enerjik. Yıldızlar hızlandı! ✨");
            }
            else // Normal
            {
                pulseSpeed = 2.0f;
                reward = 300;
            }
        }

        ResetGame();
    }
    // ---------------------------

    void Update()
    {
        // Oyun bitmediyse ve liste boş değilse
        if (!isGameFinished && starButtons.Count > 0)
        {
            // Sıradaki hedef yıldızı bul
            GameObject targetStar = starButtons[currentIndex];

            // Matematik kullanarak "Nefes Alma" efekti yap
            // pulseSpeed değişkeni artık GameManager'dan gelen veriye göre değişiyor!
            float scale = 1.0f + Mathf.PingPong(Time.time * pulseSpeed, pulseSize);
            targetStar.transform.localScale = new Vector3(scale, scale, 1);
        }
    }

    public void StarClicked(GameObject clickedStar)
    {
        if (isGameFinished) return;

        // Doğru yıldıza mı bastı?
        if (clickedStar == starButtons[currentIndex])
        {
            // ÖNCEKİ YILDIZI NORMALE DÖNDÜR
            clickedStar.transform.localScale = Vector3.one;

            // Eğer bu ilk değilse çizgi çek
            if (currentIndex > 0)
            {
                GameObject prevStar = starButtons[currentIndex - 1];
                DrawLineBetween(prevStar, clickedStar);
            }

            Debug.Log("Doğru Yıldız! ✨");
            currentIndex++;

            // Hepsi bitti mi?
            if (currentIndex >= starButtons.Count)
            {
                FinishGame();
            }
        }
    }

    void DrawLineBetween(GameObject startObj, GameObject endObj)
    {
        GameObject newLine = Instantiate(linePrefab, lineContainer);

        Vector3 posA = startObj.transform.position;
        Vector3 posB = endObj.transform.position;

        float distance = Vector3.Distance(posA, posB);
        Vector3 direction = (posB - posA).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        RectTransform rt = newLine.GetComponent<RectTransform>();
        rt.position = posA;
        rt.rotation = Quaternion.Euler(0, 0, angle);

        // Çizgiyi biraz daha uzun yap (Boşluk kalmasın)
        float extraLength = 50f;
        rt.sizeDelta = new Vector2(distance + extraLength, rt.sizeDelta.y);

        newLine.transform.SetAsFirstSibling();
    }

    void FinishGame()
    {
        isGameFinished = true;
        // Son yıldızı da normale döndür
        if (starButtons.Count > 0)
            starButtons[starButtons.Count - 1].transform.localScale = Vector3.one;

        Debug.Log("Takımyıldız Tamam! 🌌");
        EconomyManager.Instance.AddStars(reward);
    }

    public void ResetGame()
    {
        currentIndex = 0;
        isGameFinished = false;

        // Tüm yıldızların boyutunu sıfırla
        foreach (var star in starButtons)
        {
            star.transform.localScale = Vector3.one;
        }

        // Eski çizgileri temizle
        if (lineContainer != null)
        {
            foreach (Transform child in lineContainer)
            {
                if (child.name.Contains("Clone")) Destroy(child.gameObject);
            }
        }
    }
}