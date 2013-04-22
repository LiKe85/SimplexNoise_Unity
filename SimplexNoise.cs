//This is free and unencumbered software released into the public domain.
//
//Anyone is free to copy, modify, publish, use, compile, sell, or
//distribute this software, either in source code form or as a compiled
//binary, for any purpose, commercial or non-commercial, and by any
//means.
//
//In jurisdictions that recognize copyright laws, the author or authors
//of this software dedicate any and all copyright interest in the
//software to the public domain. We make this dedication for the benefit
//of the public at large and to the detriment of our heirs and
//successors. We intend this dedication to be an overt act of
//relinquishment in perpetuity of all present and future rights to this
//software under copyright law.
//
//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
//EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
//MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
//IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
//OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
//OTHER DEALINGS IN THE SOFTWARE.
//
//For more information, please refer to <http://unlicense.org/>


/// <summary>						
/// This class is an implementation of the algorithm SimplexNoise.
/// The implementation is based on the java implementation by Stefan Gustavson
/// presented in "Simplex noise demystified". Due to the use of a permutation
/// instead of random numbers the same input results always in the same output.
/// </summary>
public class SimplexNoise {

	private static byte[] perm = new byte[512]{151,160,137,91,90,15,131,13,201,
		95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,190,6,
		148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,88,
		237,149,56,87,174,20,125,136,171,168,68,175,74,165,71,134,139,48,27,166,
		77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,
		244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,
		200,196,35,130,116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,
		250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,
		17,182,189,28,42,223,183,170,213,119,248,152,2,44,154,163,70,221,153,
		101,155,167,43,172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,185,
		112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,81,
		51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,184,84,204,176,
		115,121,50,45,127,4,150,254,138,236,205,93,222,114,67,29,24,72,243,141,
		128,195,78,66,215,61,156,180,
		151,160,137,91,90,15,131,13,201,
		95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,190,6,
		148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,88,
		237,149,56,87,174,20,125,136,171,168,68,175,74,165,71,134,139,48,27,166,
		77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,
		244,102,143,54,65,25,63,161,1,216,80,73,209,76,132,187,208,89,18,169,
		200,196,35,130,116,188,159,86,164,100,109,198,173,186,3,64,52,217,226,
		250,124,123,5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,
		17,182,189,28,42,223,183,170,213,119,248,152,2,44,154,163,70,221,153,
		101,155,167,43,172,9,129,22,39,253,19,98,108,110,79,113,224,232,178,185,
		112,104,218,246,97,228,251,34,242,193,238,210,144,12,191,179,162,241,81,
		51,145,235,249,14,239,107,49,192,214,31,181,199,106,157,184,84,204,176,
		115,121,50,45,127,4,150,254,138,236,205,93,222,114,67,29,24,72,243,141,
		128,195,78,66,215,61,156,180 
    };
	
	/// <summary>
    /// Generation of noise and triangular interpolation
	/// </summary>
    /// <param name="x">X part of point to be interpolated</param>
    /// <param name="y">Y part of point to be interpolated</param>
    /// <returns>interpolated value in range [-1,1]</returns>
    public static float Generate(float x, float y)
    {
		const float F2 = 0.366025403f; // F2 = 0.5*(sqrt(3.0)-1.0)
        const float G2 = 0.211324865f; // G2 = (3.0-Math.sqrt(3.0))/6.0

        float n0, n1, n2; // Noise contributions from the three corners

        // Skew the input space to determine which simplex cell we're in
        float s = (x+y)*F2; // Hairy factor for 2D
        float xs = x + s;
        float ys = y + s;
        int i = FastFloor(xs);
        int j = FastFloor(ys);
 

        float t = (float)(i+j)*G2;
		// Unskew the cell origin back to (x,y) space
        float X0 = i-t;
        float Y0 = j-t;
		// The x,y distances from the cell origin
        float x0 = x-X0; 
        float y0 = y-Y0;

        // For the 2D case, the simplex shape is an equilateral triangle.
        // Determine which simplex we are in.
		
        // Offsets for second (middle) corner of simplex in (i,j) coords
		int i1;
		int j1; 
        
		if (x0 > y0)
		{
			// lower triangle, XY order: (0,0)->(1,0)->(1,1)
			i1 = 1;
			j1 = 0;
		} 
        else
		{
			// upper triangle, YX order: (0,0)->(0,1)->(1,1)
			i1 = 0;
			j1 = 1;
		}      

        // A step of (1,0) in (i,j) means a step of (1-c,-c) in (x,y), and
        // a step of (0,1) in (i,j) means a step of (-c,1-c) in (x,y), where
        // c = (3-sqrt(3))/6
		
		// Offsets for middle corner in (x,y) unskewed coords
        float x1 = x0 - i1 + G2; 
        float y1 = y0 - j1 + G2;
		 // Offsets for last corner in (x,y) unskewed coords
        float x2 = x0 - 1.0f + 2.0f * G2;
        float y2 = y0 - 1.0f + 2.0f * G2;

        // Wrap the integer indices at 256, to avoid indexing perm[] out of bounds
        int ii = i % 256;
        int jj = j % 256;

        // Calculate the contribution from the three corners
        float t0 = 0.5f - x0 * x0 - y0 * y0;
        
		if(t0 < 0.0f)
		{
			 n0 = 0.0f;
		}
        else
		{
            t0 *= t0;
			n0 = t0 * t0 * Grad(perm[ii+perm[jj]], x0, y0);
        }

        float t1 = 0.5f - x1 * x1 - y1 * y1;
        if(t1 < 0.0f)
		{
			n1 = 0.0f;
		}
        else
		{
            t1 *= t1;
            n1 = t1 * t1 * Grad(perm[ii+i1+perm[jj+j1]], x1, y1);
        }
	
		float t2 = 0.5f - x2 * x2 - y2 * y2;
        
		if(t2 < 0.0f)
		{
			n2 = 0.0f;
		}
        else
		{
			t2 *= t2;
            n2 = t2 * t2 * Grad(perm[ii+1+perm[jj+1]], x2, y2);
        }

        // Add contributions from each corner to get the final noise value.
        // The result is scaled to return values in the interval [-1,1].
        return (n0 + n1 + n2);
	}
	
	/// <summary>
    /// Faster implementation of floor method.
	/// </summary>
    /// <param name="x">Floating point number</param>
    /// <returns>The largest whole number less than or equal to x.</returns>
	private static int FastFloor(float x)
    {
		return (x > 0) ? ((int)x) : (((int)x) - 1);
    }
	
	/// <summary>
    /// Faster implementation of floor method.
	/// </summary>
    /// <param name="hash">Hash key to acces permutation</param>
    /// <param name="x">X-Index</param>
    /// <param name="y">Y-Index</param>
    /// <returns>The gradient of the given position</returns>
	private static float Grad(int hash, float x, float y)
    {
		int h = hash & 7;      // Convert low 3 bits of hash code
        float u = h < 4 ? x : y;  // into 8 simple gradient directions,
        float v = h < 4 ? y : x;  // and compute the dot product with (x,y).
        return ((h&1) != 0 ? -u : u) + ((h&2) != 0 ? -2.0f * v : 2.0f * v);
    }

}
