using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour {
	
	#region Variables
	
	public Block[] neighbor = new Block[4];
	
	public Transform block;
	public BlockDetector detector;
	
	string version;
	
	#region PathFinding 

	public int[] heuristicArray;
	
	#endregion
	
	
	#endregion
	
	
	#region MonoBehave Functions
	
	void Awake() {
		if(detector == null) {
			GameObject detectCollider = GameObject.CreatePrimitive(PrimitiveType.Cube); 
			detector = detectCollider.AddComponent<BlockDetector>();
			detectCollider.transform.parent = transform;
		}
		
		if(block == null) {
			block = transform.FindChild("Block");	
		}
		
	}
	
	//Not Used
    void OnBecameVisible() {
		//detector.enabled = true;
    }
	
    void OnBecameInvisible() {
		//detector.enabled = false;
    }
	
	//Debugging help
	void OnDrawGizmosSelected() {
		
		//BlockManager mg = FindObjectOfType(typeof(BlockManager)) as BlockManager;
		//Debug.Log(mg.GetBlockIndex(this));
		
		if(neighbor[(int)Neighbor.North] != null) {
			Gizmos.color = Color.red;
			//Gizmos.DrawCube(neighbor[(int)Neighbor.North].gameObject.transform.position + new Vector3(0,2,0), new Vector3(1,1,1));
			Gizmos.DrawIcon(neighbor[(int)Neighbor.North].gameObject.transform.position + new Vector3(0,2,0), "a_n");
		}
		
		if(neighbor[(int)Neighbor.South] != null) {
			Gizmos.color = Color.blue;
			//Gizmos.DrawCube(neighbor[(int)Neighbor.South].gameObject.transform.position + new Vector3(0,2,0), new Vector3(1,1,1));
			Gizmos.DrawIcon(neighbor[(int)Neighbor.South].gameObject.transform.position + new Vector3(0,2,0), "a_s");
		}
		
		if(neighbor[(int)Neighbor.West] != null) {
			Gizmos.color = Color.green;
			//Gizmos.DrawCube(neighbor[(int)Neighbor.West].gameObject.transform.position + new Vector3(0,2,0), new Vector3(1,1,1));
			Gizmos.DrawIcon(neighbor[(int)Neighbor.West].gameObject.transform.position + new Vector3(0,2,0), "a_w");
		}
		
		if(neighbor[(int)Neighbor.East] != null) {
			Gizmos.color = Color.yellow;
			//Gizmos.DrawCube(neighbor[(int)Neighbor.East].gameObject.transform.position + new Vector3(0,2,0), new Vector3(1,1,1));
			Gizmos.DrawIcon(neighbor[(int)Neighbor.East].gameObject.transform.position + new Vector3(0,2,0), "a_e");
		}
		
	}

	#endregion
	
	
	#region Other Functions
	
	public void SetVersion(string v) {
		version = v;	
	}
	
	public void SetPosition(Vector3 pos) {
		transform.position = pos;
	}
	
	public void SetScale(Vector3 scale) {
		block.localScale = scale;
		detector.SetScale(scale);
	}
	
	public IEnumerator CollidePlayer(bool collide) {
		
		if(collide)
			//block.renderer.material.SetColor("_Color", Color.red);	
			StartCoroutine(SetColorAfterTime(Color.red));
		else
			//block.renderer.material.SetColor("_Color", Color.white);
			StartCoroutine(SetColorAfterTime(Color.white));
		
		return null;
	}
	
	public IEnumerator CollidePlayerWithColor(bool collide, Color c, float duration=0.25f) {
		
		if(collide)
			//block.renderer.material.SetColor("_Color", Color.red);	
			StartCoroutine(SetColorAfterTime(c, duration));
		else
			//block.renderer.material.SetColor("_Color", Color.white);
			StartCoroutine(SetColorAfterTime(Color.white, duration));
		
		return null;
	}
	
	public void SetYPos(float y) {
		transform.position = new Vector3(transform.position.x, transform.position.y+y, transform.position.z);	
	}
	
	public void StartAnimation() {
		
		if(!block.renderer.enabled)
			block.renderer.enabled = true;
		
		string clip = version + "_Flip_" + (Random.Range(0,2) == 1 ? "X" : "Z") + "_0";
		
		if(animation[clip] != null)
			animation.Play(clip);
	}
	
	public void LightOnStart() {
		block.renderer.material.SetColor("_Color", new Color(1,1,1,1));	
	}
	
	
	public void Show() {
		block.renderer.enabled = true;
		Color c = block.renderer.material.color;
		block.renderer.material.SetColor("_Color", new Color(c.r, c.g, c.b, 0));
		
		if(version.Contains("4")) {
			StartCoroutine("AppearBlockAfterTime");
		}
	}
	
	public void Hide() {
		block.renderer.enabled = false;	
	}
	
	IEnumerator AppearBlockAfterTime() {
		
		float duration = 3;
		Color c = block.renderer.material.color;
		
	    float pointInTime = 0f;
	    while (pointInTime <= duration) {
	    	block.renderer.material.SetColor("_Color", new Color(c.r, c.g, c.b, Mathf.Lerp(0, 1, pointInTime / duration)));
	    	pointInTime += Time.deltaTime;
	    	yield return null;
	    }
	}
	
	IEnumerator SetColorAfterTime(Color color, float dur=0.25f) {
		
		float duration = dur;
		Color c = block.renderer.material.color;
		
	    float pointInTime = 0f;
	    while (pointInTime <= duration) {
			block.renderer.material.SetColor("_Color", Color.Lerp(c, color, pointInTime / duration));
	    	pointInTime += Time.deltaTime;
	    	yield return null;
	    }
	}
	
	
	public Block GetNeighbor(Neighbor n) {
		if((int)n >= 0 && (int)n<=4)
			return neighbor[(int)n];
		
		return null;
	}
	
	public void SetNeighbor(Neighbor n, Block b) {
		neighbor[(int)n] = b;	
	}
	
	#endregion
	
	
	#region PathFind 
	
	#endregion
	
}

public enum Neighbor {
	North = 0,
	East = 1,
	South = 2,
	West = 3
}
