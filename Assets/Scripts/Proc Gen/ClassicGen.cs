using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClassicGen : MonoBehaviour
{
    struct ClassicPath{
        public int prefabNumber;
        public bool isMirror;

        public Vector2 startMin;
        public Vector2 startMax;
        public Vector2 endMin;
        public Vector2 endMax;
    }

    public bool vertical = true;    //Will be used later to set up Horizontal Classic

    public GameObject[] StarterPrefabs; //Level pieces that are how the area begins
    private ClassicPath StarterPath;
    public GameObject[] LevelPrefabs;
    private List<ClassicPath> LevelPaths = new List<ClassicPath>();

    public int generationAmount;
    private int genCount = 0;
    public int reshuffleRate = 0;   //How soon in generation count to re-shuffle level paths. If too big, will reshuffle when full paths list finished
    private float currentHeight = 0;
    private Vector2 prevMin;
    private Vector2 prevMax;

    void Start()
    {
        CollectChunkData();
        AddMirrorPaths();
        ShufflePaths();
        GenerateStarter();
        GenerateLevel();

        Destroy(gameObject);
    }

    private void CollectChunkData(){
        int prefabCounter = 0;
        foreach(GameObject prefab in LevelPrefabs){ //Grab Start & End locations for every Level Prefab
            ClassicPath newPath = new ClassicPath();

            newPath.prefabNumber = prefabCounter;
            newPath.isMirror = false;

            GameObject StartMinObj = prefab.transform.Find("startMin").gameObject;
            newPath.startMin = new Vector2(StartMinObj.transform.position.x, StartMinObj.transform.position.y);

            GameObject StartMaxObj = prefab.transform.Find("startMax").gameObject;
            newPath.startMax = new Vector2(StartMaxObj.transform.position.x, StartMaxObj.transform.position.y);

            GameObject EndMinObj = prefab.transform.Find("endMin").gameObject;
            newPath.endMin = new Vector2(EndMinObj.transform.position.x, EndMinObj.transform.position.y);

            GameObject EndMaxObj = prefab.transform.Find("endMax").gameObject;
            newPath.endMax = new Vector2(EndMaxObj.transform.position.x, EndMaxObj.transform.position.y);

            LevelPaths.Add(newPath);

            prefabCounter++;
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

        mirroredPath.prefabNumber = path.prefabNumber;
        mirroredPath.isMirror = true;

        mirroredPath.startMin = new Vector2(-path.startMax.x, path.startMax.y);
        mirroredPath.startMax = new Vector2(-path.startMin.x, path.startMin.y);
        mirroredPath.endMin = new Vector2(-path.endMax.x, path.endMax.y);
        mirroredPath.endMax = new Vector2(-path.endMin.x, path.endMin.y);

        return mirroredPath;
    }

    private void ShufflePaths(){    //******CAUSING ERROR! Needs further research
        //LevelPaths = LevelPaths.OrderBy(i => Guid.NewGuid()).ToList();
    }

    private void GenerateStarter(){
        int rand = Random.Range (0, StarterPrefabs.Length);

        Instantiate(StarterPrefabs[rand], new Vector3(0,0,0), Quaternion.identity);

        GameObject EndMinObj = StarterPrefabs[rand].transform.Find("endMin").gameObject;
        prevMin = new Vector2(EndMinObj.transform.position.x, EndMinObj.transform.position.y);

        GameObject EndMaxObj = StarterPrefabs[rand].transform.Find("endMax").gameObject;
        prevMax = new Vector2(EndMaxObj.transform.position.x, EndMaxObj.transform.position.y);

        currentHeight += prevMin.y;
    }

    private void GenerateLevel(){
        for (int i = 0; i< generationAmount; i++){
            int matchingPath = -1;  //Search for path that connects to previous, generate, add height, repeat till gen amount hit
            int pathCount = 0;

            Debug.Log("Searching for >= " + prevMin.x + " & <= " + prevMax.x);

            do{
                if (LevelPaths[pathCount].startMin.x <= prevMin.x && LevelPaths[pathCount].startMax.x >= prevMax.x){
                    matchingPath = pathCount;
                }

                pathCount++;
                if (pathCount >= LevelPaths.Count-1){
                    Debug.Log("ERROR! No matches found. Performed " + pathCount + " searches.");
                    matchingPath = 0;
                }
            } while (matchingPath == -1);

            GameObject newLevelChunk = GameObject.Instantiate(LevelPrefabs[LevelPaths[matchingPath].prefabNumber], new Vector3(0,currentHeight,0), Quaternion.identity);

            if (LevelPaths[matchingPath].isMirror){
                newLevelChunk.transform.localScale = new Vector3(-1,1,1);
            }

            prevMin = LevelPaths[matchingPath].endMin;
            prevMax = LevelPaths[matchingPath].endMax;

            currentHeight += prevMin.y;
            
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
        }
    }
}