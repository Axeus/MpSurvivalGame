using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using System;


public class DragWindow : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler {

	private Vector2 offset;

	public void OnBeginDrag (PointerEventData eventData)
	{
		if (eventData.pointerId == -1)
		{
			offset = eventData.position - new Vector2(transform.parent.position.x,transform.parent.position.y);
		}
	}

	public void OnEndDrag (PointerEventData eventData)
	{

	}

	public void OnDrag (PointerEventData eventData)
	{
		if (eventData.pointerId == -1)
		{
			transform.parent.position = eventData.position - offset;
		}
	}
}
