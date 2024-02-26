using gw.unium;
using System.Collections.Generic;
using UnityEngine;
using gw.gql;
using gw.proto.utils;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(UniumComponent))]
public class QAUtil : MonoBehaviour
{
#if !UNIUM_DISABLE && (DEVELOPMENT_BUILD || UNITY_EDITOR || UNIUM_ENABLE)

    void Awake()
    {
        Unium.RoutesHTTP.Add("/qa/objects", QAUtil.HandlerObjects);
        Unium.RoutesHTTP.Add("/qa/screenResolution", QAUtil.HandlerScreenResolution);
    }

    public static void HandlerObjects(RequestAdapter req, string path)
    {
        var query = new Query("/scene//*[activeSelf=true && activeInHierarchy=true]", Unium.Root).Select();
        var objects = query.Execute();
        req.Respond(QAUtil.UnityObjectSerailize(objects));
    }

    public static void HandlerScreenResolution(RequestAdapter req, string path)
    {
        req.Respond(JsonReflector.Reflect(new
        {
            displayHeight = Display.main.systemHeight,
            displayWidth = Display.main.systemWidth,
            screenHeight = Screen.height,
            screenWidth = Screen.width,
            currentHeight = Screen.currentResolution.height,
            currentWidth = Screen.currentResolution.width,
        }));
    }

    public static string UnityObjectSerailize(List<object> objects)
    {
        var json = new JsonBuilder();
        json.BeginObject();
        json.Name("scene");
        json.StringValue((SceneManager.GetActiveScene().name));

        json.Name("objects");
        json.BeginArray();
        foreach (var o in objects)
        {
            var go = o as GameObject;
            if (go == null)
            {
                return "null";
            }
            json.BeginObject();

            json.Name("id");
            json.Value(go.GetInstanceID().ToString());
            json.Name("name");
            json.StringValue(go.name);
            json.Name("tag");
            json.StringValue(go.tag);

            // components
            json.Name("components");
            json.BeginArray();
            var components = go.GetComponents<Component>();
            foreach (var c in components)
            {
                if (c != null)
                {
                    json.StringValue(c.GetType().Name);
                }
            }
            json.EndArray();

            // transform
            // json.Name("transform");
            // json.Value(TransformSerailize(go.transform));

            // position
            json.Name("position");
            try
            {
                Vector3 pos = GetRealPositionByObject(go);
                json.Value(JsonReflector.Reflect(pos));
            } catch (System.Exception)
            {
                json.Value(JsonReflector.Reflect(Vector3.one * -1));
            }

            json.Name("parentId");
            if (go.transform && go.transform.parent)
            {
                json.Value(go.transform.parent.gameObject.GetInstanceID().ToString());
            }
            else
            {
                json.Value("0");
            }

            // children
            // json.Name("children");
            // json.BeginArray();
            // foreach (Transform child in go.transform)
            // {
            //     if (child != null)
            //     {
            //         json.StringValue(child.name);
            //     }
            // }
            // json.EndArray();
            json.EndObject();
        }
        json.EndArray();
        json.EndObject();
        return json.GetString();
    }

    public static string TransformSerailize(Transform t)
    {
        if (t == null)
        {
            return "null";
        }

        var json = new JsonBuilder();

        json.BeginObject();

        json.Name("name");
        json.StringValue(t.name);


        json.Name("position");
        json.Value(JsonReflector.Reflect(t.position));

        json.Name("rotation");
        json.Value(JsonReflector.Reflect(t.rotation));

        json.Name("scale");
        json.Value(JsonReflector.Reflect(t.localScale));

        json.EndObject();
        return json.GetString();
    }

    public static GameObject GetObjectById(int objectId)
    {
        foreach (GameObject gameObject in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (gameObject.GetInstanceID() == objectId)
                return gameObject;
        }
        return null;
    }

    public static Vector3 GetObjectScreenPosition(GameObject gameObject, Camera camera)
    {
        var selectedCamera = camera;
        var position = gameObject.transform.position;
        Canvas canvas = gameObject.GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                if (gameObject.GetComponent<RectTransform>() != null)
                {
                    Vector3[] vector3S = new Vector3[4];
                    gameObject.GetComponent<RectTransform>().GetWorldCorners(vector3S);
                    position = new Vector3((vector3S[0].x + vector3S[2].x) / 2, (vector3S[0].y + vector3S[2].y) / 2, (vector3S[0].z + vector3S[2].z) / 2);
                }
                if (canvas.worldCamera != null)
                {
                    selectedCamera = canvas.worldCamera;
                }
                return selectedCamera.WorldToScreenPoint(position);
            }

            if (gameObject.GetComponent<RectTransform>() != null)
            {
                return gameObject.GetComponent<UnityEngine.RectTransform>().position;
            }
            return camera.WorldToScreenPoint(gameObject.transform.position);
        }

        var collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            position = collider.bounds.center;
        }

        return camera.WorldToScreenPoint(position);
    }

    public static Vector3 GetRealPositionByObject(GameObject gameObject)
    {
        Vector3 position = Vector3.one * -1;
        if (gameObject.transform.position.x == 0 && gameObject.transform.position.y == 0 && gameObject.transform.position.z == 0)
        {
            return position;
        }

        if (Camera.allCamerasCount == 0)
        {
            var rectTransform = gameObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                var canvas = rectTransform.GetComponentInParent<Canvas>();
                if (canvas != null)
                    position = RectTransformUtility.PixelAdjustPoint(rectTransform.position, rectTransform, canvas.rootCanvas);
            }
            return position;
        }

        foreach (var camera1 in Camera.allCameras)
        {
            if ((camera1.cullingMask & (1 << gameObject.layer)) != 0)
            {
                position = GetObjectScreenPosition(gameObject, camera1);
                if (position.x > 0 && position.y > 0 && position.x < Screen.width && position.y < Screen.height)
                {
                    position.y = Screen.height - position.y;
                    return position;
                }
            }
        }

        position = GetObjectScreenPosition(gameObject, Camera.main);
        if (position.x > 0 && position.y > 0 && position.x < Screen.width && position.y < Screen.height)
        {
            position.y = Screen.height - position.y;
            return position;
        }

        return Vector3.one * -1;;
    }
#endif
}
