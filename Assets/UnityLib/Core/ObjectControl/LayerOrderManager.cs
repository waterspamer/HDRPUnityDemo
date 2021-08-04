using UnityEngine;

namespace Nettle {

public class LayerOrderManager : MonoBehaviour {
	enum CustomOrder{
		Order_Unknown,
		Order_Geom,
		Order_Transparent
	}
	const int OrderRange = 300;
	
	public Material[] AdditionalMaterials;
	
	bool InRange(int rangeOffset, int val){
		return (val >= (rangeOffset - OrderRange)) && (val <= (rangeOffset + OrderRange));
	}
	
	bool IsValidOrder(int val){
		//geometry order
		if(InRange(2000, val) && (val != 2000))
			return true;
		else if(InRange(3000, val) && (val != 3000))
			return true;
		return false;
	}
	
	// Use this for initialization
	void Start () {
		//Background is 1000, Geometry is 2000, AlphaTest is 2450, Transparent is 3000 and Overlay is 4000
		Renderer[] renderers = (Renderer[])FindObjectsOfType(typeof(Renderer));
		for(int i = 0; i < renderers.Length; ++i){
			
			for(int j = 0; j < renderers[i].sharedMaterials.Length; ++j){
				if(renderers[i].sharedMaterials[j].HasProperty("_CustomDrawOrder")){
					//Debug.Log("Found material with layer property! Name = " + renderers[i].sharedMaterials[j].name);
					int order = (int)renderers[i].sharedMaterials[j].GetFloat("_CustomDrawOrder");
					if(IsValidOrder(order)){
						Debug.Log("Change render queue from " + renderers[i].sharedMaterials[j].renderQueue.ToString() +
							" to " + order.ToString() + " mat= " + renderers[i].sharedMaterials[j].name);
						renderers[i].sharedMaterials[j].renderQueue = order;
					}
				}
			}
		}
		
		if(AdditionalMaterials != null){
			for(int i = 0; i < AdditionalMaterials.Length; ++i){
				if(AdditionalMaterials[i].HasProperty("_CustomDrawOrder")){
					int order = (int)AdditionalMaterials[i].GetFloat("_CustomDrawOrder");
					if(IsValidOrder(order)){
						//Debug.Log("Change render queue from " + AdditionalMaterials[i].renderQueue.ToString() +
						//	" to " + order.ToString() + " additional mat= " + AdditionalMaterials[i].name);
						AdditionalMaterials[i].renderQueue = order;
					}
				}
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
}
