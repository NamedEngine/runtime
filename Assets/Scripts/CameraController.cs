using System.Collections.Generic;
using System.Linq;
using Language.Classes;
using UnityEngine;

public class CameraController : MonoBehaviour {
    Camera _camera;
    CameraFrame _frame;
    readonly HashSet<TopDownPlayer> _players = new HashSet<TopDownPlayer>();
    public float maxHeight;
    float CurrentHeight {
        get => _camera.orthographicSize * 2;
        set => _camera.orthographicSize = value / 2;
    }

    Vector2 Position {
        get => transform.position;
        set => transform.position = new Vector3(value.x, value.y, transform.position.z);
    }

    void Awake() {
        _camera = GetComponent<Camera>();
    }

    public void SetFrame(CameraFrame frame) {
        _frame = frame;
    }

    public void AddPlayer(TopDownPlayer player) {
        _players.Add(player);
    }

    public void RemovePlayer(TopDownPlayer player) {
        _players.Remove(player);
    }

    void LateUpdate() {
        var newPosition = GetNewPosition();
        AdjustPositionAndHeight(newPosition, _frame);
    }

    Vector2 GetNewPosition() {
        if (_players.Count == 0) {
            return Position;
        }

        var newPosition = _players
            .Select(player => (Vector2) player.transform.position)
            .Aggregate(new Vector2(0 , 0), (res, pos) => res + pos) / _players.Count;

        return newPosition;
    }

    void AdjustPositionAndHeight(Vector2 newPosition, CameraFrame frame) {
        Position = newPosition;
        
        if (frame == null || frame.size.Value == Vector2.zero) {
            CurrentHeight = maxHeight;
            return;
        }
        
        CurrentHeight = Mathf.Min(maxHeight, GetMaxHeightFromFrame(frame));

        var cameraCenterBounds = GetCameraCenterBounds(frame);
        Position = cameraCenterBounds.ClosestPoint(Position);
    }
    
    float GetMaxHeightFromFrame(CameraFrame frame) {
        return Mathf.Min(frame.size.Value.y, frame.size.Value.x / _camera.aspect);
    }

    Bounds GetCameraCenterBounds(CameraFrame frame) {
        var cameraSize = new Vector2(_camera.aspect * CurrentHeight, CurrentHeight);
        return new Bounds(frame.transform.position, frame.size.Value - cameraSize);
    }
}
