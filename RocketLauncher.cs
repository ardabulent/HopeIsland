using UnityEngine;
using UnityEngine.UI;

public class RocketLauncher : MonoBehaviour
{
    [Header("Ayarlar")]
    public GameObject rocketPrefab; // Bizim roket kalýbý
    public Transform spawnPoint;    // Nereden çýkacak? (Ekranýn altý)
    public float flySpeed = 500f;   // Ne kadar hýzlý uçsun?
    public float spawnInterval = 8f; // Kaç saniyede bir çýksýn?

    private float timer;

    void Start()
    {
        // Oyun baþlar baþlamaz hemen bir tane yollasýn mý? Evetse:
        SpawnRocket();
    }

    void Update()
    {
        // Zamanlayýcý çalýþsýn
        timer += Time.deltaTime;

        // Süre dolduysa roket fýrlat
        if (timer >= spawnInterval)
        {
            SpawnRocket();
            // Bir sonraki fýrlatmayý biraz rastgele yapalým (8 ile 12 sn arasý)
            timer = 0;
            spawnInterval = Random.Range(8f, 15f);
        }
    }

    void SpawnRocket()
    {
        if (rocketPrefab == null || spawnPoint == null) return;

        // 1. Roketi Yarat (Canvas'ýn içinde yaratmasý için parent olarak transform veriyoruz)
        GameObject newRocket = Instantiate(rocketPrefab, spawnPoint.position, Quaternion.identity, transform);

        // 2. Roketin boyutunu düzelt (Bazen bozulabiliyor)
        newRocket.transform.localScale = Vector3.one;

        // 3. Rokete hareket kodu ekle (Uç ve Yok Ol)
        // Normalde ayrý script yazarýz ama pratik olsun diye buraya "geçici component" ekliyoruz.
        newRocket.AddComponent<RocketMover>().speed = flySpeed;
    }
}

// --- MÝNÝK YARDIMCI SCRÝPT (Roketin Kendisinde Çalýþacak) ---
public class RocketMover : MonoBehaviour
{
    public float speed;

    void Update()
    {
        // Yukarý uç
        transform.Translate(Vector3.up * speed * Time.deltaTime);

        // Ekranýn çok üstüne çýktýysa (Yükseklik > 2500 gibi) yok et
        // (Canvas ayarýna göre bu sayý deðiþebilir, deneyerek bul)
        if (transform.localPosition.y > 2000)
        {
            Destroy(gameObject);
        }
    }
}