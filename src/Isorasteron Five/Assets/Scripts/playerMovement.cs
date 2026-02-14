using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public Vector3 direct;
    public GameObject controlObject;
    public GameObject controlVector;
    public GameObject anchor;
    public bool up, left, down, right;
    public float speed;
    public float rotationSpeed;
    public bool moveKeysActivated, moveKeysCanRotate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speed = 100f;
        rotationSpeed = 10f;
        moveKeysActivated = true;
        moveKeysCanRotate = true;
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.W) && moveKeysActivated)
        {
            up = true;
        }
        else if (Input.GetKeyUp(KeyCode.W) && moveKeysActivated)
        {
            up = false;
        }

        if (Input.GetKey(KeyCode.A) && moveKeysActivated)
        {
            left = true;
        }
        else if (Input.GetKeyUp(KeyCode.A) && moveKeysActivated)
        {
            left = false;
        }

        if (Input.GetKey(KeyCode.S) && moveKeysActivated)
        {
            down = true;
        }
        else if (Input.GetKeyUp(KeyCode.S) && moveKeysActivated)
        {
            down = false;
        }

        if (Input.GetKey(KeyCode.D) && moveKeysActivated)
        {
            right = true;
        }
        else if (Input.GetKeyUp(KeyCode.D) && moveKeysActivated)
        {
            right = false;
        }

        if (left && !(up || down) && moveKeysCanRotate)
        {
            controlVector.transform.eulerAngles = new Vector3(controlVector.transform.rotation.eulerAngles.x, anchor.transform.rotation.eulerAngles.y - 90f, controlVector.transform.rotation.eulerAngles.z);
        }
        if (left && down && moveKeysCanRotate)
        {
            controlVector.transform.eulerAngles = new Vector3(controlVector.transform.rotation.eulerAngles.x, anchor.transform.rotation.eulerAngles.y - 135f, controlVector.transform.rotation.eulerAngles.z);
        }
        if (right && !(up || down) && moveKeysCanRotate)
        {
            controlVector.transform.eulerAngles = new Vector3(controlVector.transform.rotation.eulerAngles.x, anchor.transform.rotation.eulerAngles.y - 270f, controlVector.transform.rotation.eulerAngles.z);
        }
        if (right && up && moveKeysCanRotate)
        {
            controlVector.transform.eulerAngles = new Vector3(controlVector.transform.rotation.eulerAngles.x, anchor.transform.rotation.eulerAngles.y - 315f, controlVector.transform.rotation.eulerAngles.z);
        }

        if (!(left || right) && down && moveKeysCanRotate)
        {
            controlVector.transform.eulerAngles = new Vector3(controlVector.transform.rotation.eulerAngles.x, anchor.transform.rotation.eulerAngles.y - 180f, controlVector.transform.rotation.eulerAngles.z);
        }
        if (!(left || right) && up && moveKeysCanRotate)
        {
            controlVector.transform.eulerAngles = new Vector3(controlVector.transform.rotation.eulerAngles.x, anchor.transform.rotation.eulerAngles.y - 360f, controlVector.transform.rotation.eulerAngles.z);
        }
        if (right && down && moveKeysCanRotate)
        {
            controlVector.transform.eulerAngles = new Vector3(controlVector.transform.rotation.eulerAngles.x, anchor.transform.rotation.eulerAngles.y - 225f, controlVector.transform.rotation.eulerAngles.z);
        }
        if (left && up && moveKeysCanRotate)
        {
            controlVector.transform.eulerAngles = new Vector3(controlVector.transform.rotation.eulerAngles.x, anchor.transform.rotation.eulerAngles.y - 45f, controlVector.transform.rotation.eulerAngles.z);
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        controlObject.transform.rotation = Quaternion.Slerp(controlObject.transform.rotation, controlVector.transform.rotation, Time.fixedDeltaTime * rotationSpeed);
        Vector3 direct = new Vector3(controlObject.transform.forward.x * speed * Time.fixedDeltaTime, 0, controlObject.transform.forward.z * speed * Time.fixedDeltaTime);
        gameObject.transform.GetComponent<Rigidbody>().linearVelocity = direct;
    }
}
