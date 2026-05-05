using UnityEngine;

namespace Everwide.Flowers
{
    public class LookAtCamera : MonoBehaviour
    {
        private static Transform cam;

        void Awake()
        {
            if (cam == null)
            {
                Camera mainCam = Camera.main;
                if (mainCam != null)
                    cam = mainCam.transform;
            }
        }

        void LateUpdate()
        {
            if (cam == null) return;

            Vector3 dir = cam.position - transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
