using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PopupMouseSensitive : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public bool isOver = false;

	public void OnPointerEnter(PointerEventData eventData)
	{
		isOver = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		isOver = false;
	}

	public void OnDisable()
	{
		if (isOver)
			isOver = false;
	}
}
