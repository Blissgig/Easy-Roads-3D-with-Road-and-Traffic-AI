# Easy-Roads-3D-with-Road-and-Traffic-AI
The code to use R&amp;T AI with Easy Roads 3D (Unity3D)

Road & Traffic AI: https://assetstore.unity.com/packages/templates/systems/road-traffic-system-21626

Easy Roads 3D: https://assetstore.unity.com/packages/tools/terrain/easyroads3d-pro-469

Road & Traffic AI has their own street objects, however they are straight pieces and I liked the ability that Easy Roads 3D allows to make curved and unique roads.   So I took the nodes from R&T and tweaked the code a little bit so they can be used with ERD3D.

-------------
The New Method
(Additional info coming soon)

The old method had you, the developer, copy and pasting vast amounts of Traffic Nodes is painful and slow.  So I developed this method to easy the pain.

The process is as follows:
* Create a prefab of the Road & Traffic AI's "Traffic Node".  I also included a sphere that is flattened as this shows me the width of the lane. 
* You will need to create two Side Objects within Easy Roads 3D's main script.  One for the Right side of the road, one for the left.   The name HAS to include the word "Right" and "Left" to properly add additional lanes.  It's a hack, I know, but at the moment I have not found another method to know which side the node is on.   
* Add these Side Objects to the roads you want to affect.
* There are two scripts, attach the <insert script name> to the Easy Roads 3D's "Road Network".  
* The <script> properties:
    * Traffic Node - Add the prefab you just created to this property.
    * Lane Width - This is to help place the distance on the road.
    * Min
    * Max
    * Node Folder Name - This is used to created sub "folders" within each Road with the Lanes named.  This name 
* Press the <button>. It will add additional Lanes and connect them to each other,
* The previous folder nodes will still be left and you should delete them.  Will work on a fix as soon as possible.
* Insersections are not working at the moment.  TODO
* This does not connect each of the roads to each other.  This will have to be done manually by adding an addition <insert node property> to the list and connect it.
    




-------------
The Old Method.  
It involves a lot of manual moving of elements.  There is still value in this approach, but the new method allows for larger scale change.

The process is that a node is created and named; "NAME (0)", where "NAME" can be changed to whatever name you want, but the "(0)" is necessary as the function "GetPreviousNode()" relies on this naming convention.

With these changes you can copy n paste the first node and then move the new node to the location you desire.  The previous node will point to the new node.

All changes are made to the TrafficSystemNode.cs file.  This is found in /Assets/Traffic System/Scripts.

The following function is to be added to this script.

private TrafficSystemNode GetPreviousNode()
    {
        TrafficSystemNode returnNode = null;

        try
        {
            string sName = transform.name;
            int iStart = sName.IndexOf("(");
            int iEnd = sName.IndexOf(")");
            int iValue = System.Convert.ToInt16(sName.Substring((iStart + 1), ((iEnd - iStart) - 1)));

            sName = sName.Substring(0, (iStart - 1)) + " (" + (iValue - 1).ToString() + ")";
            GameObject trafficSystemNode = GameObject.Find(sName);

            //Just in case it does not exist
            if (trafficSystemNode != null)
            {
                returnNode = trafficSystemNode.GetComponent<TrafficSystemNode>();
            }
        }
        catch (System.Exception)
        {
            //Just eat it
        }
        
        return returnNode;
    }

------------------------------------------------------

In the existing function "Awake()" add the following code below the line; "//HACK - END"   (Though maybe it would be better to make it's own function and just call that...  Next version)

        //JAMES ROSE CODE
        //This is because I am using Easy Roads 3D and just want to copy n paste the Traffic System Nodes 
        //and have them connect to the previous node as well as have that node connect to this new node.
        if (!Application.isPlaying)
        {
            try
            {
                TrafficSystemNode previousNode = GetPreviousNode();

                if (previousNode != null)
                {
                    if (!previousNode.m_connectedNodes.Contains(this))
                    {
                        previousNode.m_connectedNodes.Add(this);
                    }
                }
            }
            catch
            {
                //Don't care, just don't screw up the original code/process
            }
        }
        //END OF JAMES CODE

------------------------------------------------------

The last function is to ADD an "Update()" function.

    void Update()
    {
        //This is to point the previous node at this node.
        if (!Application.isPlaying)
        {
            if (transform.hasChanged)
            {
                transform.hasChanged = false;
                TrafficSystemNode previousNode = GetPreviousNode();

                if (previousNode != null)
                {
                    previousNode.transform.LookAt(transform);
                }
            }
        }
    }



