using UnityEngine;

public class BlockDetector : MonoBehaviour {
	
	bool collisionWithPlayer = false;
	string tagName = "Player";
	
	
	void OnTriggerEnter(Collider c) {

		if(c.CompareTag(tagName)) {
			collisionWithPlayer = true;
			SendCollideState(collisionWithPlayer);
		}
	}
	
	void OnTriggerExit(Collider c) {
		if(c.CompareTag(tagName)) {
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