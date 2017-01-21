using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor (typeof (Sonar))]
public class SonarEditor : Editor {

    void OnSceneGUI()
    {
		Sonar sonar = (Sonar)target;
		Handles.color = Color.white;
		Handles.DrawWireArc (sonar.transform.position, Vector3.back, Vector3.up, 360, sonar.viewRadius);
		Vector3 viewAngleA = sonar.DirFromAngle (-sonar.viewAngle / 2, false);
		Vector3 viewAngleB = sonar.DirFromAngle (sonar.viewAngle / 2, false);

		Handles.DrawLine (sonar.transform.position, sonar.transform.position + viewAngleA * sonar.viewRadius);
		Handles.DrawLine (sonar.transform.position, sonar.transform.position + viewAngleB * sonar.viewRadius);

		Handles.color = Color.red;
		foreach (Transform visibleTarget in sonar.visibleTargets) {
			Handles.DrawLine (sonar.transform.position, visibleTarget.position);
		}
	}

}
