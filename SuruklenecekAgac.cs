using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// RequireComponent, sen unutursan diye Unity'nin CanvasGroup'u otomatik eklemesini saðlar. Hayat kurtarýr!
[RequireComponent(typeof(CanvasGroup))]
public class SuruklenecekAgac : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Aðaç Ayarlarý")]
    public int agacMaliyeti = 5; // Unity'den her aðaç için farklý fiyat girebilirsin (5, 10, 15)

    private Canvas canvas;
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 baslangicPozisyonu;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        // En üstteki ana Canvas'ý bulur
        canvas = GetComponentInParent<Canvas>();
    }

    // 1. Parmaðý bastýrýp sürüklemeye baþladýðý an
    public void OnBeginDrag(PointerEventData eventData)
    {
        baslangicPozisyonu = rectTransform.anchoredPosition; // Eski yerini ezberle
        canvasGroup.alpha = 0.7f; // Sürüklerken biraz saydamlaþsýn
        canvasGroup.blocksRaycasts = false; // ALTINDAKÝ TOPRAÐI GÖREBÝLMEK ÝÇÝN ÞART!
    }

    // 2. Parmaðý kaydýrdýðý sürece
    public void OnDrag(PointerEventData eventData)
    {
        // Aðacý parmakla beraber hareket ettir
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    // 3. Parmaðý ekrandan çektiði an
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f; // Rengi normale dönsün
        canvasGroup.blocksRaycasts = true; // Tekrar týklanabilir olsun

        // Aðaç marketteki orijinal yerine GERÝ DÖNSÜN (Kopyasý toprakta kalacak)
        rectTransform.anchoredPosition = baslangicPozisyonu;
    }
}