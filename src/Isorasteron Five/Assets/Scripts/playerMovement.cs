using System;
using Assets.Scripts.Projectiles;
using UnityEngine;

public class playerMovement : MonoBehaviour
{
    public Vector3 direct;
    public GameObject legsObject;
    public GameObject torsoObject;
    public Vector3 torsoOgRot;
    public GameObject controlVector;
    public GameObject anchor;
    public GameObject camera;
    public ProjectileEmitter weaponEmitter;
    public bool up, left, down, right;
    public float speed;
    public float rotationSpeed;
    public bool moveKeysActivated, moveKeysCanRotate, canAttack;
    public RaycastHit hit;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        speed = 250f;
        rotationSpeed = 10f;
        moveKeysActivated = true;
        moveKeysCanRotate = true;
        canAttack = true;
        torsoOgRot = torsoObject.transform.eulerAngles;

        // Necessary so the projectile can't hit the player
        weaponEmitter.SetParent(gameObject);
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

        if (Input.GetMouseButtonDown(0) && canAttack)
        {
            weaponEmitter.ToggleFire(true);
        }

        if (Input.GetMouseButtonUp(0))
        {
            weaponEmitter.ToggleFire(false);
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

        Debug.Log(Input.mousePosition);
        Ray ray = camera.GetComponent<Camera>().ScreenPointToRay(Input.mousePosition);
        bool rayCast = Physics.Raycast(ray, out hit);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        torsoObject.transform.LookAt(hit.point);
        torsoObject.transform.eulerAngles = new Vector3(torsoOgRot.x, torsoObject.transform.eulerAngles.y, torsoOgRot.z);
        if (up || down || left || right)
        {
            legsObject.transform.rotation = Quaternion.Slerp(legsObject.transform.rotation, controlVector.transform.rotation, Time.fixedDeltaTime * rotationSpeed);
            Vector3 direct = new Vector3(legsObject.transform.forward.x * speed * Time.fixedDeltaTime, 0, legsObject.transform.forward.z * speed * Time.fixedDeltaTime);
            gameObject.transform.GetComponent<Rigidbody>().linearVelocity = direct;
        }
        else
        {
            gameObject.transform.GetComponent<Rigidbody>().linearVelocity /= 4;
        }
    }
}
