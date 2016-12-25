using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Food : MonoBehaviour {

    public AudioClip eatingClip;

	private Characteristics characteristics;
    private float timer;
    private bool isActive;
    private AudioSource audioSrc;
    private Animator weaponAnimator;

    void Start () 
	{
        audioSrc = GetComponent<AudioSource>();

        characteristics = GetComponentInParent<Characteristics>();

        weaponAnimator = Global.player.GetChild(0).GetComponentInChildren<Animator>();
    }
	
	void Update () 
	{
		if(isActive)
		{
			timer += Time.deltaTime;
			if(timer > 1f)
			{
				timer = 0f;
				isActive = false;
			}
		}
		if(!Global.inventoryControl.IsInventoryOrContainersActive() && !isActive)
		{
			if(Input.GetButtonUp("Fire1"))
			{
				isActive = true;
				Global.playerStats.AddFullness(characteristics.power);
                Global.playerStats.Heal(characteristics.power/2);

                if (eatingClip != null)
                {
                    audioSrc.PlayOneShot(eatingClip);
                }

                weaponAnimator.SetTrigger("Attack");

                int slot = Global.inventoryControl.quickInv.slotSelected;

				ItemBehavior data = Global.inventoryControl.quickInv.slots[slot].transform.GetChild(0).GetComponent<ItemBehavior>();

				data.amount -= 1;

				if(data.amount <= 0)
				{
                    Global.inventoryControl.quickInv.items[slot] = new Item();

                    Global.inventoryControl.quickInv.SelectSlot(slot, true);

					GameObject t = Global.inventoryControl.quickInv.slots[slot].transform.GetChild(0).gameObject;

					t.transform.SetParent(null);
					Destroy(t);
				}
				else
					data.transform.GetComponentInChildren<Text>().text = data.amount.ToString();
			}
		}
	}
}
