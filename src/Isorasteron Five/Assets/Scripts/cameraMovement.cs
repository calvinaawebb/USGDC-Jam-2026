using UnityEngine;

public class cameraMovement : MonoBehaviour
{
    public GameObject camera;
    public GameObject player;
    void Start()
    {
        camera.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 2f, player.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if (camera != null)
        {
            Vector3 velocity = new Vector3(0, 0, 0);
            camera.transform.position = Vector3.SmoothDamp(camera.transform.position, player.transform.position, ref velocity, 0.05f);
        }
    }
}
