using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DynamicData;
using ReactiveUI;
using WalletWasabi.Fluent.Models.Wallets;
using WalletWasabi.Fluent.ViewModels.Login;
using WalletWasabi.Fluent.ViewModels.Navigation;
using WalletWasabi.Fluent.ViewModels.SearchBar.Patterns;
using WalletWasabi.Fluent.ViewModels.SearchBar.SearchItems;
using WalletWasabi.Fluent.ViewModels.SearchBar.Sources;
using WalletWasabi.Fluent.ViewModels.Wallets.Advanced;
using WalletWasabi.Wallets;

namespace WalletWasabi.Fluent.ViewModels.Wallets;

public partial class WalletPageViewModel : ViewModelBase, ISearchSource
{
	[AutoNotify] private bool _isLoggedIn;
	[AutoNotify] private bool _isSelected;
	[AutoNotify] private bool _isLoading;
	[AutoNotify] private string? _iconName;
	[AutoNotify] private string? _iconNameFocused;
	[AutoNotify] private WalletViewModel? _walletViewModel;
	[AutoNotify] private RoutableViewModel? _currentPage;

	private WalletPageViewModel(IWalletModel walletModel)
	{
		WalletModel = walletModel;

		// TODO: Finish partial refactor
		// Wallet property must be removed
		Wallet = Services.WalletManager.GetWallets(false).First(x => x.WalletName == walletModel.Name);

		// Show Login Page when wallet is not logged in
		this.WhenAnyValue(x => x.IsLoggedIn)
			.Where(x => !x)
			.Do(_ => ShowLogin())
			.Subscribe();

		// Show Loading page when wallet is logged in
		this.WhenAnyValue(x => x.IsLoggedIn)
			.Where(x => x)
			.Do(_ => ShowWalletLoading())
			.Subscribe();

		// Show main Wallet UI when wallet load is completed
		this.WhenAnyObservable(x => x.WalletModel.Loader.LoadCompleted)
			.Do(_ => ShowWallet())
			.Subscribe();

		this.WhenAnyValue(x => x.WalletModel.Auth.IsLoggedIn)
			.BindTo(this, x => x.IsLoggedIn);

		// Navigate to current page when IsSelected and CurrentPage change
		this.WhenAnyValue(x => x.IsSelected, x => x.CurrentPage)
			.Where(t => t.Item1)
			.Select(t => t.Item2)
			.WhereNotNull()
			.Do(x => UiContext.Navigate().To(x, NavigationTarget.HomeScreen, NavigationMode.Clear))
			.Subscribe();

		SetIcon();

		this.WhenAnyValue(x => x.IsSelected, x => x.IsLoggedIn, (b, a) => a && b)
			.Do(
				shouldShow =>
				{
					if (shouldShow)
					{
						UiContext.CustomSearch.Add(new ActionableItem("Wallet Info", "Display wallet info", async () => UiContext.Navigate().To().WalletInfo(WalletModel), "Wallet") { Icon = "nav_wallet_24_regular"} );
					}
					else
					{
						UiContext.CustomSearch.Remove(new ComposedKey("Wallet Info"));
					}
				}).Subscribe();
			

		SourceCache<ISearchItem, ComposedKey> searchItems = new(x => x.Key);
		Changes = searchItems.Connect();
	}

	
	public IWalletModel WalletModel { get; }
	public Wallet Wallet { get; set; }

	public string Title => WalletModel.Name;

	private void ShowLogin()
	{
		CurrentPage = new LoginViewModel(UiContext, WalletModel);
	}

	private void ShowWalletLoading()
	{
		CurrentPage = new LoadingViewModel(WalletModel);
		IsLoading = true;
	}

	private void ShowWallet()
	{
		WalletViewModel = WalletViewModel.Create(UiContext, this);
		CurrentPage = WalletViewModel;
		IsLoading = false;
	}

	private void SetIcon()
	{
		var walletType = WalletModel.Settings.WalletType;

		var baseResourceName = walletType switch
		{
			WalletType.Coldcard => "coldcard_24",
			WalletType.Trezor => "trezor_24",
			WalletType.Ledger => "ledger_24",
			_ => "wallet_24"
		};

		IconName = $"nav_{baseResourceName}_regular";
		IconNameFocused = $"nav_{baseResourceName}_filled";
	}

	public IObservable<IChangeSet<ISearchItem, ComposedKey>> Changes { get; }
}
