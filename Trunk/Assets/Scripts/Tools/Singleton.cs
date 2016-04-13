using UnityEngine;

/// <summary>
/// Used to create script singletons
/// </summary>
/// <typeparam name="T">The monobehaviour script type</typeparam>
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    /// <summary>
    /// Stores the instance.
    /// </summary>
	protected static T instance;
	
	/// <summary>
    /// Returns the instance of the singleton if it exists, otherwise creates it.
    /// </summary>
	public static T Instance
	{
		get
		{
			if(instance == null)
			{
				instance = (T) FindObjectOfType(typeof(T));
				
				if (instance == null)
				{
					Debug.LogError("An instance of " + typeof(T) + 
					               " is needed in the scene, but there is none.");
				}
			}
			
			return instance;
		}
	}
}