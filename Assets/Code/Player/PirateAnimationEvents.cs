using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PirateAnimationEvents : MonoBehaviour
{
    [SerializeField] GameObject tinyBullet;
    [SerializeField] float tinyBulletSpeed = 100;
    [SerializeField] float tinyBulletLifetimeSec = 3;
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
}
