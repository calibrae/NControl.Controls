﻿using System;
using NControl.Abstractions;
using NGraphics;
using Xamarin.Forms;
using System.IO;
using System.Reflection;

namespace NControl.Controls
{
	/// <summary>
	/// Svg image.
	/// </summary>
	public class SvgImage: NControlView
	{
		#region Private Members

		/// <summary>
		/// The svg graphics.
		/// </summary>
		private Graphic _graphics;

		/// <summary>
		/// The size.
		/// </summary>
		private NGraphics.Size _size;

		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="NControl.Controls.SvgImage"/> class.
		/// </summary>
		public SvgImage ()
		{
		}

		#region Properties

		/// <summary>
		/// The resource name property.
		/// </summary>
		public static BindableProperty SvgResourceProperty = 
			BindableProperty.Create(nameof(SvgResource), typeof(string), typeof(SvgImage), null,
				propertyChanged: (bindable, oldValue, newValue) => {
					var ctrl = (SvgImage)bindable;
					ctrl.UpdateSvg();
                    ctrl.Invalidate();
				});

		/// <summary>
		/// Gets or sets the name of the resource.
		/// </summary>
		/// <value>The name of the resource.</value>
		public string SvgResource {
			get => (string)GetValue (SvgResourceProperty);
		    set => SetValue (SvgResourceProperty, value);
		}

		/// <summary>
		/// The SvgAssembly property.
		/// </summary>
		public static BindableProperty SvgAssemblyProperty = 
			BindableProperty.Create(nameof(SvgAssembly), typeof(Assembly), typeof(SvgImage), null,
				propertyChanged: (bindable, oldValue, newValue) => {
					var ctrl = (SvgImage)bindable;
					ctrl.UpdateSvg();
                    ctrl.Invalidate();
				});

		/// <summary>
		/// Gets or sets the SvgAssembly of the SvgImage instance.
		/// </summary>
		/// <value>The color of the buton.</value>
		public Assembly SvgAssembly {
			get => (Assembly)GetValue (SvgAssemblyProperty);
		    set => SetValue (SvgAssemblyProperty, value);
		}

		/// <summary>
		/// The AssemblyType property.
		/// </summary>
		public static BindableProperty SvgAssemblyTypeProperty = 
			BindableProperty.Create(nameof(SvgAssemblyType), typeof(Type), typeof(SvgImage), null,
				propertyChanged: (bindable, oldValue, newValue) => {
					var ctrl = (SvgImage)bindable;
					ctrl.UpdateSvg();
                    ctrl.Invalidate();
		});

		/// <summary>
		/// Gets or sets the AssemblyType of the SvgImage instance. The type will be used
		/// to retrieve the assembly to read svgs from
		/// </summary>
		/// <value>The color of the buton.</value>
		public Type SvgAssemblyType {
			get => (Type)GetValue (SvgAssemblyTypeProperty);
		    set => SetValue (SvgAssemblyTypeProperty, value);
		}
		#endregion

		#region Private Members

		/// <summary>
		/// Updates the svg graphics.
		/// </summary>
		private void UpdateSvg()
		{
			_graphics = null;

			if ((SvgAssembly == null && SvgAssemblyType == null) || string.IsNullOrEmpty (SvgResource))
				return;

			try 
			{
				var assembly = SvgAssembly;
				if(assembly == null && SvgAssemblyType != null) 
					assembly = SvgAssemblyType.GetTypeInfo().Assembly;
				
				using (var stream = assembly.GetManifestResourceStream (SvgResource)) {

					var svgReader = new SvgReader (new StreamReader (stream));
					_graphics = svgReader.Graphic;
					_size = new NGraphics.Size(Width, Height);
				}
			} 
			catch (Exception ex) 
			{
				throw new Exception("An error occured when reading the Svg Resource: " + ex.Message);
			}
		}

		#endregion

		#region Drawing Code

		/// <summary>
		/// Draw the specified canvas and rect.
		/// </summary>
		/// <param name="canvas">Canvas.</param>
		/// <param name="rect">Rect.</param>
		public override void Draw (NGraphics.ICanvas canvas, NGraphics.Rect rect)
		{
			base.Draw (canvas, rect);

			if (_graphics != null) {
				// Draw the graphics if it is set
				if (rect.Width != Width) {
					var scale = rect.Width / Width;
					_graphics.Size = new NGraphics.Size (_size.Width * scale, _size.Height * scale);
				} else
					_graphics.Size = _size;
				
				_graphics.Draw (canvas);			
			}

			else {
				// Draw a placeholder - indicates that we are missing an icon
				#if DEBUG
				canvas.DrawRectangle (rect, null, NGraphics.Brushes.Red);
				canvas.DrawPath (new PathOp[] { 
					// Draw cross/x
					new MoveTo (rect.X, rect.Y),
					new LineTo (rect.Right, rect.Bottom),
					new MoveTo (rect.X, rect.Bottom),
					new LineTo (rect.Right, rect.Y)
				}, NGraphics.Pens.Blue, null);
				#endif
			}
		}
		#endregion

		#region Overrides
        
		/// <param name="widthConstraint">The available width for the element to use.</param>
		/// <param name="heightConstraint">The available height for the element to use.</param>
		/// <summary>
		/// This method is called during the measure pass of a layout cycle to get the desired size of an element.
		/// </summary>
		protected override SizeRequest OnMeasure (double widthConstraint, double heightConstraint)
		{
			var retVal = base.OnMeasure (widthConstraint, heightConstraint);

			if (_graphics != null) {

				var width = retVal.Request.Width;
				var height = retVal.Request.Height;
				var sizeRatio = 1.0;

				if(!double.IsPositiveInfinity(heightConstraint) && !double.IsPositiveInfinity(widthConstraint))
				{
					if(heightConstraint < widthConstraint)
						sizeRatio = Math.Abs(heightConstraint) < double.Epsilon ? _graphics.ViewBox.Size.Height : 
							heightConstraint/_graphics.ViewBox.Size.Height;
					else 
						sizeRatio = Math.Abs(widthConstraint) < double.Epsilon ? _graphics.ViewBox.Size.Width :
							widthConstraint/_graphics.ViewBox.Size.Width;
				}
				else if (!double.IsPositiveInfinity(heightConstraint)) 
				{
					sizeRatio = Math.Abs(heightConstraint) < double.Epsilon ? _graphics.ViewBox.Size.Height : 
						heightConstraint/_graphics.ViewBox.Size.Height;					
				}
				else if (!double.IsPositiveInfinity(widthConstraint)) 
				{
					sizeRatio = Math.Abs(widthConstraint) < double.Epsilon ? _graphics.ViewBox.Size.Width :
						widthConstraint/_graphics.ViewBox.Size.Width;					
				} 

				height = _graphics.ViewBox.Size.Height * sizeRatio;
				width = _graphics.ViewBox.Size.Width * sizeRatio;

				// Update retVal
				retVal.Request = new Xamarin.Forms.Size(width, height);

				// Update graphics size
				_size = new NGraphics.Size(width, height);
			}

			return retVal;

		}
		#endregion
	}
}

