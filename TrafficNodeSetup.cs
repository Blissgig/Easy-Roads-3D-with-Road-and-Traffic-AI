using EasyRoads3Dv3;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[ExecuteInEditMode]
public class TrafficNodeSetup : MonoBehaviour
{
    public float laneWidth = 5;
    public int minNodesLaneChange = 3; //This is to insure than when a vehicle changes lanes it's not DIRECTLY to the side, depending on the spacing between Nodes when created as Side Objects.  See "Distance Between Objects" in the "Road Network"/
    public int maxNodesLaneChange = 8;
    public string nodeFolderName = "HBC Traffic Nodes";  //This is used in "SetNodes()" to avoid affecting nodes that have already been moved.  Rename it for your project but be aware that the name MUST be different than the Side-Object name used when genarating the Nodes
    public Transform trafficNodeInstance;

    private ERRoadNetwork roadNetwork;
    private System.Random random;


    public void HideUIElements()
    {
        roadNetwork = new ERRoadNetwork();
        
        foreach (Transform child in transform)
        {
            HideUISubNode(child);
        }
    }

    private void HideUISubNode(Transform parent)
    {
        TrafficSystemNode tsn;

        foreach (Transform child in parent)
        {
            tsn = child.GetComponent<TrafficSystemNode>();

            if (tsn != null)
            {
                child.GetComponent<Renderer>().enabled = false;

                //This is in case, like me, you use a primative to see where the nodes end up.
                //The "Linked" item is used by the Traffic AI, so that is ignored.
                foreach(Transform subItems in child)
                {
                    if (subItems.name.ToLower() != "linked")
                    {
                        GameObject.DestroyImmediate(subItems.gameObject);
                    }
                }
            }
            else
            {
                HideUISubNode(child);
            }
        }
    }

    public void BuildTrafficNodes()
    {
        try
        {
            if (trafficNodeInstance == null)
            {
                EditorUtility.DisplayDialog(
                    "Traffic Node Required",
                    "Please add a Traffic Node Prefab to the script", 
                    "Ooops, I forgot.  I will do it now");
            }
            else
            {
                //okay, this is SO bad.  
                //Creating an instance of new redirects to exist game object, AND there is no way to connect to existing without calling NEW
                roadNetwork = new ERRoadNetwork();

                random = new System.Random(System.DateTime.Now.Millisecond);

                foreach (Transform child in transform)
                {
                    SearchSubNode(child);
                }
            }
            
        }
        catch (System.Exception ex)
        {
            HBCLogging.logException(ex);
        }
    }

    private void SearchSubNode(Transform parent)
    {
        TrafficSystemNode tsn;

        foreach (Transform child in parent)
        {
            tsn = child.GetComponent<TrafficSystemNode>();

            if (tsn != null)
            {
                SetNodes(parent);
                break;  //Once a TrafficSystemNode has been found, we then know the parent has this collection of Side Objects/TSNs, so send the parent to process and stop this loop
            }
            else
            {
                SearchSubNode(child);
            }
        }
    }

    private void SetNodes(Transform parent)
    {
        try
        {
            //Problem in that the loop is finding itself
            //This avoids recursing a folder that was created by this process
            if (parent.name.Contains(nodeFolderName)) { return; }
            
            //TODO: will need to check for the preexisting nodes, and delete them
            //   "Traffic Nodes Right Lane(0)"


            bool isRight = true;
            string sSide = "Right";
            string roadName = parent.parent.name; 
            
            //Hack: use the name to determine if this is the Right or Left hand side
            if (parent.name.ToLower().Contains("left"))
            {
                isRight = false;
                sSide = "Left";
            }

            //Get width of road, to get the number of lanes
            ERRoad erRoad = roadNetwork.GetRoadByName(roadName);
            ERRoadType roadType = erRoad.GetRoadType();
            float roadWidth = roadType.roadWidth;
            byte laneCount = System.Convert.ToByte((roadType.roadWidth / 2) / laneWidth);
            List<Transform> trafficNodes = new List<Transform>();
            Transform nodeChild;

            
            for (byte b = 0; b < laneCount; b++)
            {
                string folderName = nodeFolderName + " " + sSide + " Lane (" + b.ToString() + ")";


                //todo NOT FINDING IT WHEN IT IS INDEED there
                Transform tempFolder = parent.parent.Find(folderName);

                //Check for the existance of this folder, if it already exists, delete it.  
                //The road has been adjusted and it is safer to assume that the old nodes are no longer correct
                //...just in case
                if (tempFolder != null)
                {
                    GameObject.DestroyImmediate(tempFolder.gameObject);  
                }

                GameObject folder = new GameObject(folderName);
                
                if (b == 0)
                {
                    //Remove existing Traffic Nodes from the folder that they were created
                    //Moving them insures that they are not moved again, in a forever recursive loop
                    //and to know they are completed, and not readjusted.
                    if (isRight)
                    {
                        for (int i = 0; i < parent.childCount; i++)
                        {
                            //NOTE: Be aware that when creating the Side Object the "Combine Objects" checkbox MUST be unchecked
                            //      otherwise a few of these children will not be TrafficSystemNodes and that will cause all sorts of issues.
                            nodeChild = parent.GetChild(i);
                            nodeChild.name = roadName + " " + sSide + " node (" + i + ")";
                            trafficNodes.Add(nodeChild);
                        }
                    }
                    else
                    {
                        int iLoop = 0;
                        for (int i = (parent.childCount - 1); i > -1; i--)
                        {
                            nodeChild = parent.GetChild(i);
                            nodeChild.name = roadName + " " + sSide + " node (" + iLoop + ")";
                            trafficNodes.Add(nodeChild);
                            
                            iLoop++;
                        }
                    }

                    //Add to new parent
                    foreach (Transform trafficNode in trafficNodes)
                    {
                        trafficNode.SetParent(folder.transform);
                    }

                    //Connect node to it's next neighbor
                    ConnectNodes(folder);
                }
                else
                {
                    // PROCESS
                    // * Create new nodes
                    // * Place them
                    // * Connect them to each other - still a bit of an issue here

                    //TODO
                    // * Connect them to the previous Lane
                    // * Connect previous lane to this lane

                    //Create the nodes and place them
                    for(int i = 0; i < trafficNodes.Count; i++)
                    {
                        float nodePosition = this.laneWidth;
                        if (isRight)
                        {
                            nodePosition = -(this.laneWidth);
                        }

                        Transform trafficNode = Instantiate(trafficNodeInstance, trafficNodes[i].position + trafficNodes[i].right * nodePosition, trafficNodes[i].rotation);
                        trafficNode.gameObject.isStatic = true;
                        trafficNode.name = roadName + " " + sSide + " node (" + i + ")";
                        trafficNode.SetParent(folder.transform);   



                        //Connect to previous lane

                        //trafficNodes[i] = trafficNode;

                        //TODO: connect to next node, BUT this hasn't been created yet so....
                        //Do I need another LIST?  Do I update the current list with the new Node, then in a separate loop connect them?
                        //if (i < (folder.transform.childCount - 1))
                        //{
                        //    TrafficSystemNode nextNode = folder.transform.GetChild(i + 1).GetComponent<TrafficSystemNode>();

                        //    folder.transform.GetChild(i).GetComponent<TrafficSystemNode>().m_connectedChangeLaneNodes.Clear(); //To insure that previous values are removed
                        //    folder.transform.GetChild(i).GetComponent<TrafficSystemNode>().m_connectedChangeLaneNodes.Add(nextNode);
                        //}
                    }


                    //folder.transform.SetParent(parent.parent);

                    //Connect node to it's next neighbor
                    ConnectNodes(folder);


                    //Connect to Previous nodes to Current nodes
                    folderName = nodeFolderName + " " + sSide + " Lane (" + (b - 1).ToString() + ")";
                    GameObject previousFolder = parent.parent.Find(folderName).gameObject;
                    ConnectLanes(folder, previousFolder);
  
                }

                folder.transform.SetParent(parent.parent);  
            }

            //DestroyImmediate(parent.gameObject);  //Remove the original folder, causes an issue with the loop
            
            //TODO: intersections (someday)
        }
        catch (System.Exception ex)
        {
            HBCLogging.logException(ex);
        }
    }

    private void ConnectNodes(GameObject folder)
    {
        try
        {
            for (int i = 0; i < folder.transform.childCount; i++)
            {
                //Cannot connect the last node to anything
                //Though it would be nice to connect it to the next road.  TODO: Someday, hopefully
                if (i < (folder.transform.childCount - 1))
                {
                    TrafficSystemNode nextNode = folder.transform.GetChild(i + 1).GetComponent<TrafficSystemNode>();

                    folder.transform.GetChild(i).GetComponent<TrafficSystemNode>().m_connectedNodes.Clear();
                    folder.transform.GetChild(i).GetComponent<TrafficSystemNode>().m_connectedNodes.Add(nextNode);
                }
            }
        }
        catch 
        {   }
    }

    private void ConnectLanes(GameObject currentLane, GameObject previousLane)
    {
        try
        {
            TrafficSystemNode nextNode = new TrafficSystemNode();
            int selected;
            
            //Removing the min amount of nodes as the last items in the lane don't have anything to connect with because of this limit
            for (int i = 0; i < (currentLane.transform.childCount - this.minNodesLaneChange); i++)
            {
                //TODO: can create a loop here to add multiple lanes the car can move to
                //      Will need to check if it is already connected
                selected = random.Next((i + minNodesLaneChange), (i + maxNodesLaneChange));  

                //to insure that the selected node is not outside the number of nodes available.
                if (selected > (previousLane.transform.childCount - 1))
                {
                    selected = (previousLane.transform.childCount - 2);
                }
                nextNode = previousLane.transform.GetChild(selected).GetComponent<TrafficSystemNode>();
                currentLane.transform.GetChild(i).GetComponent<TrafficSystemNode>().m_connectedNodes.Add(nextNode);
            }

            
            //----------------------------------
            for (int i = 0; i < (previousLane.transform.childCount - this.minNodesLaneChange) ; i++)
            {
                selected = random.Next((i + minNodesLaneChange), (i + maxNodesLaneChange));

                //to insure that the selected node is not outside the number of nodes available.
                if (selected > (currentLane.transform.childCount - 1))
                {
                    selected = (currentLane.transform.childCount - 2);
                }
                nextNode = currentLane.transform.GetChild(selected).GetComponent<TrafficSystemNode>();
                previousLane.transform.GetChild(i).GetComponent<TrafficSystemNode>().m_connectedNodes.Add(nextNode);
            }
        }
        catch (System.Exception ex)
        {
            Debug.Log(ex.Message);
        }
    }
}
