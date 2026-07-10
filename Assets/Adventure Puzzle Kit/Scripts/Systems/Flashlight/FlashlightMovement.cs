using UnityEngine;

namespace AdventurePuzzleKit.FlashlightSystem
{
    // Makes the flashlight follow the main camera's position and rotation with smoothing
    public class FlashlightMovement : MonoBehaviour
    {
        private Vector3 v3Offset; // Initial position offset from camera
        private Transform followTransform; // Camera to follow
        private Transform myTransform; // Cached flashlight transform

        [SerializeField] private float _speed = 3.0f; // Rotation follow speed

        // Public property to get/set follow speed
        public float speed
        {
            get { return _speed; }
            set { _speed = value; }
        }

        void Start()
        {
            myTransform = transform;
            followTransform = Camera.main.transform;
            v3Offset = myTransform.position - followTransform.position; // Calculate initial offset
        }

        void Update()
        {
            // Maintain offset position relative to camera
            transform.position = followTransform.position + v3Offset;

            // Smoothly rotate to match camera
            transform.rotation = Quaternion.Slerp(myTransform.rotation, followTransform.rotation, speed * Time.deltaTime);
        }
    }
}
