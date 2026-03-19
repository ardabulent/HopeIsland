using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SheepCountingGame : MonoBehaviour
{
    [Header("--- PANEL VE HARİTA GEÇİŞLERİ ---")]
    public GameObject mapPanel;          // Ana Harita Paneli
    public GameObject sheepGamePanel;    // Bu oyunun ana paneli (Aç/Kapa yapmak için)

    [Header("--- KUZU AYARLARI ---")]
    public GameObject sheepPrefab;
    public RectTransform spawnPoint;
    public RectTransform jumpStartPoint;
    public RectTransform jumpEndPoint;
    public RectTransform exitPoint;

    public Sprite walkingSprite;
    public Sprite jumpingSprite;

    public float moveSpeed = 300f;
    public float jumpHeight = 150f;
    public float timeBetweenSheep = 1.2f;

    [Header("--- UI BAĞLANTILARI ---")]
    public GameObject buttonsContainer;
    public Button[] numberButtons;
    public TextMeshProUGUI starText;
    public RectTransform starCounterIcon;
    public GameObject flyingStarPrefab;
    public Transform sheepContainer;      // KUZULARIN DOĞACAĞI YER (Panel_SheepGame objesinin kendisi olabilir)

    private int currentSheepCount;
    private int totalStars = 0;
    private bool isInputActive = false;

    void Start()
    {
        // --- VELİ PANELİ TEMELİ: ORTAK HAVUZDAN YILDIZLARI ÇEK ---
        totalStars = PlayerPrefs.GetInt("Global_TotalStars", 0);

        UpdateStarUI();
    }

    // =========================================================
    // --- HARİTA VE OYUN GEÇİŞ FONKSİYONLARI ---
    // =========================================================

    public void OpenGame()
    {
        mapPanel.SetActive(false);
        sheepGamePanel.SetActive(true);

        // --- MERKEZ BANKASINDAN GÜNCEL YILDIZLARI ÇEK ---
        // Başka oyunda yıldız kazanıp buraya geldiyse, direkt o güncel rakamı görsün!
        totalStars = PlayerPrefs.GetInt("Global_TotalStars", 0);
        UpdateStarUI();

        // Oyuna her girildiğinde ne olur ne olmaz içeriyi bir temizleyelim
        TemizlikYap();

        StartNewRound();
    }

    public void CloseGame()
    {
        // 1. Arka planda çalışan tüm kuzu üretme ve uçan yıldız işlemlerini DURDUR!
        StopAllCoroutines();

        // 2. Ekranda donup kalan eski kuzuları ve yıldızları SİL!
        TemizlikYap();

        // 3. Oyunu gizle, haritayı aç
        sheepGamePanel.SetActive(false);
        mapPanel.SetActive(true);
    }

    // Hayalet kuzuları silen gizli silahımız
    void TemizlikYap()
    {
        // sheepContainer içindeki tüm objeleri tarıyoruz
        foreach (Transform child in sheepContainer)
        {
            // Unity kodla ürettiği objelerin sonuna "(Clone)" yazar.
            // Sadece bu klonları siliyoruz ki butonlarına vs. zarar gelmesin!
            if (child.name.Contains("(Clone)"))
            {
                Destroy(child.gameObject);
            }
        }
    }

    // =========================================================

    public void StartNewRound()
    {
        buttonsContainer.SetActive(false);
        currentSheepCount = Random.Range(1, 7);
        isInputActive = false;
        StartCoroutine(SpawnSheepRoutine());
    }

    IEnumerator SpawnSheepRoutine()
    {
        yield return new WaitForSeconds(1f);

        for (int i = 0; i < currentSheepCount; i++)
        {
            // Kuzuları sheepContainer içine yaratıyoruz
            GameObject sheep = Instantiate(sheepPrefab, sheepContainer, false);
            RectTransform sheepRect = sheep.GetComponent<RectTransform>();
            Image sheepImage = sheep.GetComponent<Image>();

            sheepRect.anchoredPosition = spawnPoint.anchoredPosition;
            sheep.transform.SetAsLastSibling();

            StartCoroutine(MoveAndJumpSheep(sheepRect, sheepImage));
            yield return new WaitForSeconds(timeBetweenSheep);
        }

        yield return new WaitForSeconds(1.5f);
        buttonsContainer.SetActive(true);
        isInputActive = true;
    }

    IEnumerator MoveAndJumpSheep(RectTransform sheepRect, Image sheepImage)
    {
        sheepImage.sprite = walkingSprite;
        while (Vector2.Distance(sheepRect.anchoredPosition, jumpStartPoint.anchoredPosition) > 10f)
        {
            sheepRect.anchoredPosition = Vector2.MoveTowards(sheepRect.anchoredPosition, jumpStartPoint.anchoredPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        sheepImage.sprite = jumpingSprite;
        Vector2 startPos = jumpStartPoint.anchoredPosition;
        Vector2 endPos = jumpEndPoint.anchoredPosition;
        float elapsed = 0;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float yOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            sheepRect.anchoredPosition = Vector2.Lerp(startPos, endPos, t) + new Vector2(0, yOffset);
            yield return null;
        }

        sheepImage.sprite = walkingSprite;
        while (Vector2.Distance(sheepRect.anchoredPosition, exitPoint.anchoredPosition) > 10f)
        {
            sheepRect.anchoredPosition = Vector2.MoveTowards(sheepRect.anchoredPosition, exitPoint.anchoredPosition, moveSpeed * Time.deltaTime);
            yield return null;
        }

        Destroy(sheepRect.gameObject);
    }

    public void OnNumberButtonClicked(int chosenNumber)
    {
        if (!isInputActive) return;
        isInputActive = false;

        if (chosenNumber == currentSheepCount)
        {
            RectTransform clickedBtnRect = numberButtons[chosenNumber - 1].GetComponent<RectTransform>();
            StartCoroutine(FlyStarEffect(clickedBtnRect.position, starCounterIcon.position));
        }
        else
        {
            Debug.Log("Yanlış saydın minik kahraman, tekrar dene!");
            buttonsContainer.SetActive(false);
            StartNewRound();
        }
    }

    IEnumerator FlyStarEffect(Vector3 startPos, Vector3 endPos)
    {
        GameObject flyingStar = Instantiate(flyingStarPrefab, sheepContainer);
        flyingStar.transform.position = startPos;
        flyingStar.transform.SetAsLastSibling();

        float elapsed = 0;
        float duration = 0.6f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = t * t * (3f - 2f * t);
            flyingStar.transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        Destroy(flyingStar);

        // --- VELİ PANELİ İÇİN ORTAK HAVUZA KAYIT SİSTEMİ ---
        totalStars++;
        PlayerPrefs.SetInt("Global_TotalStars", totalStars); // ORTAK HAVUZA KAYDET
        PlayerPrefs.Save(); // Kaydı kesinleştir
        // --------------------------------------

        UpdateStarUI();
        StartNewRound();
    }

    void UpdateStarUI()
    {
        if (starText != null) starText.text = totalStars.ToString();
    }
}