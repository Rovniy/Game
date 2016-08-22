using UnityEngine;
using System.Collections;

public class playerData : MonoBehaviour {
    Animator animator;
    public Transform pistol;
    public GameObject decal;

    public Transform rightHand;
    private handItem item;
    private Vector3 lookTarget;
    private Transform shootingWeaponInHand;


	// Use this for initialization
	void Start () {
        animator = GetComponent<Animator>();

    }
	
	// Update is called once per frame
	void Update () {

        //Вывожу луч из камеры для постоянного взгляда на курсор
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward * 10f);
        RaycastHit hit;

        //Взгляд и направление прицела игрока
        if (Physics.Raycast(ray, out hit))
        {
            lookTarget = hit.point;
        } else
        {
            lookTarget = ray.GetPoint(100f);
        }

        if (animator.GetBool("isRightHand") == true)
        {
            if (Input.GetMouseButtonUp(0))
            {
                Debug.Log("нажал");
                GameObject shooting_hole = Instantiate<GameObject>(decal);
                shooting_hole.transform.position = hit.point + hit.normal * 0.01f;
                shooting_hole.transform.rotation = Quaternion.LookRotation(-hit.normal);
                shooting_hole.transform.SetParent(hit.transform);
                Rigidbody r = hit.transform.gameObject.GetComponent<Rigidbody>();
                if(r != null)
                {
                    r.AddForceAtPosition(-hit.normal * 100f, hit.point);
                }
            }
                

        }
        
        


    }

    public void addHand(handItem it)
    {
        if (item != null)
        {
            item.transform.SetParent(null);
            item.gameObject.AddComponent<Rigidbody>();
        }
        it.transform.SetParent(rightHand);
        it.transform.localPosition = it.position;
        it.transform.localRotation = Quaternion.Euler(it.rotation);
        Destroy(it.GetComponent<Rigidbody>());
        item = it;
        animator.SetBool("isRightHand", true);
        animator.SetFloat("inHandRadius", it.radius);
        shootingWeaponInHand = it.transform;

    }

    void OnAnimatorIK()
    {
        //Взгляд персонажа на камеру. 
        animator.SetLookAtPosition(lookTarget);
        animator.SetLookAtWeight(1f);

        if (animator.GetBool("isRightHand") == true)
        {
            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            animator.SetIKPosition(AvatarIKGoal.RightHand, lookTarget);
        }
        
    }
}
