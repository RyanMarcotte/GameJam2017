using UnityEngine;
using UnityEditor;

[CustomEditor (typeof (Sonar))]
public class SonarEditor : Editor
{

    void OnSceneGUI()
    {
		Sonar sonar = (Sonar)target;
		Handles.color = Color.white;
		Handles.DrawWireArc (sonar.transform.position, Vector3.back, Vector3.up, 360, 4.5f);
		Vector3 viewAngleA = sonar.DirFromAngle (-90 / 2, false);
		Vector3 viewAngleB = sonar.DirFromAngle (90 / 2, false);

        Handles.DrawLine(sonar.transform.position, sonar.transform.position + viewAngleA * 4.5f);
        Handles.DrawLine(sonar.transform.position, sonar.transform.position + viewAngleB * 4.5f);
        Handles.DrawLine(sonar.transform.position, sonar.transform.position + viewAngleA * 2.25f);
        Handles.DrawLine(sonar.transform.position, sonar.transform.position + viewAngleB * 2.25f);

        Handles.color = Color.red;
		foreach (Transform visibleTarget in sonar.visibleTargets) {
			Handles.DrawLine (sonar.transform.position, visibleTarget.position);
		}
	}

}
