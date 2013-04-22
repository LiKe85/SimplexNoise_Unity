// This is free and unencumbered software released into the public domain.
//
// Anyone is free to copy, modify, publish, use, compile, sell, or
// distribute this software, either in source code form or as a compiled
// binary, for any purpose, commercial or non-commercial, and by any
// means.
//
// In jurisdictions that recognize copyright laws, the author or authors
// of this software dedicate any and all copyright interest in the
// software to the public domain. We make this dedication for the benefit
// of the public at large and to the detriment of our heirs and
// successors. We intend this dedication to be an overt act of
// relinquishment in perpetuity of all present and future rights to this
// software under copyright law.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
// IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
// OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
// ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// For more information, please refer to <http://unlicense.org/>

using UnityEngine;
using System.Collections;

/// <summary>						
/// This class can be added to an unity game object with an terrain component
/// attached. While running the scene a button is created. Pressing it will
/// result in the manipulation of the terrains heightmap by using fBm. The used
/// noisefunction is SimplexNoise and was invented by Ken Perlin.
/// fBm parameters as octaves, baseFrequency and baseAmplitude can be set in
/// the Unity Editor to get different types of heightmaps.
/// With the variable blend you can control how much the fbM will affect the
/// terrain manipulation.
/// </summary>
[RequireComponent (typeof (Terrain))]
public class CreateSimplexNoise : MonoBehaviour {
	
	// Variables for fBm
	public int octaves = 8;
	public int baseFrequency = 4;
	public float baseAmplitude = 1.0f;
	
	// Value for blending the fractal map over the existing one
	public float blend = 1.0f;// Initial 100%
	
	// Variables to handle Normalization betwenn 0 and 1
	private float normaliseMin = 0.0f;
	private float normaliseMax = 1.0f;
	
	private Terrain terrain;
	
	// Sizes fot GUI
	private int btWidth = 100;
	private int btHeight = 30;
 
	/// <summary>
	/// Initialization
	/// </summary>
	void Start () {
		
		// Check if needed terrain component is present
		terrain = GetComponent<Terrain>();
		if (terrain == null)
		{
			Debug.LogError("Missing Terrain Component");
			return;
		}
	}
	
	/// <summary>
	/// OnGUI is called once per frame
	/// </summary>
	void OnGUI () {
		
		// Layout start
		GUI.Label(new Rect(0, 0, 0, 0), "");
		GUI.BeginGroup(new Rect(Screen.width / 2 - btWidth / 2,
		    Screen.height - 2 * btHeight,
			Screen.width, Screen.height / 5));
		// Button to create Simplex Noise
		if (GUI.Button(new Rect(0, 0, btWidth, btHeight), "Simplex Noise"))
		{	
			// Read the current Terrain Data
			TerrainData terData = terrain.terrainData;
			int terWidth = terData.heightmapWidth;
			int terHeight = terData.heightmapHeight;
			// Generate fBm
			float[,] generatedHeights = GenerateSimplex(terWidth, terHeight);
			// Blend between old map and fBm map
			generatedHeights = Blend(generatedHeights,
				terData.GetHeights(0, 0, terWidth, terHeight));
			// Set result to terrain
			terData.SetHeights(0, 0, generatedHeights);
		}	
		// Layout end
		GUI.EndGroup();
	}
	
	/// <summary>
	/// Generates a heightmap using SimplexNoise.
	/// </summary>
	/// <param name="terWidth">Width of the terrains heightmap</param>
	/// <param name="terHeight">Height of the terrains heightmap</param>
    /// <returns>A heightmap as two dimensional float array</returns>
	private float[,] GenerateSimplex(int terWidth, int terHeight) {
		
		// Variable for return value
		float[,] heightMap = new float[terWidth, terHeight];
		
		int frequency = baseFrequency;
		float amplitude = baseAmplitude;
		
		for (int i = 0; i < octaves; i++)
		{

			// Ratio between terrain map and noise map
			float xRatio = (float) frequency / (float) terWidth;
			float yRatio = (float) frequency / (float) terHeight;
			
			for (int x = 0; x < terWidth; x++)
			{
				for (int y = 0; y < terHeight; y++)
				{	
					// Mapping height map to noise map
					float xIndex = (x * xRatio);
					float yIndex = (y * yRatio);
					
					float value = SimplexNoise.Generate(xIndex, yIndex);
					heightMap[x, y] += (value * amplitude);
				}
			}
			frequency *= 2;
			amplitude /= 2;
		}
		
		heightMap = Normalize(heightMap);
		return heightMap;
	}
	
	/// <summary>
	/// Normalize a generated heightmap by transform all values to the range
	/// from 0 to 1.
	/// </summary>
	/// <param name="heightMap">heightmap as two dimensional float array</param>
    /// <returns>A normalized heightmap as two dimensional float array</returns>
	private float[,] Normalize(float[,] heightMap) {
		
		int terWidth = heightMap.GetLength(0);
		int terHeight = heightMap.GetLength(1);
	
		// Variable to save the minimal height
		float heightMin = 0.0f;
		// Variable to save the maximal height
		float heightMax = 1.0f;
		
		// Find minimal and maximal height
		foreach (float height in heightMap)
		{	
			if (height < heightMin)
			{
				heightMin = height;
			}
			else if (height > heightMax)
			{
				heightMax = height;
			}
		}
		
		// Get the current range and the normalization range
		float heightRange = heightMax - heightMin;
		float normalisedHeightRange = normaliseMax - normaliseMin;
		
		// Normalize...
		for (int y = 0; y < terHeight; y++)
		{
			for (int x = 0; x < terWidth; x++)
			{
				float normalisedHeight = ((heightMap[x, y] - heightMin)
				    / heightRange) * normalisedHeightRange;
				heightMap[x, y] = normaliseMin + normalisedHeight;
			}
		}
		return heightMap;
	}

	/// <summary>
	/// Blends two heightmaps using linear interpolation.
	/// The percentaged blending factor can be set in the Unity Editor.
	/// </summary>
	/// <param name="newHeightMap">The new heightmap</param>
	/// <param name="oldHeightMap">The old heightmap</param>
    /// <returns>A heightmap as two dimensional float array</returns>
	private float[,] Blend(float[,] newHeightMap , float[,] oldHeightMap) {
		
		int terWidth = oldHeightMap.GetLength(0);
		int terHeight = oldHeightMap.GetLength(1);
		
		// Variable for return value
		float[,] heightMap = new float[terWidth, terHeight];
		
		// Linear Interpolation between new and old terrain
		for (int x = 0; x < terWidth; x++)
		{
			for (int y = 0; y < terHeight; y++)
			{	
				heightMap[x, y] = (newHeightMap[x, y] * blend
					+ oldHeightMap[x, y] * ( 1f - blend));
			}	
		}
		return heightMap;
	}
}
