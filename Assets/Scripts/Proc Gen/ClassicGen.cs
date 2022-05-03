using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Procedural Level Generator for the Classic Tower of Death Wheels style (Straight Vertical or Horizontal non-stop until death)
public class ClassicGen : MonoBehaviour
{
    //Path Data for level
    struct ClassicPath{
        public int objNumber;
        public bool isMirror;

        public Vector2 startMin;
        public Vector2 startMax;
        public Vector2 endMin;
        public Vector2 endMax;
    }

    public bool vertical = true;    //Is the Generator for a Vertical or Horizontal scene

    public GameObject LevelObjectsParent;
    public GameObject[] StarterPrefabs; //Level pieces that are how the area begins
    private ClassicPath StarterPath;
    public List<GameObject> LevelObjects = new List<GameObject>();
    private List<ClassicPath> LevelPaths = new List<ClassicPath>();

    //private int genCount = 0;
    public int reshuffleRate = 0;   //How soon in generation count to re-shuffle level paths. If too big, will reshuffle when full paths list finished
    private float currentHeight = 0;
    private Vector2 prevMin;
    private Vector2 prevMax;
    
    public GameObject Camera;
    private float genDistance = 15;

    private int matchingPath = -1;  //Placed here so yielding can occur in while to prevent potential lag
    //private int pathCount = 0;
    private bool settingPiece = false;
    private bool setupComplete = false;
    public float prePlaceCount = 5;

    void Awake()
    {
        CollectLevelObjects();
        CollectChunkData();
        AddMirrorPaths();
        ShufflePaths();
        GenerateStarter();

        for(int i=0; i<prePlaceCount; i++){
            StartCoroutine(PlaceLevelTile());
        }

        setupComplete = true;
    }

    void Update(){  //Continuously check & move level chunks depending on Camera position
        if (Camera.transform.position.y + genDistance >= currentHeight && !settingPiece && setupComplete){
            StartCoroutine(PlaceLevelTile());
        }
    }

    private void CollectLevelObjects(){
        for (int i=0; i<LevelObjectsParent.transform.childCount; i++){
            LevelObjects.Add(LevelObjectsParent.transform.GetChild(i).gameObject);
        }
    }

    private void CollectChunkData(){
        int objCounter = 0;
        foreach(GameObject obj in LevelObjects){ //Grab Start & End locations for every Level Prefab
            ClassicPath newPath = new ClassicPath();

            newPath.objNumber = objCounter;
            newPath.isMirror = false;

            GameObject StartMinObj = obj.transform.Find("startMin").gameObject;
            newPath.startMin = new Vector2(StartMinObj.transform.localPosition.x, StartMinObj.transform.localPosition.y);

            GameObject StartMaxObj = obj.transform.Find("startMax").gameObject;
            newPath.startMax = new Vector2(StartMaxObj.transform.localPosition.x, StartMaxObj.transform.localPosition.y);

            GameObject EndMinObj = obj.transform.Find("endMin").gameObject;
            newPath.endMin = new Vector2(EndMinObj.transform.localPosition.x, EndMinObj.transform.localPosition.y);

            GameObject EndMaxObj = obj.transform.Find("endMax").gameObject;
            newPath.endMax = new Vector2(EndMaxObj.transform.localPosition.x, EndMaxObj.transform.localPosition.y);

            LevelPaths.Add(newPath);

            objCounter++;
        }
    }

    private void AddMirrorPaths(){
        int initialPaths = LevelPaths.Count;
        for (int i=0; i<initialPaths; i++){
            LevelPaths.Add(GetMirrorPath(LevelPaths[i]));
        }
    }

    private ClassicPath GetMirrorPath(ClassicPath path){    //Return coordinates of Mirrored Level
        ClassicPath mirroredPath = new ClassicPath();

        mirroredPath.isMirror = true;

        mirroredPath.startMin = new Vector2(-path.startMax.x, path.startMax.y);
        mirroredPath.startMax = new Vector2(-path.startMin.x, path.startMin.y);
        mirroredPath.endMin = new Vector2(-path.endMax.x, path.endMax.y);
        mirroredPath.endMax = new Vector2(-path.endMin.x, path.endMin.y);

        //This may introduce lag
        GameObject mirrorObj = GameObject.Instantiate(LevelObjects[path.objNumber], LevelObjectsParent.transform.position, Quaternion.identity);
        mirrorObj.transform.localScale = new Vector3(-1,1,1);
        LevelObjects.Add(mirrorObj);
            
        mirroredPath.objNumber = LevelObjects.Count-1;

        return mirroredPath;
    }

    private void ShufflePaths(){    //******CAUSING ERROR! Needs further research
        //LevelPaths = LevelPaths.OrderBy(i => Guid.NewGuid()).ToList();

        //Could have array of GameObjects, delete when under camera?

        for (int i = 0; i < LevelPaths.Count; i++) {
            ClassicPath temp = LevelPaths[i];
            int randomIndex = Random.Range(i, LevelPaths.Count);
            LevelPaths[i] = LevelPaths[randomIndex];
            LevelPaths[randomIndex] = temp;
        }
    }

    private void GenerateStarter(){
        int rand = Random.Range (0, StarterPrefabs.Length);

        ClassicPath starterPath = new ClassicPath();

        GameObject newStarterChunk = GameObject.Instantiate(StarterPrefabs[rand], new Vector3(0,0,0), Quaternion.identity);

        GameObject EndMinObj = StarterPrefabs[rand].transform.Find("endMin").gameObject;
        starterPath.endMin = new Vector2(EndMinObj.transform.position.x, EndMinObj.transform.position.y);

        GameObject EndMaxObj = StarterPrefabs[rand].transform.Find("endMax").gameObject;
        starterPath.endMax = new Vector2(EndMaxObj.transform.position.x, EndMaxObj.transform.position.y);

        starterPath.startMin = Vector2.zero;
        starterPath.startMax = Vector2.zero;

        bool mirrorStarter = false;
        if (Random.Range(0,2) == 1){
            mirrorStarter = true;
        }

        if (mirrorStarter){
            starterPath = GetMirrorPath(starterPath);

            newStarterChunk.transform.localScale = new Vector3(-1,1,1);
        }

        prevMin = starterPath.endMin;
        prevMax = starterPath.endMax;
        currentHeight += prevMin.y + 1;
    }

    /*IEnumerator GenerateLevel(){
        settingPiece = true;

        matchingPath = -1;  //Search for path that connects to previous, generate, add height, repeat till gen amount hit
        pathCount = 0;

        do{
            if (LevelPaths[pathCount].startMin.x <= prevMin.x+1 && LevelPaths[pathCount].startMax.x >= prevMax.x-1){
                matchingPath = pathCount;
            }

            pathCount++;
            if (pathCount >= LevelPaths.Count-1){
                Debug.Log("ERROR! No matches found. Performed " + pathCount + " searches.");
                matchingPath = 0;
            }

            yield return new WaitForSeconds(.1f);  //Return to loop later to prevent lag
        } while (matchingPath == -1);

        GameObject newLevelChunk = GameObject.Instantiate(LevelPrefabs[LevelPaths[matchingPath].objNumber], new Vector3(0,currentHeight,0), Quaternion.identity);

        if (LevelPaths[matchingPath].isMirror){
            newLevelChunk.transform.localScale = new Vector3(-1,1,1);
        }

        prevMin = LevelPaths[matchingPath].endMin;
        prevMax = LevelPaths[matchingPath].endMax;

        currentHeight += prevMin.y + 1;
        
        //Look back into this, could be better way to drop to back of list
        ClassicPath pathShuffle = LevelPaths[matchingPath];
        LevelPaths.RemoveAt(matchingPath);
        LevelPaths.Add(pathShuffle);

        if (genCount >= reshuffleRate || genCount >= LevelPaths.Count){
            ShufflePaths();
            genCount = 0;
        } else {
            genCount++;
        }

        settingPiece = false;
        yield return null;
    }*/

    IEnumerator PlaceLevelTile(){
        settingPiece = true;

        matchingPath = -1;  //Search for path that connects to previous, generate, add height, repeat till gen amount hit
        //pathCount = 0;

        for (int i=0; i<LevelPaths.Count; i++){
            if (LevelPaths[i].startMin.x <= prevMin.x+1 && LevelPaths[i].startMax.x >= prevMax.x-1){
                matchingPath = i;
                break;
            }
        }

        /*do{
            Debug.Log(pathCount + " - startMin: " + LevelPaths[pathCount].startMin.x + ", startMax: " + LevelPaths[pathCount].startMax.x);
            if (LevelPaths[pathCount].startMin.x <= prevMin.x+1 && LevelPaths[pathCount].startMax.x >= prevMax.x-1){
                matchingPath = pathCount;
            }

            pathCount++;
            if (pathCount >= LevelPaths.Count-1){
                Debug.Log("ERROR! No matches found. Performed " + (pathCount+1) + " searches.");
                matchingPath = 0;
            }

            yield return 0;  //Return to loop later to prevent lag
        } while (matchingPath == -1);*/

        LevelObjects[LevelPaths[matchingPath].objNumber].transform.position = new Vector3(0, currentHeight, 0);

        prevMin = LevelPaths[matchingPath].endMin;
        prevMax = LevelPaths[matchingPath].endMax;

        currentHeight += prevMin.y + 1;

        ClassicPath pathShuffle = LevelPaths[matchingPath];
        LevelPaths.RemoveAt(matchingPath);
        LevelPaths.Add(pathShuffle);

        /*if (genCount >= reshuffleRate || genCount >= LevelPaths.Count){
            ShufflePaths();
            genCount = 0;
        } else {
            genCount++;
        }*/ //Only shuffling at start now

        settingPiece = false;
        yield return null;
    }
}