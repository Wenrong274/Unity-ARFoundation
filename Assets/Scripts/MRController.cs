using Cysharp.Threading.Tasks;
using DG.Tweening;
using Lean.Touch;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class MRController : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManagerInitializer ARTrackedImageManagerInitializer;
    [SerializeField] private SwitchButton ARPlaneButton;
    [SerializeField] private ScanItem ScanGroup;
    [SerializeField] private GameObject mapPerfab;
    [SerializeField] private Transform mapPerfabTra;
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private ARRaycastManager m_RaycastManager;
    [SerializeField] private ARPlaneManager m_ARPlaneManager;
    [SerializeField] private ARSession ARSession;
    [SerializeField] private LeanTouch LeanTouch;
    [SerializeField] private string trackedImageName = "trackables_01";
    private List<ARRaycastHit> m_Hits = new List<ARRaycastHit>();
    private Vector2 screenCenter;
    private Vector3 worldPosition;
    private bool IsTrackingImage = false;
    private bool ARPlaneOn = false;
    private bool TrackingOn = false;
    private bool IsShow = false;


    private void Start()
    {
        if (Application.isEditor)
        {
            IsShow = false;
            ShowModel(true);
        }
        else
        {
            IsShow = true;
            screenCenter = new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0.0f);
            ShowModel(false);
            _ = InitTask();
        }
        StartMRCamrea(true);
    }

    public void StartMRCamrea(bool on)
    {
        m_ARPlaneManager.gameObject.SetActive(on);
    }

    private async UniTask InitTask()
    {
        m_TrackedImageManager = ARTrackedImageManagerInitializer.TrackedImageManager;
        await UniTask.Delay(1000);
        ShowARPlane(ARPlaneOn);
        m_TrackedImageManager.trackedImagesChanged += OnTrackedImageChanged;
        m_ARPlaneManager.planesChanged += OnARPlaneChanged;
        await UniTask.Delay(1000);
        ListAllImages();
    }

    private void ShowModel(bool on)
    {
        if (IsShow == on)
            return;
        IsShow = on;
        string layerName = on ? "Default" : "Hide";
        int Layer = LayerMask.NameToLayer(layerName);
        ScanGroup.Show(!on);
        SetLayerRecursively(mapPerfab, Layer);
    }

    private void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, layer);
        }
    }
    private void Update()
    {
        if (IsTrackingImage)
            return;

        if (!TrackingOn)
            return;

        if (m_RaycastManager.Raycast(screenCenter, m_Hits, TrackableType.PlaneWithinPolygon))
        {
            worldPosition = m_Hits[0].pose.position;
            ShowModel(true);
            mapPerfabTra.position = worldPosition;
        }
    }

    void ListAllImages()
    {
        Debug.Log($"There are {m_TrackedImageManager.trackables.count} images being tracked.");
        foreach (var trackedImage in m_TrackedImageManager.trackables)
        {
            Debug.Log($"Image: {trackedImage.referenceImage.name} is at " +
                        $"{trackedImage.transform.position}.");
        }
    }

    void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        SetTrackedImageUpdated(eventArgs.updated, trackedImageName);
    }

    void SetTrackedImageUpdated(List<ARTrackedImage> eventArgs, string trackedImageName)
    {
        IsTrackingImage = false;
        foreach (var image in eventArgs)
        {
            if (image.referenceImage.name == trackedImageName)
            {
                IsTrackingImage = image.trackingState == TrackingState.Tracking;
                if (IsTrackingImage)
                {
                    var trackedTra = image.transform;
                    ShowModel(true);
                    mapPerfabTra.position = trackedTra.position;
                    mapPerfabTra.eulerAngles = trackedTra.eulerAngles;
                    mapPerfabTra.localScale = Vector3.one;
                    worldPosition = trackedTra.position;
                }
                break;
            }
        }

        if (LeanTouch.UseTouch != !IsTrackingImage)
            LeanTouch.UseTouch = !IsTrackingImage;
    }

    private void OnARPlaneChanged(ARPlanesChangedEventArgs eventArgs)
    {
        SetARPlaneUpdated(eventArgs.updated);
    }

    private void SetARPlaneUpdated(List<ARPlane> updated)
    {
        ShowARPlane(ARPlaneOn);
    }

    public void ResetARCore()
    {
        ARSession.Reset();
        ShowModel(false);
        mapPerfabTra.eulerAngles = Vector3.zero;
        mapPerfabTra.localScale = Vector3.one;
    }

    public void SwitchARPlane()
    {
        ARPlaneOn = !ARPlaneOn;
        ShowARPlane(ARPlaneOn);
    }

    public void OnARModelPos(bool on)
    {
        TrackingOn = on;
    }

    private void ShowARPlane(bool on)
    {
        foreach (var plane in m_ARPlaneManager.trackables)
            plane.gameObject.SetActive(on);

        ARPlaneButton.SetOn(on);
    }

    [System.Serializable]
    public class SwitchButton
    {
        [SerializeField] private Button button;
        [SerializeField] private Sprite sprOn;
        [SerializeField] private Sprite sprOff;
        public bool On = false;

        public void Switch()
        {
            SetOn(!On);
        }

        public void SetOn(bool on)
        {
            On = on;
            button.image.sprite = on ? sprOn : sprOff;
        }
    }

    [System.Serializable]
    public class ScanItem
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform ScanRT;

        public void Show(bool on)
        {
            if (DOTween.IsTweening(canvasGroup))
                DOTween.Kill(canvasGroup);

            int val = on ? 1 : 0;
            canvasGroup.DOFade(val, 0.3f);
            ScanAnim(on);
        }

        private void ScanAnim(bool on)
        {
            if (DOTween.IsTweening(ScanRT))
                DOTween.Kill(ScanRT);
            if (on)
                ScanRT.DOScale(Vector3.one * 1.2f, 1.25f)
                    .SetEase(Ease.Linear)
                    .SetLoops(-1, LoopType.Yoyo);
            else
                ScanRT.localScale = Vector3.one;
        }
    }

}
