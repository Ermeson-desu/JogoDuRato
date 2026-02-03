using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Helper;

namespace GameDuMouse
{
    public enum PlayerState
    {
        Idle,
        Running,
        TransitionToRun,
        TransitionToIdle
    }

    public class AnimationController
    {
        private Dictionary<PlayerState, Animation> animations = new();
        public Animation CurrentAnimation { get; private set; }
        private SpriteEffects currentEffects = SpriteEffects.None;
        public PlayerState CurrentState { get; private set; }
        private bool isTransitioning = false;
        private double transitionTimer = 0;
        private double transitionDuration;

        public Vector2 Position
        {
            get => CurrentAnimation.Position;
            set
            {
                foreach (var anim in animations.Values)
                    anim.Position = value;
            }
        }

        public void AddAnimation(PlayerState state, Animation animation)
        {
            animations[state] = animation;
            if (CurrentAnimation == null)
            {
                CurrentAnimation = animation;
                CurrentState = state;
            }
        }
        public void StartTransition(PlayerState nextState, double duration)
        {
            if (!animations.ContainsKey(nextState)) return;
            isTransitioning = true;
            transitionTimer = 0;
            transitionDuration = duration;
            CurrentAnimation.Reset();
            CurrentAnimation = animations[nextState];
            CurrentAnimation.Effects = currentEffects;
            CurrentState = nextState;
        }
        public void SetState(PlayerState state)
        {
            if (animations.ContainsKey(state))
            {
                CurrentAnimation = animations[state];
                CurrentAnimation.Effects = currentEffects;
                CurrentAnimation.Reset();
                CurrentState = state;
            }
        }
        public void Update(GameTime gameTime)
        {
            if (isTransitioning)
            {
                transitionTimer += gameTime.ElapsedGameTime.TotalMilliseconds;
                if (transitionTimer >= transitionDuration)
                {
                    isTransitioning = false;
                    if (CurrentState == PlayerState.TransitionToRun)
                        SetState(PlayerState.Running);
                    else if (CurrentState == PlayerState.TransitionToIdle)
                        SetState(PlayerState.Idle);
                }
            }
            CurrentAnimation.Update(gameTime);
        }
        public void Draw(GameTime gameTime)
        {
            CurrentAnimation.Draw(gameTime);
        }
        public SpriteEffects Effects
        {
            get => currentEffects;
            set
            {
                currentEffects = value;
                foreach (var anim in animations.Values)
                {
                    anim.Effects = currentEffects;
                    
                }
            }
        }
    }
}