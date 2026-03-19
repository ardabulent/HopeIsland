using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // KRİTİK: Bunu eklemelisin

public class Balloon : MonoBehaviour, IPointerDownHandler // Arayüzü ekledik
{
    public float speed = 250f;
    public GameObject explosionPrefab;
    public AudioClip popSound;

    // UI Tıklamasını algılayan fonksiyon
    public void OnPointerDown(PointerEventData eventData)
    {
        PopBalloon(true);
    }

    void Update()
    {
        float currentSpeed = speed;

        // JÜRİ İÇİN AGRESİF HIZ FARKI
        if (GameControl.Difficulty == "Zor")
        {
            currentSpeed *= 3.5f; // 5 tane isabet yaparsa balonlar gerçekten 'kaçıyor' gibi olsun
        }
        else if (GameControl.Difficulty == "Kolay")
        {
            currentSpeed *= 0.3f; // Panele basarsa oyun aşırı yavaşlasın (Kontrol hissi)
        }

        transform.Translate(Vector3.up * currentSpeed * Time.deltaTime);
        if (transform.localPosition.y > 1000) Destroy(gameObject);
    }

    public void PopBalloon(bool isCorrectBalloon = true)
    {
        if (popSound != null) AudioSource.PlayClipAtPoint(popSound, Camera.main.transform.position, 1.0f);

        BalloonSpawner spawner = FindFirstObjectByType<BalloonSpawner>();
        if (spawner != null) spawner.OnBalloonClicked(isCorrectBalloon);

        if (isCorrectBalloon && EconomyManager.Instance != null) EconomyManager.Instance.AddStars(10);

        if (explosionPrefab != null)
        {
            GameObject boom = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            boom.transform.SetParent(transform.parent); // UI içinde görünmesi için parent'a bağla
            Destroy(boom, 1.0f);
        }
        Destroy(gameObject);
    }
}