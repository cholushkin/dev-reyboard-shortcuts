using UnityEditor;
using UnityEngine;
using System;

namespace GameLib
{
    // Controls Unity Scene View camera navigation with authentic Blender numpad shortcuts and parity.
    [CreateAssetMenu(menuName = "GameLib/Debug/DevKeyboardShortcuts/DevTools/Blender Camera Nav Tool", fileName = "BlenderCameraNavTool")]
    public class BlenderCameraNavDevTool : DevActionTool
    {
        public enum CameraNavAction
        {
            ViewTop,             
            ViewBottom,          
            ViewFront,           
            ViewBack,            
            ViewRight,           
            ViewLeft,            
            ViewFlip180,         

            TogglePerspOrtho,    
            FrameSelected,       
            ToggleIsolation,     

            LookThroughActiveCamera,   
            AlignActiveCameraToView,   
            SetActiveObjectAsCamera,

            OrbitLeft,
            OrbitRight,
            OrbitUp,
            OrbitDown,

            PanLeft,
            PanRight,
            PanUp,
            PanDown,

            ZoomIn,
            ZoomOut,
            
            RollLeft,
            RollRight,

            ViewLocalTop,
            ViewLocalBottom,
            ViewLocalFront,
            ViewLocalBack,
            ViewLocalRight,
            ViewLocalLeft
        }

        [Serializable]
        public class NavigationSensitivity
        {
            [Tooltip("Angle in degrees to rotate the camera when using Orbit keys.")]
            public float orbitStepAngle = 15.0f;

            [Tooltip("Multiplier for panning speed relative to current viewport zoom size.")]
            public float panStepMultiplier = 0.1f;

            [Tooltip("Percentage to zoom in or out per step relative to current viewport zoom size.")]
            public float zoomStepMultiplier = 0.15f;
        }

        [Header("Action Configuration")]
        [Tooltip("The specific Blender camera navigation action this tool asset will execute.")]
        public CameraNavAction actionType = CameraNavAction.ViewFront;

        [Header("Sensitivity Settings")]
        [Tooltip("Configuration specifically for Orbit, Pan, and Zoom actions.")]
        public NavigationSensitivity sensitivity = new NavigationSensitivity();

        public override void Execute()
        {
            SceneView sceneView = GetActiveSceneView();
            if (sceneView == null)
            {
                Debug.LogWarning("[BlenderCameraNav] No active Scene View found to navigate.");
                return;
            }

            if (sceneView.in2DMode)
            {
                sceneView.in2DMode = false;
                sceneView.ShowNotification(new GUIContent("2D Mode Disabled"));
                Debug.Log("[BlenderCameraNav] 2D mode disabled.");
            }

            switch (actionType)
            {
                case CameraNavAction.ViewTop:
                    SetViewOrientation(sceneView, Vector3.down, Vector3.forward);
                    break;
                case CameraNavAction.ViewBottom:
                    SetViewOrientation(sceneView, Vector3.up, Vector3.forward);
                    break;
                case CameraNavAction.ViewFront:                    
                    SetViewOrientation(sceneView, Vector3.forward, Vector3.up);
                    break;
                case CameraNavAction.ViewBack:                    
                    SetViewOrientation(sceneView, Vector3.back, Vector3.up);
                    break;
                case CameraNavAction.ViewRight:
                    SetViewOrientation(sceneView, Vector3.left, Vector3.up);
                    break;
                case CameraNavAction.ViewLeft:
                    SetViewOrientation(sceneView, Vector3.right, Vector3.up);
                    break;
                case CameraNavAction.ViewFlip180:
                    OrbitCamera(sceneView, 180f, 0f);
                    sceneView.ShowNotification(new GUIContent("View Flipped 180°"));
                    break;

                case CameraNavAction.TogglePerspOrtho:
                    sceneView.orthographic = !sceneView.orthographic;
                    sceneView.ShowNotification(new GUIContent(sceneView.orthographic ? "Orthographic" : "Perspective"));
                    break;
                case CameraNavAction.FrameSelected:
                    sceneView.FrameSelected();
                    break;
                case CameraNavAction.ToggleIsolation:
                    ToggleLocalViewIsolation(sceneView);
                    break;

                case CameraNavAction.LookThroughActiveCamera:
                    LookThroughCamera(sceneView);
                    break;
                case CameraNavAction.AlignActiveCameraToView:
                    AlignCameraToSceneView(sceneView);
                    break;
                case CameraNavAction.SetActiveObjectAsCamera: 
                    SetActiveObjectAsCamera(sceneView); 
                    break;

                case CameraNavAction.OrbitLeft:
                    OrbitCamera(sceneView, -sensitivity.orbitStepAngle, 0f);
                    break;
                case CameraNavAction.OrbitRight:
                    OrbitCamera(sceneView, sensitivity.orbitStepAngle, 0f);
                    break;
                case CameraNavAction.OrbitUp:
                    OrbitCamera(sceneView, 0f, sensitivity.orbitStepAngle);
                    break;
                case CameraNavAction.OrbitDown:
                    OrbitCamera(sceneView, 0f, -sensitivity.orbitStepAngle);
                    break;

                case CameraNavAction.PanLeft:
                    PanCamera(sceneView, -sensitivity.panStepMultiplier, 0f);
                    break;
                case CameraNavAction.PanRight:
                    PanCamera(sceneView, sensitivity.panStepMultiplier, 0f);
                    break;
                case CameraNavAction.PanUp:
                    PanCamera(sceneView, 0f, sensitivity.panStepMultiplier);
                    break;
                case CameraNavAction.PanDown:
                    PanCamera(sceneView, 0f, -sensitivity.panStepMultiplier);
                    break;

                case CameraNavAction.ZoomIn:
                    ZoomCamera(sceneView, -sensitivity.zoomStepMultiplier);
                    break;
                case CameraNavAction.ZoomOut:
                    ZoomCamera(sceneView, sensitivity.zoomStepMultiplier);
                    break;
                
                case CameraNavAction.RollLeft: 
                    RollCamera(sceneView, sensitivity.orbitStepAngle); 
                    break;
                case CameraNavAction.RollRight: 
                    RollCamera(sceneView, -sensitivity.orbitStepAngle); 
                    break;

                case CameraNavAction.ViewLocalTop: 
                    SetLocalViewOrientation(sceneView, Vector3.down, Vector3.forward); 
                    break;
                case CameraNavAction.ViewLocalBottom: 
                    SetLocalViewOrientation(sceneView, Vector3.up, Vector3.forward); 
                    break;
                case CameraNavAction.ViewLocalFront: 
                    SetLocalViewOrientation(sceneView, Vector3.forward, Vector3.up); 
                    break;
                case CameraNavAction.ViewLocalBack: 
                    SetLocalViewOrientation(sceneView, Vector3.back, Vector3.up); 
                    break;
                case CameraNavAction.ViewLocalRight: 
                    SetLocalViewOrientation(sceneView, Vector3.left, Vector3.up); 
                    break;
                case CameraNavAction.ViewLocalLeft: 
                    SetLocalViewOrientation(sceneView, Vector3.right, Vector3.up); 
                    break;
            }

            sceneView.Repaint();
        }

        private static void SetViewOrientation(SceneView view, Vector3 lookDirection, Vector3 upDirection)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection, upDirection);
            view.LookAt(view.pivot, targetRotation, view.size, view.orthographic);
            view.isRotationLocked = false; 
            view.ShowNotification(new GUIContent(GetViewName(lookDirection)));
        }

        private static void OrbitCamera(SceneView view, float deltaYaw, float deltaPitch)
        {
            Vector3 euler = view.rotation.eulerAngles;
            float pitch = euler.x;
            float yaw = euler.y;
            float roll = euler.z;

            if (pitch > 180f) pitch -= 360f;

            pitch = Mathf.Clamp(pitch + deltaPitch, -89.8f, 89.8f);
            yaw += deltaYaw;

            Quaternion newRotation = Quaternion.Euler(pitch, yaw, roll);
            view.LookAt(view.pivot, newRotation, view.size, view.orthographic);
        }

        private static void PanCamera(SceneView view, float deltaX, float deltaY)
        {
            float stepDistance = view.size;
            Vector3 right = view.camera.transform.right;
            Vector3 up = view.camera.transform.up;

            Vector3 newPivot = view.pivot + (right * deltaX * stepDistance) + (up * deltaY * stepDistance);
            view.LookAt(newPivot, view.rotation, view.size, view.orthographic);
        }

        private static void ZoomCamera(SceneView view, float zoomDeltaMultiplier)
        {
            float newSize = view.size * (1.0f + zoomDeltaMultiplier);
            newSize = Mathf.Clamp(newSize, 0.001f, 100000f);
            view.LookAt(view.pivot, view.rotation, newSize, view.orthographic);
        }

        private static void ToggleLocalViewIsolation(SceneView view)
        {
            var visMgr = SceneVisibilityManager.instance;
            if (visMgr == null) return;

            if (visMgr.IsCurrentStageIsolated())
            {
                visMgr.ExitIsolation();
                view.ShowNotification(new GUIContent("Global View"));
            }
            else
            {
                if (Selection.gameObjects != null && Selection.gameObjects.Length > 0)
                {
                    visMgr.Isolate(Selection.gameObjects, true);
                    view.ShowNotification(new GUIContent("Local View (Isolated Selected)"));
                }
                else
                {
                    view.ShowNotification(new GUIContent("Select an Object to Isolate!"));
                }
            }
        }

        private static void LookThroughCamera(SceneView view)
        {
            Camera targetCam = Selection.activeGameObject?.GetComponent<Camera>() ?? Camera.main;
            if (targetCam == null)
            {
                targetCam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            }

            if (targetCam != null)
            {
                view.AlignViewToObject(targetCam.transform);
                view.ShowNotification(new GUIContent($"Camera View ({targetCam.name})"));
            }
            else
            {
                view.ShowNotification(new GUIContent("No Camera Found in Scene!"));
            }
        }

        private static void AlignCameraToSceneView(SceneView view)
        {
            Camera targetCam = Selection.activeGameObject?.GetComponent<Camera>() ?? Camera.main;
            if (targetCam == null)
            {
                targetCam = UnityEngine.Object.FindFirstObjectByType<Camera>();
            }

            if (targetCam != null)
            {
                Undo.RecordObject(targetCam.transform, "Align Camera to View");
                targetCam.transform.position = view.camera.transform.position;
                targetCam.transform.rotation = view.camera.transform.rotation;
                view.ShowNotification(new GUIContent($"Aligned '{targetCam.name}' to View"));
                Debug.Log($"[BlenderCameraNav] Successfully aligned camera '{targetCam.name}' to current Scene View position/rotation.");
            }
            else
            {
                view.ShowNotification(new GUIContent("No Camera Found to Align!"));
            }
        }
        
        private static void RollCamera(SceneView view, float rollAngle)
        {
            Quaternion rollRotation = Quaternion.AngleAxis(rollAngle, view.camera.transform.forward);
            Quaternion newRotation = rollRotation * view.rotation;
            view.LookAt(view.pivot, newRotation, view.size, view.orthographic);
        }

        /// Snaps the view to a local axis of the currently selected object.
        private static void SetLocalViewOrientation(SceneView view, Vector3 localLook, Vector3 localUp)
        {
            Transform target = Selection.activeTransform;
            if (target == null)
            {
                SetViewOrientation(view, localLook, localUp); // Fallback to global
                return;
            }

            // Convert local directional vectors to world space
            Vector3 worldLook = target.TransformDirection(localLook);
            Vector3 worldUp = target.TransformDirection(localUp);
            Quaternion targetRotation = Quaternion.LookRotation(worldLook, worldUp);

            // CRITICAL FIX: Unity's native mouse orbit breaks if the camera has Z-axis roll.
            // We strip the Z-axis rotation (roll) to prevent the viewport from locking.
            Vector3 euler = targetRotation.eulerAngles;
            targetRotation = Quaternion.Euler(euler.x, euler.y, 0f);

            // Snap to the object's position with the corrected rotation
            view.LookAt(target.position, targetRotation, view.size, view.orthographic);
            view.isRotationLocked = false; 
            view.ShowNotification(new GUIContent($"Local {GetViewName(localLook)}"));
        }

        private static void SetActiveObjectAsCamera(SceneView view)
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                view.ShowNotification(new GUIContent("Select an object to set as camera!"));
                return;
            }

            Camera cam = selected.GetComponent<Camera>();
            if (cam == null)
            {
                Undo.AddComponent<Camera>(selected);
            }

            view.AlignViewToObject(selected.transform);
            view.ShowNotification(new GUIContent($"Set Active Camera: {selected.name}"));
        }

        private static SceneView GetActiveSceneView()
        {
            if (SceneView.lastActiveSceneView != null)
                return SceneView.lastActiveSceneView;

            if (SceneView.sceneViews.Count > 0)
                return (SceneView)SceneView.sceneViews[0];

            return null;
        }

        private static string GetViewName(Vector3 dir)
        {
            if (dir == Vector3.down) return "Top View";
            if (dir == Vector3.up) return "Bottom View";
            if (dir == Vector3.forward) return "Front View";
            if (dir == Vector3.back) return "Back View";
            if (dir == Vector3.left) return "Right View";
            if (dir == Vector3.right) return "Left View";
            return "Custom View";
        }
    }
}