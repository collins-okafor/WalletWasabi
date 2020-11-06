using Avalonia;
using Avalonia.Markup.Xaml.Styling;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive.Threading.Tasks;
using System.Windows.Input;
using WalletWasabi.Fluent.ViewModels.Dialogs;

namespace WalletWasabi.Fluent.ViewModels
{
	public class SettingsPageViewModel : NavBarItemViewModel
	{
		private string _randomString;

		public SettingsPageViewModel(NavigationStateViewModel navigationState) : base(navigationState, NavigationTarget.Dialog)
		{
			Title = "Settings";

			BackCommand = ReactiveCommand.Create(() => ClearNavigation());

			NextCommand = ReactiveCommand.Create(() => navigationState.HomeScreen?.Invoke().Router.Navigate.Execute(new SettingsPageViewModel(navigationState)));

			OpenDialogCommand = ReactiveCommand.CreateFromTask(async () => await ConfirmSetting.Handle("Please confirm the setting:").ToTask());

			OpenDialogScreenCommand = ReactiveCommand.Create(() => navigationState.DialogScreen?.Invoke().Router.Navigate.Execute(new SettingsPageViewModel(navigationState)));

			ConfirmSetting = new Interaction<string, bool>();

			ConfirmSetting.RegisterHandler(
				async interaction =>
				{
					var x = new TestDialogViewModel(navigationState, NavigationTarget.Default, interaction.Input);
					var result = await x.ShowDialogAsync(navigationState.DialogHost());
					interaction.SetOutput(result);
				});

			ChangeThemeCommand = ReactiveCommand.Create(() =>
			{
				var currentTheme = Application.Current.Styles.Select(x => (StyleInclude)x).FirstOrDefault(x => x.Source is { } && x.Source.AbsolutePath.Contains("Themes"));

				if (currentTheme?.Source is { } src)
				{
					var themeIndex = Application.Current.Styles.IndexOf(currentTheme);

					var newTheme = new StyleInclude(new Uri("avares://WalletWasabi.Fluent/App.xaml"))
					{
						Source = new Uri($"avares://WalletWasabi.Fluent/Styles/Themes/{(src.AbsolutePath.Contains("Light") ? "BaseDark" : "BaseLight")}.xaml")
					};

					Application.Current.Styles[themeIndex] = newTheme;
				}
			});
		}

		public ICommand NextCommand { get; }

		public ICommand OpenDialogCommand { get; }

		public ICommand OpenDialogScreenCommand { get; }

		public Interaction<string, bool> ConfirmSetting { get; }

		public ICommand ChangeThemeCommand { get; }

		public override string IconName => "settings_regular";
	}
}