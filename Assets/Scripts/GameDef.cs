// metadata of the games and animals in the main menu
using MalbersAnimations;
using System.Collections.Generic;
using System;
using UnityEngine;
using System.Linq;
using Assets.Scripts;
using PowerPetsRescue;
using MalbersAnimations.Utilities;
using MalbersAnimations.Controller;
using UnityEditorInternal;
using System.Runtime.InteropServices;
using UnityEngine.UIElements;

public class GameDef
{
    public string name;
    public Type GameType; // A GameInstance or derived type

    public PPRStartTransition StartTransition; // In case its a workflow based game
    public PPRState SingletonState; // In case its not a flow based game

    //*** If its to freeplay with an animal set a gameobject
    public List<AnimalDef> animals = new List<AnimalDef>();

    // Baby Animal that you get when feeding is complete
    public AnimalDef FeedingRewardAnimal;

    public string DisplayName
    {
        get => StartTransition != null ? StartTransition.DisplayName : name;
    }
}
