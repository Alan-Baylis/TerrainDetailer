﻿namespace Pingerman.Tools.Terrain
{
    using UnityEngine;

#if UNITY_EDITOR
    using UnityEditor;
#endif

    /// <summary>
    /// Instantiator for terrain details
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public class TerrainDetailer : MonoBehaviour
    {
#if UNITY_EDITOR

        #region Parameters

        /// <summary>
        /// Name of scene child folder for new details
        /// </summary>
        [Tooltip("Name of scene child folder for new details")]
        [SerializeField]
        string folderName;

        /// <summary>
        /// Detail prefab
        /// </summary>
        [Tooltip("Detail prefab")]
        [SerializeField]
        GameObject detailPrefab;

        /// <summary>
        /// Freeze time by one click
        /// </summary>
        [Tooltip("Freeze time by one click")]
        [Range(1f, 10f)]
        [SerializeField]
        float clickFreeze = 1f;

        /// <summary>
        /// Terrain layer mask
        /// </summary>
        [Tooltip("Terrain layer mask")]
        [SerializeField]
        LayerMask terrainMask;

        #endregion

        #region Fields

        /// <summary>
        /// State of detailer activate
        /// </summary>
        [HideInInspector]
        [SerializeField]
        bool activated;

        /// <summary>
        /// Freeze left time by one click
        /// </summary>
        float clickFreezeLeft;

        #endregion

        #region Properties
        #endregion

        #region Public Methods

        /// <summary>
        /// Activate detailer mode
        /// </summary>
        public void DetailOn() { this.activated = true; }

        /// <summary>
        /// Deactivate detailer mode
        /// </summary>
        public void DetailOff() { this.activated = false; }

        #endregion

        #region Private Methods

        /// <summary>
        /// Cast a ray for try set detail on the collider
        /// </summary>
        private void TrySetDetail(SceneView sceneView)
        {
            //Check detailer mode
            if (!this.activated)
                return;

            //Check needed objects
            if (this.folderName == "" || !this.detailPrefab)
                return;

            //Check click freeze
            if (this.clickFreezeLeft > 0f)
            {
                this.clickFreezeLeft -= Time.fixedDeltaTime;
                return;
            }

            var controlId = GUIUtility.GetControlID(FocusType.Passive);
            var currentEvent = Event.current;
            if (currentEvent == null)
                return;

            var mousePos = currentEvent.mousePosition;
            if (mousePos == null)
                return;

            Ray ray = HandleUtility.GUIPointToWorldRay(mousePos);
            RaycastHit hit;
            var currentType = currentEvent.GetTypeForControl(controlId);

            if ((currentType == EventType.MouseDown || currentType == EventType.mouseDrag) && currentEvent.button == 0)
            {
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, this.terrainMask))
                {
                    GUIUtility.hotControl = controlId;

                    SetDetail(hit);

                    currentEvent.Use();
                }
            }
            else
            {
                if (currentType == EventType.mouseUp || currentType == EventType.MouseMove)
                {
                    GUIUtility.hotControl = 0;

                    currentEvent.Use();
                }
            }

        }

        /// <summary>
        /// Set detail on the point
        /// </summary>
        void SetDetail(RaycastHit hit)
        {
            //Reset freeze left time
            this.clickFreezeLeft = this.clickFreeze;

            var folder = this.transform.Find(this.folderName);
            if (!folder)
                folder = SetNewFolder();

            Instantiate(this.detailPrefab, hit.point, Quaternion.identity, folder);
        }

        /// <summary>
        /// Set new folder for instantiate new details
        /// </summary>
        /// <returns>Folder for new details</returns>
        Transform SetNewFolder()
        {
            var go = new GameObject(this.folderName);
            go.transform.parent = this.transform;
            return go.transform;
        }

        #endregion

        #region Unity

        void OnEnable()
        {
            if (!Application.isPlaying)
            {
                //EditorApplication.update += DrawGizmos;
                SceneView.onSceneGUIDelegate += TrySetDetail;
            }

            this.clickFreezeLeft = this.clickFreeze;
        }

        private void OnDisable()
        {
            if (!Application.isPlaying)
            {
                SceneView.onSceneGUIDelegate -= TrySetDetail;
                //EditorApplication.update -= DrawGizmos;
            }

        }

        private void OnValidate()
        {
            this.clickFreezeLeft = this.clickFreeze;
        }

        #endregion

#endif

    }
}
