using Sandbox;
using System;
using TSS.UI;
using System.Linq;

namespace TSS
{
	public partial class PunchQT : Entity
	{
		[Net]
		public int Type { get; set; }

		[Net, Predicted]
		public float MyTime { get; set; }

		[Net, Predicted]
		public float TargetTime { get; set; }

		[Net]
		public TSSPlayer Player { get; set; }

		[Net, Predicted]
		public TimeSince TimeSinceSpawned { get; set; }

		public PunchQTPanel Panel;

		public override void Spawn()
		{
			base.Spawn();
			MyTime =  ((60f/140f) * 2f);
			Transmit = TransmitType.Always;
			TimeSinceSpawned = 0f;
		}

		public new void Delete()
		{
			base.Delete();
			Panel?.Delete( true );
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();
			TimeSinceSpawned = 0f;

			Random Rand = new Random();
			Panel = new PunchQTPanel( this, new Vector2( Rand.Float( -20f, 20f ), Rand.Float( -20f, 20f ) ) );
			switch ( Type )
			{
				case 0:
					Panel.Key.Text = Input.GetButtonOrigin( "forward" ).ToUpper();
					break;
				case 1:
					Panel.Key.Text = Input.GetButtonOrigin( "backward" ).ToUpper();
					break;
				case 2:
					Panel.Key.Text = Input.GetButtonOrigin( "left" ).ToUpper();
					break;
				case 3:
					Panel.Key.Text = Input.GetButtonOrigin( "right" ).ToUpper();
					break;
			}
			MyTime = ((60f / 140f) * 2f);
		}

		[GameEvent.Client.BuildInput]
		public void BuildPunchInput( )
		{
			bool pressed = false;
			if ( Type == 0 )
			{
				pressed = Input.Pressed( "forward" );
			}
			if ( Type == 1 )
			{
				pressed = Input.Pressed( "backward" );
			}
			if ( Type == 2 )
			{
				pressed = Input.Pressed( "left" );
			}
			if ( Type == 3 )
			{
				pressed = Input.Pressed( "right" );
			}

			if ( TimeSinceSpawned < MyTime - 0.15f )
			{
				if ( Input.Pressed( "forward" ) || Input.Pressed( "backward" ) || Input.Pressed( "left" ) || Input.Pressed( "right" ) )
				{
					Panel.Finished = true;
					Panel.Failed = true;


					ConsoleSystem.Run( "delete_punch", this.NetworkIdent );
					return;
				}
			}



			if ( TimeSinceSpawned > MyTime - 0.15f && TimeSinceSpawned < MyTime + 0.15f )
			{
				if ( pressed )
				{
					ConsoleSystem.Run( "Punch" );

					if ( Game.IsClient )
					{
						Panel.Finished = true;
					}

					ConsoleSystem.Run( "delete_punch", this.NetworkIdent );
				}
			}
		}

		[ConCmd.Server( "delete_punch")]
		public static void DeletePunch(int i)
		{
			var ent = Entity.All.Where( x => x.NetworkIdent == i ).Any();
			if ( ent )
			{
				Entity.All.Where( x => x.NetworkIdent == i ).First().Delete();
			}
		}

		[GameEvent.Tick]
		public void Simulate()
		{
			if ( Player == null )
			{
				return;
			}
			
			if( Player.CurrentExercise != Exercise.Punch )
			{
				if ( Game.IsServer )
				{
					Delete();
				}
				Panel?.Delete();
			}

			

			

			if ( TimeSinceSpawned > MyTime+0.15f )
			{
				if ( Game.IsClient )
				{
					if ( Panel != null && !Panel.Finished )
					{
						Panel?.Delete( true );
					}
				}
				if ( Game.IsServer )
				{
					Delete();
					return;
				}
			}
		}
	}
}
