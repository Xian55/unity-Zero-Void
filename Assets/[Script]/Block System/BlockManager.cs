using UnityEngine;
using System.Collections;

public class BlockManager : MonoBehaviour {
	
	static BlockManager _instance;
	public static BlockManager Instance {
		get {
			if(_instance == null)
				_instance = FindObjectOfType(typeof(BlockManager)) as BlockManager;
			
			return _instance;
		}
	}
	
	
	#region Variables
	Transform player;
	Vector3 blockSize = new Vector3(2, 1, 2);
	
	bool justOnce = false;

	GameObject prefab_Block;
	public static string prefab_version = "BlockV4";
	
	GameObject blockParent;
	
	
	public bool initalized = false;
	
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
	
	int	gui_lineHeight = 20;
	int gui_start_x = 5;
	int gui_start_y = 5;
	
	int gui_padding = 30;
	
	string gui_Size = "2";
	string gui_Row = "100";
	string gui_Column = "100";
	
	bool   gui_UseAnimation = false;
	bool   gui_UseFade      = true;
	
	bool gui_Fade {
		set {
			if(value == true) {
				gui_UseFade = true;
				gui_UseAnimation = false;
			}
		}
		
		get {
			return gui_UseFade;
		}
	}
	bool gui_Animation {
		set {
			if(value == true) {
				gui_UseAnimation = true;
				gui_UseFade = false;
			}
		}
		
		get {
			return gui_UseAnimation;
		}
	}
	
	bool   gui_FastSpawn = false;
	bool   gui_LigthOnStart = false;
	
	bool   gui_RandomHeight = false;

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

			GUI.Label(new Rect(gui_start_x, gui_start_y,                    105, gui_lineHeight), "Block Scale (int)"); gui_Size   = GUI.TextField(new Rect(105, gui_start_y                   , 3*gui_lineHeight, gui_lineHeight), gui_Size);
			GUI.Label(new Rect(gui_start_x, gui_start_y + gui_lineHeight,    75, gui_lineHeight), "Row (int)");         gui_Row    = GUI.TextField(new Rect(105, gui_start_y +   gui_lineHeight, 3*gui_lineHeight, gui_lineHeight), gui_Row);
			GUI.Label(new Rect(gui_start_x, gui_start_y + 2*gui_lineHeight,  75, gui_lineHeight), "Column (int)");      gui_Column = GUI.TextField(new Rect(105, gui_start_y + 2*gui_lineHeight, 3*gui_lineHeight, gui_lineHeight), gui_Column);
			
			
			GUI.Label(new Rect(gui_start_x, gui_start_y + 3*gui_lineHeight, 400, gui_lineHeight), "What method would you like to use at start?");
			
			gui_Animation = GUI.Toggle(new Rect(gui_start_x + gui_padding, gui_start_y + 4*gui_lineHeight, 100, gui_lineHeight),  gui_UseAnimation, "Animation");
			gui_Fade      = GUI.Toggle(new Rect(2*(gui_start_x + gui_padding) + GUILayoutUtility.GetRect(new GUIContent("Animation"), "label").width, gui_start_y + 4*gui_lineHeight, 200, gui_lineHeight),  gui_UseFade, "Fade");
			
			gui_FastSpawn    = GUI.Toggle(new Rect(gui_start_x, gui_start_y + 5*gui_lineHeight, 200, gui_lineHeight),    gui_FastSpawn, "Fast Spawn?");
			gui_LigthOnStart = GUI.Toggle(new Rect(gui_start_x, gui_start_y + 6*gui_lineHeight, 200, gui_lineHeight), gui_LigthOnStart, "Light On Start?");
			
			gui_RandomHeight = GUI.Toggle(new Rect(gui_start_x, gui_start_y + 7*gui_lineHeight, 200, gui_lineHeight), gui_RandomHeight, "Random Height?");
			
			if(GUI.Button(new Rect(gui_start_x + gui_padding/2, gui_start_y + 8*gui_lineHeight, 80, gui_lineHeight), "Start")) {
				
				int try_size = -1;
				int try_row = -1;
				int try_column = -1;
				
				if(int.TryParse(gui_Size, out try_size) == false && try_size > 0) 
					gui_Size = "Invalid";
				
				if(int.TryParse(gui_Row, out try_row) == false && try_row > 0) 
					gui_Row = "Invalid";
				
				if(int.TryParse(gui_Column, out try_column) == false && try_column > 0) 
					gui_Column = "Invalid";		
							
				if(try_size > 0 && try_row > 0 && try_column > 0) {
					GUIStart = false;
					
					blockSize = new Vector3(try_size, 1, try_size);
					
					row = try_row;
					column = try_column;

					useAnimation    = gui_UseAnimation;
					useFastSpawn    = gui_FastSpawn;
					useLightOnStart = gui_LigthOnStart;
					
					Init();
					
					player.SendMessage("Enable", true, SendMessageOptions.DontRequireReceiver);
					player.GetComponent<MouseLook>().enabled = true;
					Camera.main.GetComponent<MouseLook>().enabled = true;
					player.GetComponent<FireBlasterScript>().enabled = true;
				}
				
			}
		}
		
	}
	
	#endregion
	
	
	#region Blocks Functions
	
	void Init() {
		
		float startTime = Time.time;
	 
	    // (Some code here which you want to measure)
	 
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
				
				if(useLightOnStart)
					blockScript.LightOnStart();	
				
				float y = 0;
				
				if(gui_RandomHeight)
					y = Random.Range(0,2) == 0 ? 0.1f : -0.1f;
				
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
		
		if(gui_RandomHeight)
			CalculateRandomHeight();
		
		initalized = true;
		
		HeuristicDistance();
		
		float endTime = Time.time;
	    float timeElapsed = (endTime-startTime);
		Debug.Log("Init Calculacted in " + timeElapsed + " ms");	
	}
	
	IEnumerator ShowBlocks() {
		float startTime = Time.time;
		
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
							blocks[index].StartAnimation();
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
							blocks[c].StartAnimation();
						else
							blocks[c].Show();	
				}
				
				lastStep = index;
				yield return new WaitForSeconds(0.05f);
			}
		}

		float endTime = Time.time;
	    float timeElapsed = (endTime-startTime);
		Debug.Log("ShowBlocks Calculacted in " + timeElapsed + " ms");
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
		
		float startTime = Time.time;
		
		
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

		float endTime = Time.time;
	    float timeElapsed = (endTime-startTime);
		Debug.Log("RandomHeight Calculacted in " + timeElapsed + " ms");	
	}
	
	public Vector2 GetBlockVec2Index(Block block) {
		
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
	
	public int GetBlockIndex(Block block) {
			
		for(int i=0; i < blocks.Length; i++) 
		{
			if(blocks[i] == block)
				return i;	
	
		}
		return -1;	
	}
	
	public Block GetBlock(int index) {
		if(index >= 0 && index < blocks.Length)
			return blocks[index];	
		
		return null;
	}
	
	#endregion
	
	
	#region Pathfind Functions
	
	public void HeuristicDistance() {
		
		float time = 0;
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
		
		time += Time.deltaTime;
		Debug.Log("HeuristicDistance Calculacted in " + time + " ms");
	}
	
	public void GetShortestPath(Block start, Block destination) {
		//PathFind ph = new PathFind(row*column);
		
		//Vector2 startb = GetBlockIndex(start);
		//Vector2 stopb = GetBlockIndex(destination);
	
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