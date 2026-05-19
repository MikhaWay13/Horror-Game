using UnityEngine;
using UnityEngine.InputSystem;

public class Movement_Player : MonoBehaviour
{


    public InputAction move;
    public float speed = 5f;

    private void OnEnable()
    {
        move.Enable();
    }

    private void OnDisable()
    {
        move.Disable();
    }

    void Update()
    {
        Vector2 input = move.ReadValue<Vector2>();

        Vector3 movement = new Vector3(input.x, 0, input.y);

        transform.position += movement * speed * Time.deltaTime;
    }
}

