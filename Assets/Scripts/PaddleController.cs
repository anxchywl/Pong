using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [SerializeField] private float speed = 8f;
    [SerializeField] private float minY = -4f;
    [SerializeField] private float maxY = 4f;

    [SerializeField] private bool isPlayer = true;

    private void Update()
    {
        float direction = 0f;

        if (isPlayer)
        {
            if (Input.GetKey(KeyCode.W))
                direction = 1f;

            if (Input.GetKey(KeyCode.S))
                direction = -1f;
        }

        Vector3 position = transform.position;
        position.y += direction * speed * Time.deltaTime;
        position.y = Mathf.Clamp(position.y, minY, maxY);

        transform.position = position;
    }
}