//http://rss-glx.sourcearchive.com/documentation/0.9.0-2ubuntu4/noise1234_8h.html
//the Perlin Noise script is done according to the link above

using UnityEngine;
using System.Collections;

public class PerlinNoise
{
  //random jumble of all numbers 0-255
	const int B = 255;
  //permutation table(metathesi)
  //making this an int[] instead of a char[] might make the
  // code run faster on platforms with a high penalty for unaligned single
  // byte addressing.

	int[] m_perm = new int[B+B];

	public PerlinNoise(int seed)
	{
		UnityEngine.Random.seed = seed;

		int i, j, k;
		for (i = 0 ; i < B ; i++) 
		{
			m_perm[i] = i;
		}

		while (--i != 0) 
		{
			k = m_perm[i];
			j = UnityEngine.Random.Range(0, B);
			m_perm[i] = m_perm[j];
			m_perm[j] = k;
		}
	
		for (i = 0 ; i < B; i++) 
		{
			m_perm[B + i] = m_perm[i];
		}
		
	}
	//texture
	float FADE(float t) { return t * t * t * ( t * ( t * 6.0f - 15.0f ) + 10.0f ); }
	//interpolates between t,a,b
	float LERP(float t, float a, float b) { return (a) + (t)*((b)-(a)); }
	
  //Helper functions to compute gradients-dot-residualvectors (1D to 4D)
  //* Note that these generate gradients of more than unit length. To make
  //  * a close match with the value range of classic Perlin noise, the final
  //  * noise values need to be rescaled to fit nicely within [-1,1].
  //  * (The simplex noise functions as such also have different scaling.)
  //  * Note also that these noise functions are the most practical and useful
  //  * signed version of Perlin noise. To return values according to the
  //  * RenderMan specification from the SL noise() and pnoise() functions,
  //  * the noise values need to be scaled and offset to [0,1],

	float GRAD1(int hash, float x ) 
	{
    int h = hash % 15; 
    float grad = 1.0f + (h % 7);/* Gradient value 1.0, 2.0, ..., 8.0 */
    if ((h % 8) < 4) grad = -grad; /* Set a random sign for the gradient */
    return (grad * x); /* Multiply the gradient with the distance */
	}
	
	float GRAD2(int hash, float x, float y)
	{
    int h = hash % 7; /* Convert low 3 bits of hash code */
    float u = h < 4 ? x : y; /* into 8 simple gradient directions, */
    float v = h < 4 ? y : x; /* and compute the dot product with (x,y). */
		int hn = h%2;
		int hm = (h/2)%2;
    	return ((hn != 0) ? -u : u) + ((hm != 0) ? -2.0f*v : 2.0f*v);
	}
	
	
	float GRAD3(int hash, float x, float y , float z)
	{
    int h = hash % 15;  /* Convert low 4 bits of hash code into 12 simple */
    float u = (h < 8) ? x : y;  /* gradient directions, and compute dot product. */
    	float v = (h<4) ? y : (h==12||h==14) ? x : z;
		int hn = h%2;
		int hm = (h/2)%2;
    	return ((hn != 0) ? -u : u) + ((hm != 0) ? -v : v);
	}
	
	float Noise1D( float x )
	{
		//returns a noise value between -0.5 and 0.5
	    int ix0, ix1;
	    float fx0, fx1;
	    float s, n0, n1;
	
	    ix0 = (int)Mathf.Floor(x); 	// Integer part of x
	    fx0 = x - ix0;       	// Fractional part of x
	    fx1 = fx0 - 1.0f;
	    ix1 = ( ix0+1 ) & 0xff;
	    ix0 = ix0 & 0xff;    	// Wrap to 0..255
      // (because px might be greater than 256)
		
	    s = FADE(fx0);
	
	    n0 = GRAD1(m_perm[ix0], fx0);
	    n1 = GRAD1(m_perm[ix1], fx1);
	    return 0.188f * LERP( s, n0, n1);
	}
	
	float Noise2D( float x, float y )
	{
		//returns a noise value between -0.75 and 0.75
	    int ix0, iy0, ix1, iy1;
	    float fx0, fy0, fx1, fy1, s, t, nx0, nx1, n0, n1;
	
	    ix0 = (int)Mathf.Floor(x); 	// Integer part of x
	    iy0 = (int)Mathf.Floor(y); 	// Integer part of y
	    fx0 = x - ix0;        	// Fractional part of x
	    fy0 = y - iy0;        	// Fractional part of y
	    fx1 = fx0 - 1.0f;
	    fy1 = fy0 - 1.0f;
	    ix1 = (ix0 + 1) & 0xff; // Wrap to 0..255
	    iy1 = (iy0 + 1) & 0xff;
	    ix0 = ix0 & 0xff;
	    iy0 = iy0 & 0xff;
	    
	    t = FADE( fy0 );
	    s = FADE( fx0 );
	
		nx0 = GRAD2(m_perm[ix0 + m_perm[iy0]], fx0, fy0);
	    nx1 = GRAD2(m_perm[ix0 + m_perm[iy1]], fx0, fy1);
		
	    n0 = LERP( t, nx0, nx1 );
	
	    nx0 = GRAD2(m_perm[ix1 + m_perm[iy0]], fx1, fy0);
	    nx1 = GRAD2(m_perm[ix1 + m_perm[iy1]], fx1, fy1);
		
	    n1 = LERP(t, nx0, nx1);
	
	    return 0.507f * LERP( s, n0, n1 );
	}
	
	float Noise3D( float x, float y, float z )
	{
		//returns a noise value between -1.5 and 1.5
	    int ix0, iy0, ix1, iy1, iz0, iz1;
	    float fx0, fy0, fz0, fx1, fy1, fz1;
	    float s, t, r;
	    float nxy0, nxy1, nx0, nx1, n0, n1;
	
	    ix0 = (int)Mathf.Floor( x ); // Integer part of x
	    iy0 = (int)Mathf.Floor( y ); // Integer part of y
	    iz0 = (int)Mathf.Floor( z ); // Integer part of z
	    fx0 = x - ix0;        // Fractional part of x
	    fy0 = y - iy0;        // Fractional part of y
	    fz0 = z - iz0;        // Fractional part of z
	    fx1 = fx0 - 1.0f;
	    fy1 = fy0 - 1.0f;
	    fz1 = fz0 - 1.0f;
	    ix1 = ( ix0 + 1 ) & 0xff; // Wrap to 0..255
	    iy1 = ( iy0 + 1 ) & 0xff;
	    iz1 = ( iz0 + 1 ) & 0xff;
	    ix0 = ix0 & 0xff;
	    iy0 = iy0 & 0xff;
	    iz0 = iz0 & 0xff;
	    
	    r = FADE( fz0 );
	    t = FADE( fy0 );
	    s = FADE( fx0 );
	
		nxy0 = GRAD3(m_perm[ix0 + m_perm[iy0 + m_perm[iz0]]], fx0, fy0, fz0);
	    nxy1 = GRAD3(m_perm[ix0 + m_perm[iy0 + m_perm[iz1]]], fx0, fy0, fz1);
	    nx0 = LERP( r, nxy0, nxy1 );
	
	    nxy0 = GRAD3(m_perm[ix0 + m_perm[iy1 + m_perm[iz0]]], fx0, fy1, fz0);
	    nxy1 = GRAD3(m_perm[ix0 + m_perm[iy1 + m_perm[iz1]]], fx0, fy1, fz1);
	    nx1 = LERP( r, nxy0, nxy1 );
	
	    n0 = LERP( t, nx0, nx1 );
	
	    nxy0 = GRAD3(m_perm[ix1 + m_perm[iy0 + m_perm[iz0]]], fx1, fy0, fz0);
	    nxy1 = GRAD3(m_perm[ix1 + m_perm[iy0 + m_perm[iz1]]], fx1, fy0, fz1);
	    nx0 = LERP( r, nxy0, nxy1 );
	
	    nxy0 = GRAD3(m_perm[ix1 + m_perm[iy1 + m_perm[iz0]]], fx1, fy1, fz0);
	   	nxy1 = GRAD3(m_perm[ix1 + m_perm[iy1 + m_perm[iz1]]], fx1, fy1, fz1);
	    nx1 = LERP( r, nxy0, nxy1 );
	
	    n1 = LERP( t, nx0, nx1 );
	    
	    return 0.936f * LERP( s, n0, n1 );
	}
	
	public float FractalNoise1D(float x, int octNum, float frq, float amp)
	{
		float gain = 1.0f;
		float sum = 0.0f;
	
		for(int i = 0; i < octNum; i++)
		{
			sum +=  Noise1D(x*gain/frq) * amp/gain;
			gain *= 2.0f;
		}
		return sum;
	}
	
	public float FractalNoise2D(float x, float y, int octNum, float frq, float amp)
	{
		float gain = 1.0f;
		float sum = 0.0f;
	
		for(int i = 0; i < octNum; i++)
		{
			sum += Noise2D(x*gain/frq, y*gain/frq) * amp/gain;
			gain *= 2.0f;
		}
		return sum;
	}
	
	public float FractalNoise3D(float x, float y, float z, int octNum, float frq, float amp)
	{
		float gain = 1.0f;
		float sum = 0.0f;
	
		for(int i = 0; i < octNum; i++)
		{
			sum +=  Noise3D(x*gain/frq, y*gain/frq, z*gain/frq) * amp/gain;
			gain *= 2.0f;
		}
		return sum;
	}

}













