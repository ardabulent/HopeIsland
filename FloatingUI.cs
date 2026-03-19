using UnityEngine;

public class FloatingUI : MonoBehaviour
{
    [Header("Süzülme Ayarlarý")]
    public float floatSpeed = 2f;      // Ne kadar hýzlý inip kalkacak?
    public float floatStrength = 15f;  // Ne kadar yükseðe/alçaða gidecek? (Piksel cinsinden)

    private Vector3 startPos;
    private RectTransform rectTrans;

    void Start()
    {
        rectTrans = GetComponent<RectTransform>();
        // Baþlangýç pozisyonunu kaydet
        startPos = rectTrans.anchoredPosition;
    }

    void Update()
    {
        // Matematiksel Sinüs dalgasý ile yumuþak hareket hesapla
        float newY = startPos.y + Mathf.Sin(Time.time * floatSpeed) * floatStrength;

        // Yeni pozisyonu uygula
        rectTrans.anchoredPosition = new Vector3(startPos.x, newY, 0);
    }
}