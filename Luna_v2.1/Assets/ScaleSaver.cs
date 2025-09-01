using UnityEngine;
using PixelCrushers;

[AddComponentMenu("Pixel Crushers/Save System/Savers/Scale Saver")]
public class ScaleSaver : Saver
{
    public Transform target;

    public override string RecordData()
    {
        if (target == null) target = this.gameObject.transform;
        Vector3 s = target.localScale;
        return $"{s.x},{s.y},{s.z}";
    }

    public override void ApplyData(string data)
    {
        if (target == null) target = this.gameObject.transform;
        if (!string.IsNullOrEmpty(data))
        {
            var parts = data.Split(',');
            if (parts.Length == 3)
            {
                float x, y, z;
                if (float.TryParse(parts[0], out x) && float.TryParse(parts[1], out y) && float.TryParse(parts[2], out z))
                {
                    target.localScale = new Vector3(x, y, z);
                }
            }
        }
    }
}
