using UnityEngine;
using System.Collections;

/// <summary>
/// Block.
/// </summary>

public class Block : MonoBehaviour {
	
	#region Variables

    public static Material sharedMat = null;

    public string version;
	
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
	
	private void Awake() {

        if (sharedMat == null)
        {
            sharedMat = (Material)Resources.Load("Material/Floor_Default");
        }

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
	
	//Debugging help
	private void OnDrawGizmosSelected() {
		
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

        #region Color Change

    private void SetColor(Color color) {
		block.renderer.material.SetColor("_Color", color);
	}

    private Color GetColor()
    {
		return block.renderer.material.color;
	}
	
	
	public void SetColorNormal(Color color) {
		//normalColor = color;
        block.renderer.material = sharedMat;
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

        #endregion


    public void SetVersion(string v) {
		version = v;	
	}


        #region Transfrom Changes

    public void SetPosition(Vector3 pos) {
		transform.position = pos;
	}
	
	public void SetScale(Vector3 scale) {
		block.localScale = scale;
		detector.SetScale(scale);
	}
	
	
	public void AddPosition(Vector3 p) {
		transform.position = new Vector3(transform.position.x + p.x, transform.position.y + p.y, transform.position.z + p.z);
	}
	
	public void AddYPos(float y) {
		transform.position = new Vector3(transform.position.x, transform.position.y+y, transform.position.z);	
	}

        #endregion


        #region Collision with Player

    public IEnumerator CollidePlayer(bool collide) {
		
		if(collide) {
			StartCoroutine(SetColorAfterTime(activeColor));
		}
		else {
			StartCoroutine(SetColorAfterTime(normalColor));
		}
		
		return null;
	}
	
	public IEnumerator CollidePlayerWithColor(bool collide, Color color, float duration=0.25f) {
		
		if(collide)
			StartCoroutine(SetColorAfterTime(color, duration));
		else
			StartCoroutine(SetColorAfterTime(normalColor, duration));
		
		return null;
	}

        #endregion

        #region Appear and Disappear

    public IEnumerator Show() {
		block.renderer.enabled = true;
		SetColorAlpha(0f);
		
		//4 - Because the 4th prefab contain the new Shader which handle transparency and combined texture.
		if(version.Contains("4"))
			StartCoroutine(AppearBlockAfterTime());
		
		yield return new WaitForSeconds(4f);

        SetColorNormal(normalColor);

		ready = true;
	}
	
	public void Hide() {
		block.renderer.enabled = false;	
	}

    public IEnumerator StartAnimation()
    {

        if (!block.renderer.enabled)
            block.renderer.enabled = true;

        string clip = version + "_Flip_" + (Random.Range(0, 2) == 1 ? "X" : "Z") + "_0";

        if (animation[clip] != null)
        {
            animation.Play(clip);
            yield return new WaitForSeconds(animation[clip].length + 0.2f);
        }

        SetColorNormal(normalColor);

        ready = true;
    }

    public void LightOnStart()
    {
        SetColor(normalColor);
    }

        #endregion


    private IEnumerator AppearBlockAfterTime() {
		
		float duration = 3;
		Color c = GetColor();
		
	    float pointInTime = 0f;
	    while (pointInTime <= duration) 
		{
	    	SetColor(new Color(c.r, c.g, c.b, Mathf.Lerp(0, 1, pointInTime / duration)));
	    	pointInTime += Time.deltaTime;
	    	yield return null;
	    }

        /*
            if (GetColor().a >= 0.98f)
            {
                SetColorNormal(normalColor);
            }
         
         */
    }
	
	private IEnumerator SetColorAfterTime(Color newColor, float dur=0.25f) {
		
		float duration = dur;
		Color oldColor = GetColor();
		
	    float pointInTime = 0f;
	    while (pointInTime <= duration) {
			SetColor(Color.Lerp(oldColor, newColor, pointInTime / duration));
	    	pointInTime += Time.deltaTime;

            if (newColor == normalColor)
            {
                SetColorNormal(Color.white);
            }

	    	yield return null;
	    }
	}


        #region Neighbor System

    public Block GetNeighbor(Neighbor n) {
		if((int)n >= 0 && (int)n<=4)
			return neighbor[(int)n];
		
		return null;
	}
	
	public Block GetNeighbor(int nIndex) {
		if(nIndex >= 0 && nIndex <= 4)
			return neighbor[nIndex];
		
		return null;
	}
	
	public void SetNeighbor(Neighbor n, Block b) {
		neighbor[(int)n] = b;
    }

        #endregion


    #endregion

}

public enum Neighbor {
	North = 0,
	East = 1,
	South = 2,
	West = 3
}
