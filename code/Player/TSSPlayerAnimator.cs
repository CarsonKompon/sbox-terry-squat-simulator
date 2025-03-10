﻿using Sandbox;
using System;

namespace TSS 
{ 
	public class TSSPlayerAnimator : PawnAnimator
	{
		TimeSince TimeSinceFootShuffle = 60;


		float duck;

		public override void Simulate()
		{
			var idealRotation = Rotation.LookAt( Input.Rotation.Forward.WithZ( 0 ), Vector3.Up );

			DoRotation( idealRotation );
			DoWalk();

			//
			// Let the animation graph know some shit
			//
			bool sitting = HasTag( "sitting" );
			bool noclip = HasTag( "noclip" ) && !sitting;

			SetAnimParameter( "b_grounded", GroundEntity != null || noclip || sitting );
			SetAnimParameter( "b_noclip", noclip );
			SetAnimParameter( "b_sit", sitting );

			if ( Host.IsClient && Client.IsValid() )
			{
				SetAnimParameter( "voice", Client.TimeSinceLastVoice < 0.5f ? Client.VoiceLevel : 0.0f );
			}

			Vector3 aimPos = Pawn.EyePosition + Input.Rotation.Forward * 200;
			Vector3 lookPos = aimPos;

			//
			// Look in the direction what the player's input is facing
			//
			SetLookAt( "lookat_pos", lookPos ); // old
			SetLookAt( "aimat_pos", aimPos ); // old

			SetLookAt( "aim_eyes", lookPos );
			SetLookAt( "aim_head", lookPos );
			SetLookAt( "aim_body", aimPos );

			SetAnimParameter( "b_ducked", HasTag( "ducked" ) ); // old

			if ( HasTag( "ducked" ) ) duck = duck.LerpTo( 1.0f, Time.Delta * 10.0f );
			else duck = duck.LerpTo( 0.0f, Time.Delta * 5.0f );

			SetAnimParameter( "duck", duck );


			SetAnimParameter( "holdtype", 0 );
			SetAnimParameter( "aimat_weight", 0.5f ); // old
			SetAnimParameter( "aim_body_weight", 0.5f );


		}

		public virtual void DoRotation( Rotation idealRotation )
		{
			//
			// Our ideal player model rotation is the way we're facing
			//
			var allowYawDiff = 90;

			float turnSpeed = 0.01f;
			if ( HasTag( "ducked" ) ) turnSpeed = 0.1f;

			//
			// If we're moving, rotate to our ideal rotation
			//
			Rotation = Rotation.Slerp( Rotation, idealRotation, WishVelocity.Length * Time.Delta * turnSpeed );

			//
			// Clamp the foot rotation to within 120 degrees of the ideal rotation
			//
			Rotation = Rotation.Clamp( idealRotation, allowYawDiff, out var change );

			//
			// If we did restrict, and are standing still, add a foot shuffle
			//
			if ( change > 1 && WishVelocity.Length <= 1 ) TimeSinceFootShuffle = 0;

			SetAnimParameter( "b_shuffle", TimeSinceFootShuffle < 0.1 );
		}

		void DoWalk()
		{
			// Move Speed
			{
				var dir = Velocity;
				var forward = Rotation.Forward.Dot( dir );
				var sideward = Rotation.Right.Dot( dir );

				var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

				SetAnimParameter( "move_direction", angle );
				SetAnimParameter( "move_speed", Velocity.Length );
				SetAnimParameter( "move_groundspeed", Velocity.WithZ( 0 ).Length );
				SetAnimParameter( "move_y", sideward );
				SetAnimParameter( "move_x", forward );
				SetAnimParameter( "move_z", Velocity.z );
			}

			// Wish Speed
			{
				var dir = WishVelocity;
				var forward = Rotation.Forward.Dot( dir );
				var sideward = Rotation.Right.Dot( dir );

				var angle = MathF.Atan2( sideward, forward ).RadianToDegree().NormalizeDegrees();

				SetAnimParameter( "wish_direction", angle );
				SetAnimParameter( "wish_speed", WishVelocity.Length );
				SetAnimParameter( "wish_groundspeed", WishVelocity.WithZ( 0 ).Length );
				SetAnimParameter( "wish_y", sideward );
				SetAnimParameter( "wish_x", forward );
				SetAnimParameter( "wish_z", WishVelocity.z );
			}
		}

		public override void OnEvent( string name )
		{
			// DebugOverlay.Text( Pos + Vector3.Up * 100, name, 5.0f );

			if ( name == "jump" )
			{
				Trigger( "b_jump" );
			}

			base.OnEvent( name );
		}
	}
}
