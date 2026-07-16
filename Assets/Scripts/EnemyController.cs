using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform ball;
    [SerializeField] private float speed = 5f;

    private void Update()
    {
        if (ball == null)
            return;

        Vector3 position = transform.position;

        position.y = Mathf.MoveTowards(
            position.y,
            ball.position.y,
            speed * Time.deltaTime
        );

        position.y = Mathf.Clamp(position.y, -4f, 4f);
        transform.position = position;
    }
}