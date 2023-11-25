using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBulletController : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>(); 
    }

    public void UpdateBullet(Transform lookAt)
    {
        transform.LookAt(lookAt);
        rb.velocity = transform.forward * 10f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            return;
        
        Destroy(this.gameObject);
    }
}
