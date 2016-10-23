using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerIOClient;

namespace MultiAlt
{
	public delegate void Heard(Alt Whisperer, Alt me, string Message, params object[] Told);

    public class Alt
	{
		#region Variables
		/// <summary>
		/// The custom data the owner put on the alt, i.e.
		/// thisalt["cool"] = true;
		/// thisalt["pi"] = 3.141;
		/// </summary>
		private Dictionary<string, object> privData = new Dictionary<string, object>();

		/// <summary>
		/// The altmanager that controls this alt.
		/// </summary>
		private AltManager master;

		/// <summary>
		/// The client the alt is using to login to EE
		/// </summary>
		internal Client client;

		/// <summary>
		/// The connection the alt is using to join worlds
		/// </summary>
		internal Connection connection;

		/// <summary>
		/// If the alt is in a world
		/// </summary>
		internal bool inworld = false;

		/// <summary>
		/// The AltId the owner set on the alt at construction time
		/// </summary>
		public string AltId { get; private set; }

		/// <summary>
		/// When another alt whispers to this alt, this event gets fired.
		/// </summary>
		public event Heard Heard;
		#endregion

		#region Constructor
		/// <summary>
		/// Create a new alt.
		/// </summary>
		/// <param name="altId">The custom id for the alt, this can be anything you want.</param>
		/// <param name="alt">The client the alt uses to login to an EE world</param>
		/// <param name="master_">The master of the alt, who this alt has to obey.</param>
		public Alt(string altId, Client alt, AltManager master_)
		{
			AltId = altId;
			master = master_;
			client = alt;
		}
		#endregion

		/// <summary>
		/// Tell another alt something
		/// </summary>
		/// <param name="Whisper">The alt to whisper to</param>
		/// <param name="Message">The message to tell it</param>
		/// <param name="Telling">What you're telling the alt</param>
		public void TellAlt(Alt Whisperer, string Message, params object[] Telling)
		{
			Whisperer.Heard(this, Whisperer, Message, Telling);
		}

		#region Sending Messages
		/// <summary>
		/// Send a message to EE
		/// </summary>
		/// <param name="e">The message to send</param>
		public void Send(Message e)
		{
			connection.Send(e);
		}

		/// <summary>
		/// Send a message to EE
		/// </summary>
		/// <param name="type">The type of message, i.e. "b" or "m" or "init"</param>
		/// <param name="args">The arguments the messge has</param>
		public void Send(string type, params object[] args)
		{
			connection.Send(type, args);
		}
		#endregion

		#region World Handling

		/// <summary>
		/// Join a world
		/// </summary>
		/// <param name="roomId">The roomId</param>
		/// <param name="joinData">The join data</param>
		public void JoinRoom(string roomId, Dictionary<string, string> joinData)
		{
			Connection joinWorld = client.Multiplayer.JoinRoom(roomId, joinData);
			JoinWorld(joinWorld);
		}

		/// <summary>
		/// Create a Join Room
		/// </summary>
		/// <param name="roomId">The room id</param>
		/// <param name="roomType">The room type</param>
		/// <param name="visible">Is the world visiable</param>
		/// <param name="roomData">The roomdata you provide</param>
		/// <param name="joinData">The joindata you provide</param>
		public void CreateJoinRoom(string roomId, string roomType, bool visible, Dictionary<string, string> roomData, Dictionary<string, string> joinData)
		{
			if (connection != null)
			{
				if (connection.Connected)
				{
					Disconnect();
				}
			}
			Connection joinWorld = client.Multiplayer.CreateJoinRoom(roomId, roomType, visible, roomData, joinData);
			JoinWorld(joinWorld);
		}
	

		/// <summary>
		/// Join a world with the given connection
		/// </summary>
		/// <param name="_joinWorld">The connection given</param>
		internal void JoinWorld(Connection _joinWorld)
		{
			inworld = true;
			connection = _joinWorld;
			connection.OnMessage += connection_OnMessage;
			connection.OnDisconnect += connection_OnDisconnect;
		}

		/// <summary>
		/// Disconnect from the world.
		/// </summary>
		public void Disconnect()
		{
			connection.Disconnect();
			inworld = false;
		}
		#endregion

		#region Internal Message Handlers
		/// <summary>
		/// When the alt gets disconnected
		/// </summary>
		/// <param name="sender">Who disconnected the alt</param>
		/// <param name="message">What the error message was</param>
		internal void connection_OnDisconnect(object sender, string message)
		{
			inworld = false;
			master.AltGotDisconnected(sender, message, this);
		}

		/// <summary>
		/// When the alt recieves a message
		/// </summary>
		/// <param name="sender">Who sent it</param>
		/// <param name="e">The message recieved</param>
		internal void connection_OnMessage(object sender, Message e)
		{
			inworld = true;
			master.AltGotMessage(e, this);
		}
		#endregion

		/// <summary>
		/// Set internal data for the alt
		/// </summary>
		/// <param name="index">The index to reference internal data</param>
		/// <returns>The internal data set that you set it. If no data set, it automatically returns null.</returns>
		public object this[string index]
		{
			get
			{
				if (privData.ContainsKey(index))
				{
					return privData[index];
				}
				else
				{
					return null;
				}
			}
			set
			{
				privData[index] = value;
			}
		}
	}
}
