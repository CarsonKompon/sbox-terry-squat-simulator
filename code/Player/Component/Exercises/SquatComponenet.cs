﻿using Sandbox;
using System;


namespace TSS
{
	public partial class SquatComponent : ExerciseComponent
	{
		#region Members
		/// <summary>
		/// A value to drive whether or not we're in the 'up' or 'down' squat position. Used to drive animation and figure out when we've completed a full squat.
		/// </summary>
		[Net, Predicted]
		public int Squat { get; set; }

		/// <summary>
		/// A reference to the barbell model
		/// </summary>
		public ModelEntity Barbell;
		#endregion

		public override void Initialize()
		{
			ExerciseType = Exercise.Squat;
		}

		public override void Simulate( IClient client )
		{
			base.Simulate( client );
			var cam = Entity.Components.GetOrCreate<TSSCameraComponent>();
			SimulateSquatting( cam );

		}

		public override void Cleanup()
		{
			Barbell?.Delete();
			Barbell = null;
		}

		/// <summary>
		/// Initialize the squatting exercise state
		/// </summary>
		public void StartSquatting()
		{
			CreateBarbellClient();
			Squat = 0;
		}


		public void CreateBarbellClient()
		{
			var _boneTransform = Entity.GetBoneTransform( "spine_1", true );
			Barbell?.Delete();
			Barbell = new ModelEntity( "models/dumbbll/dumbbell.vmdl" );
			Barbell.Position = (Vector3)(_boneTransform.Position);
			Barbell.Position += Vector3.Up * 30f;
			Barbell.SetParent( Entity, "spine_1" );
			Barbell.Rotation = _boneTransform.Rotation * Rotation.From( 0, 90, 0 );
		}

		public void SetBarbellPosition()
		{
			if(Barbell == null){
				return;
			}
			var _boneTransform = Entity.GetBoneTransform( "spine_1", true );
			Barbell.SetParent( Entity, null );
			Barbell.Position = (Vector3)(_boneTransform.Position);
			Barbell.Position += Vector3.Up * 30f;
			Barbell.SetParent( Entity, "spine_1" );
			Barbell.Rotation = _boneTransform.Rotation * Rotation.From( 0, 90, 0 );
		}



		/// <summary>
		/// The Squatting exercise
		/// </summary>
		/// <param name="cam"></param>
		public void SimulateSquatting( TSSCameraComponent cam )
		{
			if ( cam == null )
			{
				return;
			}

			if ( Entity.TimeSinceExerciseStarted < 0.2f )
			{
				SetBarbellPosition();
			}

			//Set the anim parameter on S&Box.
			Entity.SetAnimParameter( "squat", Squat );

			if ( Entity.TimeSinceExerciseStopped < 3f && Squat != -1 && !cam.IntroComplete )
			{
				float f = (Entity.TimeSinceExerciseStopped - 1f) / 3f;
				f = MathF.Pow( f.Clamp( 0, 1f ), 3f );
				cam.Progress += Time.Delta * 0.025f * (1 - f);
			}

			if ( Entity.TimeSinceExerciseStopped < 3f && Squat != -1 && cam.IntroComplete )
			{
				cam.Progress += Time.Delta * 0.35f;
			}

			if ( Input.Pressed( "forward" ) && Input.Pressed( "backward" ) )
			{
				return;
			}

			if ( Input.Pressed( "forward" ) && (Squat == 0 || Squat == -1) && Entity.TimeSinceDownPressed > TSSGame.QUARTER_NOTE_DURATION )
			{

				if ( Squat == 0 )
				{

					Entity.ExercisePoints++;
					Entity.TargetExerciseSpeed += 0.1f;
					Entity.CreatePoint( 1 );
					Entity.SetScale( 1.2f );
					Entity.CounterBump( 0.5f );
					Entity.TimeSinceExerciseStopped = 0;


					if ( cam.Up != null )
						cam.Up.TextScale += 0.3f;
				}
				Squat = 1;
				Entity.TimeSinceUpPressed = 0;

			}

			if ( Input.Pressed( "backward" ) && (Squat == 1 || Squat == -1) && Entity.TimeSinceUpPressed > TSSGame.QUARTER_NOTE_DURATION )
			{
				Squat = 0;
				Entity.TimeSinceDownPressed = 0;
				if ( cam.Down != null )
					cam.Down.TextScale += 0.3f;
			}
		}


	}
}
