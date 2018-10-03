using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private const float AnimTime = .25f;
    private const float ZoomCloseX = 28f, ZoomCloseY = 42.4f, ZoomFarX = 51f, ZoomFarY = 74.8f;

    public GameObject[] KitchenWalls, KitchenBonusWalls, KitchenHideables;

    private class Area
    {
        public Vector3 Position { get; set; }
        public List<Wall> Walls { get; set; }
    }

    private class Action
    {
        public const int Rotate = 0;
        public const int Move = 1;
        public const int Zoom = 2;

        public readonly int ActionType;
        public Area TargetArea { get; set; }
        public int TargetOrientation { get; set; }
        public bool InwardsZoom { get; set; }

        public Action(int actionType)
        {
            ActionType = actionType;
        }
    }

    private class Wall
    {
        private const float LowerWallHeight = -3.8f;

        public readonly List<int> HideOrientations;
        public bool Hidden;

        private readonly GameObject _wallObj;
        private readonly GameObject _hideables;
        private readonly Vector3 _lowerPosition;


        public Wall(GameObject wallObj, List<int> hideOrientations, GameObject hideables = null)

        {
            _wallObj = wallObj;
            _hideables = hideables;
            HideOrientations = hideOrientations;

            _lowerPosition = new Vector3(0, LowerWallHeight, 0);
        }

        public IEnumerator Hide()
        {
            if (_hideables != null) _hideables.SetActive(false);
            var elapsed = 0f;
            while (elapsed < AnimTime)
            {
                _wallObj.transform.position =
                    Vector3.Lerp(_wallObj.transform.position, _lowerPosition, elapsed / AnimTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _wallObj.transform.position = _lowerPosition;
            Hidden = true;
        }

        public IEnumerator Show()
        {
            if (_hideables != null) _hideables.SetActive(true);

            var elapsed = 0f;
            while (elapsed < AnimTime)
            {
                _wallObj.transform.position =
                    Vector3.Lerp(_wallObj.transform.position, Vector3.zero, elapsed / AnimTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            _wallObj.transform.position = Vector3.zero;
            Hidden = false;
        }
    }

    private Area _areaKitchen, _areaCurrent;
    private Vector3 _zoomClose, _zoomFar;

    private bool _moving, _buffered;
    private int _orientation, _zoomLevel;

    private void Start()
    {
        _areaKitchen = new Area {Position = Vector3.zero, Walls = new List<Wall>()};
        for (var i = 0; i < KitchenWalls.Length; i++)
        {
            _areaKitchen.Walls.Add(new Wall(KitchenWalls[i], new List<int> {NumberWheel(i), NumberWheel(i + 1)},
                KitchenHideables[i]));
        }

        _areaKitchen.Walls.Add(new Wall(KitchenBonusWalls[0], new List<int> {1}));
        _areaKitchen.Walls.Add(new Wall(KitchenBonusWalls[1], new List<int> {3}));
        _areaCurrent = _areaKitchen;

        _zoomClose = new Vector3(ZoomCloseX, ZoomCloseY, ZoomCloseX);
        _zoomFar = new Vector3(ZoomFarX, ZoomFarY, ZoomFarX);
        _zoomLevel = 0;
    }

    private void Update()
    {
        if (_buffered) return;
        if (Input.GetKeyDown("s") && _zoomLevel != 0)
            StartCoroutine(BufferAction(new Action(Action.Zoom)));
        if (Input.GetKeyDown("w") && _zoomLevel != 1)
            StartCoroutine(BufferAction(new Action(Action.Zoom) {InwardsZoom = true}));
        if (Input.GetKeyDown("1"))
            StartCoroutine(BufferAction(new Action(Action.Move) {TargetArea = _areaKitchen}));
        if (_zoomLevel == 0) return;
        if (Input.GetKeyDown("a"))
            StartCoroutine(BufferAction(new Action(Action.Rotate) {TargetOrientation = NumberWheel(_orientation + 1)}));
        if (Input.GetKeyDown("d"))
            StartCoroutine(BufferAction(new Action(Action.Rotate) {TargetOrientation = NumberWheel(_orientation - 1)}));
    }

    private IEnumerator BufferAction(Action action)
    {
        _buffered = _moving;

        if (_buffered)
        {
            while (_moving) yield return null;
            _buffered = false;
        }

        switch (action.ActionType)
        {
            case Action.Rotate:
                StartCoroutine(Rotate(action));
                break;
            case Action.Move:
                StartCoroutine(Move(action));
                break;
            case Action.Zoom:
                StartCoroutine(ChangeZoom(action));
                break;
            default:
                yield break;
        }
    }

    private IEnumerator ChangeZoom(Action action)
    {
        _buffered = true;
        var elapsed = 0f;
        var targetPosition = action.InwardsZoom ? _zoomClose : _zoomFar;

        if (action.InwardsZoom)
        {
            StartCoroutine(_areaCurrent.Walls[0].Hide());
            StartCoroutine(_areaCurrent.Walls[3].Hide());
        }
        else
        {
            if (_orientation != 0)
            {
                StartCoroutine(Rotate(new Action(Action.Rotate) {TargetOrientation = 0}));
            }
            else
            {
                StartCoroutine(_areaCurrent.Walls[0].Show());
                StartCoroutine(_areaCurrent.Walls[3].Show());
            }
        }

        while (elapsed < AnimTime)
        {
            Camera.main.transform.position =
                Vector3.Lerp(Camera.main.transform.position, targetPosition, elapsed / AnimTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (!action.InwardsZoom && _areaCurrent.Walls[0].Hidden)
        {
            StartCoroutine(_areaCurrent.Walls[0].Show());
            StartCoroutine(_areaCurrent.Walls[3].Show());
        }

        Camera.main.transform.position = targetPosition;
        _zoomLevel = action.InwardsZoom ? 1 : 0;
        _buffered = false;
    }

    private IEnumerator Move(Action action)
    {
        var elapsed = 0f;
        _moving = true;

        _areaCurrent = action.TargetArea;
        while (elapsed < AnimTime)
        {
            transform.position = Vector3.Lerp(transform.position, action.TargetArea.Position, elapsed / AnimTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = action.TargetArea.Position;
        _moving = false;
    }

    private IEnumerator Rotate(Action action)
    {
        var elapsed = 0f;
        _moving = true;
        _orientation = action.TargetOrientation;
        foreach (var wall in _areaCurrent.Walls)
        {
            if (wall.HideOrientations.Count > 1)
            {
                if ((wall.HideOrientations[0] == _orientation || wall.HideOrientations[1] == _orientation) &&
                    !wall.Hidden)
                {
                    StartCoroutine(wall.Hide());
                    continue;
                }

                if (!(wall.HideOrientations[0] == _orientation || wall.HideOrientations[1] == _orientation) &&
                    wall.Hidden)
                {
                    StartCoroutine(wall.Show());
                }
            }
            else
            {
                if (wall.HideOrientations[0] == _orientation && !wall.Hidden)
                {
                    StartCoroutine(wall.Hide());
                    continue;
                }

                if (wall.HideOrientations[0] != _orientation && wall.Hidden) StartCoroutine(wall.Show());
            }
        }

        var targetRotation = Quaternion.Euler(Vector3.up * _orientation * 90f);
        while (elapsed < AnimTime)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, elapsed / AnimTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.rotation = targetRotation;
        _moving = false;
    }

    private static int NumberWheel(int value, int min = 0, int max = 3)
    {
        var solved = false;
        while (!solved)
        {
            if (value > max)
            {
                value = value - max - 1;
            }
            else if (value < min)
            {
                value = value + max + 1;
            }
            else solved = true;
        }

        return value;
    }
}