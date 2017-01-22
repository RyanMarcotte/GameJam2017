using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeBehaviour : MonoBehaviour {
    
	// Use this for initialization
	
	// Update is called once per frame
	void Update () {
	}

    public void Fade()
    {
        StartCoroutine("FadeMechanic");
    }
    
    private IEnumerator FadeMechanic()
    {
        yield return new WaitForSeconds(0.5f);
        var sonarRenderer = GetComponent<Renderer>();
        while (sonarRenderer.material.color.a > 0.0f)
        {
            yield return new WaitForSeconds(0.1f);
            Color color = sonarRenderer.material.color;
            color .a -= 0.05f;
            sonarRenderer.material.color = color;
        }
        
        Destroy(this.gameObject);
    }
}
