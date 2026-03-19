using UnityEngine;
using UnityEngine.EventSystems; // UI Sürükleme iþlemleri için þart
using UnityEngine.UI;

public class DraggablePlanet : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("--- HEDEF GÖLGE ---")]
    public RectTransform targetShadow;   // Bu gezegen hangi gölgeye oturacak?
    public float snapDistance = 75f;     // Ne kadar yaklaþýnca "cuk" diye otursun?

    [Header("--- OYUN YÖNETÝCÝSÝ ---")]
    public PlanetGameManager gameManager;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Canvas canvas;
    private Vector2 startPosition;
    private bool isLocked = false;       // Yerine oturdu mu?

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        canvas = GetComponentInParent<Canvas>();
        startPosition = rectTransform.anchoredPosition; // Baþlangýç yerini ezberle
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLocked) return; // Yerine oturduysa bir daha kýmýldamasýn
        canvasGroup.alpha = 0.8f; // Sürüklerken biraz þeffaf olsun, gölgeyi görelim
        canvasGroup.blocksRaycasts = false; // Altýndaki objeleri týklanabilir kýl
        transform.SetAsLastSibling(); // Sürüklenen gezegeni en öne al
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLocked) return;
        // Fareyi/Parmaðý takip et
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isLocked) return;
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Býrakýldýðý yer ile hedef gölge arasýndaki mesafeyi ölçüyoruz
        float distance = Vector2.Distance(rectTransform.anchoredPosition, targetShadow.anchoredPosition);

        if (distance <= snapDistance)
        {
            // DOÐRU YER! Cuk diye oturt.
            rectTransform.anchoredPosition = targetShadow.anchoredPosition;
            isLocked = true;

            // Yýldýz uçurmasý için Menajere haber ver!
            gameManager.PlanetPlacedCorrectly(rectTransform.position);
        }
        else
        {
            // YANLIÞ YER! Yaylanarak eski yerine dönsün.
            rectTransform.anchoredPosition = startPosition;
        }
    }

    // Oyun sýfýrlandýðýnda gezegenleri baþa almak için
    public void ResetPlanet()
    {
        isLocked = false;
        rectTransform.anchoredPosition = startPosition;
    }
}