using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BreathingMeditation : MonoBehaviour
{
    [Header("--- AYICIK GÖRSELLERÝ (Sýrayla 4 Kare) ---")]
    public Image ayicikImage;         // Ekranda gördüðümüz ayýcýk objesi
    public Sprite[] ayicikKareleri;   // Böldüðün 4 görseli buraya atacaðýz

    [Header("--- GÖRSEL SAYAÇ ---")]
    public Image ritimCemberi;        // Etrafýndaki dolan çember

    [Header("--- NEFES RÝTMÝ (Saniye) ---")]
    public float nefesAl = 4f;
    public float nefesTut = 2f;
    public float nefesVer = 4f;

    void OnEnable()
    {
        // Panel her görünür olduðunda (açýldýðýnda) nefes döngüsü baþtan baþlasýn!
        StartCoroutine(NefesDongusu());
    }

    IEnumerator NefesDongusu()
    {
        // Bu döngü çocuk çýkana kadar sonsuza dek çalýþýr
        while (true)
        {
            // --- 1. AÞAMA: NEFES AL (Çember dolar - 4 Saniye) ---
            float gecenZaman = 0;

            // Çember dolmaya baþladýðý an: 2. KARE (Eller göbekte, nefes alýyor)
            ayicikImage.sprite = ayicikKareleri[1];

            while (gecenZaman < nefesAl)
            {
                gecenZaman += Time.deltaTime;
                float oran = gecenZaman / nefesAl;

                // Çemberi doldur
                if (ritimCemberi != null) ritimCemberi.fillAmount = oran;
                yield return null;
            }

            // --- 2. AÞAMA: NEFES TUT (Çember tam dolu - 2 Saniye) ---
            // Çember dolduðu an: 3. KARE (Þiþmiþ ve gülümsüyor)
            ayicikImage.sprite = ayicikKareleri[2];
            if (ritimCemberi != null) ritimCemberi.fillAmount = 1f; // Çemberin tam dolu olduðundan emin olalým

            yield return new WaitForSeconds(nefesTut);

            // --- 3. AÞAMA: NEFES VER (Çember boþalýr - 4 Saniye) ---
            gecenZaman = 0;

            // Çember boþalmaya baþladýðý an: 4. KARE (Bulut üflüyor)
            ayicikImage.sprite = ayicikKareleri[3];

            while (gecenZaman < nefesVer)
            {
                gecenZaman += Time.deltaTime;
                float oran = gecenZaman / nefesVer;

                // Çemberi boþalt
                if (ritimCemberi != null) ritimCemberi.fillAmount = 1f - oran;
                yield return null;
            }

            // --- 4. AÞAMA: DÝNLENME (1 Saniye Bekleme) ---
            // Çember tamamen boþaldýðý an: 1. KARE (Ýlk baþtaki gözleri kýsýk dinlenme hali)
            ayicikImage.sprite = ayicikKareleri[0];
            if (ritimCemberi != null) ritimCemberi.fillAmount = 0f; // Çemberin sýfýrlandýðýndan emin olalým

            yield return new WaitForSeconds(1f);
        }
    }
}