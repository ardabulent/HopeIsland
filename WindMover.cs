using UnityEngine;

public class WindMover : MonoBehaviour
{
    public float hiz = 150f; // Rüzgarýn soldan saða akma hýzý
    public float yasamSuresi = 10f; // 10 saniye sonra silinip yok olsun ki oyunu kastýrmasýn

    private RectTransform rect;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        // Obje doðduktan 10 saniye sonra kendini imha etsin (Temizlik imandandýr!)
        Destroy(gameObject, yasamSuresi);
    }

    void Update()
    {
        // Her saniye rüzgarý saða doðru kaydýr
        rect.anchoredPosition += Vector2.right * hiz * Time.deltaTime;
    }
}