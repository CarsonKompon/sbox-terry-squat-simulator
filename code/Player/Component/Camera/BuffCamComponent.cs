using Sandbox;

namespace TSS
{
	class BuffCamComponent : EntityComponent<BuffPawn>
	{
		[ConVar.Replicated] public static bool thirdperson_orbit { get; set; } = false;

		[ConVar.Replicated] public static bool thirdperson_collision { get; set; } = true;

		[ClientInput] public Angles ViewAngles { get; set; }

		private Angles orbitAngles;
		private float orbitDistance = 150;

		[GameEvent.Client.PostCamera]
		public void OnFrame()
		{
			Camera.Position = Entity.Position;
			Vector3 targetPos;

			var center = Entity.Position + Vector3.Up * 64;

			if ( thirdperson_orbit )
			{
				Camera.Position += Vector3.Up * (Entity.CollisionBounds.Center.z * Entity.Scale);
				Camera.Rotation = Rotation.From( orbitAngles );

				targetPos = Camera.Position + Camera.Rotation.Backward * orbitDistance;
			}
			else
			{
				Camera.Position = center;
				Camera.Rotation = Rotation.Slerp(Camera.Rotation, ViewAngles.ToRotation(), Time.Delta * 3f);
				Camera.Rotation = Camera.Rotation.Angles().WithRoll( 0 ).ToRotation();

				float distance = 130.0f * Entity.Scale;
				targetPos = Camera.Position + Camera.Rotation.Right * ((Entity.CollisionBounds.Maxs.x + 15) * Entity.Scale);
				targetPos += Camera.Rotation.Forward * -distance;
			}

			if ( thirdperson_collision )
			{
				var tr = Trace.Ray( Camera.Position, targetPos )
					.Ignore( Entity )
					.Radius( 8 )
					.Run();

				Camera.Position = tr.EndPosition;
			}
			else
			{
				Camera.Position = targetPos;
			}

			Camera.FieldOfView = 70;
			Camera.FirstPersonViewer = null;
		}

		[GameEvent.Client.BuildInput]
		public void BuildInput()
		{
			if ( thirdperson_orbit && Input.Down( "walk" ) )
			{
				if ( Input.Down( "attack1" ) )
				{
					orbitDistance += Input.AnalogLook.pitch;
					orbitDistance = orbitDistance.Clamp( 0, 1000 );
				}
				else
				{
					orbitAngles.yaw += Input.AnalogLook.yaw;
					orbitAngles.pitch += Input.AnalogLook.pitch;
					orbitAngles = orbitAngles.Normal;
					orbitAngles.pitch = orbitAngles.pitch.Clamp( -89, 89 );
				}

				Input.AnalogLook = Angles.Zero;

				Input.StopProcessing = true;
			}

			Angles look = Input.AnalogLook;
			Angles viewAngles = ViewAngles;
			viewAngles += look;
			viewAngles.pitch = viewAngles.pitch.Clamp( -89f, 89f );
			viewAngles.roll = 0f;
			ViewAngles = viewAngles.Normal;
		}
	}
}
