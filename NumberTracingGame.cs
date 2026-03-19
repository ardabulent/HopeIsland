using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// IDragHandler'lar sayesinde parmak hareketlerini tam ekranda yakalýyoruz
public class NumberTracingGame : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [Header("--- PANEL VE HARÝTA GEÇÝÞLERÝ ---")]
    public GameObject mapPanel;
    public GameObject tracingGamePanel;

    [Header("--- ÝZ SÜRME AYARLARI ---")]
    public RectTransform tracingArea;     // Ekrandaki þeffaf algýlama paneli
    public Image full_Image;             // image_25.png'daki tam, kalýn pastel sayý görseli (maskenin altýnda kalacak)
    public Image path_Image;             // Senin attýðýn o kesik çizgili yörünge görseli (hep görünür)
    public RectTransform maskObject;      // "Reveal" maskesini yöneten obje (parmaðýn ucuna takacaðýz)
    public RectTransform[] waypoints;     // Sayýnýn üzerindeki görünmez kontrol noktalarý (Sýrayla)
    public float snapDistance = 70f;      // Çocuðun parmaðý noktaya ne kadar yaklaþýrsa kabul edilsin?

    [Header("--- YILDIZ SÝSTEMÝ ---")]
    public TextMeshProUGUI starText;
    public RectTransform starCounterIcon;
    public GameObject flyingStarPrefab;

    private int currentWaypointIndex = 0;
    private int totalStars = 0;
    private bool isCompleted = false;
    private Vector2 maskStartPosition;

    void Start()
    {
        // --- VELÝ PANELÝ TEMELÝ: ORTAK HAVUZDAN YILDIZLARI ÇEK ---
        totalStars = PlayerPrefs.GetInt("Global_TotalStars", 0);
        UpdateStarUI();

        // Mask objesinin baþlangýç pozisyonunu ezberle (reseti için)
        maskStartPosition = maskObject.anchoredPosition;
    }

    public void OpenGame()
    {
        mapPanel.SetActive(false);
        tracingGamePanel.SetActive(true);

        // --- MERKEZ BANKASINDAN GÜNCEL YILDIZLARI ÇEK ---
        // Baþka oyundan gelindiyse güncel yýldýzý anýnda ekrana yansýt!
        totalStars = PlayerPrefs.GetInt("Global_TotalStars", 0);
        UpdateStarUI();

        ResetGame();
    }

    public void CloseGame()
    {
        StopAllCoroutines();
        tracingGamePanel.SetActive(false);
        mapPanel.SetActive(true);
    }

    // 1. ÇOCUK EKRANA DOKUNDUÐUNDA
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isCompleted) return;
    }

    // 2. ÇOCUK PARMAÐINI KAYDIRDIKÇA (GÜNCELLENMÝÞ EFSANE VERSÝYON)
    public void OnDrag(PointerEventData eventData)
    {
        if (isCompleted) return;

        // Maskeyi/Iþýðý parmaðýn olduðu yere taþý
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            tracingArea, eventData.position, eventData.pressEventCamera, out Vector2 localPointerPosition);
        maskObject.anchoredPosition = localPointerPosition;

        // Sýradaki hedefe (noktaya) ne kadar yakýnýz?
        if (currentWaypointIndex < waypoints.Length)
        {
            // Noktalar farklý objenin içinde olduðu için, hepsinin konumunu "Ekrana" çevirip öyle ölçüyoruz!
            Vector2 waypointScreenPos = RectTransformUtility.WorldToScreenPoint(eventData.pressEventCamera, waypoints[currentWaypointIndex].position);
            float distance = Vector2.Distance(eventData.position, waypointScreenPos);

            // Eðer yeterince yaklaþtýysa (Hata payý 70 piksel)
            if (distance <= snapDistance)
            {
                // Deðdiðimiz noktayý GÖRÜNÜR ve SARI yapalým ki çalýþtýðýný anlayalým!
                waypoints[currentWaypointIndex].GetComponent<Image>().color = Color.yellow;

                currentWaypointIndex++; // Bir sonraki noktaya geç

                // Tüm noktalar bitti mi? KAZANDI!
                if (currentWaypointIndex >= waypoints.Length)
                {
                    GameWon();
                }
            }
        }
    }

    // 3. ÇOCUK PARMAÐINI ÇEKTÝÐÝNDE
    public void OnEndDrag(PointerEventData eventData)
    {
        if (isCompleted) return;

        // Elini çekerse maskeyi durdur ama KALDIÐI YERDEN devam edebilsin (Sýfýrlama yok!)
    }

    void GameWon()
    {
        isCompleted = true;

        // Son noktanýn pozisyonundan yýldýzý uçur!
        Vector3 startPos = waypoints[waypoints.Length - 1].position;
        StartCoroutine(FlyStarEffect(startPos, starCounterIcon.position));
    }

    public void ResetGame()
    {
        isCompleted = false;
        currentWaypointIndex = 0;
        maskObject.anchoredPosition = maskStartPosition; // Maskeyi baþa döndür

        // Tüm noktalarýn rengini baþa döndür (Eðer hata yapýnca rengini deðiþtiriyorsak)
        foreach (var wp in waypoints)
        {
            wp.GetComponent<Image>().color = new Color(1, 1, 1, 0.3f); // Yarý saydam
        }
    }

    // Klasik Yýldýz Uçurma ve Kaydetme Kodumuz
    IEnumerator FlyStarEffect(Vector3 startPos, Vector3 endPos)
    {
        GameObject flyingStar = Instantiate(flyingStarPrefab, tracingArea);
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

        // --- VELÝ PANELÝ ÝÇÝN ORTAK HAVUZA KAYIT SÝSTEMÝ ---
        totalStars++;
        PlayerPrefs.SetInt("Global_TotalStars", totalStars); // ORTAK HAVUZA KAYDET
        PlayerPrefs.Save(); // Kaydý kesinleþtir
        // --------------------------------------

        UpdateStarUI();

        // 2 saniye sonra yeni rakama (veya baþa) geç
        yield return new WaitForSeconds(2f);
        ResetGame();
    }

    void UpdateStarUI()
    {
        if (starText != null) starText.text = totalStars.ToString();
    }
}