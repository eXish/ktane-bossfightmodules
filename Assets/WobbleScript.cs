using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WobbleScript : MonoBehaviour
{
    private List<Vector3> _scales = new List<Vector3>();
    private List<Quaternion> _rots = new List<Quaternion>();
    private float _scale = 0f;
    private const float TIME = 1f;

    private Vector3 _start;
    private Quaternion _startRot;

    private void Awake()
    {
        _start = transform.localScale;
        _startRot = transform.localRotation;
        _scales.Add(_start);
        _scales.Add(_start);

        _rots.Add(_startRot);
        _rots.Add(_startRot);
    }

    private void Update()
    {
        _scale += Time.deltaTime;
        while(_scale > TIME)
        {
            _scales.RemoveAt(0);
            _scales.Add(new Vector3(_start.x * Random.Range(0.7f, 1.3f), _start.y * Random.Range(0.7f, 1.3f), _start.z * Random.Range(0.7f, 1.3f)));
            _rots.Add(_startRot * Quaternion.Euler(Random.Range(-15f, 15f), Random.Range(-15f, 15f), Random.Range(-15f, 15f)));
            _scale -= TIME;
        }

        transform.localScale = Vector3.Lerp(_scales[0], _scales[1], _scale / TIME);
        transform.localRotation = Quaternion.Slerp(_rots[0], _rots[1], _scale / TIME);
    }
}
