using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineAttack : MonoBehaviour
{
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
    [SerializeField] private GameObject laser;
    
    private float timer;
    public float cooldown; 
    List<GameObject> warnings = new List<GameObject>();
    List<GameObject> warnings1 = new List<GameObject>();
    List<GameObject> lasers = new List<GameObject>();

    void Start()
    {
        warnings.Add(warning1);
        warnings.Add(warning2);
        warnings.Add(warning3);
        warnings.Add(warning4);
        warnings.Add(warning5);
        warnings.Add(warning6);
        warnings.Add(warning7);
        warnings.Add(warning8);
        
        warnings1.Add(warning9);
        warnings1.Add(warning10);
        warnings1.Add(warning11);
        warnings1.Add(warning12);
        warnings1.Add(warning13);
        warnings1.Add(warning14);
        warnings1.Add(warning15);
        warnings1.Add(warning16);
        
        
        
    }

    private void LaserAttack1()
    {
        for (int i = 0; i < warnings.Count; i++)
        {
            warnings[i].SetActive(true);
            StartCoroutine(Laser(2f));
        }
    }

    private void LaserAttack2()
    {
        for (int i = 0; i < warnings1.Count; i++)
        {
            warnings1[i].SetActive(true);
        }
        StartCoroutine(Laser1(2f));
    }
    void Update()
    {
        timer += Time.deltaTime;
        if (timer > cooldown)
        {
            int randomValue = Random.Range(1,3);
            timer = 0;
            if (randomValue == 1)
            {
                LaserAttack1();
            }
            else if (randomValue == 2)
            {
                LaserAttack2();
            }
            
            
        }
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
        StartCoroutine(Deactivate(3f));
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
        StartCoroutine(Deactivate1(3f));
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
    

}
