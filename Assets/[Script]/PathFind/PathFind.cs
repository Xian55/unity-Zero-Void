using UnityEngine;
using System.Collections;
using System.Collections.Generic;

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
	
	public List<LinkData> path = new List<LinkData>();
	
	bool done_path = false;
	bool done_getBlocks = false;
	
	public int block_id_start = 0;
	public int block_id_end = 5;
	
	bool showTheWayRun = false;
	
	#endregion
	
	
	#region MonoBehave Functions
	
	void Update() {
		
		if(!BlockManager.Instance.initalized)
			return;
		
		if(!done_getBlocks) {
			StartBlock = BlockManager.Instance.GetBlock(block_id_start);
			EndBlock   = BlockManager.Instance.GetBlock(block_id_end);
			done_getBlocks = true;
		}
		else if(done_getBlocks && !done_path) {
			path = GetPath(StartBlock, EndBlock);
			done_path = true;
		}
		else {
			if(!showTheWayRun)
				StartCoroutine(ShowTheWay());
		}
		
	}
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
	
	
	#region PathFind by Xii
	
	public Block StartBlock;
	public Block EndBlock;
	
	public List<LinkData> GetPath(Block start, Block destination) {
		
		float startTime = Time.time;
		
		Block next = start;
		List<LinkData> path = new List<LinkData>();
		bool foundSelf = false;
		
		while(!foundSelf || next != destination) 
		{
			float nearestDistance = Mathf.Infinity;
			int neighborIndex = -1;
			
			for(int i=0; i<next.neighbor.Length; i++) 
			{
				Block check = next.GetNeighbor((Neighbor)i);
				
				if(check != null) 
				{
					float distance = Vector3.Distance(check.transform.position, destination.transform.position);
					
					//if(distance == 0.0f) {
					//	foundSelf = true;	
					//	Debug.Log("Found Self");
					//}
					
					if(distance < nearestDistance)
					{
						nearestDistance = distance;
						neighborIndex = i;
					}
				}
			}
			
			int nextBlockID = BlockManager.Instance.GetBlockIndex(next);
			Debug.Log(string.Format("Add new Point index:{0} | Neighbor:{1}({2})", nextBlockID, neighborIndex, ((Neighbor)neighborIndex)).ToString()  );
			
			path.Add(new LinkData(nextBlockID, neighborIndex));
			next = next.GetNeighbor((Neighbor)neighborIndex);
			
			
			//Megtalálta önmagát
			if(next == destination) {
				nextBlockID = BlockManager.Instance.GetBlockIndex(next);
				path.Add(new LinkData(nextBlockID, -1));	
				
				Debug.LogWarning(string.Format("Add END Point index:{0} | Neighbor:{1}", nextBlockID, -1));
				
				foundSelf = true;
			}
			
		}
		
		Debug.Log("Path Calculated in "+ (Time.time-startTime) +"ms! | Path Size: " + path.Count);
		return path;
	}
	
	public void OnDrawGizmos() {
		
		Gizmos.color = new Color(0.5f,0.1f,0.1f);
		
		if(path.Count > 1) {
			for(int i=1; i<path.Count; i++) {
				
				Vector3 l = BlockManager.Instance.GetBlock(path[i-1].id).transform.position;
				Block s = BlockManager.Instance.GetBlock(path[i].id);
				Vector3 sp = s.transform.position;
				Gizmos.DrawLine(new Vector3(l.x, 2, l.z), new Vector3(sp.x, 2, sp.z));
	
				Block chain = s.GetNeighbor((Neighbor)path[i].link);
				
				if(chain != null) {
					Vector3 n = chain.transform.position;
					Gizmos.DrawLine(new Vector3(sp.x, 2, sp.z), new Vector3(n.x, 2, n.z));
				}
				
				Gizmos.color += new Color(0.05f,0,0);
				
			}
		}
	}
	
	public IEnumerator ShowTheWay() {
		yield return new WaitForSeconds(path.Count * 0.05f);	
		
		Debug.Log("ShowTheWay");
		showTheWayRun = true;
		
		for(int i=0; i<path.Count; i++) {
			Block block = BlockManager.Instance.GetBlock(path[i].id);
			block.CollidePlayerWithColor(true, Color.blue);
				
			yield return new WaitForSeconds(0.5f);
			
			block.CollidePlayerWithColor(false, Color.white, 0.6f);	
		}
		
		showTheWayRun = false;
		
	}
	
	#endregion
}

[System.Serializable]
public class LinkData {
	public int id;
	public int link;
	
	public LinkData(int ID = 0, int LINK = 0) {
		link = LINK;
		id = ID;
	}
}
