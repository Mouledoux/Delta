/********************************************************************
 * Author : Code from masterprompt, project from Berenger
 * 
 * (http://forum.unity3d.com/threads/16485-quot-stitch-multiple-
 * body-parts-into-one-character-quot?p=126864&viewfull=1#post126864)
 * 
 * PS : I did the rig and the animation, but the mesh is from there
 * http://opengameart.org/content/low-poly-base-meshes-male-female
 * ******************************************************************/


using UnityEngine;
using System.Collections;

public class SwitchEquipment : MonoBehaviour 
{
	public GameObject shorty;
	
	private bool isWorn = false;
	private GameObject shortyOnceWorn;
	
	
	private void Update()
	{
		if( Input.GetKeyUp( KeyCode.Space ) ){
			if( isWorn ) RemoveEquipment();
			else AddEquipment();
		}
	}

	private void AddEquipment()
	{
		isWorn = true;
		
		// Here, boneObj must be instatiated and active (at least the one with the renderer),
		// or else GetComponentsInChildren won't work.
		SkinnedMeshRenderer[] BonedObjects = shorty.GetComponentsInChildren<SkinnedMeshRenderer>();
		foreach( SkinnedMeshRenderer smr in BonedObjects )
			ProcessBonedObject( smr ); 
		
		// We don't need the old obj, we make it disappear.
		shorty.SetActiveRecursively( false );
	}
	
	private void RemoveEquipment()
	{
		isWorn = false;		
		
		Destroy( shortyOnceWorn );
		
		shorty.SetActiveRecursively( true );
	}
	
	private void ProcessBonedObject( SkinnedMeshRenderer ThisRenderer )
	{		
	    // Create the SubObject
		shortyOnceWorn = new GameObject( ThisRenderer.gameObject.name );	
	    shortyOnceWorn.transform.parent = transform;
	
	    // Add the renderer
	    SkinnedMeshRenderer NewRenderer = shortyOnceWorn.AddComponent( typeof( SkinnedMeshRenderer ) ) as SkinnedMeshRenderer;
	
	    // Assemble Bone Structure	
	    Transform[] MyBones = new Transform[ ThisRenderer.bones.Length ];
	
		// As clips are using bones by their names, we find them that way.
	    for( int i = 0; i < ThisRenderer.bones.Length; i++ )
	        MyBones[ i ] = FindChildByName( ThisRenderer.bones[ i ].name, transform );
	
	    // Assemble Renderer	
	    NewRenderer.bones = MyBones;	
	    NewRenderer.sharedMesh = ThisRenderer.sharedMesh;	
	    NewRenderer.materials = ThisRenderer.materials;	
	}
	
	// Recursive search of the child by name.
	private Transform FindChildByName( string ThisName, Transform ThisGObj )	
	{	
	    Transform ReturnObj;
	
		// If the name match, we're return it
	    if( ThisGObj.name == ThisName )	
	        return ThisGObj.transform;
	
		// Else, we go continue the search horizontaly and verticaly
	    foreach( Transform child in ThisGObj )	
	    {	
	        ReturnObj = FindChildByName( ThisName, child );
	
	        if( ReturnObj != null )	
	            return ReturnObj;	
	    }
	
	    return null;	
	}	
}
