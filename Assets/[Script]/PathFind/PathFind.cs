using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Path find Class
/// 
/// Temporary not used because need more research about A* Star
/// 
/// </summary>

public class PathFind : MonoBehaviour {
	
	#region Variables
	
	int heuristic_value = -1;
	int movement_value  = -1;
	
	public Block parent;
	public Block[] backTrack;
	
	public List<Block> openList;
	public List<Block> closedList;
	
	public int HValue {
		get { return heuristic_value; }
		set { heuristic_value = value; }
	}
	
	public int GValue {
		get { return movement_value; }
		set { movement_value = value; }
	}
	
	public int TotalValue {
		get { return heuristic_value + movement_value; }
	}
	
	#endregion
	

	#region MonoBehave Functions
	
	#endregion
	
	
	#region Other Functions
	
	public PathFind(int capacity) {
		openList   = new List<Block>(capacity);	
		closedList = new List<Block>(capacity);
	}
	
	public void Initalize() {
			
	}
	
	public void AddToClosedList(Block block) {
		if(!ContainClosedList(block))
			closedList.Add(block);
	}
	
	public void AddToOpenList(Block block) {
		if(!ContainOpenList(block))
			openList.Add(block);
	}
	
	
	public bool ContainClosedList(Block block) {
		return (closedList.Contains(block));
	}
	
	public bool ContainOpenList(Block block) {
		return (openList.Contains(block));
	}
	
	#endregion
	
}
