using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WobbleScript : MonoBehaviour
{
    //[UnityEditor.MenuItem("dostuff/dostuff")]
    //[ExecuteInEditMode]
    //private static void dostuff()
    //{
    //    foreach(WobbleScript s in FindObjectsOfType<WobbleScript>())
    //    {
    //        s.GetComponent<KMSelectable>().Highlight = s.GetComponentInChildren<KMHighlightable>();
    //        s.GetComponent<KMSelectable>().SelectableColliders = new Collider[] { s.GetComponent<MeshCollider>() };
    //    }
    //}

    private List<Vector3> _scales = new List<Vector3>();
    private List<Quaternion> _rots = new List<Quaternion>();
    private float _scale = 0f;
    private const float TIME = 1f;

    private Vector3 _start;
    private Quaternion _startRot;

    private bool _stop;

    private void Awake()
    {
        _start = transform.localScale;
        _startRot = transform.localRotation;
        _scales.Add(_start);
        _scales.Add(_start);

        _rots.Add(_startRot);
        _rots.Add(_startRot);

        StartCoroutine(Move());

        //GetComponent<KMSelectable>().OnInteract += () => { FlyAway(); return false; };
    }

    private IEnumerator Move()
    {
        while(!_stop)
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
            yield return null;
        }
    }

    public void FlyAway()
    {
        StartCoroutine(FlyAwayInternal());
    }

    private IEnumerator FlyAwayInternal()
    {
        Vector3 start = transform.localPosition;
        Vector3 vec = transform.localPosition;
        Vector3 scale = transform.localScale;

        vec.y = 0;
        vec.Normalize();
        vec *= 0.06f;

        _stop = true;

        float time = Time.time;
        while(Time.time - time < .5f)
        {
            transform.localPosition = start + vec * (Time.time - time) * 2f;
            transform.localScale = scale * (.5f + time - Time.time) * 2f;
            yield return null;
        }

        transform.localScale = Vector3.zero;

        UnhookParent();
    }

    private void UnhookParent()
    {
        GetComponent<KMSelectable>().Parent.Children = GetComponent<KMSelectable>().Parent.Children.Where(c => c.name != name).ToArray();
        GetComponent<KMSelectable>().Parent.UpdateChildrenProperly();
    }
}
