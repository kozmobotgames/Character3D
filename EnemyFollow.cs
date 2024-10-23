using System.Collections;
using System.Collections.Generic;
using System.Net;
using UnityEngine;
using UnityEngine.AI;
public class EnemyFollow : MonoBehaviour
{
    public NavMeshAgent enemy;
    public Transform player;

    [SerializeField] private float timer = 5;
    private float bulletTime;

    public GameObject enemyBullet;
    public Transform spawnPoint;
    public float enemySpeed;

    public int scoreMilestone;
    public int maximumScoreMilestone;

    public Animator anim;
    public float range;

    public float timeRandomMin;
    public float timeRandomMax;

    // Start is called before the first frame update
    void Start()
    {
        scoreMilestone = 0;
    }

    // Update is called once per frame
    void Update()
    {
        anim.SetBool("isWalking", false);
        float distance = range;
        float dist = Vector3.Distance(enemy.transform.position, player.position);

        if(dist <= distance)
        {
            anim.SetBool("isWalking", true);
            enemy.SetDestination(player.position);
            Invoke("setEnemy", Random.Range(timeRandomMin, timeRandomMax));
        }
        
    }

    void setEnemy()
    {
        ShootAtPlayer();
    }

    void ShootAtPlayer()
    {
        bulletTime -= Time.deltaTime;

        if (bulletTime > 0) return;

        bulletTime = timer;

        GameObject bulletObj = Instantiate(enemyBullet, spawnPoint.transform.position, spawnPoint.transform.rotation) as GameObject;
        Rigidbody bulletRig = bulletObj.GetComponent<Rigidbody>();
        bulletRig.AddForce(bulletRig.transform.forward * enemySpeed);
        Destroy(bulletObj, 5f);

        scoreMilestone = ScoreManager.scoreCount;

        if(scoreMilestone >= maximumScoreMilestone && timer > 0.5f)
        {
            enemy.speed += 0.1f;
            timer -= 0.02f;
        }
    }
}
