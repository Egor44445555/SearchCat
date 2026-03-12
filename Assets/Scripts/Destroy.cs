using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destroy : MonoBehaviour
{
    [SerializeField] float time = 0.3f;

    void Start()
    {
        StartCoroutine(Timer());
    }

    IEnumerator Timer()
    {        
        yield return new WaitForSeconds(time);
        Destroy(gameObject);      
    }
}
