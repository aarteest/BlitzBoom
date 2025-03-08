using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebrisSpawner : MonoBehaviour
{
	public bool hitCollider;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (hitCollider)
		{
			Debug.Log("Debris Should be spawned");

		}
	}

	private void OnCollisionEnter(Collision collision)
	{
		if (collision.collider.CompareTag("Player"))
		{
			hitCollider = true;
			Debug.Log("Debris spawned");
			DebrisBossAbility.Instance.SpawnDebris(transform.position, transform.forward);

		}
	}

}
