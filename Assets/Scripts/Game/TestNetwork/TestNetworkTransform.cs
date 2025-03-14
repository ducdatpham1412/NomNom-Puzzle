using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Components;
using UnityEngine;

public class TestNetworkTransform : NetworkTransform {
    TestEggManager manager;
    bool isCorrecting = false;

    void Start() {
        manager = GetComponent<TestEggManager>();
    }

    protected override bool OnIsServerAuthoritative() {
        return false;
    }

    [ClientRpc]
    void ReconcileClientRpc(NetworkTransformState state, ClientRpcParams rpcParams) {
        NetworkObject networkObject = GetComponent<NetworkObject>();
        Vector3 newPosition = state.GetPosition();
        if (!state.HasPositionX) {
            newPosition.x = transform.position.x;
        }
        if (!state.HasPositionY) {
            newPosition.y = transform.position.y;
        }
        if (!state.HasPositionZ) {
            newPosition.z = transform.position.z;
        }
        Quaternion newRotation = state.GetRotation();
        if (!state.HasRotAngleX) {
            newRotation.x = transform.rotation.x;
        }
        if (!state.HasRotAngleY) {
            newRotation.y = transform.rotation.y;
        }
        if (!state.HasRotAngleZ) {
            newRotation.z = transform.rotation.z;
        }
        Vector3 newScale = state.GetScale();
        if (!state.HasScaleX) {
            newScale.x = transform.localScale.x;
        }
        if (!state.HasScaleY) {
            newScale.y = transform.localScale.y;
        }
        if (!state.HasScaleZ) {
            newScale.z = transform.localScale.z;
        }

        networkObject.GetComponent<NetworkTransform>().Teleport(
            newPosition,
            newRotation,
            newScale
        );
    }

    protected override void OnNetworkTransformStateUpdated(ref NetworkTransformState oldState, ref NetworkTransformState newState) {
        if (IsServer) {
            if (isCorrecting) {
                isCorrecting = false;
                return;
            }

            float distance = Vector3.Distance(newState.GetPosition(), oldState.GetPosition());
            if (distance > manager.moveSpeed / 10) {
                isCorrecting = true;
                ReconcileClientRpc(oldState, new ClientRpcParams {
                    Send = new ClientRpcSendParams {
                        TargetClientIds = new List<ulong>() { OwnerClientId }
                    }
                });
            }
        }
    }
}
