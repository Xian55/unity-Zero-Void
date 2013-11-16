using UnityEngine;

public class BlockDetector : MonoBehaviour {
	
	bool collisionWithPlayer = false;
	
	
	void OnTriggerEnter(Collider c) {
		
		//Debug.Log(c.name);
		
		if(c.transform.tag == "Player") {
			collisionWithPlayer = true;
			SendCollideState(collisionWithPlayer);
		}
	}
	
	void OnTriggerExit(Collider c) {
		if(c.transform.tag == "Player") {
			collisionWithPlayer = false;
			SendCollideState(collisionWithPlayer);
		}
	}
	
	
	
	void SendCollideState(bool isCollide) {
		gameObject.SendMessageUpwards("CollidePlayer", isCollide, SendMessageOptions.DontRequireReceiver);	
	}
	
	
	public void SetPosition(Vector3 pos) {
		transform.position = pos;
	}
	
	public void SetScale(Vector3 scale) {
		transform.localScale = scale;
	}
}