using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
	public Gradient gradient;
	public GradientField gradientField;

	public GradientField gradientField2;

	public Color color;
	[Header("Using UnityEngine.Gradient")]
	public MeshRenderer refClamped;
	public MeshRenderer refPingPong;
	public MeshRenderer refRepeat;

	[Header("Using new Gradient")]
	public MeshRenderer gradClamped;
	public MeshRenderer gradPingPong;
	public MeshRenderer gradRepeat;

    // Start is called before the first frame update
    void Start()
    {
    }

	public float time;
    // Update is called once per frame
    void Update()
    {
		time = Time.time * 0.3f;

		refClamped.material.color = gradient.Evaluate(time);
		gradClamped.material.color = gradientField.Evaluate(time);

		refPingPong.material.color = gradient.Evaluate(Mathf.PingPong(Time.time * .3f, 1));
		gradPingPong.material.color = gradientField.Evaluate(Time.time * .3f, GradientRepeatMode.PingPong);

		refRepeat.material.color = gradient.Evaluate(Mathf.Repeat(Time.time * .3f, 1));
		gradRepeat.material.color = gradientField.Evaluate(Time.time * .3f, GradientRepeatMode.Repeat);
	}
}
