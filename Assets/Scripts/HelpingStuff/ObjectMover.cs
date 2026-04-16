using UnityEngine;

public class ObjectMover : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotateSpeed = 120f;

    public SwordAttackController attackController;

    void Update()
    {
        if (attackController != null && attackController.IsAttacking)
            return;

        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        float up = 0f;
        if (Input.GetKey(KeyCode.E)) up = 1f;
        if (Input.GetKey(KeyCode.Q)) up = -1f;

        Vector3 move = new Vector3(h, up, v);
        transform.Translate(move * moveSpeed * Time.deltaTime, Space.World);

        float yaw = 0f;
        if (Input.GetKey(KeyCode.LeftArrow)) yaw = -1f;
        if (Input.GetKey(KeyCode.RightArrow)) yaw = 1f;

        float pitch = 0f;
        if (Input.GetKey(KeyCode.UpArrow)) pitch = -1f;
        if (Input.GetKey(KeyCode.DownArrow)) pitch = 1f;

        transform.Rotate(Vector3.up, yaw * rotateSpeed * Time.deltaTime, Space.World);
        transform.Rotate(Vector3.right, pitch * rotateSpeed * Time.deltaTime, Space.Self);
    }
}