using UnityEngine;

public class VFXAnimThreshold : MonoBehaviour
{
	public enum InputProperty
	{
		LocalRotation,
		WorldPosition,
		WorldScale,
		AbsoluteWorldVelocity
	}

	public enum OutputProperty
	{
		BirthColor,
		CurrentColor,
		BirthSize,
		CurrentSize,
		BirthLifetime,
		MaterialColor,
		MaterialFloat
	}

	public Animator animator;

	public string parameterName;

	public bool useCurves;

	[SerializeField]
	private Transform _inputTransform;

	[SerializeField]
	private ParticleSystem _particleSystem;

	[SerializeField]
	private Renderer _renderer;

	[Space(20f)]
	[Tooltip("This is the property of TargetAnimatedObject that we're listening for")]
	public InputProperty input;

	[Tooltip("Scales the particle property that is set. Properties named 'current' affect all living particles simultaneously and steal the 'X by Speed' properties from the shuriken system to do this.")]
	public OutputProperty target;

	[Tooltip("This is multiplied into the associet particle property. For Size and Lifetime Values, the Alpha Value is used.")]
	[ColorUsage(true, true)]
	public Color OnColor = Color.white;

	[ColorUsage(true, true)]
	public Color OffColor = Color.black;

	public bool UseGradient;

	public Gradient OutputGradient;

	[Space(20f)]
	public Vector3 OnValue = Vector3.one;

	public Vector3 OffValue = Vector3.zero;

	[Tooltip("Masks the sensitivity to the input vector. A value of 0,1,0 is only affected by the Y value of the input property.")]
	public Vector3 ChannelMask = Vector3.one;

	[Space(20f)]
	public Vector3 CurrentInputValue;

	public Color OutputColor;

	public int[] materialsToControl;

	public string MaterialValueName = "_Color";

	private Material[] mats;

	public float thresholdVal;

	public Vector3 lastPos = Vector3.zero;

	public Vector3 velocity = Vector3.zero;

	public Vector3 currentPos = Vector3.zero;

	private void Awake()
	{
		if (UseGradient && OutputGradient == null)
		{
			Debug.LogError("No valid gradient for " + base.gameObject.GetFullPath());
		}
		if (useCurves && animator == null)
		{
			Debug.LogError("No valid animator for " + base.gameObject.GetFullPath());
		}
		if (_particleSystem == null)
		{
			_particleSystem = GetComponent<ParticleSystem>();
		}
		if (_renderer == null)
		{
			_renderer = GetComponent<Renderer>();
		}
		if (_inputTransform == null)
		{
			_inputTransform = base.gameObject.transform;
		}
	}

	private void Update()
	{
		CurrentInputValue = CalculateInput();
		thresholdVal = FindThresholdVal();
		thresholdVal = Mathf.Clamp01(thresholdVal);
		if (useCurves)
		{
			thresholdVal = GetParamValue();
		}
		else
		{
			thresholdVal = FindThresholdVal();
			thresholdVal = Mathf.Clamp01(thresholdVal);
		}
		switch (target)
		{
		case OutputProperty.BirthColor:
			SetBirthColor();
			break;
		case OutputProperty.CurrentColor:
			SetCurrentColor();
			break;
		case OutputProperty.BirthSize:
		case OutputProperty.CurrentSize:
			SetCurrentSize();
			break;
		case OutputProperty.BirthLifetime:
			SetBirthLifetime();
			break;
		case OutputProperty.MaterialColor:
			SetMaterialVector();
			break;
		case OutputProperty.MaterialFloat:
			SetMaterialFloat();
			break;
		}
	}

	private Vector3 CalculateInput()
	{
		return input switch
		{
			InputProperty.LocalRotation => LocalRotationInput(), 
			InputProperty.WorldPosition => WorldPositionInput(), 
			InputProperty.WorldScale => WorldScaleInput(), 
			InputProperty.AbsoluteWorldVelocity => SolveVelocity(), 
			_ => CurrentInputValue, 
		};
	}

	private Vector3 LocalRotationInput()
	{
		if (_inputTransform != null)
		{
			return _inputTransform.localEulerAngles;
		}
		return CurrentInputValue;
	}

	private Vector3 WorldPositionInput()
	{
		if (_inputTransform != null)
		{
			return _inputTransform.position;
		}
		return CurrentInputValue;
	}

	private Vector3 WorldScaleInput()
	{
		if (_inputTransform != null)
		{
			return _inputTransform.lossyScale;
		}
		return CurrentInputValue;
	}

	private Vector3 SolveVelocity()
	{
		if (_inputTransform != null)
		{
			velocity = new Vector3(Mathf.Abs(_inputTransform.position.x - lastPos.x), Mathf.Abs(_inputTransform.position.y - lastPos.y), Mathf.Abs(_inputTransform.position.z - lastPos.z));
			lastPos = _inputTransform.position;
			return velocity;
		}
		return CurrentInputValue;
	}

	private float GetParamValue()
	{
		return animator.GetFloat(parameterName);
	}

	private float FindThresholdVal()
	{
		Vector3 zero = Vector3.zero;
		zero.x = Mathf.InverseLerp(OnValue.x, OffValue.x, CurrentInputValue.x);
		zero.y = Mathf.InverseLerp(OnValue.y, OffValue.y, CurrentInputValue.y);
		zero.z = Mathf.InverseLerp(OnValue.z, OffValue.z, CurrentInputValue.z);
		return Vector3.Dot(zero, ChannelMask);
	}

	private void SetCurrentColor()
	{
		if (_particleSystem == null)
		{
			Debug.LogError("No valid Particle System for " + base.gameObject.GetFullPath());
			return;
		}
		ParticleSystem.ColorBySpeedModule colorBySpeed = _particleSystem.colorBySpeed;
		OutputColor = Color.Lerp(OnColor, OffColor, thresholdVal);
		if (UseGradient)
		{
			OutputColor = OutputGradient.Evaluate(thresholdVal);
		}
		colorBySpeed.color = OutputColor;
	}

	private void SetBirthColor()
	{
		if (_particleSystem == null)
		{
			Debug.LogError("No valid Particle System for " + base.gameObject.GetFullPath());
			return;
		}
		ParticleSystem.MinMaxGradient startColor = _particleSystem.main.startColor;
		OutputColor = Color.Lerp(OnColor, OffColor, thresholdVal);
		if (UseGradient)
		{
			startColor.color = OutputGradient.Evaluate(thresholdVal);
		}
		startColor.color = OutputColor;
	}

	private void SetCurrentSize()
	{
		if (_particleSystem == null)
		{
			Debug.LogError("No valid Particle System for " + base.gameObject.GetFullPath());
			return;
		}
		ParticleSystem.SizeBySpeedModule sizeBySpeed = _particleSystem.sizeBySpeed;
		OutputColor = Color.Lerp(OnColor, OffColor, thresholdVal);
		if (UseGradient)
		{
			OutputColor = OutputGradient.Evaluate(thresholdVal);
		}
		sizeBySpeed.sizeMultiplier = OutputColor.a;
	}

	private void SetBirthLifetime()
	{
		if (_particleSystem == null)
		{
			Debug.LogError("No valid Particle System for " + base.gameObject.GetFullPath());
			return;
		}
		_ = _particleSystem.main.startLifetime;
		OutputColor = Color.Lerp(OnColor, OffColor, thresholdVal);
		if (UseGradient)
		{
			OutputColor = OutputGradient.Evaluate(thresholdVal);
		}
		_ = (ParticleSystem.MinMaxCurve)OutputColor.a;
	}

	private void SetMaterialVector()
	{
		mats = _renderer?.materials;
		if (mats == null)
		{
			Debug.LogError("No valid Renderer for " + base.gameObject.GetFullPath());
			return;
		}
		OutputColor = Color.Lerp(OnColor, OffColor, thresholdVal);
		if (UseGradient)
		{
			OutputColor = OutputGradient.Evaluate(thresholdVal);
		}
		for (int i = 0; i < materialsToControl.Length; i++)
		{
			int num = materialsToControl[i];
			mats[num].SetVector(MaterialValueName, OutputColor);
		}
		_renderer.materials = mats;
	}

	private void SetMaterialFloat()
	{
		mats = _renderer?.materials;
		if (mats == null)
		{
			Debug.LogError("No valid Renderer for " + base.gameObject.GetFullPath());
			return;
		}
		OutputColor = Color.Lerp(OnColor, OffColor, thresholdVal);
		if (UseGradient)
		{
			OutputColor = OutputGradient.Evaluate(thresholdVal);
		}
		for (int i = 0; i < materialsToControl.Length; i++)
		{
			int num = materialsToControl[i];
			mats[num].SetFloat(MaterialValueName, OutputColor.a);
		}
		_renderer.materials = mats;
	}
}
