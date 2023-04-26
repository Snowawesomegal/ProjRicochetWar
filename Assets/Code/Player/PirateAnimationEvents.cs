using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateAnimationEvents : MonoBehaviour
{
    [SerializeField] GameObject tinyBullet;
    [SerializeField] GameObject blunderbusExplosionPrefab;
    [SerializeField] float tinyBulletSpeed = 100;
    [SerializeField] float tinyBulletLifetimeSec = 3;
    [SerializeField] float blunderbusLifetimeSec = 3;

    [SerializeField] List<BlunderbusExplosionPresets> blunderbusPresetList;

    List<float> tinyBulletConeAdjustments = new List<float>() { 0, -5, 5 };
    Control1 c1;

    private void Start()
    {
        c1 = GetComponent<Control1>();
    }

    [SerializeField] GameObject smallBullet;

    public void TinyBulletCone(float angle)
    {
        foreach (float i in tinyBulletConeAdjustments)
        {
            Vector2 direction = AngleMath.Vector2FromAngle(angle - i) * tinyBulletSpeed;
            direction = new Vector2(direction.x * (c1.facingRight?1:-1), direction.y);

            GameObject newBullet = Instantiate(tinyBullet, transform.position, Quaternion.identity);
            newBullet.GetComponent<Rigidbody2D>().AddForce(direction);
            Destroy(newBullet, tinyBulletLifetimeSec);
            newBullet.transform.GetChild(0).GetComponent<HitboxInfo>().owner = gameObject;
        }

    }

    public void BlunderbusExplosion(string name)
    {
        BlunderbusExplosionPresets settings = null;
        foreach(BlunderbusExplosionPresets i in blunderbusPresetList)
        {
            if (i.name == name)
            {
                settings = i;
                break;
            }
        }

        if (settings == null)
        {
            Debug.Log("No Blunderbus Explosion settings exist in the list with the name " + name);
            return;
        }

        GameObject newBullet = Instantiate(blunderbusExplosionPrefab, transform.position + new Vector3(settings.offset.x * (c1.facingRight?1:-1), settings.offset.y, 0), Quaternion.identity);

        Vector2 angle = AngleMath.Vector2FromAngle(settings.angle) * new Vector2(c1.facingRight ? 1 : -1, 1);
        float floatAngle = Vector2.Angle(Vector2.right, angle) * (angle.y < 0 ? -1 : 1);

        newBullet.transform.rotation = Quaternion.Euler(0, 0, floatAngle - 90);
        Destroy(newBullet, blunderbusLifetimeSec);
        foreach (Transform i in newBullet.transform.GetChild(0))
        {
            i.GetComponent<HitboxInfo>().owner = gameObject;
        }
    }

    [System.Serializable]
    class BlunderbusExplosionPresets
    {
        public Vector3 offset;
        public float angle;
        public string name;
    }
}
