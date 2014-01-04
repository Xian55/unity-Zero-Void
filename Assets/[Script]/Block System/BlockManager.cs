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
	
	public Chunk chunk;
	
	public Block[] blocks;
	public int row = 10;
	public int column = 10;
	
	public float padding = .1f;
	
	public float waitFewSeconds = 3;
	
	#region GUI Variables
	public bool GUIStart = false;
	bool GUIStart_InProgress = false;

	int	gui_lineHeight = 20;
	int gui_start_x = 10;
	int gui_start_y = 10;
	
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
			if(value == true) 
			{
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
	
	//Waypoints
	WayPointAgent wp;
	string gui_wp_start_id = "0";
	string gui_wp_end_id   = "42";
	bool   gui_wp_random_end = false;
	

    //FPS
    private float updateInterval = .016f;
 
    private float accum = 0; // FPS accumulated over the interval
    private float frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

	#endregion

	#endregion
	
	
	#region MonoBehave Functions
	
	void Awake() {
		prefab_Block = Resources.Load("Prefab/" + prefab_version) as GameObject;
		player       = GameObject.FindWithTag("Player").transform;
		blockParent  = GameObject.FindWithTag("Respawn");
		
		//!HACK needs to improve this system to handle multiple WP
		wp           = FindObjectOfType(typeof(WayPointAgent)) as WayPointAgent;
		
		if(prefab_Block && !GUIStart)
			Initalize();
		
		if(GUIStart) {
			SetPlayerComponents(false);
			GUIStart_InProgress = true;
		}
	}

    void Start()
    {
        timeleft = updateInterval;  
    }

	void Update() 
	{
		if(!GUIStart || !GUIStart_InProgress) 
		{
			if(waitFewSeconds < 0) 
			{
				if(prefab_Block && !justOnce && initalized) 
				{
					StartCoroutine("ShowBlocks");
					justOnce = true;
				}
			}
			else
				waitFewSeconds -= Time.deltaTime;
		}
		
		if(player.position.y < -5) 
		{
			player.position = new Vector3(0,5,0);
			player.rotation = Quaternion.identity;
		}
		
		else if(Input.GetKey(KeyCode.T)) 
		{
			Screen.fullScreen = !Screen.fullScreen;
			
			if(!GUIStart)
				Screen.lockCursor = !Screen.lockCursor;
		}	


        //fps
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            // display two fractional digits (f2 format)
            float fps = accum / frames;
            string format = System.String.Format("{0:F2} FPS", fps);
            guiText.text = format;

            if (fps < 30)
                guiText.material.color = Color.yellow;
            else
                if (fps < 10)
                    guiText.material.color = Color.red;
                else
                    guiText.material.color = Color.green;

            timeleft = updateInterval;
            accum = 0.0F;
            frames = 0;
        }

	}
	
	void OnGUI() {
		ShowStartGUI();
	}
	
	#endregion
	
	
	#region Blocks Functions
	
	void Initalize() {
		
		float startTime = Time.time;
	 
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
				blocks[index] = blockScript;
				
				CalculateNeighbors(x, z);
				
				index++;
			}
		}
		
		if(gui_RandomHeight)
			CalculateRandomHeight();
		
		initalized = true;
		
		
		//chunk = new Chunk(15, 5);
		chunk = gameObject.AddComponent<Chunk>();
		chunk.Set(15,5);
		chunk.MoveBlocks(new Vector3(0, 1, 0));
		
		
		HeuristicDistance();
		
		Debug.Log("Init Calculacted in " + (Time.time-startTime) + " ms");	
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
							StartCoroutine(GetBlock(index).StartAnimation());
						else
							StartCoroutine(GetBlock(index).Show());
						
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
							StartCoroutine(GetBlock(c).StartAnimation());
						else
							StartCoroutine(GetBlock(c).Show());	
				}
				
				lastStep = index;
				yield return new WaitForSeconds(0.05f);
			}
		}

		Debug.Log("ShowBlocks Calculacted in " + (Time.time-startTime) + " ms");
	}
	
	void CalculateNeighbors(int x, int z) 
	{
		int index = row * x + z;
		
		//South - North
		if(index > 0 && z < column && index % row != 0) 
		{
			GetBlock(index).SetNeighbor(Neighbor.South, GetBlock(index-1));
			GetBlock(index-1).SetNeighbor(Neighbor.North, GetBlock(index));
		}
		
		//East - West
		if(index >= row) 
		{
			GetBlock(index).SetNeighbor(Neighbor.West, GetBlock(index-row));
			GetBlock(index-row).SetNeighbor(Neighbor.East, GetBlock(index));
		}	
	}
	
	void CalculateRandomHeight() {
		
		float startTime = Time.time;

		int index = 0;
		for(int x=0; x<row; x++) 
		{
			for(int z=0; z<column; z++) 
			{
				if(index == 0) 
				{
					index++;
					continue;
				}
			
				float y = 0;
				
				bool south = false;
				bool north = false;
				
				bool west = false;
				bool east = false;
				
				if(GetBlock(index).GetNeighbor(Neighbor.South) != null) {
					y += GetBlock(index).GetNeighbor(Neighbor.South).transform.position.y;
					south = true;
				}
				
				if(GetBlock(index).GetNeighbor(Neighbor.North) != null) {
					y += GetBlock(index).GetNeighbor(Neighbor.North).transform.position.y;
					north = true;
				}
				
				if(GetBlock(index).GetNeighbor(Neighbor.West) != null) {
					y += GetBlock(index).GetNeighbor(Neighbor.West).transform.position.y;
					west = true;
				}
				
				if(GetBlock(index).GetNeighbor(Neighbor.East) != null) {
					y += GetBlock(index).GetNeighbor(Neighbor.East).transform.position.y;
					east = true;
				}
				
				if(south && north) 
					y /= 2 + 0.2f;
				else if(west && east)
					y /= 2 + 0.2f;
				else
					y = 0.2f;
				
				//Debug.Log(GetBlock(index).name + " Calculated:" + y);
				GetBlock(index).AddYPos(y);
				
				index++;
			}	
		}

		Debug.Log("RandomHeight Calculacted in " + (Time.time-startTime) + " ms");	
	}
	
	void SetPlayerComponents(bool b) 
	{
		player.SendMessage("Enable", b, SendMessageOptions.DontRequireReceiver);
		player.GetComponent<MouseLook>().enabled = b;
		player.GetComponent<FireBlasterScript>().enabled = b;
		Camera.main.GetComponent<MouseLook>().enabled = b;
	}
	
	string StringToIntWithError(string src, out int try_out) 
	{
		if(int.TryParse(src, out try_out) == false || try_out < 0) 
			return "Invalid";	

		return src;
	}
	
	
	void ShowStartGUI() {
		
		if(GUIStart_InProgress) 
		{
			GUI.color = Color.red;
			
			GUI.Box(new Rect(gui_start_x-5, gui_start_y-5,                      280, 9*gui_lineHeight+5), "Grid System Settings");
			
			GUI.color = Color.white;
			
			GUI.Label(new Rect(gui_start_x, gui_start_y + gui_lineHeight,   105, gui_lineHeight), "Block Scale (int)"); gui_Size   = GUI.TextField(new Rect(105, gui_start_y +   gui_lineHeight, 3*gui_lineHeight, gui_lineHeight), gui_Size);
			GUI.Label(new Rect(gui_start_x, gui_start_y + 2*gui_lineHeight,  75, gui_lineHeight), "Row (int)");         gui_Row    = GUI.TextField(new Rect(105, gui_start_y + 2*gui_lineHeight, 3*gui_lineHeight, gui_lineHeight), gui_Row);
			GUI.Label(new Rect(gui_start_x, gui_start_y + 3*gui_lineHeight,  75, gui_lineHeight), "Column (int)");      gui_Column = GUI.TextField(new Rect(105, gui_start_y + 3*gui_lineHeight, 3*gui_lineHeight, gui_lineHeight), gui_Column);
			
			GUI.Label(new Rect(gui_start_x, gui_start_y + 4*gui_lineHeight, 400, gui_lineHeight), "What method would you like to use at start?");
			
			gui_Animation = GUI.Toggle(new Rect(gui_start_x + gui_padding, gui_start_y + 5*gui_lineHeight, 100, gui_lineHeight),  gui_UseAnimation, "Animation");
			gui_Fade      = GUI.Toggle(new Rect(2*(gui_start_x + gui_padding) + GUILayoutUtility.GetRect(new GUIContent("Animation"), "label").width, gui_start_y + 5*gui_lineHeight, 200, gui_lineHeight),  gui_UseFade, "Fade");
			
			gui_FastSpawn    = GUI.Toggle(new Rect(gui_start_x, gui_start_y + 6*gui_lineHeight, 200, gui_lineHeight),    gui_FastSpawn, "Fast Spawn?");
			gui_LigthOnStart = GUI.Toggle(new Rect(gui_start_x, gui_start_y + 7*gui_lineHeight, 200, gui_lineHeight), gui_LigthOnStart, "Light On Start?");
			
			gui_RandomHeight = GUI.Toggle(new Rect(gui_start_x, gui_start_y + 8*gui_lineHeight, 200, gui_lineHeight), gui_RandomHeight, "Random Height?");
			
			
			//WayPoint
			if(wp != null) {
				GUI.color = Color.red;
				GUI.Box  (new Rect(gui_start_x + 295, gui_start_y-5,                  250, 9*gui_lineHeight+5), "Waypoints System Settings");
				
				GUI.color = Color.white;
				GUI.Label(new Rect(gui_start_x + 305, gui_start_y +   gui_lineHeight, 150, gui_lineHeight), "Start Block Index (int)");   gui_wp_start_id = GUI.TextField(new Rect(150 + 300, gui_start_y +   gui_lineHeight, 3*gui_lineHeight, gui_lineHeight), gui_wp_start_id);
				
				gui_wp_random_end = GUI.Toggle(new Rect(gui_start_x + 305, gui_start_y + 2*gui_lineHeight, 150, gui_lineHeight),  gui_wp_random_end, "Use Random End Point");
				
				if(!gui_wp_random_end) {
					GUI.Label( new Rect(gui_start_x + 305, gui_start_y + 3*gui_lineHeight, 150, gui_lineHeight), "End Block Index   (int)");   gui_wp_end_id   = GUI.TextField(new Rect(150 + 300, gui_start_y + 3*gui_lineHeight, 3*gui_lineHeight, gui_lineHeight), gui_wp_end_id);
				}
			}
			
			
			if(GUI.Button(new Rect(gui_start_x + gui_padding + 215, gui_start_y + 9.5f*gui_lineHeight, 80, gui_lineHeight), "Start")) 
			{	
				int try_size     = -1;
				int try_row      = -1;
				int try_column   = -1;
				int try_wp_start = -1;
				int try_wp_end   = -1;
				
				
				gui_Size    = StringToIntWithError(  gui_Size, out try_size);
				gui_Row     = StringToIntWithError(   gui_Row, out try_row);
				gui_Column  = StringToIntWithError(gui_Column, out try_column);
				
				if(wp != null) {
					gui_wp_start_id = StringToIntWithError(gui_wp_start_id, out try_wp_start);
					gui_wp_end_id   = StringToIntWithError(  gui_wp_end_id, out try_wp_end);
				}
				
				
				//If everything is OK and Valid
				if(try_size > 0 && try_row > 0 && try_column > 0 || (wp != null && try_wp_start > 0 && try_wp_end > 0)) {
					
					blockSize = new Vector3(try_size, 1, try_size);
					
					row = try_row;
					column = try_column;

					useAnimation    = gui_UseAnimation;
					useFastSpawn    = gui_FastSpawn;
					useLightOnStart = gui_LigthOnStart;
					
					Initalize();
					
					//WayPoints add
					if(wp != null) {
						wp.block_id_start = try_wp_start;
						wp.block_id_end   =  gui_wp_random_end ? Random.Range(0, row * column) : try_wp_end; 
					}
					
					GUIStart_InProgress = false;
					SetPlayerComponents(true);
				}
			}	
		}
			
	}
	
	
	public Vector2 GetBlockVec2Index(Block block) {
		
		for(int x=0; x < row; x++) 
		{
			for (int z=0; z < column; z++)
			{
				int index = row * x + z;
				
				if(GetBlock(index) == block) {
					return new Vector2(x,z);	
				}
			}	
		}
		return new Vector2(-1,-1);
	}
	
	public int GetBlockIndex(Block block) {
			
		for(int i=0; i < blocks.Length; i++) 
		{
			if(GetBlock(i) == block)
				return i;	
	
		}
		return -1;	
	}
	
	public Block GetBlock(int index) {
		if(index >= 0 && index < blocks.Length)
			return blocks[index];	
		
		Debug.LogError("GetBlock -> Block Not Found");
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
				
				GetBlock(index).heuristicArray = new int[row*column];
				
				for (int bx = 0; bx < row; bx++)
				{
					for (int bz = 0; bz < column; bz++)
					{	
						int p_index = row * bx + bz;
						GetBlock(index).heuristicArray[p_index] = Mathf.Abs(x - bx) + Mathf.Abs(z - bz);
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