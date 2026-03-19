using UnityEngine;

public class WorldSwitcher : MonoBehaviour
{
    public Transform cameraTransform; // Main Camera'yý sürükle
    public float world2PositionX = 15f; // 2. Resmin X konumu kaçsa onu yaz
    public float speed = 5f;

    private bool isMoving = false;
    private float targetX;

    void Start()
    {
        targetX = cameraTransform.position.x; // Baþlangýçta olduðumuz yer
    }

    void Update()
    {
        // Kamerayý hedefe doðru yumuþakça kaydýr
        if (Mathf.Abs(cameraTransform.position.x - targetX) > 0.1f)
        {
            float newX = Mathf.Lerp(cameraTransform.position.x, targetX, Time.deltaTime * speed);
            cameraTransform.position = new Vector3(newX, cameraTransform.position.y, -10);
        }
    }

    // Butona basýnca bu çalýþacak
    public void GoToNextWorld()
    {
        targetX = world2PositionX; // Hedefi deðiþtir
    }
}