using UnityEngine;

public class MoveCloud : MonoBehaviour
{
    public float speed = 0.5f;   // Ne kadar hýzlý gitsin?
    public float resetPos = 10f; // Ekranýn neresinden çýkýnca baþa dönsün?
    public float startPos = -10f;// Baþa dönünce nereden baþlasýn?

    void Update()
    {
        // Saða doðru git
        transform.Translate(Vector3.right * speed * Time.deltaTime);

        // Eðer çok uzaða gittiyse (ekrandan çýktýysa)
        if (transform.position.x > resetPos)
        {
            // En baþa (sol tarafa) geri ýþýnla
            Vector3 newPos = transform.position;
            newPos.x = startPos;
            transform.position = newPos;
        }
    }
}