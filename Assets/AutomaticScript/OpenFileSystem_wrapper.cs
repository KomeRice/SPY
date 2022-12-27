using UnityEngine;
using FYFY;

public class OpenFileSystem_wrapper : BaseWrapper
{
	public UnityEngine.GameObject screen;
	public UnityEngine.UI.InputField newCampaignField;
	public UnityEngine.UI.Dropdown campaignDropdown;
	public UnityEngine.UI.Toggle newCampaignToggle;
	public UnityEngine.UI.InputField newLevelField;
	public UnityEngine.UI.Dropdown levelDropdown;
	public UnityEngine.UI.Toggle newLevelToggle;
	public UnityEngine.UI.Text warningMessage;
	public UnityEngine.GameObject height;
	public UnityEngine.GameObject width;
	public UnityEngine.UI.Button confirmButton;
	private void Start()
	{
		this.hideFlags = HideFlags.NotEditable;
		MainLoop.initAppropriateSystemField (system, "screen", screen);
		MainLoop.initAppropriateSystemField (system, "newCampaignField", newCampaignField);
		MainLoop.initAppropriateSystemField (system, "campaignDropdown", campaignDropdown);
		MainLoop.initAppropriateSystemField (system, "newCampaignToggle", newCampaignToggle);
		MainLoop.initAppropriateSystemField (system, "newLevelField", newLevelField);
		MainLoop.initAppropriateSystemField (system, "levelDropdown", levelDropdown);
		MainLoop.initAppropriateSystemField (system, "newLevelToggle", newLevelToggle);
		MainLoop.initAppropriateSystemField (system, "warningMessage", warningMessage);
		MainLoop.initAppropriateSystemField (system, "height", height);
		MainLoop.initAppropriateSystemField (system, "width", width);
		MainLoop.initAppropriateSystemField (system, "confirmButton", confirmButton);
	}

	public void campaignDropdownChanged()
	{
		MainLoop.callAppropriateSystemMethod (system, "campaignDropdownChanged", null);
	}

	public void newLevelToggleChanged()
	{
		MainLoop.callAppropriateSystemMethod (system, "newLevelToggleChanged", null);
	}

	public void guaranteeOne(UnityEngine.GameObject go)
	{
		MainLoop.callAppropriateSystemMethod (system, "guaranteeOne", go);
	}

	public void newCampaignToggleChanged()
	{
		MainLoop.callAppropriateSystemMethod (system, "newCampaignToggleChanged", null);
	}

	public void refreshLevelDropdown()
	{
		MainLoop.callAppropriateSystemMethod (system, "refreshLevelDropdown", null);
	}

	public void validateName()
	{
		MainLoop.callAppropriateSystemMethod (system, "validateName", null);
	}

	public void confirmButtonClicked()
	{
		MainLoop.callAppropriateSystemMethod (system, "confirmButtonClicked", null);
	}

	public void quitButtonClicked()
	{
		MainLoop.callAppropriateSystemMethod (system, "quitButtonClicked", null);
	}

}
