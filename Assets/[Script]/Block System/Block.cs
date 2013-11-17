using UnityEngine;
using System.Collections;

/// <summary>
/// Block.
/// </summary>

public class Block : MonoBehaviour {
	
	#region Variables
	
	string version;
	
	public Block[] neighbor = new Block[4];
	
	public Transform block;
	public BlockDetector detector;
	
	public bool ready;
	
	public Color activeColor = Color.red;
	public Color normalColor = Color.white;
	
	#region PathFinding 

	public int[] heuristicArray;
	
	#endregion
	
	#endregion
	
	
	#region MonoBehave Functions
	
	void Awake() {
		
		ready = false;
		
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
    void OnBecameVisible() { }
	
    void OnBecameInvisible() { }
	
	//Debugging help
	void OnDrawGizmosSelected() {
		
		if(neighbor[(int)Neighbor.North] != null) {
			Gizmos.color = Color.red;
			Gizmos.DrawIcon(neighbor[(int)Neighbor.North].gameObject.transform.position + new Vector3(0,2,0), "a_n");
		}
		
		if(neighbor[(int)Neighbor.South] != null) {
			Gizmos.color = Color.blue;
			Gizmos.DrawIcon(neighbor[(int)Neighbor.South].gameObject.transform.position + new Vector3(0,2,0), "a_s");
		}
		
		if(neighbor[(int)Neighbor.West] != null) {
			Gizmos.color = Color.green;
			Gizmos.DrawIcon(neighbor[(int)Neighbor.West].gameObject.transform.position + new Vector3(0,2,0), "a_w");
		}
		
		if(neighbor[(int)Neighbor.East] != null) {
			Gizmos.color = Color.yellow;
			Gizmos.DrawIcon(neighbor[(int)Neighbor.East].gameObject.transform.position + new Vector3(0,2,0), "a_e");
		}
		
	}

	#endregion
	
	
	#region Other Functions
	
	void SetColor(Color color) {
		block.renderer.material.SetColor("_Color", color);
	}
	
	Color GetColor() {
		return block.renderer.material.color;
	}
	
	
	public void SetColorNormal(Color color) {
		normalColor = color;
	}
	
	public void SetColorActive(Color color) {
		activeColor = color;
	}
	
	public void SetColors(Color normal, Color active) {
		SetColorNormal(normal);	
		SetColorActive(active);
	}
	
	public void SetColorAlpha(float a) {
		Color c = GetColor();
		SetColor(new Color(c.r, c.g, c.b, a));	
	}
	
	
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
	
	public void SetYPos(float y) {
		transform.position = new Vector3(transform.position.x, transform.position.y+y, transform.position.z);	
	}
	
	
	public IEnumerator CollidePlayer(bool collide) {
		
		if(collide)
			StartCoroutine(SetColorAfterTime(activeColor));
		else
			StartCoroutine(SetColorAfterTime(normalColor));
		
		return null;
	}
	
	public IEnumerator CollidePlayerWithColor(bool collide, Color color, float duration=0.25f) {
		
		if(collide)
			StartCoroutine(SetColorAfterTime(color, duration));
		else
			StartCoroutine(SetColorAfterTime(normalColor, duration));
		
		return null;
	}
	
	
	public void LightOnStart() {
		SetColor(normalColor);	
	}
	
	
	public IEnumerator StartAnimation() {
		
		if(!block.renderer.enabled)
			block.renderer.enabled = true;
		
		string clip = version + "_Flip_" + (Random.Range(0,2) == 1 ? "X" : "Z") + "_0";
		
		if(animation[clip] != null) {
			animation.Play(clip);
			yield return new WaitForSeconds(animation[clip].length);	
		}
		
		ready = true;	
	}
	
	public IEnumerator Show() {
		block.renderer.enabled = true;
		SetColorAlpha(0f);
		
		//4 - Because the 4th prefab contain the new Shader which handle transparency and combined texture.
		if(version.Contains("4"))
			StartCoroutine(AppearBlockAfterTime());
		
		yield return new WaitForSeconds(3);
		ready = true;
	}
	
	public void Hide() {
		block.renderer.enabled = false;	
	}
	
	
	IEnumerator AppearBlockAfterTime() {
		
		float duration = 3;
		Color c = GetColor();
		
	    float pointInTime = 0f;
	    while (pointInTime <= duration) 
		{
	    	SetColor(new Color(c.r, c.g, c.b, Mathf.Lerp(0, 1, pointInTime / duration)));
	    	pointInTime += Time.deltaTime;
	    	yield return null;
	    }
	}
	
	IEnumerator SetColorAfterTime(Color newColor, float dur=0.25f) {
		
		float duration = dur;
		Color oldColor = GetColor();
		
	    float pointInTime = 0f;
	    while (pointInTime <= duration) {
			SetColor(Color.Lerp(oldColor, newColor, pointInTime / duration));
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
	
}

public enum Neighbor {
	North = 0,
	East = 1,
	South = 2,
	West = 3
}
