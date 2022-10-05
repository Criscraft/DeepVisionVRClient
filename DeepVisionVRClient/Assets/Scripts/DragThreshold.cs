using UnityEngine;
using UnityEngine.EventSystems;

    public class DragThreshold : MonoBehaviour
    {
        private Canvas myCanvas;
        private int defaultDrag = 30;

        void Start()
        {
            //defaultDrag = EventSystem.current.pixelDragThreshold;
            myCanvas = this.GetComponent<Canvas>();
            EventSystem.current.pixelDragThreshold = (int)(defaultDrag * myCanvas.scaleFactor);
        }
    }
