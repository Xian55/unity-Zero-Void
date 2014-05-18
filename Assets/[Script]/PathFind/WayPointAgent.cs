using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WayPointAgent : MonoBehaviour {
	
	#region Variables
	
	public bool debug = true;
	
	bool done_path = false;
	bool done_getBlocks = false;
	
	bool showTheWayRun = false;
	
	
	public Block StartBlock;
	public Block EndBlock;
	
	public int block_id_start = 0;
	public int block_id_end = 5;
	
	
	public List<LinkData> path = new List<LinkData>();
	
	#endregion
	
	
	#region MonoBehave Functions

	void Start () { }
	
	void Update () {
		if(!BlockManager.Instance.isInitalized)
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
	
	
	#region PathFind by Xii
	
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
				Block check = next.GetNeighbor(i);
				
				if(check != null) 
				{
					float distance = Vector3.Distance(check.transform.position, destination.transform.position);
					
					if(distance < nearestDistance)
					{
						nearestDistance = distance;
						neighborIndex = i;
					}
				}
			}
			
			int nextBlockID = BlockManager.Instance.GetBlockIndex(next);
			if(debug)
				Debug.Log(string.Format("Add new Point index:{0} | Neighbor:{1}({2})", nextBlockID, neighborIndex, ((Neighbor)neighborIndex)).ToString()  );
			
			path.Add(new LinkData(nextBlockID, neighborIndex));
			next = next.GetNeighbor(neighborIndex);
			
			
			//Megtalálta önmagát
			if(next == destination) {
				nextBlockID = BlockManager.Instance.GetBlockIndex(next);
				path.Add(new LinkData(nextBlockID, -1));	
				
				if(debug)
					Debug.LogWarning(string.Format("Add END Point index:{0} | Neighbor:{1}", nextBlockID, -1));
				
				foundSelf = true;
			}
		}
		
		if(debug)
			Debug.Log("Path Calculated in "+ (Time.time-startTime) +"ms! | Path Size: " + path.Count);
		
		return path;
	}
	
	public void OnDrawGizmos() {
		
		Gizmos.color = new Color(0.5f,0.1f,0.1f);
		
		if(path.Count > 1) 
		{
			for(int i=1; i<path.Count; i++) 
			{
				Vector3 l = BlockManager.Instance.GetBlock(path[i-1].id).transform.position;
				Block s = BlockManager.Instance.GetBlock(path[i].id);
				Vector3 sp = s.transform.position;
				Gizmos.DrawLine(new Vector3(l.x, 2, l.z), new Vector3(sp.x, 2, sp.z));
	
				Block chain = s.GetNeighbor(path[i].link);
				
				if(chain != null) {
					Vector3 n = chain.transform.position;
					Gizmos.DrawLine(new Vector3(sp.x, 2, sp.z), new Vector3(n.x, 2, n.z));
				}
				
				Gizmos.color += new Color(0.05f, 0, 0);
				
			}
		}
	}
	
	public IEnumerator ShowTheWay() {
		
		showTheWayRun = true;
		
		yield return new WaitForSeconds(path.Count * 0.05f);	
		
		if(debug)
			Debug.Log("ShowTheWay");

		for(int i=0; i<path.Count; i++) 
		{
			Block block = BlockManager.Instance.GetBlock(path[i].id);
			
			if(block.ready) 
			{
				block.CollidePlayerWithColor(true, Color.blue);
					
				yield return new WaitForSeconds(0.5f);
				
				block.CollidePlayerWithColor(false, Color.white, 0.6f);	
			}
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
