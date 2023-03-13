using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTrackedImageManagerInitializer : MonoBehaviour
{
    [SerializeField] private ARTrackedImageManager m_TrackedImageManager;
    [SerializeField] private XRReferenceImageLibrary ReferenceImages;
    [SerializeField] private GameObject fakeMrObj;
    public ARTrackedImageManager TrackedImageManager => m_TrackedImageManager;

    private void Awake()
    {
        m_TrackedImageManager = GetComponent<ARTrackedImageManager>();

        if (m_TrackedImageManager == null)
        {
            Debug.Log("Tracked Image Manager is Null.");
            m_TrackedImageManager = gameObject.AddComponent<ARTrackedImageManager>();
            m_TrackedImageManager.referenceLibrary = ReferenceImages;
            m_TrackedImageManager.requestedMaxNumberOfMovingImages = 1;
            m_TrackedImageManager.trackedImagePrefab = fakeMrObj;
            m_TrackedImageManager.enabled = true;
        }
    }
}
