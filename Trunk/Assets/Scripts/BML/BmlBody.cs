using System;
using System.Collections.Generic;
using UnityEngine;
namespace Behaviour
{
	/// <summary>
	/// Groups BML behaviours that sync with one another / are generated from a FML chunk
	/// </summary>
	public class BMLBody
	{
		public Dictionary<string, BMLChunk> Chunks { get { return chunks; } }
		Dictionary<string, BMLChunk> chunks = new Dictionary<string, BMLChunk>();

		public bool isDone = false;
		public bool syncComplete = false;

		public float latestEnd = 0f;



		public Speech SpeechChunk { get; private set; }

		BMLBody executeAfter;
		BMLBody executeWith;

		public bool IsReady() {
			return executeAfter == null || executeAfter.isDone;
		}

		public bool Synchronized() {
			return executeWith == null || executeWith.SpeechChunk == null || executeWith.syncComplete;
		}

		public bool NeedsSync() {
			return executeWith != null;
		}

		public Dictionary<string, BMLChunk> GetSyncChunks() {
			if (executeWith != null) {
				return executeWith.Chunks;
			} else {
				return null;
			}
		}

		public void ExecuteAfter(BMLBody body) {
			executeAfter = body;
		}

		public void ExecuteWith (BMLBody body)
		{
			executeWith = body;
		}

		public void AddChunk(BMLChunk chunk) {
			if (chunk.Type == BMLChunkType.Speech)
				SpeechChunk = chunk as Speech;
			else {
				if(chunks.ContainsKey(chunk.ID)) return;
				chunks.Add (chunk.ID, chunk);
				latestEnd = Math.Max (latestEnd, chunk.End); 
			}
		}
	}
}

