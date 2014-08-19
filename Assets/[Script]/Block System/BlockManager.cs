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
    private Transform _player;
    private Vector3 _blockSize = new Vector3(2, 1, 2);

    public bool initalized = false;
    private bool _isSpawned = false;

    private GameObject _prefab_Block;
	public static string prefab_version = "BlockV4";


    private GameObject _blockParent;
	
	public bool isInitalized = false;
	
	public bool isUseAnimation = false;
	public bool isUseFastSpawn = true;
	public bool isUseLightOnStart = false;
	
	public Chunk chunk;
	
	public Block[] blocks;
	public int row = 10;
	public int column = 10;
	
	public float padding = .1f;
	
	public float waitToSpawn = 3;
	

	    #region GUI Variables

	public bool isGUIStart = false;

	private bool _isGUIStart_InProgress = false;

    private int _gui_lineHeight = 20;
    private int _gui_start_x = 10;
    private int _gui_start_y = 10;

    private int _gui_padding = 30;

    private string _gui_Size = "2";
    private string _gui_Row = "100";
    private string _gui_Column = "100";

   
    private bool _is_gui_UseFade = true;
    private bool is_gui_Fade
    {
		set {
			if(value == true) {
				_is_gui_UseFade = true;
				_is_gui_UseAnimation = false;
			}
		}
		
		get {
			return _is_gui_UseFade;
		}
	}

    private bool _is_gui_UseAnimation = false;
    private bool gui_Animation
    {
		set {
			if(value == true) 
			{
				_is_gui_UseAnimation = true;
				_is_gui_UseFade = false;
			}
		}
		
		get {
			return _is_gui_UseAnimation;
		}
	}


    private bool _is_gui_FastSpawn = false;
    private bool _is_gui_LigthOnStart = false;

    private bool _is_gui_RandomHeight = false;
	
	//Waypoints
    private WayPointAgent _wp;
    private string _gui_wp_start_id = "0";
    private string _gui_wp_end_id = "42";
    private bool _is_gui_wp_random_end = false;
	

    //FPS
    private float _updateInterval = .016f;
 
    private float _accum = 0; // FPS accumulated over the interval
    private float _frames = 0; // Frames drawn over the interval
    private float _timeleft; // Left time for current interval

	    #endregion

	#endregion
	
	
	#region MonoBehave Functions
	
	private void Awake() {
		_prefab_Block = Resources.Load("Prefab/" + prefab_version) as GameObject;
		_player       = GameObject.FindWithTag("Player").transform;
		_blockParent  = GameObject.FindWithTag("Respawn");
		
		//!HACK needs to improve this system to handle multiple WP
		_wp           = FindObjectOfType(typeof(WayPointAgent)) as WayPointAgent;
		
		if(_prefab_Block && !isGUIStart)
			Initalize();
		
		if(isGUIStart) {
			SetPlayerComponents(false);
			_isGUIStart_InProgress = true;
		}
	}

    private void Start()
    {
        _timeleft = _updateInterval;  
    }

    private void Update() 
	{
		if(!isGUIStart || !_isGUIStart_InProgress) 
		{
			if(waitToSpawn < 0) 
			{
				if(_prefab_Block && !_isSpawned && isInitalized) 
				{
					StartCoroutine("ShowBlocks");
					_isSpawned = true;
				}
			}
			else
				waitToSpawn -= Time.deltaTime;
		}
		

        //FIX Player Fall down
		if(_player.position.y < -5) 
		{
			_player.position = new Vector3(0, 5, 0);
			_player.rotation = Quaternion.identity;
		}
		

        //Screen Full/Windowed
		else if(Input.GetKey(KeyCode.T)) 
		{
			Screen.fullScreen = !Screen.fullScreen;
			
			if(!isGUIStart)
				Screen.lockCursor = !Screen.lockCursor;
		}	


        //FPS calculation
        _timeleft -= Time.deltaTime;
        _accum += Time.timeScale / Time.deltaTime;
        ++_frames;


        //FPS Display
        if (_timeleft <= 0.0f)
        {
            // display two fractional digits (f2 format)
            float fps = _accum / _frames;
            string format = System.String.Format("{0:F2} FPS", fps);
            guiText.text = format;

            if (fps < 30)
                guiText.material.color = Color.yellow;
            else
                if (fps < 10)
                    guiText.material.color = Color.red;
                else
                    guiText.material.color = Color.green;

            _timeleft = _updateInterval;
            _accum = 0.0f;
            _frames = 0;
        }

	}

    private void OnGUI() 
    {
		ShowStartGUI();
	}
	
	#endregion
	
	
	#region Blocks Functions

    private void Initalize()
    {
		float startTime = Time.time;
	 
		blocks = new Block[row*column];
		int index = 0;
		
		for(int x=0; x<row; x++) {
			for(int z=0; z<column; z++) {
				
				GameObject block = Instantiate(_prefab_Block, Vector3.zero, Quaternion.identity) as GameObject;
				
				block.name = x + "_" + z; 
				block.transform.parent = _blockParent.transform;
				
				Block blockScript = block.GetComponent<Block>();
				
				if(index > 0)
					blockScript.Hide();
				
				blockScript.SetScale(_blockSize);
				blockScript.SetVersion(prefab_version);
				
				if(isUseLightOnStart)
					blockScript.LightOnStart();	
				
				float y = 0;
				
				if(_is_gui_RandomHeight)
					y = Random.Range(0,2) == 0 ? 0.1f : -0.1f;
				
				blockScript.SetPosition(new Vector3( x*(_blockSize.x+padding), y, z*(_blockSize.z+padding)));
				blocks[index] = blockScript;
				
				CalculateNeighbors(x, z);
				
				index++;
			}
		}
		
		if(_is_gui_RandomHeight)
			CalculateRandomHeight();
		
		isInitalized = true;
		
		
		//chunk = new Chunk(15, 5);
		chunk = gameObject.AddComponent<Chunk>();
		chunk.Set(15,5);
		chunk.MoveBlocks(new Vector3(0, 1, 0));
		
		
		HeuristicDistance();
		
		Debug.Log("Init Calculacted in " + (Time.time-startTime) + " ms");	
	}

    private IEnumerator ShowBlocks()
    {
		float startTime = Time.time;
		
		int index = 0;
		int lastStep = 0;
		
		for(int x=0; x<row; x++) 
		{	
			for(int z=0; z<column; z++) 
			{
				//Slow animation
				if(!isUseFastSpawn) 
				{
					if(index > 0) 
					{ 
						if(isUseAnimation)
							StartCoroutine(GetBlock(index).StartAnimation());
						else
							StartCoroutine(GetBlock(index).Show());
						
						yield return new WaitForSeconds(0.1f);
					}
				}
				
				index++;
			}
			
			//Fast animation
			if(isUseFastSpawn) 
			{
				for(int c=lastStep; c < index; c++) 
				{
					if(c > 0)
						if(isUseAnimation)
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

    private void CalculateNeighbors(int x, int z) 
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

    private void CalculateRandomHeight()
    {
		
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

    private void SetPlayerComponents(bool b) 
	{
		_player.SendMessage("Enable", b, SendMessageOptions.DontRequireReceiver);
		_player.GetComponent<MouseLook>().enabled = b;
		_player.GetComponent<FireBlasterScript>().enabled = b;
		Camera.main.GetComponent<MouseLook>().enabled = b;
	}

    private string StringToIntWithError(string src, out int try_out) 
	{
		if(int.TryParse(src, out try_out) == false || try_out < 0) 
			return "Invalid";	

		return src;
	}


    private void ShowStartGUI()
    {
		
		if(_isGUIStart_InProgress) 
		{
			GUI.color = Color.red;
			
			GUI.Box(new Rect(_gui_start_x-5, _gui_start_y-5,                      280, 9*_gui_lineHeight+5), "Grid System Settings");
			
			GUI.color = Color.white;
			
			GUI.Label(new Rect(_gui_start_x, _gui_start_y + _gui_lineHeight,   105, _gui_lineHeight), "Block Scale (int)"); _gui_Size   = GUI.TextField(new Rect(105, _gui_start_y +   _gui_lineHeight, 3*_gui_lineHeight, _gui_lineHeight), _gui_Size);
			GUI.Label(new Rect(_gui_start_x, _gui_start_y + 2*_gui_lineHeight,  75, _gui_lineHeight), "Row (int)");         _gui_Row    = GUI.TextField(new Rect(105, _gui_start_y + 2*_gui_lineHeight, 3*_gui_lineHeight, _gui_lineHeight), _gui_Row);
			GUI.Label(new Rect(_gui_start_x, _gui_start_y + 3*_gui_lineHeight,  75, _gui_lineHeight), "Column (int)");      _gui_Column = GUI.TextField(new Rect(105, _gui_start_y + 3*_gui_lineHeight, 3*_gui_lineHeight, _gui_lineHeight), _gui_Column);
			
			GUI.Label(new Rect(_gui_start_x, _gui_start_y + 4*_gui_lineHeight, 400, _gui_lineHeight), "What method would you like to use at start?");
			
			gui_Animation = GUI.Toggle(new Rect(_gui_start_x + _gui_padding, _gui_start_y + 5*_gui_lineHeight, 100, _gui_lineHeight),  _is_gui_UseAnimation, "Animation");
			is_gui_Fade      = GUI.Toggle(new Rect(2*(_gui_start_x + _gui_padding) + GUILayoutUtility.GetRect(new GUIContent("Animation"), "label").width, _gui_start_y + 5*_gui_lineHeight, 200, _gui_lineHeight),  _is_gui_UseFade, "Fade");
			
			_is_gui_FastSpawn    = GUI.Toggle(new Rect(_gui_start_x, _gui_start_y + 6*_gui_lineHeight, 200, _gui_lineHeight),    _is_gui_FastSpawn, "Fast Spawn?");
			_is_gui_LigthOnStart = GUI.Toggle(new Rect(_gui_start_x, _gui_start_y + 7*_gui_lineHeight, 200, _gui_lineHeight), _is_gui_LigthOnStart, "Light On Start?");
			
			_is_gui_RandomHeight = GUI.Toggle(new Rect(_gui_start_x, _gui_start_y + 8*_gui_lineHeight, 200, _gui_lineHeight), _is_gui_RandomHeight, "Random Height?");
			
			
			//WayPoint
			if(_wp != null) {
				GUI.color = Color.red;
				GUI.Box  (new Rect(_gui_start_x + 295, _gui_start_y-5,                  250, 9*_gui_lineHeight+5), "Waypoints System Settings");
				
				GUI.color = Color.white;
				GUI.Label(new Rect(_gui_start_x + 305, _gui_start_y +   _gui_lineHeight, 150, _gui_lineHeight), "Start Block Index (int)");   _gui_wp_start_id = GUI.TextField(new Rect(150 + 300, _gui_start_y +   _gui_lineHeight, 3*_gui_lineHeight, _gui_lineHeight), _gui_wp_start_id);
				
				_is_gui_wp_random_end = GUI.Toggle(new Rect(_gui_start_x + 305, _gui_start_y + 2*_gui_lineHeight, 150, _gui_lineHeight),  _is_gui_wp_random_end, "Use Random End Point");
				
				if(!_is_gui_wp_random_end) {
					GUI.Label( new Rect(_gui_start_x + 305, _gui_start_y + 3*_gui_lineHeight, 150, _gui_lineHeight), "End Block Index   (int)");   _gui_wp_end_id   = GUI.TextField(new Rect(150 + 300, _gui_start_y + 3*_gui_lineHeight, 3*_gui_lineHeight, _gui_lineHeight), _gui_wp_end_id);
				}
			}
			
			
			if(GUI.Button(new Rect(_gui_start_x + _gui_padding + 215, _gui_start_y + 9.5f*_gui_lineHeight, 80, _gui_lineHeight), "Start")) 
			{	
				int try_size     = -1;
				int try_row      = -1;
				int try_column   = -1;
				int try_wp_start = -1;
				int try_wp_end   = -1;
				
				
				_gui_Size    = StringToIntWithError(  _gui_Size, out try_size);
				_gui_Row     = StringToIntWithError(   _gui_Row, out try_row);
				_gui_Column  = StringToIntWithError(_gui_Column, out try_column);
				
				if(_wp != null) {
					_gui_wp_start_id = StringToIntWithError(_gui_wp_start_id, out try_wp_start);
					_gui_wp_end_id   = StringToIntWithError(  _gui_wp_end_id, out try_wp_end);
				}
				
				
				//If everything is OK and Valid
				if(try_size > 0 && try_row > 0 && try_column > 0 || (_wp != null && try_wp_start > 0 && try_wp_end > 0)) {
					
					_blockSize = new Vector3(try_size, 1, try_size);
					
					row = try_row;
					column = try_column;

					isUseAnimation    = _is_gui_UseAnimation;
					isUseFastSpawn    = _is_gui_FastSpawn;
					isUseLightOnStart = _is_gui_LigthOnStart;
					
					Initalize();
					
					//WayPoints add
					if(_wp != null) {
						_wp.block_id_start = try_wp_start;
						_wp.block_id_end   =  _is_gui_wp_random_end ? Random.Range(0, row * column) : try_wp_end; 
					}
					
					_isGUIStart_InProgress = false;
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