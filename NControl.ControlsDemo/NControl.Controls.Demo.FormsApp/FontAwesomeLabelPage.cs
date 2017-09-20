using System;
using Xamarin.Forms;

namespace NControl.Controls.Demo.FormsApp
{
	public class FontAwesomeLabelPage: ContentPage
	{
		public FontAwesomeLabelPage ()
		{
			Title = "FontAwesomeLabel";	
		}

		protected override void OnAppearing ()
		{
			base.OnAppearing ();

		    const int count = 0xf2e0 - 0xf000;
            const int colCount = 10;

		    var rowLayout = new StackLayout() { 			                
                Orientation = StackOrientation.Vertical,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
			};

			var c = 0xf000;
			for (var row = 0; row < count / colCount; row++)
			{
			    var colLayout = new StackLayout
			    {
			        Orientation = StackOrientation.Horizontal,
                    HorizontalOptions = LayoutOptions.StartAndExpand,                    
			    };

				for (var col = 0; col < colCount; col++)
				{
				    var innerStack = new StackLayout
				    {
				        Orientation = StackOrientation.Vertical

				    };
					var label = new FontAwesomeLabel
					{
					    Text = ((char) c++).ToString(),
                        HorizontalOptions = LayoutOptions.CenterAndExpand,
					};
				    var subLabel = new Label
				    {
				        Text = c.ToString("X4")
				    };
                    innerStack.Children.Add(label);
                    innerStack.Children.Add(subLabel);
                    colLayout.Children.Add(innerStack);
				}
                rowLayout.Children.Add(colLayout);
			}

            Content = new ScrollView { Margin = 14, Content = rowLayout };
		}
	}
}

