﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleViewer.Properties;
using Grabacr07.KanColleWrapper;
using Livet;

namespace Grabacr07.KanColleViewer.Models
{
	public class NotifierHost : NotificationObject
	{
		#region singleton members

		private static readonly NotifierHost instance = new NotifierHost();

		public static NotifierHost Instance
		{
			get { return instance; }
		}

		#endregion

		private NotifierHost() { }

		public void Initialize(KanColleClient client)
		{
			client.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "IsStarted") this.InitializeCore(client);
			};

		}

		private void InitializeCore(KanColleClient client)
		{
			client.Homeport.Repairyard.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Docks") UpdateRepairyard(client.Homeport.Repairyard);
			};
			UpdateRepairyard(client.Homeport.Repairyard);

			client.Homeport.Dockyard.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Docks") UpdateDockyard(client.Homeport.Dockyard);
			};
			UpdateDockyard(client.Homeport.Dockyard);

			client.Homeport.Organization.PropertyChanged += (sender, args) =>
			{
				if (args.PropertyName == "Fleets") UpdateFleets(client.Homeport.Organization);
			};
			UpdateFleets(client.Homeport.Organization);
			UpdateCritical();
		}

		private static void UpdateRepairyard(Repairyard repairyard)
		{
			foreach (var dock in repairyard.Docks.Values)
			{
				dock.Completed += (sender, args) =>
				{
					if (Settings.Current.NotifyRepairingCompleted)
					{
						PluginHost.Instance.GetNotifier().Show(
							NotifyType.Repair,
							Resources.Repairyard_NotificationMessage_Title,
							string.Format(Resources.Repairyard_NotificationMessage, args.DockId, args.Ship.Info.Name),
							() => App.ViewModelRoot.Activate());
					}
				};
			}
		}

		private static void UpdateDockyard(Dockyard dockyard)
		{
			foreach (var dock in dockyard.Docks.Values)
			{
				dock.Completed += (sender, args) =>
				{
					if (Settings.Current.NotifyBuildingCompleted)
					{
						var shipName = Settings.Current.CanDisplayBuildingShipName
							? args.Ship.Name
							: Resources.Common_ShipGirl;

						PluginHost.Instance.GetNotifier().Show(
							NotifyType.Build,
							Resources.Dockyard_NotificationMessage_Title,
							string.Format(Resources.Dockyard_NotificationMessage, args.DockId, shipName),
							() => App.ViewModelRoot.Activate());
					}
				};
			}
		}

		private static void UpdateCritical()
		{
			KanColleClient.Current.OracleOfCompass.CriticalCondition += () =>
			{

				if (Models.Settings.Current.EnableCriticalNotify)
				{
					PluginHost.Instance.GetNotifier().Show(
						NotifyType.Critical,
						Resources.ReSortie_CriticalConditionMessage_Title, Resources.ReSortie_CriticalConditionMessage,
					() => App.ViewModelRoot.Activate());
				}
				if (Models.Settings.Current.EnableCriticalAccent)
					App.ViewModelRoot.Mode = Mode.CriticalCondition;
			};


			KanColleClient.Current.OracleOfCompass.CriticalCleared += () =>
			{
				if (App.ViewModelRoot.Mode != Mode.Started) App.ViewModelRoot.Mode = Mode.Started;
			};

		}
		private static void UpdateFleets(Organization organization)
		{
			foreach (var fleet in organization.Fleets.Values)
			{
				fleet.Expedition.Returned += (sender, args) =>
				{
					if (Settings.Current.NotifyExpeditionReturned)
					{
						PluginHost.Instance.GetNotifier().Show(
							NotifyType.Expedition,
							Resources.Expedition_NotificationMessage_Title,
							string.Format(Resources.Expedition_NotificationMessage, args.FleetName),
							() => App.ViewModelRoot.Activate());
					}
				};

				fleet.State.Condition.Rejuvenated += (sender, args) =>
				{
					if (Settings.Current.NotifyFleetRejuvenated)
					{
						PluginHost.Instance.GetNotifier().Show(
							NotifyType.Rejuvenated,
							Resources.ReSortie_NotificationMessage_Title,
							string.Format(Resources.ReSortie_NotificationMessage, args.FleetName),
							() => App.ViewModelRoot.Activate());
					}
				};
			}
		}
	}
}
