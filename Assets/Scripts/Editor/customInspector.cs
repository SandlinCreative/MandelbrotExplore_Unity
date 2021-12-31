using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CSMandelbrot))]
public class customInspector : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        CSMandelbrot csm = target as CSMandelbrot;
        if (GUILayout.Button("Recenter"))
        {
            //csm.CenterScreen();
        }
    }
}
