using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TrafficNodeSetup))]
public class TrafficNodeSetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        TrafficNodeSetup trafficNodeSetup = (TrafficNodeSetup)target;

        if (GUILayout.Button("Build Traffic Nodes"))
        {
            trafficNodeSetup.BuildTrafficNodes();
        }

        if (GUILayout.Button("Hide Node UI Items"))
        {
            trafficNodeSetup.HideUIElements();
        }
    }
}
