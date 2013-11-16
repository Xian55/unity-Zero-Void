using UnityEngine;
using System.Collections;

/// <summary>
/// This script is atteched to the Blaster projectile and it
/// governs the behaviour of the projectile. 
/// </summary>

[RequireComponent(typeof(Rigidbody))]
public class BlasterScript : MonoBehaviour {
	
	public GameObject blasterExplosion;
	
	Transform myTransform;
	float projectileSpeed = 20;
	bool expended = false;
	
	RaycastHit hit;
	float range = 1.5f;
	
	float expireTime = 5;
	
	// Use this for initialization
	void Start () {
		myTransform = transform;
		StartCoroutine(DestroyAfterTime());
	}
	
	// Update is called once per frame
	void Update () {
		myTransform.Translate(Vector3.forward * projectileSpeed * Time.deltaTime);
	
		if(Physics.Raycast(myTransform.position, myTransform.forward, out hit, range) && !expended) {
			if(hit.transform.CompareTag("Block")) {
				expended = true;
				myTransform.renderer.enabled = false;
				myTransform.light.enabled = false;
				
				//Blaster Exposion
				if(blasterExplosion)
					Instantiate(blasterExplosion, hit.point, Quaternion.identity);
			}
		}
	}
	
	IEnumerator DestroyAfterTime() {
		yield return new WaitForSeconds(expireTime);
		Destroy(myTransform.gameObject);
	}
	
}
