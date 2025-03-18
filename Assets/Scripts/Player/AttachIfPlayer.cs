using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

namespace BNG
{
    public class ActivateIfPlayer : NetworkBehaviour
    {
        [Header("Local Player Transforms")]
        public Transform PlayerHeadTransform;
        public Transform PlayerLeftHandTransform;
        public Transform PlayerRightHandTransform;

        [Header("Remote Player Transforms")]
        public Transform RemoteHeadTransform;
        public Transform RemoteLeftHandTransform;
        public Transform RemoteRightHandTransform;

        [Header("Hand Components")]
        public HandController LeftHandController;
        public HandController RightHandController;
        public Animator RemoteLeftHandAnimator;
        public Animator RemoteRightHandAnimator;
        public Grabber LeftGrabber;
        public Grabber RightGrabber;

        [SerializeField] private float HandAnimationSpeed = 20f;

        private GrabbablesInTrigger gitLeft;
        private GrabbablesInTrigger gitRight;
        private Dictionary<ulong, double> requestedGrabbables = new();
        private bool disabledObjects;

        // Network variables
        // private NetworkVariable<Vector3> netHeadPosition = new();
        // private NetworkVariable<Quaternion> netHeadRotation = new();
        // private NetworkVariable<Vector3> netLeftHandPosition = new();
        // private NetworkVariable<Quaternion> netLeftHandRotation = new();
        // private NetworkVariable<Vector3> netRightHandPosition = new();
        // private NetworkVariable<Quaternion> netRightHandRotation = new();

        // private NetworkVariable<float> netLeftGrip = new();
        // private NetworkVariable<float> netRightGrip = new();
        // private NetworkVariable<float> netLeftPoint = new();
        // private NetworkVariable<float> netRightPoint = new();
        // private NetworkVariable<float> netLeftThumb = new();
        // private NetworkVariable<float> netRightThumb = new();
        // private NetworkVariable<bool> netLeftHoldingItem = new();
        // private NetworkVariable<bool> netRightHoldingItem = new();
        // Update these network variables to allow client authority
        private NetworkVariable<Vector3> netHeadPosition = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<Quaternion> netHeadRotation = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<Vector3> netLeftHandPosition = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<Quaternion> netLeftHandRotation = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<Vector3> netRightHandPosition = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<Quaternion> netRightHandRotation = new(writePerm: NetworkVariableWritePermission.Owner);

        private NetworkVariable<float> netLeftGrip = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> netRightGrip = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> netLeftPoint = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> netRightPoint = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> netLeftThumb = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<float> netRightThumb = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> netLeftHoldingItem = new(writePerm: NetworkVariableWritePermission.Owner);
        private NetworkVariable<bool> netRightHoldingItem = new(writePerm: NetworkVariableWritePermission.Owner);

        public override void OnNetworkSpawn()
        {
            if (!IsOwner)
            {
                enabled = true;
                return;
            }

            InitializeLocalPlayer();
            enabled = true;
        }

        private void InitializeLocalPlayer()
        {
            LeftGrabber = GameObject.Find("LeftController").GetComponentInChildren<Grabber>();
            gitLeft = LeftGrabber.GetComponent<GrabbablesInTrigger>();

            RightGrabber = GameObject.Find("RightController").GetComponentInChildren<Grabber>();
            gitRight = RightGrabber.GetComponent<GrabbablesInTrigger>();

            AssignPlayerObjects();
        }

        private void Update()
        {
            if (IsOwner)
            {
                UpdateOwnerPositions();
                toggleObjects(false);
                UpdateHandControllerValues(true);
                UpdateHandControllerValues(false);
                UpdateRemoteAnimations();
            }
            else
            {
                UpdateRemotePositions();

                toggleObjects(true);
                UpdateRemoteAnimations();
            }
        }

        // private void UpdateOwnerPositions()
        // {


        //     netHeadPosition.Value = PlayerHeadTransform.position;
        //     netHeadRotation.Value = PlayerHeadTransform.rotation;
        //     netLeftHandPosition.Value = PlayerLeftHandTransform.position;
        //     netLeftHandRotation.Value = PlayerLeftHandTransform.rotation;
        //     netRightHandPosition.Value = PlayerRightHandTransform.position;
        //     netRightHandRotation.Value = PlayerRightHandTransform.rotation;



        //     if (LeftHandController)
        //     {
        //         UpdateHandControllerValues(true);
        //     }
        //     if (RightHandController)
        //     {
        //         UpdateHandControllerValues(false);
        //     }
        // }

        private void UpdateOwnerPositions()
        {
            // Update network variables with local transforms
            netHeadPosition.Value = PlayerHeadTransform.position;
            netHeadRotation.Value = PlayerHeadTransform.rotation;
            netLeftHandPosition.Value = PlayerLeftHandTransform.position;
            netLeftHandRotation.Value = PlayerLeftHandTransform.rotation;
            netRightHandPosition.Value = PlayerRightHandTransform.position;
            netRightHandRotation.Value = PlayerRightHandTransform.rotation;

            // Update remote transforms to match local transforms while preserving rig hierarchy
            RemoteHeadTransform.position = PlayerHeadTransform.position;
            RemoteHeadTransform.rotation = PlayerHeadTransform.rotation;
            RemoteLeftHandTransform.position = PlayerLeftHandTransform.position;
            RemoteLeftHandTransform.rotation = PlayerLeftHandTransform.rotation;
            RemoteRightHandTransform.position = PlayerRightHandTransform.position;
            RemoteRightHandTransform.rotation = PlayerRightHandTransform.rotation;


        }

        private void UpdateRemotePositions()
        {
            RemoteHeadTransform.position = netHeadPosition.Value;
            RemoteHeadTransform.rotation = netHeadRotation.Value;
            RemoteLeftHandTransform.position = netLeftHandPosition.Value;
            RemoteLeftHandTransform.rotation = netLeftHandRotation.Value;
            RemoteRightHandTransform.position = netRightHandPosition.Value;
            RemoteRightHandTransform.rotation = netRightHandRotation.Value;
        }

        private void UpdateHandControllerValues(bool isLeft)
        {
            if (isLeft)
            {
                netLeftGrip.Value = LeftHandController.GripAmount;
                netLeftPoint.Value = LeftHandController.PointAmount;
                netLeftThumb.Value = LeftHandController.ThumbAmount;
                netLeftHoldingItem.Value = LeftHandController.grabber.HoldingItem;
            }
            else
            {
                netRightGrip.Value = RightHandController.GripAmount;
                netRightPoint.Value = RightHandController.PointAmount;
                netRightThumb.Value = RightHandController.ThumbAmount;
                netRightHoldingItem.Value = RightHandController.grabber.HoldingItem;
            }
        }

        private void UpdateRemoteAnimations()
        {
            if (RemoteLeftHandAnimator)
            {
                UpdateHandAnimator(RemoteLeftHandAnimator, netLeftGrip.Value, netLeftPoint.Value,
                    netLeftThumb.Value, netLeftHoldingItem.Value);
            }
            if (RemoteRightHandAnimator)
            {
                UpdateHandAnimator(RemoteRightHandAnimator, netRightGrip.Value, netRightPoint.Value,
                    netRightThumb.Value, netRightHoldingItem.Value);
            }
        }

        private void UpdateHandAnimator(Animator animator, float grip, float point, float thumb, bool holdingItem)
        {
            animator.SetFloat("Flex", grip);
            animator.SetLayerWeight(2, point);
            animator.SetLayerWeight(1, thumb);

            if (holdingItem)
            {
                animator.SetLayerWeight(0, 0);
                animator.SetFloat("Flex", 1);
                animator.SetLayerWeight(1, 0);
                animator.SetLayerWeight(2, 0);
            }
        }

        public void AssignPlayerObjects()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            PlayerHeadTransform = getChildTransformByName(player.transform, "IKEyeTarget");
            PlayerLeftHandTransform = GameObject.Find("IKLeftHandTarget").transform;
            PlayerRightHandTransform = GameObject.Find("IKRightHandTarget").transform;



            LeftHandController = PlayerLeftHandTransform.parent.GetComponentInChildren<HandController>();
            RightHandController = PlayerRightHandTransform.parent.GetComponentInChildren<HandController>();
        }

        private Transform getChildTransformByName(Transform search, string name)
        {
            Transform[] children = search.GetComponentsInChildren<Transform>();
            foreach (Transform child in children)
            {
                if (child.name == name) return child;
            }
            return null;
        }

        private void toggleObjects(bool enableObjects)
        {
            RemoteHeadTransform.gameObject.SetActive(enableObjects);
            RemoteLeftHandTransform.gameObject.SetActive(enableObjects);
            RemoteRightHandTransform.gameObject.SetActive(enableObjects);
            disabledObjects = !enableObjects;
        }
    }
}