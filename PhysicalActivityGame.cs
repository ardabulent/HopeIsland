using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhysicalActivityGame : MonoBehaviour
{
    [Header("--- GÖRSELLER ---")]
    public Image ayicikImage;         // Sahnedeki ayýcýðýmýz
    public Image ritimCemberi;        // Etrafýndaki sayaç çemberi

    [Header("--- AYICIK KARELERÝ (5 Adet) ---")]
    [Tooltip("0: Hazýr, 1: Kollar, 2: Zýplama, 3: Dönüþ, 4: Tek Ayak")]
    public Sprite[] ayicikKareleri;

    [Header("--- SPOR AYARLARI ---")]
    public float hareketSuresi = 5f;  // Her hareket kaç saniye sürsün?
    public float dinlenmeSuresi = 2f; // Hareketler arasý kaç saniye mola?
    public float animasyonHizi = 0.5f; // Ayýcýk saniyede kaç kere hareket etsin?

    void OnEnable()
    {
        // Kanka, paneli her açtýðýmýzda spor baþtan taptaze baþlasýn!
        StartCoroutine(SporDongusu());
    }

    IEnumerator SporDongusu()
    {
        // 4 farklý spor hareketimiz var (Ýndeks 1, 2, 3 ve 4)
        int[] hareketSirasi = { 1, 2, 3, 4 };

        while (true) // Çocuk panelden çýkana kadar döngü devam eder
        {
            foreach (int hareketIndex in hareketSirasi)
            {
                float gecenZaman = 0;

                // --- 1. AÞAMA: HAREKET ZAMANI (Çember Dolar) ---
                while (gecenZaman < hareketSuresi)
                {
                    gecenZaman += Time.deltaTime;

                    // Çemberi yavaþça dolduruyoruz
                    if (ritimCemberi != null)
                        ritimCemberi.fillAmount = gecenZaman / hareketSuresi;

                    // --- EFSANEVÝ ANÝMASYON HÝLESÝ ---
                    // Zamaný kullanarak ayýcýðý "Hazýr" (0) ve "Hareket" (hareketIndex) arasýnda sürekli deðiþtiriyoruz!
                    if (Mathf.PingPong(gecenZaman, animasyonHizi * 2) < animasyonHizi)
                    {
                        ayicikImage.sprite = ayicikKareleri[hareketIndex]; // Spor yap!
                    }
                    else
                    {
                        ayicikImage.sprite = ayicikKareleri[0]; // Hazýr ola geç!
                    }

                    yield return null;
                }

                // --- 2. AÞAMA: DÝNLENME MOLASI ---
                ayicikImage.sprite = ayicikKareleri[0]; // Mola! Hazýr olda bekle.
                if (ritimCemberi != null)
                    ritimCemberi.fillAmount = 0f; // Çemberi sýfýrla ki çocuk molayý anlasýn.

                yield return new WaitForSeconds(dinlenmeSuresi);
            }
        }
    }
}