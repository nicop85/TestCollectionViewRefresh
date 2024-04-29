using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace TestCollectionViewRefresh
{
    public partial class MainPage : ContentPage
    {
        #region Properties
        private List<ShowableUser>? originalList;

        private bool _initialized = false;
        public bool Initialized
        {
            get => _initialized;
            set
            {
                _initialized = value;
                OnPropertyChanged(nameof(Initialized));
                OnPropertyChanged(nameof(RefreshList));
            }
        }

        public ICommand RefreshList
        {
            get => new Command(() => { 
                if (Initialized) 
                    InitializeUsersList(true);

                usersListContainer.IsRefreshing = false;
            });
        }

        public ICommand OnPinSelectedChangeCommand
        {
            get => new Command((param) =>
            {
                var modifiedShowableUser = (ShowableUser)param;

                modifiedShowableUser.IsFavorite = !modifiedShowableUser.IsFavorite;

                var userFound = originalList?.Find(p => p.ID.Equals(modifiedShowableUser.ID));
                if (userFound != null)
                {
                    userFound.IsFavorite = modifiedShowableUser.IsFavorite;
                }

                // Calling update of the displayed list
                UpdateUsersToShow(originalList?.OrderBy(p => p.FullName));

                // Alternative executing the RefreshList Command that works as expected when user triggers it
                // by the desktopRefreshButton or using the gesture in mobile to trigger the RefreshView 
                //RefreshList.Execute(this);
            });
        }

        public ObservableCollection<ShowableUser> UsersToShow { get; set; }
        #endregion

        public MainPage()
        {
            UsersToShow = new();

            InitializeComponent();

            BindingContext = this;
        }

        protected override void OnAppearing()
        {
            Initialized = false;

            base.OnAppearing();

            desktopRefreshButton.IsVisible = DeviceInfo.Idiom.Equals(DeviceIdiom.Desktop);

            InitializeUsersList();

            Initialized = true;
        }

        private void InitializeUsersList(bool forceRefresh = false)
        {
            if(originalList == null || forceRefresh)
            {
                originalList = FakeAccessToADatbaseToGetTheElements();
            }

            UpdateUsersToShow(originalList?.OrderBy(p => p.FullName));
        }

        private List<ShowableUser> FakeAccessToADatbaseToGetTheElements()
        {
            // The first time we return for a fresh new list
            if(originalList == null)
            {
                return new List<ShowableUser>()
                {
                    new(1, "username 1", false),
                    new(2, "username 2", false),
                    new(3, "username 3", false),
                    new(4, "username 4", false),
                    new(5, "username 5", false)
                };
            }
            // After that we return the already initialized list to preserve the changes
            else
            {
                return originalList;
            }
        }

        private void UpdateUsersToShow(IEnumerable<ShowableUser>? usersToShow)
        {
            UsersToShow.Clear();

            if (usersToShow == null)
                return;

            var favoritesFirst = usersToShow.OrderBy(p => !p.IsFavorite).ToList();

            foreach (var user in favoritesFirst)
            {
                UsersToShow.Add(user);
            }
        }
    }

    public class ShowableUser : INotifyPropertyChanged
    {
        public long ID { get; set; }
        public string FullName { get; set; }

        public bool _isFavorite = false;
        public bool IsFavorite
        {
            get
            {
                return _isFavorite;
            }
            set
            {
                _isFavorite = value;
                OnPropertyChanged(nameof(IsFavorite));
            }
        }

        public ShowableUser(long id, string fullName, bool isFavorite)
        {
            ID = id;
            FullName = fullName;
            IsFavorite |= isFavorite;
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}
