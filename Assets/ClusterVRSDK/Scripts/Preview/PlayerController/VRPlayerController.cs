using UnityEngine;
using UnityEngine.XR;

namespace ClusterVRSDK.Preview.PlayerController
{
    public class VRPlayerController : MonoBehaviour, IPlayerController
    {
        [SerializeField] CharacterController characterController;
        [SerializeField] float moveSpeed;
        [SerializeField] Transform cameraTransform;
        float fallingSpeed;
        float rotationCoolTime;

        public Transform PlayerTransform => characterController.transform;
        public Transform CameraTransform => cameraTransform;

        const string RiftDevice = "Rift";
        bool IsOculusRift => XRDevice.model.Contains(RiftDevice);

        public void ActivateCharacterController(bool isActive)
        {
            characterController.enabled = isActive;
        }

        void Update()
        {
            var x = GetLeftHorizontal();
            var z = GetLeftVertical();
            var rot = GetRightHorizontal();
            var direction = new Vector3(x, 0, -z);
            direction.Normalize();
            direction = Quaternion.Euler(0, cameraTransform.eulerAngles.y, 0) * direction;
            var velocity = direction * moveSpeed;
            if (characterController.isGrounded)
            {
                fallingSpeed = 0;
            }
            else
            {
                fallingSpeed += Time.deltaTime * 9.81f;
            }

            if (Mathf.Abs(rot) > 0.3 && rotationCoolTime <= 0.0f)
            {
                PlayerTransform.eulerAngles += Vector3.up * Mathf.Sign(rot) * 45;
                rotationCoolTime = 0.3f;
            }
            rotationCoolTime -= Time.unscaledDeltaTime;

            velocity.y = -fallingSpeed;

            if (velocity.magnitude > 0.01)
            {
                characterController.Move(velocity * Time.deltaTime);
            }
        }

        float GetLeftHorizontal()
        {
            if (IsOculusRift)
            {
                return Input.GetAxisRaw("VRPreviewLeftHorizontalAxis");
            }

            return Input.GetButton("VRPreviewLeftPadPush") ? Input.GetAxisRaw("VRPreviewLeftHorizontalAxis") : 0f;
        }

        float GetLeftVertical()
        {
            if (IsOculusRift)
            {
                return Input.GetAxisRaw("VRPreviewLeftVerticalAxis");
            }

            return Input.GetButton("VRPreviewLeftPadPush") ? Input.GetAxisRaw("VRPreviewLeftVerticalAxis") : 0f;
        }

        float GetRightHorizontal()
        {
            if (IsOculusRift)
            {
                return Input.GetAxisRaw("VRPreviewRightHorizontalAxis");
            }

            return Input.GetButton("VRPreviewRightPadPush") ? Input.GetAxisRaw("VRPreviewRightHorizontalAxis") : 0f;
        }
    }
}
