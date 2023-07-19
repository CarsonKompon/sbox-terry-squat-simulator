using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using TSS.UI;

namespace TSS
{
	public partial class YogaQT : Entity
	{
		/// <summary>
		/// A list of the possible combos for the yoga positions
		/// </summary>
		string[] combos { get; set; } = { "0123", "11230", "001122", "21330", "011230" };

		/// <summary>
		/// The combo we're using for this QT
		/// </summary>
		string currentCombo { get; set; }

		/// <summary>
		/// The current index, starting at 0
		/// </summary>
		public int index { get; set; }


		public TSSPlayer Player { get; set; }

		public YogaQTPanel Panel;
		public TimeSince TimeSinceSpawned;

		public int pose;

		public override void Spawn()
		{
			base.Spawn();
			
			Random Rand = new Random();

			pose = Rand.Int( 0, 4 );

			currentCombo = combos[pose];

			Panel = new YogaQTPanel( this, new Vector2( Rand.Float( -20f, 20f ), Rand.Float( -20f, 20f) ), currentCombo );
			Player = Entity.All.OfType<TSSPlayer>().First();
			TimeSinceSpawned = 0;
		}

		public override void ClientSpawn()
		{
			base.ClientSpawn();


		}

		[GameEvent.Client.BuildInput]
		public void BuildYogaInput( )
		{
			Random Rand = new Random();
			if ( currentCombo == null )
			{
				currentCombo = Rand.FromArray( combos );
			}

			index = index.Clamp( 0, currentCombo.Length - 1 );
			var type = currentCombo[index];

			bool b = CheckType( type );

			if ( CheckFailure( type ) )
			{
				Panel.Failed = true;
				Panel.TimeSinceFinished = 0;
				Delete();
			}

			if ( b )
			{
				index++;
			}

			if ( index >= currentCombo.Length )
			{
				Panel.Finished = true;
				Panel.TimeSinceFinished = 0;
				ConsoleSystem.Run( "yoga_pose", pose + 1 );
				Delete();
			}

			if ( Player.CurrentExercise != Exercise.Yoga )
			{
				if ( Game.IsServer )
				{
					Delete();
				}
				Panel?.Delete();
			}

			if ( TimeSinceSpawned > 3f )
			{
				Panel.Failed = true;
				Panel.TimeSinceFinished = 0;
				Delete();
			}
		}


		/// <summary>
		/// In theory there's a way better way to do this, but I'm not really sure how
		/// TODO: I'm sure there's some bitwise shit we could do to figure this out
		/// </summary>
		bool CheckFailure( char type )
		{

			if ( type == '0' )
			{

				if ( Input.Pressed( "right" ) || Input.Pressed( "left" ) || Input.Pressed( "backward" ) )
				{
					return true;
				}
			}

			if ( type == '1' )
			{
				if ( Input.Pressed( "right" ) || Input.Pressed( "left" ) || Input.Pressed( "forward" ) )
				{
					return true;
				}
			}

			if ( type == '2' )
			{
				if ( Input.Pressed( "backward" ) || Input.Pressed( "left" ) || Input.Pressed( "forward" ) )
				{
					return true;
				}
			}

			if ( type == '3' )
			{
				if ( Input.Pressed( "backward" ) || Input.Pressed( "right" ) || Input.Pressed( "forward" ) )
				{
					return true;
				}
			}

			return false;
		}

		public bool CheckType( char c )
		{
			switch ( c )
			{
				case '0':
					return Input.Pressed( "forward" );
				case '1':
					return Input.Pressed( "backward" );
				case '2':
					return Input.Pressed( "right" );
				case '3':
					return Input.Pressed( "left" );
			}
			return false;
		}
	}
}
