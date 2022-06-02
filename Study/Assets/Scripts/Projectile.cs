using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 acceleration;
    private Vector3 velocity;
    [SerializeField]
    private float speed;
    private Action<Projectile> kill;
    private float liveTimer = 0.0f;
    private Vector3 reflect;
    public void InitProjectile(Action<Projectile> killAction )
    {
        kill = killAction;
        liveTimer = 0.0f;
        velocity = Vector3.zero;
        acceleration = transform.forward;
        reflect = transform.forward;
    }
    private void Update()
    {
        if (liveTimer >= 3.0)
        {
            kill(this);
        }
        else
        {
            liveTimer += Time.deltaTime;
        }
    }
    private void FixedUpdate()
    {
        acceleration = transform.forward;
        velocity += (acceleration * speed ) * Time.deltaTime;
        transform.position += velocity * Time.deltaTime;
    }
    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint[] contacts = new ContactPoint[collision.contactCount];
            collision.GetContacts(contacts);
        Vector3 contactSum = Vector3.zero;
        for (int i = 0; i < contacts.Length; i++)
        {
             contactSum += contacts[i].normal;
        }
        Vector3 normalV = contactSum / contacts.Length;
        Vector3 reflectionVector = MathReflection.Reflect(normalV.normalized, collision.GetContact(0).point, transform.position);
        reflect = reflectionVector;
        transform.forward = reflectionVector.normalized;
        velocity = Vector3.zero;
    }
    private void OnDrawGizmos()
    {
        Handles.color = Color.black;
        Handles.DrawLine(transform.position, transform.position + reflect * 10);
    }
}
