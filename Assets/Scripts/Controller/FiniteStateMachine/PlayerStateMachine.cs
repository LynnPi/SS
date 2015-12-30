using UnityEngine;
using System.Collections;
using PalmPioneer.StateMachine;

public class PlayerStateMachine : FiniteStateMachine {

    protected override void Init() {
        ShowInQuestState showInQuest = new ShowInQuestState();
        AddState( showInQuest );

        PrepareLandingState prepareLanding = new PrepareLandingState();
        AddState( prepareLanding );

        PrepareLaunchState prepareLaunch = new PrepareLaunchState();
        AddState( prepareLaunch );

        LaunchState launch = new LaunchState();
        AddState( launch );

        LaunchPauseState launchPause = new LaunchPauseState();
        AddState( launchPause );

        DecelerateLaunchState decelerate = new DecelerateLaunchState();
        AddState( decelerate );

        LandedState landed = new LandedState();
        AddState( landed );

        BlockedOffState blocked = new BlockedOffState();
        AddState( blocked );

        defaultState = showInQuest;

        AddTransition<ShowInQuestState, LandedState>("StartExploring");
        AddTransition<PrepareLandingState, LandedState>("Landing");
        AddTransition<LandedState, PrepareLaunchState>("TakingOff");
        AddTransition<PrepareLaunchState, LaunchState>( "Pushing" );
        AddTransition<LaunchState, DecelerateLaunchState>( "Decelerate" );
        AddTransition<DecelerateLaunchState, LaunchState>( "Pushing" );
        AddTransition<DecelerateLaunchState, LaunchPauseState>("PauseLaunch");
        AddTransition<DecelerateLaunchState, PrepareLandingState>( "OnEnterCollider" );
        AddTransition<LaunchPauseState, LaunchState>( "Pushing" );
        AddTransition<LaunchPauseState, PrepareLandingState>( "OnEnterCollider" );
        AddTransition<LaunchState, PrepareLandingState>( "OnEnterCollider" );
        AddTransition<LaunchState, BlockedOffState>("Blocked");
        AddTransition<LaunchPauseState, BlockedOffState>( "Blocked" );
        AddTransition<LaunchState, BlockedOffState>( "Blocked" );
        AddTransition<BlockedOffState, LaunchPauseState>( "BlockedFinished" );
        AddTransition<PrepareLaunchState, LandedState>( "Reland" );
    }
}
