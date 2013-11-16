using UnityEngine;
using System.Collections;

public class BlockManager : MonoBehaviour {
	
	#region Variables
	Transform player;
	Vector3 blockSize = new Vector3(2, 1, 2);
	
	bool justOnce = false;

	GameObject prefab_Block;
	string prefab_version = "BlockV3";
	
	GameObject blockParent;
	
	
	bool initalized = false;
	
	public bool useAnimation = false;
	public bool useFastSpawn = true;
	public bool useLightOnStart = false;
	
	
	public Block[] blocks;
	public int row = 10;
	public int column = 10;
	
	public float padding = .1f;
	
	public float waitFewSeconds = 3;
	
	//For GUI START
	public bool GUIStart = false;
	string gui_Size = "2";
	string gui_Row = "100";
	string gui_Column = "100";
	bool   gui_UseAnimation = false;
	bool   gui_FastSpawn = false;
	bool   gui_LigthOnStart = false;

	#endregion
	
	
	#region MonoBehave Functions
	
	void Awake() {
		prefab_Block = Resources.Load("Prefab/" + prefab_version) as GameObject;
		player       = GameObject.FindWithTag("Player").transform;
		blockParent  = GameObject.FindWithTag("Respawn");
		
		
		if(prefab_Block && !GUIStart)
			Init();
		
		if(GUIStart) {
			player.SendMessage("Enable", false, SendMessageOptions.DontRequireReceiver);
			player.GetComponent<MouseLook>().enabled = false;
			Camera.main.GetComponent<MouseLook>().enabled = false;
			player.GetComponent<FireBlasterScript>().enabled = false;
		}
	}
	
	void Update() {
		
		if(!GUIStart) {
			if(waitFewSeconds < 0) {
				if(prefab_Block && !justOnce && initalized) {
					StartCoroutine("ShowBlocks");
					
					justOnce = true;
				}
			}
			else
				waitFewSeconds -= Time.deltaTime;
		}
		
		if(player.position.y < -5) {
			player.position = new Vector3(0,5,0);
			player.rotation = Quaternion.identity;
		}
		
		else if(Input.GetKey(KeyCode.T)) {
			Screen.fullScreen = !Screen.fullScreen;
			
			if(!GUIStart)
				Screen.lockCursor = !Screen.lockCursor;
		}	
	}
	
	void OnGUI() {
		
		if(GUIStart) {

			GUI.Label(new Rect(5, 5, 105, 20), "Block Scale (int)"); gui_Size  = GUI.TextField(new Rect(105, 5, 40, 20), gui_Size);
			GUI.Label(new Rect(5, 25, 75, 20), "Row (int)");         gui_Row   = GUI.TextField(new Rect(105, 25, 40, 20), gui_Row);
			GUI.Label(new Rect(5, 45, 75, 20), "Column (int)");       gui_Column = GUI.TextField(new Rect(105, 45, 40, 20), gui_Column);
			
			gui_UseAnimation = GUI.Toggle(new Rect(35, 75, 200, 20),  gui_UseAnimation, "Animation?");
			gui_FastSpawn    = GUI.Toggle(new Rect(35, 95, 200, 20),  gui_FastSpawn,    "Fast Spawn?");
			gui_LigthOnStart = GUI.Toggle(new Rect(35, 115, 200, 20), gui_LigthOnStart, "Light On Start?");
			
			if(GUI.Button(new Rect(45, 145, 80, 25), "Start")) {
				
				int sc = int.Parse(gui_Size);
				blockSize = new Vector3(sc, 1, sc);
				
				row = int.Parse(gui_Row);
				column = int.Parse(gui_Column);
				
				useAnimation = gui_UseAnimation;
				useFastSpawn = gui_FastSpawn;
				useLightOnStart = gui_LigthOnStart;
				
				Init();
				GUIStart = false;
				
				player.SendMessage("Enable", true, SendMessageOptions.DontRequireReceiver);
				player.GetComponent<MouseLook>().enabled = true;
				Camera.main.GetComponent<MouseLook>().enabled = true;
				player.GetComponent<FireBlasterScript>().enabled = true;
			}
		}
		
	}
	
	#endregion
	
	
	#region Blocks Functions
	
	void Init() {
		
		float time = 0;
		
		blocks = new Block[row*column];
		int index = 0;
		
		for(int x=0; x<row; x++) {
			for(int z=0; z<column; z++) {
				
				GameObject block = Instantiate(prefab_Block, Vector3.zero, Quaternion.identity) as GameObject;
				
				block.name = x + "_" + z; 
				block.transform.parent = blockParent.transform;
				
				Block blockScript = block.GetComponent<Block>();
				
				if(index > 0)
					blockScript.Hide();
				
				blockScript.SetScale(blockSize);
				blockScript.SetVersion(prefab_version);
				
				if(useLightOnStart) {
					blockScript.LightOnStart();	
				}
				
				float y = Random.Range(0,2) == 0 ? 0.1f : -0.1f;		
				blockScript.SetPosition(new Vector3( x*(blockSize.x+padding), y, z*(blockSize.z+padding)));
				
				//Slow animation...
				//if(!useFastAnimation) {
				//	if(count > 0) { 
				//		blockScript.DoStartAnim();
				//		yield return new WaitForSeconds(0.1f);
				//	}
				//}
				
				blocks[index] = blockScript;
				
				CalculateNeighbors(x, z);
				
				index++;
			}
			
			//Fast Animation
			//if(useFastAnimation) {
			//	for(int c=lastStep; c<count; c++) {
			//		if(c>0)
			//			blocks[c].DoStartAnim();	
			//	}
			//	
			//	lastStep = count;
			//	yield return new WaitForSeconds(0.1f);
			//}
		}
		
		CalculateRandomHeight();
		
		initalized = true;
		
		HeuristicDistance();
		
		time += Time.deltaTime;
		Debug.Log("Calculacted in " + time + " ms");
	}
	
	IEnumerator ShowBlocks() {
		
		Debug.Log("ShowBlocks");
		
		int index = 0;
		int lastStep = 0;
		
		for(int x=0; x<row; x++) 
		{	
			for(int z=0; z<column; z++) 
			{
				//Slow animation
				if(!useFastSpawn) 
				{
					if(index > 0) 
					{ 
						if(useAnimation)
							blocks[index].DoStartAnim();
						else
							blocks[index].Show();
						
						yield return new WaitForSeconds(0.1f);
					}
				}
				
				index++;
			}
			
			//Fast animation
			if(useFastSpawn) 
			{
				for(int c=lastStep; c < index; c++) 
				{
					if(c > 0)
						if(useAnimation)
							blocks[c].DoStartAnim();
						else
							blocks[c].Show();	
				}
				
				lastStep = index;
				yield return new WaitForSeconds(0.05f);
			}
	}
	}
	
	void CalculateNeighbors(int x, int z) {
		
		int index = row * x + z;
		
		//South - North
		if(index > 0 && z < column && index % row != 0) {
			blocks[index].SetNeighbor(Neighbor.South, blocks[index-1]);
			blocks[index-1].SetNeighbor(Neighbor.North, blocks[index]);
		}
		
		//East - West
		if(index >= row) {
			blocks[index].SetNeighbor(Neighbor.West, blocks[index-row]);
			blocks[index-row].SetNeighbor(Neighbor.East, blocks[index]);
		}
		
	}
	
	void CalculateRandomHeight() {
		
		int index = 0;
		for(int x=0; x< row; x++) {
			for(int z=0; z<column; z++) {
				
				if(index != 0) {
				
					float y = 0;
					
					bool south = false;
					bool north = false;
					
					bool west = false;
					bool east = false;
					
					if(blocks[index].GetNeighbor(Neighbor.South) != null) {
						y += blocks[index].GetNeighbor(Neighbor.South).transform.position.y;
						south = true;
					}
					
					if(blocks[index].GetNeighbor(Neighbor.North) != null) {
						y += blocks[index].GetNeighbor(Neighbor.North).transform.position.y;
						north = true;
					}
					
					if(blocks[index].GetNeighbor(Neighbor.West) != null) {
						y += blocks[index].GetNeighbor(Neighbor.West).transform.position.y;
						west = true;
					}
					
					if(blocks[index].GetNeighbor(Neighbor.East) != null) {
						y += blocks[index].GetNeighbor(Neighbor.East).transform.position.y;
						east = true;
					}
					
					if(south && north) {
						y /= 2 + 0.2f;
						//(Random.Range(0,2) == 0 ? 0.2f : -0.2f);	
					}
					
					else if(west && east) {
						y /= 2 + 0.2f;
						//(Random.Range(0,2) == 0 ? 0.2f : -0.2f);	
					}
					
					else {
						y = 0.2f;
							//Random.Range(0,2) == 0 ? 0.1f : -0.1f;	
					}
					
					//Debug.Log(blocks[index].name + " Calculated:" + y);
					blocks[index].SetYPos(y);
				}
				
				index++;
			}
		}
	}
	
	public Vector2 GetBlockIndex(Block block) {
		
		for(int x=0; x < row; x++) 
		{
			for (int z=0; z < column; z++)
			{
				int index = row * x + z;
				
				if(blocks[index] == block) {
					return new Vector2(x,z);	
				}
			}	
		}
		return new Vector2(-1,-1);
	}
	
	#endregion
	
	
	#region Pathfind Functions
	
	public void HeuristicDistance() {
		
		int index = 0;
		
		for (int x = 0; x < row; x++)
		{
			for (int z = 0; z < column; z++)
			{
				index = row * x + z;
				
				blocks[index].heuristicArray = new int[row*column];
				
				for (int bx = 0; bx < row; bx++)
				{
					for (int bz = 0; bz < column; bz++)
					{	
						int p_index = row * bx + bz;
						blocks[index].heuristicArray[p_index] = Mathf.Abs(x - bx) + Mathf.Abs(z - bz);
					}
				}
			}
		}
	}
	
	public void GetShortestPath(Block start, Block destination) {
		PathFind ph = new PathFind(row*column);
		
		Vector2 startb = GetBlockIndex(start);
		Vector2 stopb = GetBlockIndex(destination);
	
		/*
		for(int i=0; i< System.Enum.GetValues(typeof(Neighbor)); i++)
		{
			if(start.neighbor[i] != null) {
				
			}
		}
		*/	
			
		/*	
		for(int x=0; x < row; x++) 
		{
			for (int z=0; z < column; z++)
			{
				int index = row * x + z;
				
				}
			}	
		}
		*/
	
	}
	
		
	#endregion
	
}
