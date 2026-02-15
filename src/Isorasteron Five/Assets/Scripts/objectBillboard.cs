using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectBillboard : MonoBehaviour
{
    public GameObject target;

    // Update is called once per frame
    void Update()
    {
        gameObject.transform.LookAt(target.transform);
    }
}
