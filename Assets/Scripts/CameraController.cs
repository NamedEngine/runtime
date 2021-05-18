using System.Collections.Generic;
using System.Linq;
using Language.Classes;
using UnityEngine;

public class CameraController : MonoBehaviour {
    Camera _camera;
    CameraFrame _frame;
    List<TopDownPlayer> _players = new List<TopDownPlayer>();
    float _currentMaxHeight;

    void Awake() {
        _camera = GetComponent<Camera>();
    }

    public void SetFrame(CameraFrame frame) {
        _frame = frame;
    }

    public void AddPlayer(TopDownPlayer player) {
        _players.Add(player);
    }

    void LateUpdate() {
        if (_players.Count == 0) {
            return;
        }

        var newPos = _players
            .Select(player => player.transform.position)
            .Aggregate(new Vector3(0 , 0, transform.position.z), (res, pos) => res + pos) / _players.Count;

        if (_frame == null || _frame.size.Value == Vector2.zero) {
            transform.position = newPos;
            return;
        }
        //TODO frame logic

        // _currentMaxHeight
        // var frameBounds = GetFrameBounds(_frame);
        // var cameraBounds = _camera.
        transform.position = newPos;
    }

    static Bounds GetFrameBounds(CameraFrame frame) {
        return new Bounds(frame.transform.position, frame.size.Value);
    }
}
