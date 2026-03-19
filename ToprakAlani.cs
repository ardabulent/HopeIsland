using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ToprakAlani : MonoBehaviour, IDropHandler
{
    private bool doluMu = false; // Buraya daha önce aðaç dikildi mi?
    private SanalBahceManager bahceManager;

    void Start()
    {
        // Manager'ý otomatik bul, seni tek tek sürüklemekten kurtarýr!
        bahceManager = FindObjectOfType<SanalBahceManager>();
    }

    // Üzerine bir þey (Aðaç) býrakýldýðýnda çalýþan kod
    public void OnDrop(PointerEventData eventData)
    {
        if (doluMu) return; // Zaten aðaç varsa hiçbir þey yapma

        // Býrakýlan obje gerçekten bizim aðaçlardan biri mi?
        if (eventData.pointerDrag != null)
        {
            SuruklenecekAgac birakilanAgac = eventData.pointerDrag.GetComponent<SuruklenecekAgac>();

            if (birakilanAgac != null)
            {
                // Parasý yetiyorsa aðacý dik!
                if (bahceManager.AgacSatinAl(birakilanAgac.agacMaliyeti))
                {
                    AgaciDik(birakilanAgac.GetComponent<Image>().sprite);
                }
            }
        }
    }

    void AgaciDik(Sprite agacGorseli)
    {
        // 1. Yeni bir Image objesi yarat
        GameObject dikilenAgac = new GameObject("DikilenAgac");
        dikilenAgac.transform.SetParent(this.transform); // Topraðýn içine koy

        // --- KANKA'NIN HAYAT KURTARAN ÖLÇEKLENDÝRME DÜZELTMESÝ ---
        // Sürüklenen aðaç markette büyük olabilir, ama topraða dikilirken scale'ini resetlemeliyiz.
        dikilenAgac.transform.localScale = Vector3.one; // (1,1,1) yap

        // 2. RectTransform ayarlarý (Bulunduðu karenin içine sýðdýrmak için)
        RectTransform rect = dikilenAgac.AddComponent<RectTransform>();

        // Anchors'ý Stretch (Her yöne yasla) yapýyoruz (0,0'dan 1,1'e)
        rect.anchorMin = Vector2.zero; // Sol alt corner
        rect.anchorMax = Vector2.one;  // Sað üst corner

        // Offsets'i (Kenar boþluklarýný) sýfýrla ki tam sýðsýn
        rect.offsetMin = Vector2.zero; // left, bottom
        rect.offsetMax = Vector2.zero; // right, top

        // 3. Resmini ayarla
        Image img = dikilenAgac.AddComponent<Image>();
        img.sprite = agacGorseli;

        // --- KRÝTÝK DEÐÝÞÝKLÝK ---
        // ESKÝ: img.SetNativeSize(); // Bu satýrý siliyoruz! Dev gibi yapýyordu.

        // YENÝ: Aðacýn þeklini bozmadan sýðdýran sihirli bileþen
        AspectRatioFitter arf = dikilenAgac.AddComponent<AspectRatioFitter>();
        arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent; // Bulunduðu karenin içine sýðdýr

        // Aðaç resminin oranýný (en/boy) hesaplayýp AspectRatioFitter'a veriyoruz
        if (agacGorseli != null)
        {
            arf.aspectRatio = (float)agacGorseli.rect.width / agacGorseli.rect.height;
        }

        doluMu = true; // Artýk burasý dolu!
    }
}