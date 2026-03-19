using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class WorldSelectionManager : MonoBehaviour
{
    public static WorldSelectionManager Instance;

    [Header("UI Ayarlarý")]
    [Tooltip("Kartlar dýþýndaki diðer her þeyin (baþlýk, arkaplan vb.) silikleþmesi için")]
    public CanvasGroup backgroundCanvasGroup;

    void Awake()
    {
        Instance = this;
    }

    // Kart týklandýðýnda bu fonksiyon çalýþýr
    public void SelectWorld(RectTransform clickedCard, string targetSceneName)
    {
        StartCoroutine(ZoomIntoCardAndLoad(clickedCard, targetSceneName));
    }

    IEnumerator ZoomIntoCardAndLoad(RectTransform card, string sceneName)
    {
        // 1. Týklanan kartý Canvas'ýn en önüne al ki diðerlerinin üstünde büyüsün
        card.SetAsLastSibling();

        // Baþlangýç deðerlerini kaydet
        Vector2 startPos = card.anchoredPosition;
        Vector3 startScale = card.localScale;

        // Hedef deðerler (Ekranýn tam ortasý ve devasa bir boyut)
        Vector2 targetPos = Vector2.zero;
        Vector3 targetScale = new Vector3(20f, 20f, 1f); // Ekraný kaplayacak kadar büyüt

        float duration = 0.8f; // Animasyonun süresi (0.8 saniye çok premium hissettirir)
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // SmoothStep ile animasyona "yavaþ baþla, hýzlan, yavaþ bitir" hissi veriyoruz (Apple tarzý)
            float t = Mathf.SmoothStep(0, 1, elapsed / duration);

            // Kartý ortaya kaydýr ve büyüt
            card.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            card.localScale = Vector3.Lerp(startScale, targetScale, t);

            // Arka planý ve diðer yazýlarý yavaþça karart/silikleþtir
            if (backgroundCanvasGroup != null)
            {
                backgroundCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            }

            yield return null; // Bir sonraki frame'i bekle
        }

        // 2. Animasyon bitti! Sahneyi yükle.
        SceneManager.LoadScene(sceneName);
    }
}