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
        /// <summary>
        /// The chunks, indexed by id
        /// </summary>
		public Dictionary<string, BMLChunk> Chunks { get { return chunks; } }
		Dictionary<string, BMLChunk> chunks = new Dictionary<string, BMLChunk>();

        /// <summary>
        /// Is the body done executing?
        /// </summary>
		public bool isDone = false;
        /// <summary>
        /// Is the body done synchronising the behaviours with the speech (if any)?
        /// </summary>
		public bool syncComplete = false;

        /// <summary>
        /// The total duration of the body
        /// </summary>
		public float latestEnd = 0f;


        /// <summary>
        /// Speech chunk, if any
        /// </summary>
		public Speech SpeechChunk { get; private set; }

        /// <summary>
        /// A body it should execute after
        /// </summary>
		BMLBody executeAfter;
        /// <summary>
        /// A body it should execute alongside
        /// </summary>
		BMLBody executeWith;

        /// <summary>
        /// Checks whether there is a chunk it should execute after
        /// and whether it's done if so
        /// </summary>
        /// <returns>Whether it is ready to execute</returns>
		public bool IsReady() {
			return executeAfter == null || executeAfter.isDone;
		}

        /// <summary>
        /// Checks whether it should execute alongside another body
        /// and if so, whether it has a speech chunk and the syncing
        /// of the speech chunk with the behaviours is done.
        /// </summary>
        /// <returns>Whether it is ready to be executed alongside another body</returns>
		public bool Synchronized() {
			return executeWith == null || executeWith.SpeechChunk == null || executeWith.syncComplete;
		}

        /// <summary>
        /// Checks whether the body needs to be synchronised with another.
        /// </summary>
        /// <returns>Whether it has a body to sync with.</returns>
		public bool NeedsSync() {
			return executeWith != null;
		}

        /// <summary>
        /// If the body is synchronising with another (in the event of speech syncing with
        /// behaviour), this returns the behaviour chunks.
        /// </summary>
        /// <returns>Behaviour chunks of the body to sync with</returns>
		public Dictionary<string, BMLChunk> GetSyncChunks() {
			if (executeWith != null) {
				return executeWith.Chunks;
			} else {
				return null;
			}
		}

        /// <summary>
        /// Set a body that the body should execute after.
        /// </summary>
        /// <param name="body">The body this body should execute after</param>
		public void ExecuteAfter(BMLBody body) {
			executeAfter = body;
		}

        /// <summary>
        /// Set a body the body should execute with.
        /// </summary>
        /// <param name="body">Body to execute alongside</param>
		public void ExecuteWith (BMLBody body)
		{
			executeWith = body;
		}

        /// <summary>
        /// Add a chunk to the body, becomes the sole SpeechChunk
        /// if it is a SpeechChunk.
        /// </summary>
        /// <param name="chunk">The chunk to add</param>
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

