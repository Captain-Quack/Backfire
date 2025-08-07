using System.Collections;
using System.Collections.Generic;
using Backfire;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;


public class BossController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    // Shooting set up
    [SerializeField] private GameObject bulletPrefab;

    [SerializeField] private Transform bulletSpawnPoint;

    [SerializeField] private float radius = 5f;
    private float bulletCount;



    private PlayerController player;



    public float bulletCooldown;
    //Spike creation
    [SerializeField] private GameObject spike1;
    [SerializeField] private GameObject spike2;
    [SerializeField] private GameObject spike3;
    [SerializeField] private GameObject spike4;
    [SerializeField] private GameObject spike5;
    [SerializeField] private GameObject spike6;
    [SerializeField] private GameObject spike7;

    [SerializeField] private GameObject spike8;

    //pre laser
    [SerializeField] private GameObject warning1;
    [SerializeField] private GameObject warning2;
    [SerializeField] private GameObject warning3;
    [SerializeField] private GameObject warning4;
    [SerializeField] private GameObject warning5;
    [SerializeField] private GameObject warning6;
    [SerializeField] private GameObject warning7;
    [SerializeField] private GameObject warning8;

    [SerializeField] private GameObject warning9;
    [SerializeField] private GameObject warning10;
    [SerializeField] private GameObject warning11;
    [SerializeField] private GameObject warning12;
    [SerializeField] private GameObject warning13;
    [SerializeField] private GameObject warning14;
    [SerializeField] private GameObject warning15;
    [SerializeField] private GameObject warning16;

    //horizontal warning shit

    [SerializeField] private GameObject warning18;
    [SerializeField] private GameObject warning19;
    [SerializeField] private GameObject warning20;
    [SerializeField] private GameObject warning21;

    [SerializeField] private GameObject laser;

    [SerializeField] private GameObject laserHorizontal;

    // stupid stuff and stuff and stuff and stuff nah its the spinny lasers we love lasers
    [SerializeField] private GameObject spinnyLaser2;
    [SerializeField] private GameObject spinnyLaser3;
    [SerializeField] private GameObject spinnyLaser4;
    [SerializeField] private GameObject spinnyLaser5;

    [SerializeField] private GameObject rotator;
    public float rotateSpeed;
    public float spinDuration;



    //post laser stuff
    private float timer;
    public float laserCooldown;
    List<GameObject> warnings = new List<GameObject>();
    List<GameObject> warnings1 = new List<GameObject>();
    private List<GameObject> warningsHorizontal = new List<GameObject>();
    List<GameObject> lasers = new List<GameObject>();

    //teleport and spike area
    List<GameObject> spikes = new List<GameObject>();
    public Vector2 teleportAreaCenter;
    public Vector2 teleportAreaSize;
    private int counter = 0;
    List<GameObject> afterSpikes = new List<GameObject>();
    public float cooldownTeleport = 2f;
    public bool teleportAttack = false;
    private bool teleportAttackReal = false;
    private int count = 0;
    public bool laserAttack = false;
    public float bossTimer;
    private int laserCount;

    public float cooldown;
    private bool attacking;
    private Vector3 initialPosition;

    List<GameObject> spinnyLasers = new List<GameObject>();

    public bool phase1 = true;
    public bool phase2 = false;
    public bool phase3 = false;
    [SerializeField] private GameObject rotate;
    [SerializeField] private GameObject orbitalBullet;
    [SerializeField] private int count2;
    [SerializeField] private GameObject gravity;
    [SerializeField] private GameObject slashRadius;
    [SerializeField] private GameObject slash;
    [SerializeField] private float safeDistance;

    public float attackDelay = 2f;
    public float spikeDelay;
    public float laserDelay = 2f;
    public float laserGoAway = 3f;
    public GameObject cursedTechniqueRed;
    public int range1;
    public int range2;
    public Animator anim;
    private bool beginning = true;
    private bool playBlackholeSound = true;
    void Start()
    {
        initialPosition = transform.position;
        //spikes being added and teleport being set
        spikes.Add(spike1);
        spikes.Add(spike2);
        spikes.Add(spike3);
        spikes.Add(spike4);
        spikes.Add(spike5);
        spikes.Add(spike6);
        spikes.Add(spike7);
        spikes.Add(spike8);
        teleportAreaCenter = new Vector2(transform.position.x, transform.position.y);
        //pre laser adding to warning
        warnings.Add(warning1);
        warnings.Add(warning2);
        warnings.Add(warning3);
        warnings.Add(warning4);
        warnings.Add(warning5);
        warnings.Add(warning6);
        warnings.Add(warning7);
        warnings.Add(warning8);
        //pre laser 2 addign to warning
        warnings1.Add(warning9);
        warnings1.Add(warning10);
        warnings1.Add(warning11);
        warnings1.Add(warning12);
        warnings1.Add(warning13);
        warnings1.Add(warning14);
        warnings1.Add(warning15);
        warnings1.Add(warning16);
        //horizontal warnigns 1 for laser

        warningsHorizontal.Add(warning18);
        warningsHorizontal.Add(warning19);
        warningsHorizontal.Add(warning20);
        warningsHorizontal.Add(warning21);
        player = FindAnyObjectByType<PlayerController>();


        spinnyLasers.Add(spinnyLaser2);
        spinnyLasers.Add(spinnyLaser3);
        spinnyLasers.Add(spinnyLaser4);
        spinnyLasers.Add(spinnyLaser5);

        rotator.SetActive(false);
        Physics2D.gravity = new Vector2(0, -9.81f);


    }


    // Update is called once per frame
    void FixedUpdate()
    {
        if (player.currentState is PlayingState && phase1)
        {
            if (!attacking && beginning)
            {
                StartCoroutine(Phase1());

            }
            else if (!attacking && !beginning)
            {
                RealPhase1();
            }
        }

        if (player.currentState is PlayingState && phase2)
        {
            if (!attacking && beginning)
            {
                StartCoroutine(Phase2());
            }
            else if (!attacking && !beginning)
            {
                RealPhase2();
            }
        }

        if (player.currentState is PlayingState && phase3)
        {
            if (!attacking && beginning)
            {
                StartCoroutine(Phase3());
            }
            else if (!attacking && !beginning)
            {
                RealPhase3();
            }
        }
        
       
    }

    private IEnumerator Phase1()
    {
        attacking = true;
        yield return new WaitForSeconds(4.5f);
        int randomVal = Random.Range(1, 4);
        if (randomVal == 1)
        {
            StartCoroutine(LaserAttack());
        }
        else if (randomVal == 2)
        {
            teleportAttackReal = true;
            StartCoroutine(Teleport());
        }
        else if (randomVal == 3)
        {
            StartCoroutine(Shooting());
        }
        else if (randomVal == 4)
        {
            StartCoroutine(gravityChange());
        }
        else if (randomVal == 5)
        {
            StartCoroutine(blackhole());
        }
        else if (randomVal == 6)
        {
            StartCoroutine(Red());
        }
        else if (randomVal == 7)
        {
            SpinnyLasers();
        }

        beginning = false;
    }

    private IEnumerator Phase2()
    {
        attacking = true;
        yield return new WaitForSeconds(4.5f);
        int randomVal = Random.Range(1, 5);
        if (randomVal == 1)
        {
            StartCoroutine(gravityChange());
        }
        else if (randomVal == 2)
        {
            teleportAttackReal = true;
            StartCoroutine(Teleport());
        }
        else if (randomVal == 3)
        {
            StartCoroutine(blackhole());
        }
        else if (randomVal == 4)
        {
            SpinnyLasers();
        }
       

        beginning = false;
    }
    private IEnumerator Phase3()
    {
        attacking = true;
        yield return new WaitForSeconds(4.5f);
        int randomVal = Random.Range(1, 6);
        if (randomVal == 1)
        {
            StartCoroutine(LaserAttack());
        }
        else if (randomVal == 2)
        {
            teleportAttackReal = true;
            StartCoroutine(Teleport());
        }
        else if (randomVal == 3)
        {
            StartCoroutine(gravityChange());
        }
        else if (randomVal == 4)
        {
            StartCoroutine(blackhole());
        }
        else if (randomVal == 5)
        {
            StartCoroutine(Red());
        }
        else if (randomVal == 6)
        {
            SpinnyLasers();
        }
        

        beginning = false;
    }

    private void RealPhase1()
    {
        attacking = true;
        int randomVal = Random.Range(range1, range2);
        if (randomVal == 1)
        {
            anim.SetTrigger("Laser");
            StartCoroutine(LaserAttack());
        }
        else if (randomVal == 2)
        {
            teleportAttackReal = true;
            StartCoroutine(Teleport());
        }
        else if (randomVal == 3)
        {
            StartCoroutine(Shooting());
        }
        else if (randomVal == 4)
        {
            StartCoroutine(gravityChange());
        }
        else if (randomVal == 5)
        {
            StartCoroutine(blackhole());
        }
        else if (randomVal == 6)
        {
            StartCoroutine(Red());
        }
        else if (randomVal == 7)
        {
            SpinnyLasers();
        }
    }

    private void RealPhase2()
    {
        attacking = true;
        int randomVal = Random.Range(range1, range2);
        if (randomVal == 1)
        {
            StartCoroutine(gravityChange());
        }
        else if (randomVal == 2)
        {
            teleportAttackReal = true;
            StartCoroutine(Teleport());
        }
        else if (randomVal == 3)
        {
            StartCoroutine(blackhole());
        }
        else if (randomVal == 4)
        {
            SpinnyLasers();
        }
    }

    private void RealPhase3()
    {
        attacking = true;
        int randomVal = Random.Range(4, 5);
        if (randomVal == 1)
        {
            StartCoroutine(LaserAttack());
        }
        else if (randomVal == 2)
        {
            teleportAttackReal = true;
            StartCoroutine(Teleport());
        }
        else if (randomVal == 3)
        {
            StartCoroutine(gravityChange());
        }
        else if (randomVal == 4)
        {
            StartCoroutine(blackhole());
        }
        else if (randomVal == 5)
        {
            StartCoroutine(Red());
        }
        else if (randomVal == 6)
        {
            SpinnyLasers();
        }
    }

    public void Phase(int phase)
    {
        if (phase == 1)
        {
            phase1 = true;
            phase2 = false;
            phase3 = false;
        }
        else if (phase == 2)
        {
            phase1 = false;
            phase2 = true;
            phase3 = false;
        }
        else if (phase == 3)
        {
            phase1 = false;
            phase2 = false;
            phase3 = true;
        }
    }
    private IEnumerator ballSpinner()
    {
        List<GameObject> orbs = new List<GameObject>();
        for (int i = 0; i < count2; i++)
        {
            GameObject bullet = Instantiate(orbitalBullet, transform.position, Quaternion.identity);
            OrbittingBullet orbScript = bullet.GetComponent<OrbittingBullet>();
            orbScript.center = transform;
            orbScript.offSett = i * 360f / count2;
            orbs.Add(bullet);
            yield return new WaitForSeconds(0.05f);
        }

        yield return new WaitForSeconds(10f);
        for (int i = orbs.Count - 1; i >= 0; i--)
        {
            Destroy(orbs[i]);
        }
        attacking = false;
    }

    private IEnumerator blackhole()
    {
        Debug.Log("Blackhole ran");
        RuntimeManager.PlayOneShot("event:/SFX/Game/Boss/Gravity Pull");
        
        playBlackholeSound = false;
        anim.Play("Gravity", 0, 0f);
        gravity.SetActive(true);
        //slashRadius.SetActive(true);
        yield return new WaitForSeconds(3f);
        slashRadius.SetActive(false);
        slash.SetActive(true);
        yield return new WaitForSeconds(0.4f);
        slash.SetActive(false);
        gravity.SetActive(false);
        yield return new WaitForSeconds(attackDelay);
        attacking = false;
    }
    private IEnumerator gravityChange()
    {
        RuntimeManager.PlayOneShot("event:/SFX/Game/Boss/Change Gravity");
        Debug.Log("This happened");
        int randomValue = Random.Range(1, 5);
        if (randomValue == 1)
        {
            rotate.transform.rotation = Quaternion.Euler(0, 0, -90);
            yield return new WaitForSeconds(attackDelay);
            Physics2D.gravity = new Vector2(-9.81f, 0);

        }
        else if (randomValue == 2)
        {
            rotate.transform.rotation = Quaternion.Euler(0, 0, 90);
            yield return new WaitForSeconds(attackDelay);
            Physics2D.gravity = new Vector2(9.81f, 0);

        }
        else if (randomValue == 3)
        {
            rotate.transform.rotation = Quaternion.Euler(0, 0, 180);

            yield return new WaitForSeconds(attackDelay);
            Physics2D.gravity = new Vector2(0, 9.81f);
        }
        else if (randomValue == 4)
        {
            rotate.transform.rotation = Quaternion.Euler(0, 0, 0);
            yield return new WaitForSeconds(attackDelay);
            Physics2D.gravity = new Vector2(0, -9.81f);

        }
        yield return new WaitForSeconds(attackDelay);
        attacking = false;

    }

    private IEnumerator LaserAttack()
    {
        laserCount = 0;

        while (laserCount < 1)
        {
            int randomValue = Random.Range(1, 4);
            if (randomValue == 1)
            {
                anim.Play("BossLasers", 0, 0f);
                LaserAttack1();
            }
            else if (randomValue == 2)
            {
                anim.Play("BossLasers", 0, 0f);
                LaserAttack2();
            }
            else if (randomValue == 3)
            {
                anim.Play("BossLasers",0,0f);
                LaserAttack3();
            }

            laserCount++;
            yield return new WaitForSeconds(laserCooldown);
        }

        yield return new WaitForSeconds(attackDelay);
        attacking = false;

    }



    private IEnumerator SpawnSpikes()
    {
        for (int i = 0; i < spikes.Count; i++)
        {
            RuntimeManager.PlayOneShot("event:/SFX/Game/Boss/Summon Spike");
            GameObject spike = Instantiate(spikes[i], transform.position, spikes[i].transform.rotation);
            afterSpikes.Add(spike);
            yield return new WaitForSeconds(0.05f);

        }

        yield return new WaitForSeconds(0.2f);
        for (int i = afterSpikes.Count - 1; i >= 0; i--)
        {
            RuntimeManager.PlayOneShot("event:/SFX/Game/Boss/Shoot Spike");
            StartCoroutine(LaunchSpikes(afterSpikes[i]));
            afterSpikes.RemoveAt(i);
        }
    }


    private IEnumerator Teleport()
    {
        while (teleportAttackReal)
        {
            RuntimeManager.PlayOneShot("event:/SFX/Game/Boss/Teleport");
            yield return new WaitForSeconds(cooldownTeleport);
            anim.Play("TeleportBoss", 0, 0f);
            yield return new WaitForSeconds(0.3f);
            TeleportSet();
            count++;
            if (count == 3)
            {
                teleportAttackReal = false;
                count = 0;
            }

        }
        yield return new WaitForSeconds(attackDelay);
        transform.position = initialPosition;
        attacking = false;

    }

    private IEnumerator LaunchSpikes(GameObject spiker)
    {
        yield return new WaitForSeconds(0.05f);
        Spikey spikeScript = spiker.GetComponent<Spikey>();
        if (spikeScript != null)
        {
            StartCoroutine(spikeScript.Launch());
        }
    }


    private void TeleportSet()
    {
        Vector2 randomPosition;

        do
        {
            float randomX = Random.Range(
                teleportAreaCenter.x - teleportAreaSize.x / 2,
                teleportAreaCenter.x + teleportAreaSize.x / 2);

            float randomY = Random.Range(
                teleportAreaCenter.y - teleportAreaSize.y / 2,
                teleportAreaCenter.y + teleportAreaSize.y / 2);

            randomPosition = new Vector2(randomX, randomY);

        }
        while (Vector2.Distance(randomPosition, player.transform.position) < safeDistance);

        transform.position = randomPosition;
        StartCoroutine(SpawnSpikes());
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(teleportAreaCenter, teleportAreaSize);
    }

    private void LaserAttack1()
    {
        for (int i = 0; i < warnings.Count; i++)
        {
            warnings[i].SetActive(true);
            StartCoroutine(Laser(laserDelay));
        }
    }
    private void SpinnyLasers()
    {
        for (int i = 0; i < spinnyLasers.Count; i++)
        {
            spinnyLasers[i].SetActive(true);

        }
        anim.Play("RotatingLasers", 0, 0f);
        StartCoroutine(SpinnyLaser(1.17f));
    }

    private IEnumerator SpinnyLaser(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        EventInstance inst = RuntimeManager.CreateInstance("event:/SFX/Game/Boss/Laser Attack");
        inst.start();
        for (int i = 0; i < spinnyLasers.Count; i++)
        {
            spinnyLasers[i].SetActive(false);

        }
        float timery = 0f;
        rotator.SetActive(true);
        while (timery < spinDuration)
        {
            rotator.transform.Rotate(-rotateSpeed * Time.deltaTime * Vector3.forward);
            timery += Time.deltaTime;
            yield return null;
        }
        rotator.SetActive(false);
        rotator.transform.rotation = Quaternion.identity;
        yield return new WaitForSeconds(attackDelay);
        inst.setParameterByName("Laser Attack End", 1);
        inst.release();
        attacking = false;

    }
    private void LaserAttack2()
    {
        for (int i = 0; i < warnings1.Count; i++)
        {
            warnings1[i].SetActive(true);
        }

        StartCoroutine(Laser1(laserDelay));
    }

    private void LaserAttack3()
    {
        for (int i = 0; i < warningsHorizontal.Count; i++)
        {
            warningsHorizontal[i].SetActive(true);
        }

        StartCoroutine(Laser3(laserDelay));
    }

    private IEnumerator Laser(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        for (int i = 0; i < warnings.Count; i++)
        {
            GameObject beam = Instantiate(laser, warnings[i].transform.position, Quaternion.identity);
            lasers.Add(beam);
            warnings[i].SetActive(false);
        }

        StartCoroutine(Deactivate(laserGoAway));
    }

    private IEnumerator Laser1(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        for (int i = 0; i < warnings1.Count; i++)
        {
            GameObject beam = Instantiate(laser, warnings1[i].transform.position, Quaternion.identity);
            lasers.Add(beam);
            warnings1[i].SetActive(false);
        }
        StartCoroutine(Deactivate1(laserGoAway));
    }

    private IEnumerator Laser3(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        for (int i = 0; i < warningsHorizontal.Count; i++)
        {
            GameObject beam = Instantiate(laserHorizontal, warningsHorizontal[i].transform.position,
                laserHorizontal.transform.rotation);
            lasers.Add(beam);
            warningsHorizontal[i].SetActive(false);
        }

        StartCoroutine(Deactivate3(laserGoAway));
    }


    private IEnumerator Deactivate(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        for (int i = warnings.Count - 1; i >= 0; i--)
        {
            Destroy(lasers[i]);
            lasers.RemoveAt(i);
        }
    }

    private IEnumerator Deactivate1(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        for (int i = warnings1.Count - 1; i >= 0; i--)
        {
            Destroy(lasers[i]);
            lasers.RemoveAt(i);
        }
    }

    private IEnumerator Deactivate3(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        for (int i = warningsHorizontal.Count - 1; i >= 0; i--)
        {
            Destroy(lasers[i]);
            lasers.RemoveAt(i);
        }




    }




    private void Shot()

    {

        if (withinRange())

        {
            anim.Play("ShootingBoss", 0, 0f);
            Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.identity);


        }

    }

    private IEnumerator Red()
    {
        RuntimeManager.PlayOneShot("event:/SFX/Game/Boss/Orb Shoot");
        anim.Play("ShootingBoss", 0, 0f);
        Instantiate(cursedTechniqueRed, bulletSpawnPoint.position, Quaternion.identity);
        yield return new WaitForSeconds(attackDelay);
        attacking = false;
    }



    private bool withinRange()

    {

        float radiusSqr = radius * radius;

        Vector2 playerPosition = player.transform.position;

        Vector2 position = transform.position;

        if ((playerPosition - position).sqrMagnitude <= radiusSqr)

        {

            return true;

        }

        else

        {

            return false;

        }

    }

    private void OnDrawGizmos()

    {

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(gameObject.transform.position, radius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.gameObject.transform.position, safeDistance);


    }

    private IEnumerator Shooting()
    {
        bulletCount = 0;

        while (bulletCount <= 2)
        {
            Shot();
            bulletCount++;
            yield return new WaitForSeconds(bulletCooldown);

        }
        yield return new WaitForSeconds(attackDelay);
        attacking = false;



    }
    public void SpawnSFX()
    {
        RuntimeManager.PlayOneShot("event:/SFX/Game/Boss/Appear");
    }
}
