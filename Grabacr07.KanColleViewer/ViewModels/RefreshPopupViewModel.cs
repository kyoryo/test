﻿
namespace Grabacr07.KanColleViewer.ViewModels
{
	public class RefreshPopupViewModel : WindowViewModel
	{
		public RefreshPopupViewModel()
		{
			
		}
		public void RefreshNav()
		{
			KanColleViewer.Views.MainWindow.Current.RefreshNavigator();
			this.PopupClose();
		}
		public void PopupClose()
		{
			KanColleViewer.Views.RefreshPopup.Current.Close();
		}
	}
}
