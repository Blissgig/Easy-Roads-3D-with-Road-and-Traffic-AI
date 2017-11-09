# Easy-Roads-3D-with-Road-and-Traffic-AI
The code to use R&amp;T AI with Easy Roads 3D (Unity3D)

Road & Traffic AI: https://assetstore.unity.com/packages/templates/systems/road-traffic-system-21626

Easy Roads 3D: https://assetstore.unity.com/packages/tools/terrain/easyroads3d-pro-469

Road & Traffic AI has their own street objects, however they are straight pieces and I liked the ability that Easy Roads 3D allows to make curved and unique roads.   So I took the nodes from R&T and tweaked the code a little bit so they can be used with ERD3D.

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



