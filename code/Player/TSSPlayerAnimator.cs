using Sandbox;
using System;

namespace TSS 
{ 
	public class TSSPlayerAnimatorComponent : EntityComponent<TSSPlayer>
	{

		public void Simulate( IClient cl )
		{
			if(!Entity.IsValid()) return;
			if(Entity.Controller == null) return;

			var allowYawDiff = 90;
			var turnSpeed = 0.01f;
			if ( Entity.Controller.HasTag( "ducked" ) ) turnSpeed = 0.1f;
			var idealRotation = Rotation.LookAt( Camera.Rotation.Forward.WithZ( 0 ), Vector3.Up );
			Entity.Rotation = Rotation.Slerp( Entity.Rotation, idealRotation, Entity.Controller.WishVelocity.Length * Time.Delta * turnSpeed );
			Entity.Rotation = Entity.Rotation.Clamp( idealRotation, allowYawDiff, out var shuffle ); // lock facing to within 45 degrees of look direction

			CitizenAnimationHelper animHelper = new CitizenAnimationHelper( Entity );

			animHelper.WithWishVelocity(Entity.Controller.WishVelocity);
			animHelper.WithVelocity(Entity.Controller.Velocity);
			animHelper.WithLookAt(Entity.EyePosition + Entity.EyeRotation.Forward * 100.0f, 1.0f, 1.0f, 0.5f);
			animHelper.AimAngle = Entity.ViewAngles.ToRotation();
			animHelper.FootShuffle = shuffle;
			animHelper.IsSitting = Entity.Controller.HasTag( "sitting" );
			animHelper.IsNoclipping = Entity.Controller.HasTag( "noclip" );
			animHelper.IsGrounded = (Entity.GroundEntity != null);
			animHelper.VoiceLevel = (Game.LocalPawn == Entity) ? Voice.Level : Entity.Client.Voice.CurrentLevel;
			animHelper.DuckLevel = MathX.Lerp(animHelper.DuckLevel, Entity.Controller.HasTag("ducked") ? 1 : 0, Time.Delta * 10.0f);

			if(Entity.Controller.HasEvent("jump")) animHelper.TriggerJump();

			if ( Entity.ActiveChild is BaseCarriable carry )
			{
				carry.SimulateAnimator( animHelper );
			}
			else
			{
				animHelper.HoldType = CitizenAnimationHelper.HoldTypes.None;
				animHelper.AimBodyWeight = 0.5f;
			}


		}
	}
}
