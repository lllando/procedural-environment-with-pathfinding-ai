using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcBulletController : MonoBehaviour
{
    private Rigidbody rb;

    private GameObject shotBy;

    public GameObject ShotBy => shotBy;

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
        if (other.CompareTag("NPC"))
            return;

        if (other.CompareTag("PlayerBullet"))
            return;
        
        Destroy(this.gameObject);
    }

    public void SetShooter(GameObject shooter)
    {
        shotBy = shooter;
    }
}
