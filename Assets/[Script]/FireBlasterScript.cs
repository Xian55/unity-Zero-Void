using UnityEngine;
using System.Collections;

public class FireBlasterScript : MonoBehaviour {
	
	public GameObject blaster;
	
	Transform cameraHeadTransform;
	Vector3 launchPosition = new Vector3();
	
	//Timer for Fire
	float fireRate = 0.2f;
	float nextFire = 0;
	
	// Use this for initialization
	void Start () {
		cameraHeadTransform = Camera.main.transform;
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetMouseButton(0) && Time.time > nextFire) {
			nextFire = Time.time + fireRate;
			launchPosition = cameraHeadTransform.TransformPoint(0, 0, 0.2f);			
			Instantiate(blaster, launchPosition, cameraHeadTransform.rotation);		
		}
	}
}
