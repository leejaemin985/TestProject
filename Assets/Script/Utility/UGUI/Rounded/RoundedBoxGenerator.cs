using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace AppLibrary.UGUI
{
	[DisallowMultipleComponent]                     //You can only have one of these in every object.
	[RequireComponent(typeof(RectTransform), typeof(Image))]
	[AddComponentMenu("UI/RoundedBox", 0)]
	public class RoundedBoxGenerator : UIBehaviour
	{
		private static readonly int Props = Shader.PropertyToID("_WidthHeightRadius");
		private static readonly int prop_Thickness = Shader.PropertyToID("_BorderThickness");
		private static readonly int prop_OuterUV = Shader.PropertyToID("_OuterUV");
		private static readonly string materialPath = "UI/RoundedCorners/RoundedCorners";
		//private static readonly string boxName = "RoundedBox";
		//private static readonly string undoText = "Create Rounded Box";
		private Vector4 outerUV = new Vector4(0, 0, 1, 1); //sprite를 사용하는 이미지들은 값이다름

		[SerializeField] private int radius = 20;
		public int Radius { get => radius; set { radius = value; Refresh(); } }
		[SerializeField] private int thickness = 0;
		public int Thickness { get => thickness; set { thickness = value; Refresh(); } }

		private Material material;
		
		[HideInInspector, SerializeField] private Image image;

#if UNITY_EDITOR
		protected override void OnValidate()
		{
			Validate();
			Refresh();
		}
#endif

		protected override void OnDestroy()
		{
			if (image != null)
			{
				image.material = null;      //This makes so that when the component is removed, the UI material returns to null
			}
			if (Application.isPlaying)
			{
				Destroy(material);
			}
			else
			{
				DestroyImmediate(material);
			}
			image = null;
			material = null;
		}

		protected override void OnEnable()
		{
			Validate();
			Refresh();
		}

		protected override void OnRectTransformDimensionsChange()
		{
			if (enabled && material != null)
			{
				Refresh();
			}
		}

		public void Validate()
		{
			if (material == null)
			{
				material = new Material(Shader.Find(materialPath));
				material.enableInstancing = true;
			}

			if (image == null)
			{
				TryGetComponent(out image);
			}

			if (image != null)
			{
				image.material = material;
			}

			if (image is Image uiImage && uiImage.sprite != null)
			{
				outerUV = UnityEngine.Sprites.DataUtility.GetOuterUV(uiImage.sprite);
			}

			if(image != null)
			{
				image.enabled = false;
				image.enabled = true;
			}
		}

		public void Refresh()
		{
			var rect = ((RectTransform)transform).rect;

			//Multiply radius value by 2 to make the radius value appear consistent with ImageWithIndependentRoundedCorners script.
			//Right now, the ImageWithIndependentRoundedCorners appears to have double the radius than this.
			float finalRadius = radius * 2;

			if (rect.height > rect.width)
			{
				if (radius * 2 > rect.width)
					finalRadius = rect.width;
			}
			else
			{
				if (radius * 2 > rect.height)
					finalRadius = rect.height;
			}

			material.SetVector(Props, new Vector4(rect.width, rect.height, finalRadius, 0));
			material.SetVector(prop_OuterUV, outerUV);
			material.SetFloat(prop_Thickness, thickness);

			if (CanvasUpdateRegistry.IsRebuildingLayout())
				return;
			
			image.enabled = false;
			image.enabled = true;
		}
	}
}