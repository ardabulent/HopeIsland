using UnityEngine;

public class ButtonPulse : MonoBehaviour
{
    [Header("Ayarlar")]
    public float speed = 3.0f;      // Ne kadar hýzlý atacak?
    public float strength = 0.1f;   // Ne kadar büyüyecek? (0.1 = %10 büyür)

    private Vector3 initialScale;

    void Start()
    {
        // Baþlangýçtaki boyutunu hafýzaya al
        initialScale = transform.localScale;
    }

    void Update()
    {
        // Matematiksel Sinüs dalgasý ile pürüzsüz büyü/küçül (Nefes alma efekti)
        float scale = 1 + Mathf.Sin(Time.time * speed) * strength;

        // Yeni boyutu uygula
        transform.localScale = initialScale * scale;
    }
}