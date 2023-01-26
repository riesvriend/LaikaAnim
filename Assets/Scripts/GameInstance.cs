using Assets.Scripts;
using MalbersAnimations;
using Oculus.Interaction;
using PowerPetsRescue;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GameInstance : MonoBehaviour
{
    public GameDef gameDef;
    public PlaygroundInput playground;

    public PPRState state;

    public List<AnimalInstance> animals = new List<AnimalInstance>();
    public AnimalInstance activeAnimal { get; private set; }

    protected CombingTask combingTask = null;
    protected FeedingTask feedingTask = null;

    protected GameObject comb;
    protected GameObject apple;

    protected int? gameDurationSeconds;
    protected DateTime startedUtc;
    protected const int checkTimeoutFrameInterval = 30;
    protected int checkTimeoutFrameCounter = 0;

    public ProgressModel ProgressModel
    {
        get => playground.plankUI.ProgressModel;
    }

    // Dont call this 'Start' as that collides with a Unity event name
    public virtual void StartGame()
    {
        //playground.EnableInteractors(gameDef.usesPokeInteractors);

        playground.pettingHandPoseHandler.game = this;

        if (gameDef.SingletonState != null)
            state = gameDef.SingletonState;

        var startTransition = gameDef.StartTransition;
        if (startTransition != null)
        {
            state = startTransition.NextState;
            gameDurationSeconds = gameDef.StartTransition.GameDurationSeconds;
        }
        else
        {
            gameDurationSeconds = null;
        }
        startedUtc = DateTime.UtcNow;
        ProgressModel.IsGameOver = false;

        SynchronizeGameWithState(gameDef.StartTransition);
    }

    private void Execute(PPRTransition transition)
    {
        state = transition.NextState;
        SynchronizeGameWithState(transition);
    }

    private void SynchronizeGameWithState(PPRTransition transition)
    {
        playground.table.SetActive(state.IsTableVisible);

        if (state.IsCombVisible)
        {
            if (combingTask == null)
            {
                InstantiateComb();

                combingTask = gameObject.AddComponent<CombingTask>();
                combingTask.GrabbableObject = comb;
                combingTask.game = this;
            }
        }
        else // comb not visible
            DestroyCombingTask();

        if (state.IsAppleVisible)
        {
            InstantiateApple();

            feedingTask = gameObject.AddComponent<FeedingTask>();
            feedingTask.GrabbableObject = apple;
            feedingTask.game = this;
        }
        else // apple not visible
            DestroyFeedingTask();

        var animalsToAdd = gameDef.animals; // For single state games that are not Flow/transition based
        if (transition != null)
            animalsToAdd = transition.AnimalsToAdd;

        foreach (var animalDef in animalsToAdd)
            AddAnimal(animalDef);

        ProgressModel.ScoreProgress.IsVisible = state.IsScored;
        if (state.IsScored)
        {
            ProgressModel.ScoreProgress.Title =
                $"Top: {gameDef.StartTransition.TopScoreAnimalCount}. Score:";
        }

        ProgressModel.TimeProgress.IsVisible = state.IsScored; // Consider adding IsTimed
        ProgressModel.TimeProgress.Title = "Time left:";

        ProgressModel.TaskProgress.IsVisible = state.UsesProgressBar;
    }

    private void Update()
    {
        if (checkTimeoutFrameCounter > 0)
            checkTimeoutFrameCounter -= 1;
        else
        {
            checkTimeoutFrameCounter = checkTimeoutFrameInterval;

            if (
                gameDurationSeconds.HasValue
                && gameDurationSeconds.Value > 0
                && !ProgressModel.IsGameOver
            )
            {
                var secondsLeft = SecondsLeft();
                ProgressModel.TimeProgress.ProgressBarText = $"{secondsLeft} seconds";
                ProgressModel.TimeProgress.Percentage =
                    100f - (float)secondsLeft / gameDurationSeconds.Value * 100;

                if (secondsLeft == 0)
                {
                    ProgressModel.IsGameOver = true;
                    ProgressModel.TaskProgress.IsVisible = false;

                    var score = animals.Count;
                    var isNewTopScore = score > gameDef.StartTransition.TopScoreAnimalCount;
                    if (isNewTopScore)
                    {
                        ProgressModel.ScoreProgress.Title = "TOP SCORE!";
                        gameDef.StartTransition.TopScoreAnimalCount = score;
                    }

                    ProgressModel.TimeProgress.Title = "";
                    ProgressModel.TimeProgress.ProgressBarText = "GAME OVER";
                    playground.PlaySoundGameOver();
                    StartCoroutine(RemoveObjectsOnGameOverCoRoutine());
                }
            }
        }

        var ai = activeAnimal?.ai;
        if (ai == null)
            return;

        // Workaround: clear the animals target once it has arrived there.
        // This way, we prevent the animals from running back to the target if
        // one of the animals is re-selected to come back to the player
        if (!ai.ActiveAgent && ai.Target != null)
            ai.Target = null;
    }

    private int SecondsLeft()
    {
        var timeSpent = (int)(DateTime.UtcNow - startedUtc).TotalSeconds;
        var timeLeft = Math.Max(gameDurationSeconds.Value - timeSpent, 0);
        return timeLeft;
    }

    private AnimalInstance AddAnimal(AnimalDef animalDef)
    {
        var animal = playground.InstantiateAnimal(animalDef);
        animals.Add(animal);

        ProgressModel.ScoreProgress.Percentage = ((float)animals.Count / 20) * 100;
        if (ProgressModel.ScoreProgress.Percentage > 100)
            ProgressModel.ScoreProgress.Percentage = 100;
        var plural = animals.Count != 1 ? "s" : "";
        ProgressModel.ScoreProgress.ProgressBarText = $"{animals.Count} animal{plural}";

        SetActiveAnimal(animal);
        SyncTasksWithAnimals();

        var animalPosition = PositionAnimal(animal);

        // Place the apple 90 degrees from the animal (to the side of the animal)
        var applePosition =
            animalPosition.Position
            - Quaternion.AngleAxis(140, Vector3.up) * animalPosition.Forward * -0.4f;

        PositionGrabInteractable(apple, applePosition);
        // Drop the comb on the apple
        PositionGrabInteractable(comb, applePosition + Vector3.up * 0.3f);

        // Send the new animal to the player (in case its dropped to far away)
        // playground.pettingHandPoseHandler.HandleCome();

        return animal;
    }

    private void PositionGrabInteractable(GameObject interactable, Vector3 position)
    {
        if (interactable != null)
        {
            var grabInteractable = interactable.GetComponent<GrabInteractable>();
            // Only reposition the item if its not being held at the moment
            if (!grabInteractable.SelectingInteractors.Any())
                interactable.transform.position = position;
        }
    }

    private void SyncTasksWithAnimals()
    {
        foreach (var animal in animals)
        {
            if (combingTask != null && animal.animalDef.CanBeCombed)
                combingTask.SyncAnimals(animals);

            if (feedingTask != null && animal.animalDef.EatsApples)
                feedingTask.SyncAnimals(animals);
        }
    }

    public void SetActiveAnimal(AnimalInstance animal)
    {
        activeAnimal = animal;
    }

    public struct AnimalPosition
    {
        public Vector3 Forward;
        public Vector3 Position;
    }

    AnimalPosition PositionAnimal(AnimalInstance animal)
    {
        if (animal?.ai != null)
        {
            // Prevent it to start running off directly and running through the player
            animal.ai.Stop();
            animal.ai.ClearTarget();
        }

        var camera = playground.cameraOrEyeTransform;

        var forward = camera.forward;
        // Forward from camera needs to be projected to the horizontal plane
        forward.y = 0f;
        forward = forward.normalized;
        if (state.IsTableVisible)
            // When sitting at the table, try to align the virtual table with the real table by disregarding camera/head
            // and assume the world view is reset to face the table
            forward = Vector3.forward;

        var animalPos = camera.position;
        if (animal != null)
        {
            animalPos += forward * animal.animalDef.animalDistanceFromCameraInMeter;
        }
        Quaternion animalRotation = Quaternion.identity;
        Transform animalTransform = camera;
        if (animal != null)
        {
            // Animal is on the floor (0), unless its on the table
            animalPos.y = 0f;

            // https://stackoverflow.com/questions/22696782/placing-an-object-in-front-of-the-camera
            var animalYRotation = new Quaternion(
                0.0f,
                camera.transform.rotation.y,
                0.0f,
                camera.transform.rotation.w
            ).eulerAngles;
            if (state.IsTableVisible)
                animalYRotation = forward;

            animalRotation = Quaternion.Euler(
                animalYRotation.x + 180,
                animalYRotation.y,
                animalYRotation.z + 180
            );

            animalTransform = animal.gameObject.transform;
            animalTransform.position = animalPos;
            animalTransform.rotation = animalRotation;
        }

        if (state.IsTableVisible)
        {
            var tablePos = camera.position + forward * playground.tableDistanceFromCameraInMeter;

            // put the table-top below the camera
            // TODO: instead of hardcoding the 0.6 meter between eyes and table top, consider
            // measuring the height of the lowest hand/controller and presume this to be the user's table height
            // or, give the user a control mechanism to move the virtual table up or down
            tablePos.y = -0.11f; // 85cm - 11 = 74cm, a common table.

            playground.table.transform.position = tablePos;
            playground.table.transform.rotation = animalRotation;

            // Move the animal up, onto the table
            var animalHeightOnTableTop = Math.Max(
                0f,
                tablePos.y
                    + playground.tableHeight
                    + 0.2f /* margin */
                    + 0.8f // 80cm above the table to create a drop from the sky effect
            );
            if (animalTransform != null)
                animalTransform.position = new Vector3(
                    animalPos.x,
                    animalHeightOnTableTop,
                    animalPos.z
                );
        }

        return new AnimalPosition { Forward = forward, Position = animalTransform.position };
    }

    private void DestroyComb()
    {
        DestroyInteractable(ref comb);
    }

    private void DestroyApple()
    {
        DestroyInteractable(ref apple);
    }

    private void DestroyInteractable(ref GameObject interactableObject)
    {
        if (interactableObject != null)
        {
            interactableObject.SetActive(false);
            var destoryThis = interactableObject;
            interactableObject = null;
            // The delay is most likely not needed, it was a workaround for a bug with the grab interactable
            // when the poke interactable took precedence over it.
            StartCoroutine(DestroyAfterDelay(destoryThis));
            //// TODO: first release grab, and create co-route do delete the object after a short while
            //var grabInteractable = interactableObject.GetComponent<GrabInteractable>();
            //grabInteractable.SelectingInteractors
            //    .ToList()
            //    .ForEach(interactor => grabInteractable.RemoveInteractor(interactor));
            //Destroy(interactableObject);
        }
    }

    IEnumerator DestroyAfterDelay(GameObject destoryThis)
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(destoryThis);
    }

    private void InstantiateComb()
    {
        comb = GameObject.Instantiate(playground.CombTemplate);
        comb.SetActive(true);
    }

    private void InstantiateApple()
    {
        apple = GameObject.Instantiate(playground.AppleTemplate);
        apple.SetActive(true);
    }

    /// <summary>
    /// We dont rely on Destroy to prevent that we can access all the
    /// related objects
    /// </summary>
    public void Stop()
    {
        if (combingTask != null)
            combingTask.Stop();
    }

    private void OnDestroy()
    {
        DestroyAllTasks();

        while (animals.Count > 0)
        {
            var animal = animals[0];
            animals.RemoveAt(0);
            animal.Dispose();
        }
    }

    private void DestroyAllTasks()
    {
        DestroyCombingTask();
        DestroyFeedingTask();
    }

    private void DestroyFeedingTask()
    {
        if (feedingTask != null)
        {
            DestroyApple();
            Destroy(feedingTask);
            feedingTask = null;
        }
    }

    private void DestroyCombingTask()
    {
        if (combingTask != null)
        {
            DestroyComb();
            Destroy(combingTask);
            combingTask = null;
        }
    }

    public void SetTarget(IWayPoint wayPoint)
    {
        var ai = activeAnimal?.ai;
        if (ai == null)
            return;
        ai.SetTarget(wayPoint.transform);
    }

    public PPRTransition NextTransition
    {
        get
        {
            var nextTransitions = state.NextTransitions;
            // For now, the task completed / gameover transitions must be in that order
            // TODO: add logic filtering based on condition expression / arguments in the transition such as IsTimeout
            return nextTransitions?.FirstOrDefault();
        }
    }

    internal void TaskCompleted(CombingTask brushingTask)
    {
        playground.PlaySoundTaskCompleted();

        var transition = NextTransition;
        if (transition == null)
            // Non-flow based game
            AddAnimal(animals.First().animalDef);
        else
            Execute(transition);
    }

    internal void TaskCompleted(FeedingTask feedingTask)
    {
        playground.PlaySoundTaskCompleted();

        var transition = NextTransition;
        if (transition == null)
        {
            // Non-flow based game
            // add a baby animal after feeding
            var reward = gameDef.FeedingRewardAnimal;
            if (reward == null)
                reward = FirstAnimal.animalDef;
            AddAnimal(reward);
        }
        else
            Execute(transition);
    }

    internal void OnActivatePopupMenu(bool isMenuActive)
    {
        // Don't do this as sometimes the animals start to fly away...
        // (animator sometimes disables gravity or something)
        //EnableAnimalAnimations(enable: !isMenuActive);
    }

    private void EnableAnimalAnimations(bool enable)
    {
        // when the popup menu is activated, we get performance problems that prevent
        // the user to click on the popup menu items.
        // Therefore, we disable all animals in the game while the menu is active
        foreach (var animal in animals)
        {
            if (animal.gameObject.TryGetComponent<Animator>(out var animator))
                animator.enabled = enable;
        }
    }

    // Hack
    public AnimalInstance FirstAnimal
    {
        get => animals.FirstOrDefault();
    }

    IEnumerator RemoveObjectsOnGameOverCoRoutine()
    {
        var wait = new WaitForSeconds(0.8f);
        IEnumerator Remove(GameObject item)
        {
            if (item == null)
                yield return null;
            else
            {
                item.SetActive(false);
                yield return wait;
            }
        }

        EnableAnimalAnimations(enable: false);

        yield return Remove(comb);
        yield return Remove(apple);
        foreach (var animal in animals)
            yield return Remove(animal.gameObject);
    }
}

public class JustShowTheAnimalGame : FreeplayGame
{
    public override void StartGame()
    {
        base.StartGame();
    }
}

public class HomeScreenGame : FreeplayGame
{
    public override void StartGame()
    {
        base.StartGame();
    }
}

public class FreeplayGame : GameInstance
{
    public override void StartGame()
    {
        base.StartGame();

        /*

            1.	x De controller trilt als je borstelt
            2.	x Toon een progress bar voor het borstelen op de menuplank
            3.	x Klaar met borstelen, een nieuwe konijn in random kleur
            4.	x Klaar met borstelen, een nieuwe konijn in dezelfde kleur
            6.	x Voer de appel
            7.	x Mjom mjom
            8.	x baby konijn
            9.
            10.	x De konijnen lopen naar de appel
        11.
         */
    }
}

public class RoomScaleGame : GameInstance
{
    public override void StartGame()
    {
        base.StartGame();
    }
}

public class FlowGame : GameInstance
{
    private PPRStartTransition StartTransition => gameDef.StartTransition;

    public override void StartGame()
    {
        base.StartGame();

        /*

            1.	x De controller trilt als je borstelt
            2.	x Toon een progress bar voor het borstelen op de menuplank
            3.	x Klaar met borstelen, een nieuwe konijn in random kleur
            4.	x Klaar met borstelen, een nieuwe konijn in dezelfde kleur
            6.	x Voer de appel
            7.	x Mjom mjom
            8.	x baby konijn
            9.
            10.	x De konijnen lopen naar de appel
            11.
         */
    }
}
