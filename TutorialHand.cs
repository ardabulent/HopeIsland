using UnityEngine;
using UnityEngine.UI;

public class TutorialHand : MonoBehaviour
{
    [Header("Hedefler")]
    public RectTransform startPoint; // Baþlangýç (Tohum Butonu)
    public RectTransform endPoint;   // Bitiþ (Toprak Çukuru)

    [Header("Ayarlar")]
    public float speed = 2.0f;       // Elin hýzý

    private Vector3 startPos;
    private Vector3 endPos;

    void Start()
    {
        // Oyun baþlayýnca hedeflerin konumunu hafýzaya al
        if (startPoint != null) startPos = startPoint.position;
        if (endPoint != null) endPos = endPoint.position;
    }

    void Update()
    {
        // Eðer hedefler yoksa çalýþma (Hata vermesin)
        if (startPoint == null || endPoint == null) return;

        // Zamanla 0 ile 1 arasýnda gidip gelen bir sayý üret (Döngü)
        // Repeat: 0'dan baþlar 1'e gider, sonra küt diye 0'a döner.
        float progress = Mathf.Repeat(Time.time * speed, 1f);

        // Eli iki nokta arasýnda yürüt
        transform.position = Vector3.Lerp(startPos, endPos, progress);

        // EKSTRA CÝLA: El hedefe yaklaþýnca biraz küçülsün (Týklama hissi)
        if (progress > 0.8f)
        {
            transform.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.8f, (progress - 0.8f) * 5);
        }
        else
        {
            transform.localScale = Vector3.one; // Normale dön
        }
    }

    // Bu fonksiyonu çocuk ekrana dokununca çaðýracaðýz
    public void HideHand()
    {
        gameObject.SetActive(false); // Eli kapat
    }
}