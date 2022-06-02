using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;


public class ProjectileSpawn : MonoBehaviour
{
    [SerializeField]
    private int _spawnAmount = 0;
    private ObjectPool<Projectile> pool;
    [SerializeField]
    private Transform spawnTransform;
    [SerializeField]
    private Projectile _projectilePrefab;
    private float spawnTimer = 0.0f;
    [SerializeField]
    private float spawnTime = 0.0f;
    // Start is called before the first frame update
    void Start()
    {
        pool = new ObjectPool<Projectile>(() =>
       {
           return Instantiate(_projectilePrefab);
       }, projectile =>
       projectile.gameObject.SetActive(true),
       projectile =>
       projectile.gameObject.SetActive(false),
       projectile =>
       Destroy(projectile),
       false, _spawnAmount, _spawnAmount + 20);
    }
    private void Update()
    {
        if (spawnTimer >= spawnTime)
        {
            Spawn();
            spawnTimer = 0.0f;
        }
        else
        {
            spawnTimer += Time.deltaTime;
        }
    }
    private void Spawn()
    {
        var obj = pool.Get();
        obj.transform.position = spawnTransform.position;
        obj.transform.forward = spawnTransform.forward;
        obj.InitProjectile(KillProjectile);
    }
    private void KillProjectile(Projectile obj)
    {
        pool.Release(obj);
    }
}
