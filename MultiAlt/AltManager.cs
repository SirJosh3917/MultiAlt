using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PlayerIOClient;
using System.Threading;

namespace MultiAlt
{
	public delegate void Recieve(Message e, Connection con, Alt id);
	public delegate void Disconnect(object sender, string message, Alt disconnected);

	public class AltManager
	{
		/// <summary>
		/// The alts it has
		/// </summary>
		private List<Alt> alts;

		/// <summary>
		/// Happens when an event is recieved from one of the alts
		/// </summary>
		public event Recieve GotMessage;

		/// <summary>
		/// Happens when an alt is disconnected.
		/// </summary>
		public event Disconnect Disconnected;

		/// <summary>
		/// Construct an AltManager
		/// </summary>
		public AltManager()
		{
			alts = new List<Alt>();
		}

		/// <summary>
		/// Whisper to all of the alts something
		/// </summary>
		/// <param name="Message">The message to tell them</param>
		/// <param name="Telling">What you're telling them</param>
		public void TellAllAlts(string Message, params object[] Telling)
		{
			foreach(Alt i in alts)
			{
				i.TellAlt(i, Message, Telling);
			}
		}

		#region Split Work
		/// <summary>
		/// Split the work between all of the alts.
		/// </summary>
		/// <param name="BeforeSend">The Action to do before sending the message</param>
		/// <param name="Work">The list of messages to send</param>
		/// <param name="AfterSend">The Action to do after sending a message</param>
		/// <param name="DetermingSend">The Action to determine whether or not to send the message. This also controls BeforeSend firing, and AfterSend firing.</param>
		public void SplitWork(Action BeforeSend, List<Message> Work, Action AfterSend, Func<Message, bool> DetermingSend)
		{
			List<List<Message>> AltWork = new List<List<Message>>();

			int Alts = alts.Count;
			int WorkLoad = Work.Count;

			int MessagesPerAlt = WorkLoad / Alts;

			for (int a = 0; a < Alts; a++)
			{
				List<Message> WorkPayload = new List<Message>();
				for (int b = 0; b < MessagesPerAlt; b++)
				{
					WorkPayload.Add(Work[0]);
					Work.RemoveAt(0);
				}
				if (a == Alts - 1)
				{
					while (Work.Count > 0)
					{
						WorkPayload.Add(Work[0]);
						Work.RemoveAt(0);
					}
				}
				AltWork.Add(WorkPayload);
			}

			foreach (Alt i in alts)
			{
					List<Message> seperateWork = new List<Message>();
					foreach (Message x in AltWork[0])
					{
						seperateWork.Add(x);
					}
					new Thread(delegate()
					{
						foreach (Message x in seperateWork)
						{
							if (DetermingSend == null)
							{
								if (BeforeSend != null)
								{ BeforeSend(); }
								i.Send(x);
								if (AfterSend != null)
								{ AfterSend(); }
							}
							else if(DetermingSend(x))
							{
								if (BeforeSend != null)
								{ BeforeSend(); }
								i.Send(x);
								if (AfterSend != null)
								{ AfterSend(); }
							}
						}
					}).Start();
				AltWork.RemoveAt(0);
			}
		}

		#region Overloads
		public void SplitWork(Action BeforeSend, List<Message> Work)
		{
			SplitWork(BeforeSend, Work, null, null);
		}

		public void SplitWork(List<Message> Work, Action AfterSend)
		{
			SplitWork(null, Work, AfterSend, null);
		}

		public void SplitWork(List<Message> Work)
		{
			SplitWork(null, Work, null, null);
		}

		public void SplitWork(Action BeforeSend, List<Message> Work, Func<Message, bool> DetermineSend)
		{
			SplitWork(BeforeSend, Work, null, DetermineSend);
		}

		public void SplitWork(List<Message> Work, Action AfterSend, Func<Message, bool> DetermineSend)
		{
			SplitWork(null, Work, AfterSend, DetermineSend);
		}

		public void SplitWork(List<Message> Work, Func<Message, bool> DetermineSend)
		{
			SplitWork(null, Work, null, DetermineSend);
		}

		#endregion
		#endregion

		#region Alt Events
		/// <summary>
		/// Occurs when an alt gets disconnected
		/// </summary>
		/// <param name="sender">Who disconnected them</param>
		/// <param name="message">The message recieved when disconnected</param>
		/// <param name="id">The alt that disconnected</param>
		internal void AltGotDisconnected(object sender, string message, Alt id)
		{
			if (alts.Contains(id))
			{
				Disconnected(sender, message, id);
			}
		}

		/// <summary>
		/// Occurs when an alt gets a message
		/// </summary>
		/// <param name="e">The message</param>
		/// <param name="id">The id of the alt</param>
		internal void AltGotMessage(Message e, Alt id)
		{
			if(alts.Contains(id))
			{
				GotMessage(e, id.connection, id);
			}
		}
		#endregion

		#region Getting Alts
		/// <summary>
		/// Get an alt at an index given.
		/// Alts are stored in a list so it depends on which alt you've added first/last
		/// </summary>
		/// <param name="index">The index to find it at</param>
		/// <returns>null if not found, otherwise returns an alt based on the index given.</returns>
		public Alt GetAlt(int index)
		{
			if(alts.Count > index)
			{
				return alts[index];
			}
			throw new IndexOutOfRangeException();
		}

		/// <summary>
		/// Get an alt based on their string alt id
		/// </summary>
		/// <param name="AltId">The string ID you gave them</param>
		/// <returns>null if not found, otherwise returns the alt you want.</returns>
		public Alt GetAlt(string AltId)
		{
			foreach(Alt i in alts)
			{
				if(i.AltId == AltId)
				{
					return i;
				}
			}
			return null;
		}
		#endregion

		/// <summary>
		/// If all the other alts contain the same index and value
		/// </summary>
		/// <param name="index">The index</param>
		/// <param name="value">The value to check</param>
		/// <returns>If all the other alts contain the same index and value, returns true. Else, returns false.</returns>
		public bool AllAltsContain(string index, object value)
		{
			foreach(Alt i in alts)
			{
				if(i[index] != value)
				{
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// Checks if one of the alts contains this value
		/// </summary>
		/// <param name="index">The index to check</param>
		/// <param name="value">The value to check</param>
		/// <returns>If one of the alts contains this value, returns true. Else, returns false.</returns>
		public bool AltsContain(string index, object value)
		{
			foreach(Alt i in alts)
			{
				if(i[index] == value)
				{
					return true;
				}
			}
			return false;
		}

		public void AddAlt(Alt e)
		{
			foreach(Alt i in alts)
			{
				if(i.AltId == e.AltId)
				{
					throw new ArgumentException("An alt with the same id already exists in the Alt Manager.");
				}
			}
			alts.Add(e);
		}

		/// <summary>
		/// Gets the list of all the alts in use.
		/// </summary>
		/// <returns>A list of alts in use.</returns>
		public List<Alt> GetAlts()
		{
			List<Alt> ret = new List<Alt>();
			foreach(Alt i in alts)
			{
				ret.Add(i);
			}
			return ret;
		}
		
		#region Removing Alts
		/// <summary>
		/// Remove an alt
		/// </summary>
		/// <param name="e"></param>
		public void RemoveAlt(Alt e)
		{
			alts.Remove(e);
		}

		/// <summary>
		/// Remove an alt based on their id
		/// </summary>
		/// <param name="AltId">Their alt id</param>
		public void RemoveAlt(string AltId)
		{
			Alt got = GetAlt(AltId);

			if (got != null)
			{
				alts.Remove(got);
			}
		}

		/// <summary>
		/// Remove an alt at a certain index
		/// </summary>
		/// <param name="index">The index the alt was found at</param>
		public void RemoveAlt(int index)
		{
			Alt got = GetAlt(index);

			if (got != null)
			{
				alts.Remove(got);
			}
		}
		#endregion

		#region Sending Messages
		/// <summary>
		/// Make all the alts send a message
		/// </summary>
		/// <param name="e">The message to send</param>
		public void Send(Message e)
		{
			foreach (Alt i in alts)
			{
				if(i.connection != null)
				{
					if(i.connection.Connected)
					{
						i.connection.Send(e);
					}
				}
			}
		}

		/// <summary>
		/// Make all the alts send a message
		/// </summary>
		/// <param name="type">The type of message, i.e. "b", or "m", or "init"</param>
		/// <param name="values">The values within that message</param>
		public void Send(string type, params object[] values)
		{
			foreach (Alt i in alts)
			{
				if (i.connection != null)
				{
					if (i.connection.Connected)
					{
						i.connection.Send(type, values);
					}
				}
			}
		}
		#endregion

		#region Joining Worlds
		/// <summary>
		/// Force all the alts to JoinRoom a world
		/// </summary>
		/// <param name="roomId">The roomId</param>
		/// <param name="joinData">The join data</param>
		public void JoinRoom(string roomId, Dictionary<string, string> joinData)
		{
			foreach(Alt i in alts)
			{
				Connection joinWorld = i.client.Multiplayer.JoinRoom(roomId, joinData);
				i.JoinWorld(joinWorld);
			}
		}

		/// <summary>
		/// Force all the alts to CreatJoinRoom a world
		/// </summary>
		/// <param name="roomId">The room id to join</param>
		/// <param name="roomType">The room type</param>
		/// <param name="visible">If the world is visible</param>
		/// <param name="roomData">The room data</param>
		/// <param name="joinData">The join data</param>
		public void CreateJoinRoom(string roomId, string roomType, bool visible, Dictionary<string, string> roomData, Dictionary<string, string> joinData)
		{
			foreach (Alt i in alts)
			{
				if (i.connection != null)
				{
					if (i.connection.Connected)
					{
						i.connection.Disconnect();
					}
				}
				Connection joinWorld = i.client.Multiplayer.CreateJoinRoom(roomId, roomType, visible, roomData, joinData);
				i.JoinWorld(joinWorld);
			}
		}
		#endregion
	}
}
