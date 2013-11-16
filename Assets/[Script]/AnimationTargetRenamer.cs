#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;


public class AnimationTargetRenamer : EditorWindow {
 
    [MenuItem("Window/Animation Target Renamer")]
    public static void OpenWindow() {
       GetWindow<AnimationTargetRenamer>("Animation Target Renamer");
	}
 
    void Update() {
       Repaint();
    }
 
    void OnGUI() {
 
       GameObject go = Selection.activeGameObject;
 
       if (go != null && go.animation != null) {
         Animation ani = go.animation;
         AnimationClipCurveData[] curveDatas = AnimationUtility.GetAllCurves(ani.clip, true);
 
         List<string> targetObjectPaths = new List<string>();
         foreach (var curveData in curveDatas) {
          if (!targetObjectPaths.Contains(curveData.path)) {
              targetObjectPaths.Add(curveData.path);
          }
         }
 
         Dictionary<string, string> newNames = new Dictionary<string, string>();
         foreach (var target in targetObjectPaths) {
          string newName = GUILayout.TextField(target);
          if (newName != target) {
              newNames.Add(target, newName);
          }
         }
 
         foreach (var pair in newNames) {
          string oldName = pair.Key;
          string newName = pair.Value;
 
          foreach (var curveData in curveDatas) {
              if (curveData.path == oldName) {
                 curveData.path = newName;
              }
          }
         }
 
         if (newNames.Count > 0) {
          ani.clip.ClearCurves();
          foreach (var curveData in curveDatas) {
              ani.clip.SetCurve(curveData.path, curveData.type, curveData.propertyName, curveData.curve);
          }
         }
 
       } else {
         GUILayout.Label("Please select an object with an animation.");
       }
 
    }
 
}
#endif